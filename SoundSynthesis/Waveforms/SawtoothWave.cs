namespace SoundSynthesis.Waveforms
{
    public class SawtoothWave
    {
        public static Int16[] GenerateSawtoothWave(double frequency, int sampleRate, double durationSeconds, float amplitude = 1.0f)
        {
            int totalSamples = (int)(sampleRate * durationSeconds);
            Int16[] pcmData = new Int16[totalSamples];
            double samplesPerCycle = (double)sampleRate / frequency;

            for (int n = 0; n < totalSamples; n++)
            {
                // Generate saw wave value between -1.0 and 1.0
                double t = (n % samplesPerCycle) / samplesPerCycle;
                double sampleValue = (2.0 * t) - 1.0;

                // Convert to 16-bit signed PCM
                pcmData[n] = (Int16)(amplitude * sampleValue * Int16.MaxValue);
            }
            return pcmData;
        }
    }
}
