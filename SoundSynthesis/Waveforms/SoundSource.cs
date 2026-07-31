using Silk.NET.OpenAL;

namespace ControlGraph.Nodes
{
    public class SoundSource
    {
        private uint _bufferId;

        public SoundSource(byte[] samples, int sampleRate, int channels, int bitsPerSample)
        {
            var al = AL.GetApi(true);

            _bufferId = al.GenBuffer();
            var format = channels == 1 ? (bitsPerSample == 8 ? BufferFormat.Mono8 : BufferFormat.Mono16) : (bitsPerSample == 8 ? BufferFormat.Stereo8 : BufferFormat.Stereo16);
            al.BufferData(_bufferId, format, samples, sampleRate);

            uint source = al.GenSource();
            al.SetSourceProperty(source, SourceInteger.Buffer, _bufferId);
        }
    }
}
