using Silk.NET.OpenAL;

namespace ControlGraph
{
    public unsafe class Speaker
    {
        public unsafe Speaker()
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

        public class BufferInfo
        {
            public uint BufferId { get; set; }
            public short[] PcmData { get; set; }
            public bool IsQueued { get; set; }
        }

        public class SourceInfo
        {
            public uint SourceId { get; set; }
            public IEnumerable<BufferInfo> Buffers { get; set; }
            public Func<short[], bool> FillBuffer { get; set; }
            public Action StopComplete { get; set; }

            public BufferFormat Format { get; set; }
            public int SampleRate { get; set; }
            public bool IsPlaying { get; set; }
        }

        private IList<SourceInfo> _soundSources = new List<SourceInfo>();

        public void PlaySoundSource(uint sourceId)
        {
            var sourceInfo = _soundSources.FirstOrDefault(s => s.SourceId == sourceId);
            if (sourceInfo == null)
                return; // uhhh, the caller made a mistake?

            var al = AL.GetApi(true);
            foreach (var buffer in sourceInfo.Buffers)
            {
                if (sourceInfo.FillBuffer.Invoke(buffer.PcmData))
                {
                    al.BufferData(buffer.BufferId, sourceInfo.Format, buffer.PcmData, sourceInfo.SampleRate);

                    var copy = buffer.BufferId;
                    al.SourceQueueBuffers(sourceInfo.SourceId, 1, &copy);
                    Console.WriteLine($"Start SourceQueueBuffers: sourceId:{sourceInfo.SourceId} bufferId{copy}");
                    buffer.IsQueued = true;
                }
            }

            sourceInfo.IsPlaying = true;

            al.SourcePlay(sourceInfo.SourceId);
        }

        // support 8bps buffers? what of 24?
        public uint CreateSoundSource(
            Func<short[], bool> fillBuffer,
            Action? stopComplete,
            Func<short[]> createBuffer,
            int numBuffers = 2,
            int sampleRate = 44100, int bitsPerSample = 16, int channels = 1)
        {
            var al = AL.GetApi(true);
            var format = channels == 1 ? bitsPerSample == 8 ? BufferFormat.Mono8 : BufferFormat.Mono16 : bitsPerSample == 8 ? BufferFormat.Stereo8 : BufferFormat.Stereo16;

            var buffers = new List<BufferInfo>();

            var bufferIds = new uint[numBuffers];
            fixed (uint* ptr = bufferIds)
            {
                al.GenBuffers(numBuffers, ptr);

                for (var i = 0; i < numBuffers; i++)
                {
                    var info = new BufferInfo
                    {
                        BufferId = ptr[i],
                        PcmData = createBuffer()
                    };
                    buffers.Add(info);
                }
            }

            var source = new SourceInfo
            {
                SourceId = al.GenSource(),
                Format = format,
                SampleRate = sampleRate,
                Buffers = buffers,
                FillBuffer = fillBuffer,
                StopComplete = stopComplete
            };
            
            _soundSources.Add(source);

            return source.SourceId;
        }

        public void UpdateBufferQueues()
        {
            var al = AL.GetApi(true);

            foreach (var source in _soundSources)
            {
                if (!source.IsPlaying)
                    continue;

                int buffersProcessed = 0;
                // 3. Monitor and refill buffers in your update loop
                al.GetSourceProperty(source.SourceId, GetSourceInteger.BuffersProcessed, &buffersProcessed);//out var buffersProcessed);

                while (buffersProcessed > 0)
                {
                    uint bufferId = 0;

                    // Unqueue exactly 1 processed buffer
                    unsafe
                    {
                        al.SourceUnqueueBuffers(source.SourceId, 1, &bufferId);

                        buffersProcessed--;

                        var idcopy = bufferId;
                        var buffer = source.Buffers.Where(b => b.BufferId == idcopy).First();
                        buffer.IsQueued = false;

                        // Refill the buffer using your audio data provider
                        if (source.FillBuffer(buffer.PcmData))
                        {
                            al.BufferData(buffer.BufferId, source.Format, buffer.PcmData, source.SampleRate);

                            // Queue the refilled buffer back to the source
                            al.SourceQueueBuffers(source.SourceId, 1, &bufferId);
                            buffer.IsQueued = true;
                            Console.WriteLine($"Stream SourceQueueBuffers: sourceId:{source.SourceId} bufferId{bufferId}");
                        }

                        if (!source.Buffers.Any(b => b.IsQueued))
                        {
                            source.IsPlaying = false;
                            al.SourceStop(source.SourceId);
                            source.StopComplete?.Invoke();
                        }
                    }
                }
            }
        }
    }
}
