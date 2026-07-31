using SoundSynthesis.AM;
using SoundSynthesis.Waveforms;

namespace SoundSynthesis.Sounds
{
    public class WarGamesTicTacHigh : WarGamesTicTacToe
    {
        public WarGamesTicTacHigh()
        {
            _freqency = 300;
        }
    }

    public class WarGamesTicTacLow : WarGamesTicTacToe
    {
        public WarGamesTicTacLow()
        {
            _freqency = 150;
        }
    }

    public abstract class WarGamesTicTacToe
    {
        private double _nextAngle = 0.0;
        private long _nextSample = 0;
        private int _sampleRate = 44100;
        protected int _freqency = 100;

        public WarGamesTicTacToe(int sampleRate=44100)
        {
            sampleRate = _sampleRate;
        }

        public void Reset()
        {
            _nextAngle = 0.0;
            _nextSample = 0;
        }

        public bool TryFillBuffer(Int16[] buffer)
        {
            var speed = 0.05;

            _nextAngle = TriangleWave.GenerateTriangleWave(_nextAngle, buffer, _freqency, _sampleRate);
            var nextSample = AdsrEnvelope.ShapeAsAdsr(
                _nextSample,
                buffer, _sampleRate,
                speed * 0.5,
                speed * 1.0,
                speed * 6.0,
                speed * 1.0,
                0.5,
                0.2);

            if (nextSample == 0)
                return false;

            _nextSample = nextSample;
            return true;
        }
    }
}
