namespace ControlGraph.Nodes
{
    public class SinWave
    {
        public static long GenerateSineWave(long startSample, Int16[] buffer, double frequency, int sampleRate, float amplitude = 1.0f)
        {
            var s = startSample;
            for (int i = 0; i < buffer.Length; i++)
            {
                // Calculate the time 't' for the current sample 'i'
                double t = (double)s / sampleRate;

                // Calculate the value of the sine wave at time 't'
                buffer[i] = (Int16)(amplitude * Math.Sin(2 * Math.PI * frequency * t) * Int16.MaxValue);

                s++;
            }
            return s;
        }
    }
}
