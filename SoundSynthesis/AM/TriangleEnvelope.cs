namespace ControlGraph.AM
{
    public class TriangleEnvelope
    {
        public static long ShapeAsTriangle(long startSample, short[] buffer, int sampleRate, double riseTime, double fallTime)
        {
            double fallPoint = riseTime * sampleRate;
            double donePoint = (riseTime + fallTime) * sampleRate;

            if (startSample >= donePoint)
                return 0;

            var sample = startSample;
            for (int i = 0; i < buffer.Length; i++)
            {
                var amplitude = 0.0;
                if (sample < fallPoint)
                    amplitude = (double)(sample / fallPoint);
                else if (sample < donePoint)
                    amplitude = (double)(donePoint - sample) / (double)(donePoint - fallPoint);

                buffer[i] = (short)(amplitude * buffer[i]);
                sample++;
            }

            return startSample + buffer.Length;
        }
     }
}
