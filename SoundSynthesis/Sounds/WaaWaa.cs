namespace SoundSynthesis.Sounds
{
    public class WaaWaa : OnOffSound
    {
        private double? _lastVolume = null;

        private double _frequency;
        private double _freqMin = 1;
        private double _freqMax = 100;
        private double _angle = 0.0;
        private double _angleRate = 0.05;

        public WaaWaa()
        {
            GetSample = GetWaaSample;
            _frequency = _freqMin;
        }

        public double GetWaaSample(double t)
        {
            var volume = Waveform.Sin(t, _frequency);
            if (_lastVolume < 0.0 && volume >= 0.0)
            {
                var lerp = 1.0 - ((1.0 + Math.Cos(_angle)) / 2.0);
                _frequency = _freqMin + (lerp * (_freqMax - _freqMin));
                _angle += _angleRate;
                ResetSampleIndex();
            }

            _lastVolume = volume;

            return Waveform.Sin(t, 300, volume);
        }

        public override void OnStopped()
        {
            base.OnStopped();

            _angle = 0.0;
        }
    }
}
