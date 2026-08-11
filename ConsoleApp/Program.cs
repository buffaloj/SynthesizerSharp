using ConsoleApp;
using SoundSynthesis;
using SoundSynthesis.Sounds;
using System.Runtime.InteropServices;

var keyboard = new Keyboard();
var speaker = new Speaker();

var bufferSize = (int)(0.1 * 44100);

var pianoKeys = new List<OnOffSound>()
{
    MakeSound('1', Piano.C3),
    MakeSound('2', Piano.CS3),
    MakeSound('3', Piano.D3),
    MakeSound('4', Piano.DS3),
    MakeSound('5', Piano.E3),
    MakeSound('6', Piano.F3),
    MakeSound('7', Piano.FS3),
    MakeSound('8', Piano.G3),
    MakeSound('9', Piano.GS3),
    MakeSound('0', Piano.A3),
    MakeSound('-', Piano.AS3),
    MakeSound('=', Piano.B3),

    MakeSound('q', Piano.C4),
    MakeSound('w', Piano.CS4),
    MakeSound('e', Piano.D4),
    MakeSound('r', Piano.DS4),
    MakeSound('t', Piano.E4),
    MakeSound('y', Piano.F4),
    MakeSound('u', Piano.FS4),
    MakeSound('i', Piano.G4),
    MakeSound('o', Piano.GS4),
    MakeSound('p', Piano.A4),
    MakeSound('[', Piano.AS4),
    MakeSound(']', Piano.B4),
    MakeSound('\\', Piano.C5)
};

var wavFile = new WavFile("boing.wav");
keyboard.Key('a').Pressed += (c) => speaker.PlayPcmSamples(wavFile.PcmData);

AddSound('s', new Siren());
AddSound('d', new WaaWaa());
AddSound('f', new WarGamesTicTacToeLow());
AddSound('g', new WarGamesTicTacToeHigh());
AddSound('h', new DeniedSound());

keyboard.KeyPressed += Console.Write;

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

#region Helpers
OnOffSound MakeSound(char key, GetSample getSample)
{
    var sound = new OnOffSound(getSample);
    var soundSourceId = speaker.CreateSoundSource(sound.TryGetSample, sound.Reset, () => new short[bufferSize]);
    keyboard.Key(key).Pressed += (c) => { sound.On(); speaker.PlaySoundSource(soundSourceId); };
    keyboard.Key(key).Released += (c) => sound.Off();
    return sound;
}

OnOffSound AddSound(char key, OnOffSound sound)
{
    var soundSourceId = speaker.CreateSoundSource(sound.TryGetSample, sound.Reset, () => new short[bufferSize]);
    keyboard.Key(key).Pressed += (c) => { sound.On(); speaker.PlaySoundSource(soundSourceId); };
    keyboard.Key(key).Released += (c) => sound.Off();
    return sound;
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