namespace SoundSynthesis
{
    public class Waveform
    {
        public static double Scale(double scale, double sample)
        {
            return sample * scale;
        }

        public static double Clip(double limit, double sample)
        {
            if (Math.Abs(sample) > limit && limit > 0)
            {
                return Math.Sign(sample) * limit;
            }

            return sample;
        }

        public static double Compress(params double[] samples)
        {
            if (samples == null || samples.Length == 0)
            {
                return 0.0;
            }

            var limit = 1.0;
            double sum = 0.0;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += samples[i];
            }

            // Scale back the amplitude if the sum exceeds the limit
            if (Math.Abs(sum) > limit && limit > 0)
            {
                return Math.Sign(sum) * limit;
            }

            return sum;
        }

        public static double Average(params double[] samples)
        {
            return samples.Sum() / samples.Length;
        }

        public static double Sum(params double[] samples)
        {
            return samples.Sum();
        }

        public static double Envelope(double t, double lengthSeconds, float[] amplitudes)
        {
            if (t > lengthSeconds)
                return 0.0f;

            var step = lengthSeconds / (amplitudes.Length-2);

            var p = (int)Math.Floor(t / step);
            var offset = t % step;
            var lerpFactor = offset / step;
            return amplitudes[p] + lerpFactor * (amplitudes[p + 1] - amplitudes[p]);
        }

        public static double Sin(double t, double frequency, double amplitude = 1.0)
        {
            return Math.Sin(2 * Math.PI * frequency * t) * amplitude;
        }

        public static double Square(double t, double frequency, double amplitude = 1.0)
        {
            return Math.Sin(2 * Math.PI * frequency * t) >= 0 ? amplitude : -amplitude;
        }

        public static double Triangle(double t, double frequency, double amplitude = 1.0)
        {
            double val = 2 * Math.Abs(2 * (t * frequency - Math.Floor(t * frequency + 0.5))) - 1;
            return val * amplitude;
        }

        public static double Sawtooth(double t, double frequency, double amplitude = 1.0)
        {
            double phase = t * frequency;
            double value = 2.0 * (phase - Math.Floor(0.5 + phase));

            return value * amplitude;
        }
    }

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
