
namespace SoundSynthesis.Sounds
{
    public class DeniedSound : OnOffSound
    {
        public DeniedSound() : base(DeniedWaveform) {}

        public static double Envelope(double t, double speed) => Waveform.Adsr(t, speed * 0.5, speed * 1.0, speed * 6.0, speed * 1.0, 0.9, 0.8);
        public static double DeniedWaveform(double t) => Waveform.Triangle(t, 100, Envelope(t, 0.05));
    }
}
