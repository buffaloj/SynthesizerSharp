namespace SoundSynthesis.Sounds
{
    public class Siren
    {
        private int _nextSampleIndex = 0;
        private double? _lastSample = null;

        private double _frequency;
        private double _freqMin = 200;
        private double _freqMax = 800;
        private double _angle = 0.0;
        private double _angleRate = 0.025;

        private bool _on = false;

        public Siren()
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
        }

        public double? GetSample(double timeStep)
        {
            var t = _nextSampleIndex++ * timeStep;
            if (_on)
            {
                var sample = Waveform.Sin(t, _frequency);
                if (_lastSample < 0.0 && sample >= 0.0)
                {
                    var lerp = (1.0 + Math.Sin(_angle)) / 2.0;
                    _frequency = _freqMin + (lerp * (_freqMax - _freqMin));
                    _angle += _angleRate;
                    _nextSampleIndex = 0;
                }

                _lastSample = sample;
                return sample;
            }

            return null;
        }
    }
}
