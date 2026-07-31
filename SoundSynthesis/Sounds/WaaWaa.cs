namespace SoundSynthesis.Sounds
{
    public class WaaWaa
    {
        private int _sampleRate = 44100;

        private double _lfAngle = 0.0;
        private double _lfAngleB = 0.0;

        protected double _lfoLow = 2.0;
        protected double _lfoHigh = 8.0;

        protected double _freqency1 = 500;
        protected double _freqency2 = 500;

        private double _lfoIncrement = 0.0;
        private double _angleIncrement = 0.001;

        private double _angleIncrement1;// 0.02;
        private double _angleIncrement2 = 0.02;

        private double _nextAngle1;
        private double _nextAngle2;

        protected double _hfMin = 200;
        protected double _hfMax = 800;

        private bool _stopped = false;

        public WaaWaa(int sampleRate=44100)
        {
            _lfoIncrement = (twoPi * _lfoLow) / _sampleRate;
            _angleIncrement1 = (twoPi * _freqency1) / _sampleRate;
            _angleIncrement2 = (twoPi * _freqency2) / _sampleRate;
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
            _nextAngle1 = 0;
            _nextAngle2 = 0;
            _lfAngleB = 0;
        }

        private double twoPi = 2.0 * Math.PI;

        public bool TryFillBuffer(Int16[] buffer)
        {
            if (_stopped)
                return false;

            for (int i = 0; i < buffer.Length; i++)
            {
                var sample1 = (Int16)(Math.Sin(_nextAngle1) * Int16.MaxValue);
                var sample2 = (Int16)(Math.Sin(_nextAngle2) * Int16.MaxValue);

                var lfoLerp = (1.0 + Math.Sin(_lfAngleB)) / 2.0;
                _lfAngleB += _lfoIncrement;
                if (_lfAngleB > twoPi)
                {
                    _lfAngleB -= twoPi;

                    var lerp = (1.0 + Math.Sin(_lfAngle)) / 2.0;
                    _lfoIncrement = (twoPi * (_lfoLow + (lerp * (_lfoHigh - _lfoLow)))) / _sampleRate;
                    //_lfAngleB += _angleIncrement;
                }

                buffer[i] = (short)(sample1 * lfoLerp);// (Int16)((sample1+ sample2)/2.0);

                _nextAngle1 += _angleIncrement1;
                if (_nextAngle1 > twoPi)
                {
                    _nextAngle1 -= twoPi;
                }

                _nextAngle2 += _angleIncrement2;
                if (_nextAngle2 > twoPi)
                {
                    _nextAngle2 -= twoPi;

                    var lerp = (1.0 + Math.Sin(_lfAngle)) / 2.0;
                    _angleIncrement2 = (twoPi * (_hfMin + (lerp * (_hfMax - _hfMin)))) / _sampleRate;
                    _lfAngle += _angleIncrement;
                }
            }

            return true;
        }
    }
}
