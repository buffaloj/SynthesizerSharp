namespace SoundSynthesis.Waveforms
{
    public class TriangleWave
    {
        public static void GenerateSingleTriangleWave(ref short sampleStart, ref double angleStart, Int16[] buffer, double frequency, int sampleRate)
        {
            int numberOfSamples = buffer.Length;
            double angleIncrement = frequency / sampleRate;
            double angle = angleStart;

            var index = sampleStart;
            for (int i = sampleStart; i < buffer.Length; i++)
            {
                double sample; // Normalized -1 to 1

                // Logic to define a triangle wave normalized from -1 to 1 based on the angle
                if (angle < 0.25)
                {
                    sample = 4 * angle; // Rising slope from 0 to 1
                }
                else if (angle < 0.75)
                {
                    sample = 4 * (0.5 - angle); // Falling slope from 1 to -1
                }
                else
                {
                    sample = 4 * (angle - 1); // Rising slope from -1 to 0
                }

                // Scale the sample to the 16-bit PCM range and cast to short
                buffer[i] = (short)(sample * Int16.MaxValue);

                // Increment the angle for the next sample and wrap around at 1.0
                angle += angleIncrement;
                bool periodDone = false;
                while (angle >= 1)
                {
                    periodDone = true;
                    angle -= 1;
                }

                index++;
                if (periodDone)
                {
                    sampleStart = index;
                    angleStart = angle;
                    return;// index;
                }
            }

            sampleStart = index;
            angleStart = angle;
            //return index;   // the next angle to use
        }

        public static double GenerateTriangleWave(double angleStart, Int16[] buffer, double frequency, int sampleRate)
        {
            int numberOfSamples = buffer.Length;
            double angleIncrement = frequency / sampleRate;
            double angle = angleStart; // Normalized 0 to 1

            for (int i = 0; i < buffer.Length; i++)
            {
                double sample; // Normalized -1 to 1

                // Logic to define a triangle wave normalized from -1 to 1 based on the angle
                if (angle < 0.25)
                {
                    sample = 4 * angle; // Rising slope from 0 to 1
                }
                else if (angle < 0.75)
                {
                    sample = 4 * (0.5 - angle); // Falling slope from 1 to -1
                }
                else
                {
                    sample = 4 * (angle - 1); // Rising slope from -1 to 0
                }

                // Scale the sample to the 16-bit PCM range and cast to short
                buffer[i] = (short)(sample * Int16.MaxValue);

                // Increment the angle for the next sample and wrap around at 1.0
                angle += angleIncrement;
                while (angle >= 1)
                {
                    angle -= 1;
                }
            }

            return angle;   // the next angle to use
        }
    }
}
