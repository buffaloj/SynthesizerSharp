using ConsoleApp;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using SoundSynthesis;
using SoundSynthesis.Sounds;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

var keyboard = new Keyboard();
var speaker = new Speaker();

var wavFile = new WavFile("boing.wav");
//var wavFile = new WavFile("c2.wav");

keyboard.Key('f').Pressed += (c) => speaker.PlayPcmSamples(wavFile.PcmData);

var bufferSize = (int)(0.1 * 44100);

var piano = new Piano();
var pianoSourceId = speaker.CreateSoundSource(piano.TryGetSample, piano.Reset, () => new short[bufferSize]);
keyboard.Key('o').Pressed += (c) => { piano.On(); speaker.PlaySoundSource(pianoSourceId); };
keyboard.Key('o').Released += (c) => piano.Off();

var siren = new Siren();
var sirenSourceId = speaker.CreateSoundSource(siren.TryGetSample, siren.Reset, () => new short[bufferSize]);
keyboard.Key('i').Pressed += (c) => { siren.On(); speaker.PlaySoundSource(sirenSourceId); };
keyboard.Key('i').Released += (c) => siren.Off();

var waawaa = new WaaWaa();
var waaSourceId = speaker.CreateSoundSource(waawaa.TryGetSample, waawaa.Reset, () => new short[bufferSize]);
keyboard.Key('u').Pressed += (c) => { waawaa.On(); speaker.PlaySoundSource(waaSourceId);  };
keyboard.Key('u').Released += (c) => waawaa.Off();

var adsrSound = new DeniedSound();
var adsrSourceId = speaker.CreateSoundSource(adsrSound.TryGetSample, adsrSound.Reset, () => new short[bufferSize]);
keyboard.Key('y').Pressed += (c) => { adsrSound.On(); speaker.PlaySoundSource(adsrSourceId); };
keyboard.Key('y').Released += (c) => adsrSound.Off();

var ticTacLow = new WarGamesTicTacToeLow();
var ttlSourceId = speaker.CreateSoundSource(ticTacLow.TryGetSample, ticTacLow.Reset, () => new short[bufferSize]);
keyboard.Key('r').Pressed += (c) => { ticTacLow.On(); speaker.PlaySoundSource(ttlSourceId); };
keyboard.Key('r').Released += (c) => ticTacLow.Off();

var ticTacHigh = new WarGamesTicTacToeHigh();
var tthSourceId = speaker.CreateSoundSource(ticTacHigh.TryGetSample, ticTacHigh.Reset, () => new short[bufferSize]);
keyboard.Key('t').Pressed += (c) => { ticTacHigh.On(); speaker.PlaySoundSource(tthSourceId);  };
keyboard.Key('t').Released += (c) => ticTacHigh.Off();

keyboard.KeyPressed += Console.Write;

var orgSamples = (!wavFile.Stereo) ? wavFile.PcmData : ConvertStereoToMono(wavFile.PcmData);

short[] samples = new short[orgSamples.Length / 2];
// Copy byte data block directly into the short array
Buffer.BlockCopy(orgSamples, 0, samples, 0, orgSamples.Length);

float[] floatSamples = Array.ConvertAll(samples, s => (float)s / (float)short.MaxValue);

CalculateSpectrogram(floatSamples);

while (true)
{
    if (IsTerminalWindowActive())
    {
        keyboard.Poll();
        speaker.UpdateBufferQueues();
    }
    else
        Thread.Sleep(200);
}

#region STFT
static List<Complex32[]> CalculateSTFT(float[] signal, int frameSize, int hopSize)
{
    List<Complex32[]> spectrogram = new List<Complex32[]>();

    // 1. Generate a Hann Window to smooth frame edges
    //var window = Window.Hann(frameSize);
    var window = Array.ConvertAll(Window.Hann(frameSize), x => (float)x);

    // 2. Slide the window across the signal
    for (int start = 0; start <= signal.Length - frameSize; start += hopSize)
    {
        // Create a frame buffer for Complex numbers
        Complex32[] frame = new Complex32[frameSize];

        // 3. Copy signal chunk and apply the window function
        for (int i = 0; i < frameSize; i++)
        {
            var realPart = signal[start + i] * window[i];
            frame[i] = new Complex32(realPart, 0f);
        }

        // 4. Compute the Forward FFT in-place
        Fourier.Forward(frame, FourierOptions.Matlab);

        // 5. Store the spectrum chunk
        spectrogram.Add(frame);
    }

    return spectrogram;
}

static void CalculateSpectrogram(float[] signal)
{
    // Example usage loop
    var result = CalculateSTFT(signal, 1024, 512);
    var magnitudeGrid = ProcessToLogMagnitudes(result);
    SaveStretchedSpectrogram(magnitudeGrid, "output.png");

    //foreach (var timeFrame in result)
    //{
    //    // Real signals are symmetric; you only need the first half (Nyquist frequency)
    //    for (int i = 0; i < timeFrame.Length / 2; i++)
    //    {
    //        float magnitude = timeFrame[i].Magnitude;
    //        // Optional: Convert to decibels for better human-hearing visualization
    //        double db = 20 * Math.Log10(magnitude + 1e-6);
    //    }
    //}
}

static float[,] ProcessToLogMagnitudes(List<Complex32[]> stftOutput)
{
    int timeSlots = stftOutput.Count;
    // We only need the first half (up to Nyquist frequency) because real signals are symmetric
    int freqBins = stftOutput[0].Length / 2;

    float[,] grid = new float[timeSlots, freqBins];
    float minDb = -80f; // Silence threshold 
    float maxDb = 0f;   // Maximum expected loudness

    for (int t = 0; t < timeSlots; t++)
    {
        for (int f = 0; f < freqBins; f++)
        {
            float magnitude = stftOutput[t][f].Magnitude;

            // Convert to Decibels
            float db = 20f * MathF.Log10(magnitude + 1e-6f);

            // Normalize the dB value between 0.0f (silent) and 1.0f (loudest)
            float normalized = (db - minDb) / (maxDb - minDb);
            grid[t, f] = Math.Clamp(normalized, 0f, 1f);
        }
    }
    return grid;
}
static Bitmap CreateBitmapFromGrid(float[,] magnitudeGrid)
{
    int width = magnitudeGrid.GetLength(0);  // Time axis
    int height = magnitudeGrid.GetLength(1); // Frequency axis

    // 1. Create a 24-bit RGB Bitmap
    Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
   // using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
   //{
   // Lock the bitmap memory for direct fast pixel writing
        BitmapData bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly,
            bitmap.PixelFormat);

        int stride = bitmapData.Stride;
        IntPtr scan0 = bitmapData.Scan0;

        unsafe
        {
            byte* p = (byte*)(void*)scan0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Invert Y axis: Low audio frequencies go at the bottom of the image
                    float intensity = magnitudeGrid[x, height - 1 - y];

                    // Calculate color channels (Classic Jet / Heat Map)
                    byte r = 0, g = 0, b = 0;
                    if (intensity < 0.33f)
                    {
                        b = (byte)(intensity / 0.33f * 255);
                    }
                    else if (intensity < 0.66f)
                    {
                        g = (byte)((intensity - 0.33f) / 0.33f * 255);
                        b = (byte)(255 - g);
                    }
                    else
                    {
                        r = (byte)((intensity - 0.66f) / 0.34f * 255);
                        g = (byte)(255 - r);
                    }

                    // Calculate memory position. Windows Bitmaps store pixels as BGR
                    int index = (y * stride) + (x * 3);
                    p[index] = b; // Blue
                    p[index + 1] = g; // Green
                    p[index + 2] = r; // Red
                }
            }
        }

        // Unlock the memory buffer
        bitmap.UnlockBits(bitmapData);

        return bitmap;
    //}
}

static void SaveStretchedSpectrogram(float[,] magnitudeGrid, string outputPath)
{
    int originalWidth = magnitudeGrid.GetLength(0);
    int height = magnitudeGrid.GetLength(1);

    // Target a square image where Width == Height
    int targetWidth = height;

    // 1. Generate the original un-stretched bitmap using the fast LockBits code
    using (Bitmap originalBitmap = CreateBitmapFromGrid(magnitudeGrid))
    {
        // 2. Create a new blank square canvas
        using (Bitmap stretchedBitmap = new Bitmap(targetWidth, height, PixelFormat.Format24bppRgb))
        {
            // 3. Use Graphics to draw and stretch the image
            using (Graphics g = Graphics.FromImage(stretchedBitmap))
            {
                // Set high quality scaling so the pixels don't look blurry or blocky
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Draw the old image into the new square boundaries
                g.DrawImage(originalBitmap, 0, 0, targetWidth, height);
            }

            // 4. Save the final square image
            stretchedBitmap.Save(outputPath, ImageFormat.Png);
        }
    }
}

byte[] ConvertStereoToMono(byte[] stereoData)
{
    // A stereo array has 2 channels. Mono will use half the bytes.
    byte[] monoData = new byte[stereoData.Length / 2];

    // A stereo frame is 4 bytes (2 bytes for Left, 2 bytes for Right).
    for (int i = 0; i < stereoData.Length; i += 4)
    {
        // 1. Reconstruct 16-bit signed integers for Left and Right
        short left = (short)((stereoData[i + 1] << 8) | (stereoData[i] & 0xff));
        short right = (short)((stereoData[i + 3] << 8) | (stereoData[i + 2] & 0xff));

        // 2. Average the two channels to create the mono sample
        short monoSample = (short)((left + right) / 2);

        // 3. Convert the mono sample back to 2 bytes and store it
        int targetIndex = i / 2;
        monoData[targetIndex] = (byte)(monoSample & 0xff);       // Low Byte
        monoData[targetIndex + 1] = (byte)((monoSample >> 8) & 0xff); // High Byte
    }

    return monoData;
}

#endregion

#region PInvoke vars
const uint GA_ROOTOWNER = 3; 

[DllImport("user32.dll")]
static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
static extern IntPtr GetForegroundWindow();

[DllImport("kernel32.dll", SetLastError = true)]
static extern IntPtr GetConsoleWindow();

static bool IsTerminalWindowActive()
{
    // 1. Get the process's console window pointer
    IntPtr consoleHandle = GetConsoleWindow();
    if (consoleHandle == IntPtr.Zero) 
        return false;

    // 2. Get the actual visible root window (Handles WT.exe abstractions)
    IntPtr terminalRootHandle = GetAncestor(consoleHandle, GA_ROOTOWNER);

    // 3. Get whatever window is currently on top in Windows
    IntPtr foregroundHandle = GetForegroundWindow();

    // 4. In Windows Terminal, the foreground window matches the root window frame
    return terminalRootHandle == foregroundHandle;
}
#endregion