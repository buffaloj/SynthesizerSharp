namespace ConsoleApp
{
    public class WavFile
    {
        public byte[] PcmData { get; }
        public bool Stereo { get; }
        public WavFile(string filename)
        {
            var wavPath = Path.Combine(Directory.GetCurrentDirectory(), filename);
            var wavFile = File.ReadAllBytes(wavPath);

            // Assuming standard 44-byte WAV header, data starts at index 44
            PcmData = new byte[wavFile.Length - 44];
            Array.Copy(wavFile, 44, PcmData, 0, PcmData.Length);

            Stereo = wavFile[22] == 2;
        }
    }
}
