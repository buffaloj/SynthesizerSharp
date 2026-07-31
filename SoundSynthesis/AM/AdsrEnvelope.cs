namespace ControlGraph.AM
{
    public class AdsrEnvelope
    {
        public static long ShapeAsAdsr(
            long startSample, 
            short[] buffer, int sampleRate,
            double attack,
            double decay,
            double sustain, 
            double release,
            double sustainStartAmplitude,
            double sustainEndAmplitude)
        {
            double attackDonePoint = attack * sampleRate;
            double decayDonePoint = (attack + decay) * sampleRate;
            double sustainDonePoint = (attack + decay + sustain) * sampleRate;
            double releaseDonePoint = (attack + decay + sustain + release) * sampleRate;

            if (startSample >= releaseDonePoint)
                return 0;

            var sample = startSample;
            for (int i = 0; i < buffer.Length; i++)
            {
                var amplitude = 0.0;
                if (sample < attackDonePoint)
                    amplitude = (double)(sample / attackDonePoint);
                else if (sample < decayDonePoint)
                    amplitude = sustainStartAmplitude + (1.0 - sustainStartAmplitude) * (double)(decayDonePoint - sample) / (double)(decayDonePoint - attackDonePoint);
                else if (sample < sustainDonePoint)
                {
                    var lerpFactor = (double)(sample - decayDonePoint) / (double)(sustainDonePoint - decayDonePoint);
                    amplitude = sustainStartAmplitude + lerpFactor * (sustainEndAmplitude - sustainStartAmplitude);
                }
                else if (sample < releaseDonePoint)
                    amplitude = sustainEndAmplitude * (double)(releaseDonePoint - sample) / (double)(releaseDonePoint - sustainDonePoint);

                buffer[i] = (short)(amplitude * buffer[i]);
                sample++;
            }

            return startSample + buffer.Length;
        }
     }
}
