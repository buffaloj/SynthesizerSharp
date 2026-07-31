using SoundSynthesis.Waveforms;

namespace SoundSynthesis.Sounds
{
    public class Siren
    {
        private int _sampleRate = 44100;
        protected double _highFreqency = 100;
        private double _angleIncrement = 0.02;
        private double _lfAngle = 0.0;
        private short _nextSample;
        private double _nextAngle;

        protected double _hfMin = 200;
        protected double _hfMax = 800;

        private bool _stopped = false;

        public Siren(int sampleRate=44100)
        {
            sampleRate = _sampleRate;
        }

        public void KeyDown()
        {
            Reset();
            _stopped = false;
        }

        public void KeyUp()
        {
            _stopped = true;
        }

        public void Reset()
        {
            _nextSample = 0;
            _nextAngle = 0;
        }

        public bool TryFillBuffer(Int16[] buffer)
        {
            if (_stopped)
                return false;

            var periods = 0;
            while (_nextSample < buffer.Length)
            {
                TriangleWave.GenerateSingleTriangleWave(ref _nextSample, ref _nextAngle, buffer, _highFreqency, _sampleRate);
                var lerp = (1.0 + Math.Sin(_lfAngle)) / 2.0;
                _highFreqency = _hfMin + (lerp * (_hfMax - _hfMin));
                _lfAngle += _angleIncrement;
                periods++;
            }
            _nextSample = 0;

            return true;
        }
    }
}
