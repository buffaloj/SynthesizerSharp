namespace SoundSynthesis.Sounds
{
    public class WaaWaa
    {
        private int _nextSampleIndex = 0;
        private double? _lastVolume = null;

        private double _frequency;
        private double _freqMin = 1;
        private double _freqMax = 100;
        private double _angle = 0.0;
        private double _angleRate = 0.05;

        private bool _on = false;

        public WaaWaa()
        {
            _frequency = _freqMin;
        }

        public void On()
        {
            _on = true;
        }

        public void Off()
        {
            _on = false;
        }

        public void Reset()
        {
            _nextSampleIndex = 0;
            _frequency = _freqMin;
        }

        public double? GetSample(double timeStep)
        {
            var t = _nextSampleIndex++ * timeStep;
            if (_on)
            {
                var volume = Waveform.Sin(t, _frequency);
                if (_lastVolume < 0.0 && volume >= 0.0)
                {
                    var lerp = 1.0 - ((1.0 + Math.Cos(_angle)) / 2.0);
                    _frequency = _freqMin + (lerp * (_freqMax - _freqMin));
                    _angle += _angleRate;
                    _nextSampleIndex = 0;
                }

                _lastVolume = volume;

                return Waveform.Sin(t, 300, volume);
            }

            return null;
        }
    }
}
