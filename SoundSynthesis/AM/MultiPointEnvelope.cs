namespace ControlGraph.AM
{
    public class MultiPointEnvelope
    {
        public static long Shape(
            long startSample,  
            short[] buffer, 
            int sampleRate,
            float[] amplitudes, 
            double lengthSeconds)
        {
            var donePoint = lengthSeconds * sampleRate;
            if (startSample >= donePoint)
                return 0;

            var step = donePoint / (amplitudes.Length-1);

            var sample = startSample;
            for (int i = 0; i < buffer.Length; i++)
            {
                var p = (int)Math.Floor(sample / step);
                var offset = sample % step;
                var lerpFactor = offset / (step-1);
                var amplitude = amplitudes[p] + lerpFactor * (amplitudes[p+1] - amplitudes[p]);

                buffer[i] = (short)(amplitude * buffer[i]);
                sample++;
            }

            return startSample + buffer.Length;
        }
     }
}
