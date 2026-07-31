namespace ControlGraph.Nodes
{
    public class SquareWave
    {
        public static Int16[] GenerateSquareWave(double frequency, int sampleRate, double durationSeconds, float amplitude = 1.0f)
        {
            var numberOfSamples = (int)(sampleRate * durationSeconds);
            var buffer = new Int16[numberOfSamples];
            var samplesPerCycle = sampleRate / frequency;
            var halfSamplesPerCycle = samplesPerCycle / 2.0;

            for (int i = 0; i < numberOfSamples; i++)
            {
                // Use the modulo of the current sample index to determine if it's in the first or second half of the cycle
                // If i % samplesPerCycle is less than halfSamplesPerCycle, the value is high, otherwise low.
                if ((i % samplesPerCycle) < halfSamplesPerCycle)
                {
                    buffer[i] = (Int16)(amplitude * Int16.MaxValue);
                }
                else
                {
                    buffer[i] = (Int16)(-amplitude * Int16.MaxValue);
                }
            }
            return buffer;
        }

        //IEnumerable<Tuple<double, double>> SquareWave(double freq, double lowAmp, double highAmp)
        //{
        //    for (var x = 0.0; true; x += 1.0)
        //    {
        //        var y = ((int)(x / freq) % 2) == 0 ? lowAmp : highAmp;
        //        yield return Tuple.Create(x, y);
        //    }
        //}
    }
}
