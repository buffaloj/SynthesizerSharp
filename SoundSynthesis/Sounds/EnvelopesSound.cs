using SpectrogramTool.Wpf;
using static SpectrogramTool.Wpf.WesternNotation;

namespace SoundSynthesis.Sounds
{
    public class EnvelopeSound
    {
        public int? NoteIndex { get; set; }
        public int? Octave { get; set; }
        public IEnumerable<Envelope> Envelopes { get; set; }
    }

    public class Envelope
    {
        public float? Frequency { get; set; }
        public int? FundamentalMult { get; set; }
        public float Length { get; set; }
        public float[] Amplitudes { get; set; }

        public new string ToString()
        {
            return $"FundamentalMult: {FundamentalMult}, Frequency: {Frequency}, MaxAmplitude: {Amplitudes.Max()}";
        }
    }

    public static class EnvelopesSound
    {
        public static GetSample ToWaveform(this EnvelopeSound sound, double scale)
        {
            Func<double, double>[] waveforms = sound.Envelopes.Select(e => new Func<double, double>((t) => Sin(t, e.ResolveWestern(sound.NoteIndex, sound.Octave), e.Amplitudes, e.Length))).ToArray();

            var func = new GetSample((t) => Waveform.Scale(scale, Waveform.Sum(waveforms, t)));
            return func;
        }

        static double Sin(double t, double freq, float[] envelope, double length) => Waveform.Sin(t, freq, Waveform.Envelope(t, length, envelope));
    }

    public static class FrequencyResolver
    {
        public static double ResolveWestern(this Envelope envelope, int? note, int? octave)
        {
            if (envelope.FundamentalMult.HasValue && envelope.FundamentalMult > 0 && note.HasValue && octave.HasValue)
            {
                var fundamental = F((Notes)note.Value, octave.Value, Pitch.Stuttgart);
                return fundamental * (double)envelope.FundamentalMult;
            }

            if (envelope.Frequency.HasValue)
                return envelope.Frequency.Value;

            return 0.0;
            

        }
    }
}
