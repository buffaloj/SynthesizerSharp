namespace SoundSynthesis.Sounds
{
    public class AdsrSound
    {
        public static double Envelope(double t, double speed) => Waveform.Adsr(t, speed * 0.5, speed * 1.0, speed * 6.0, speed * 1.0, 0.8, 0.5);
        public static double AdsrWaveform(double t) => Waveform.Triangle(t, 100, Envelope(t, 0.05));
    }
}
