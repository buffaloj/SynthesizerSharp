namespace SoundSynthesis.Sounds
{
    public class WarGamesTicTacToe
    {
        public static double Envelope(double t, double speed) => Waveform.Adsr(t, speed * 0.5, speed * 1.0, speed * 6.0, speed * 1.0, 0.8, 0.5);
        public static double Wave(double t, double freq) => Waveform.Triangle(t, freq, Envelope(t, 0.05));

        public static double LowWaveform(double t) => Wave(t, 150);
        public static double HighWaveform(double t) => Wave(t, 300);
    }
}
