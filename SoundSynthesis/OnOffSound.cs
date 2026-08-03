namespace SoundSynthesis
{
    public class OnOffSound
    {
        private int _nextSample = 0;
        private Func<double, double> _getSample;

        public OnOffSound(Func<double, double> getSampleFunc)
        {
            _getSample = getSampleFunc;
        }

        private bool _on = false;
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
            _nextSample = 0;
        }

        public double? GetSample(double timeStep)
        {
            var t = _nextSample++ * timeStep;
            return _on ? _getSample(t) : null;
        }
    }
}
