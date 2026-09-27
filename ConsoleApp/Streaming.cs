using Silk.NET.OpenAL;

namespace SoundGeneration
{
    public class Channel
    {
        private IDictionary<uint, short[]> _buffers = new Dictionary<uint, short[]>();
        private int _sampleRate;
        private uint _sourceId;

        public unsafe Channel(int sampleRate)
        {
            _sampleRate = sampleRate;

            var al = AL.GetApi(true);
            _sourceId = al.GenSource();

            var bufferSize = (uint)(0.1 * sampleRate);
            var numBuffers = 2;

            var bufferIds = new uint[numBuffers];
            fixed (uint* ptr = bufferIds)
            {
                al.GenBuffers(numBuffers, ptr);

                for (var i = 0; i < numBuffers; i++)
                    _buffers[ptr[i]] = new short[bufferSize];
            }
        }

        public unsafe void Play(Func<double, double?> getNextSample)
        {
            Console.WriteLine($"Play started - num buffers={_buffers.Count()}");

            foreach (var buffer in _buffers)
            {
                var sampleCount = FillBuffer(getNextSample, buffer.Value);
                if (sampleCount == 0)
                    break;

                QueueBuffer(buffer.Key, buffer.Value);
                Console.WriteLine("Buffer Queued");
            }

            AL.GetApi(true).SourcePlay(_sourceId);
            Console.WriteLine("Play done");
        }

        private int FillBuffer(Func<double, double?> getNextSample, short[] buffer)
        {
            int sampleCount = 0;
            var timeStep = 1.0 / _sampleRate;
            for (int i = 0; i < buffer.Length; i++)
            {
                var sample = getNextSample(timeStep);
                if (sample != null)
                {
                    buffer[i] = (Int16)(sample.Value * Int16.MaxValue);
                    sampleCount++;
                }
                else
                    buffer[i] = 0;
            }

            return sampleCount;
        }

        private unsafe void QueueBuffer(uint bufferId, short[] buffer)
        {
            var al = AL.GetApi(true);
            al.BufferData(bufferId, BufferFormat.Mono16, buffer, _sampleRate);
            al.SourceQueueBuffers(_sourceId, 1, &bufferId);
        }

        public unsafe bool Update(Func<double, double?> getNextSample)
        {
            var al = AL.GetApi(true);

            int buffersProcessed = 0;
            al.GetSourceProperty(_sourceId, GetSourceInteger.BuffersProcessed, &buffersProcessed);

            int buffersFreshlyQueued = 0;
            while (buffersProcessed > 0)
            {
                uint bufferId = 0;

                unsafe
                {
                    al.SourceUnqueueBuffers(_sourceId, 1, &bufferId);

                    buffersProcessed--;

                    var buffer = _buffers[bufferId];

                    var sampleCount = FillBuffer(getNextSample, buffer);
                    if (sampleCount == 0)
                        continue;

                    QueueBuffer(bufferId, buffer);
                    buffersFreshlyQueued++;
                }
            }

            int buffersQueued = 0;
            al.GetSourceProperty(_sourceId, GetSourceInteger.BuffersQueued, &buffersQueued);
            buffersQueued += buffersFreshlyQueued;
            if (buffersQueued == 0)
                al.SourceStop(_sourceId);

            return buffersQueued > 0;
        }
    }

    public unsafe class Mixer
    {
        private Stack<Channel> _available = new Stack<Channel>();
        private IDictionary<Channel, Stream> _playing = new Dictionary<Channel, Stream>();

        public unsafe Mixer()
        {
            // 1. Get the OpenAL context API and open the default device
            var alc = ALContext.GetApi(true);
            var device = alc.OpenDevice(null);
            if (device == null)
            {
                Console.WriteLine("Could not open default OpenAL device.");
                return;
            }

            // 2. Create an OpenAL context and make it current
            var context = alc.CreateContext(device, null);
            alc.MakeContextCurrent(context);
        }

        // This is a poc method and shouldn't be used.  it creates resources every time you play a sound and they don't get released
        public uint PlayPcmSamples(byte[] pcmSamples, int sampleRate = 44100, int bitsPerSample = 16, int channels = 1)
        {
            // 3. Get the OpenAL API
            var al = AL.GetApi(true);

            // 5. Create an OpenAL buffer and fill it with audio data
            uint buffer = al.GenBuffer();
            var format = channels == 1 ? bitsPerSample == 8 ? BufferFormat.Mono8 : BufferFormat.Mono16 : bitsPerSample == 8 ? BufferFormat.Stereo8 : BufferFormat.Stereo16;
            al.BufferData(buffer, format, pcmSamples, sampleRate);

            // 6. Create an OpenAL source and attach the buffer
            uint source = al.GenSource();
            al.SetSourceProperty(source, SourceInteger.Buffer, buffer);

            // 7. Play the sound
            al.SourcePlay(source);

            return source;
        }

        public void PlayStream(Func<double, double?> getNextSample, Action stopComplete, int sampleRate = 44100)
        {
            var channel = _available.Any() ? _available.Pop() : new Channel(sampleRate);

            _playing[channel] = new Stream(getNextSample, stopComplete);
            channel.Play(getNextSample);
        }

        public void Update()
        {
            var channels = _playing.Keys.ToArray();
            foreach(var channel in channels)
            {
                if (!channel.Update(_playing[channel].GetNextSample))
                {
                    _playing[channel].StopComplete?.Invoke();
                    _available.Push(channel);
                    _playing.Remove(channel);
                }
            }
        }

        #region Containers
        private class Stream
        {
            public Func<double, double?> GetNextSample { get; private set; }
            public Action StopComplete { get; private set; }

            public Stream(Func<double, double?> getNextSample, Action onStop)
            {
                GetNextSample = getNextSample;
                StopComplete = onStop;
            }
        }
        #endregion
    }
}
