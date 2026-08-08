namespace SoundSynthesis.Sounds
{
    public class Siren : OnOffSound
    {
        private double? _lastSample = null;

        private double _frequency;
        private double _freqMin = 200;
        private double _freqMax = 800;
        private double _angle = 0.0;
        private double _angleRate = 0.025;

        public Siren()
        {
            GetSample = GetSirenSample;
            _frequency = _freqMin;
        }

        public double GetSirenSample(double t)
        {
            var sample = Waveform.Sin(t, _frequency);
            if (_lastSample < 0.0 && sample >= 0.0)
            {
                var lerp = (1.0 + Math.Sin(_angle)) / 2.0;
                _frequency = _freqMin + (lerp * (_freqMax - _freqMin));
                _angle += _angleRate;
                ResetSampleIndex();
            }

            _lastSample = sample;
            return sample;
        }
    }
}
