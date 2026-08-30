namespace SoundSynthesis.Sounds
{
    public class Envelope
    {
        public float Frequency { get; set; }
        public float Index { get; set; }
        public float Length { get; set; }
        public float[] Amplitudes { get; set; }

        public new string ToString()
        {
            return $"Index: {Index}, Frequency: {Frequency}, MaxAmplitude: {Amplitudes.Max()}";
        }
    }

    public static class EnvelopesSound
    {
        public static GetSample ToWaveform(this IEnumerable<Envelope> envelopes, double scale)
        {
            Func<double, double>[] waveforms = envelopes.Select(e => new Func<double, double>((t) => Sin(t, e.Frequency, e.Amplitudes, e.Length))).ToArray();

            var func = new GetSample((t) => Waveform.Scale(scale, Waveform.Sum(waveforms, t)));
            return func;
        }

        static double Sin(double t, double freq, float[] envelope, double length) => Waveform.Sin(t, freq, Waveform.Envelope(t, length, envelope));
    }
}
