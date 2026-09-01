using ConsoleApp;
using SoundSynthesis;
using SoundSynthesis.Sounds;
using System.Runtime.InteropServices;
using System.Text.Json;

var keyboard = new Keyboard();
var speaker = new Speaker();

var bufferSize = (int)(0.1 * 44100);

var pianoKeys = new List<OnOffSound>()
{
    LoadSound('1', "C:\\Projects\\Sounds\\Piano\\C3.snd"),
    LoadSound('2', "C:\\Projects\\Sounds\\Piano\\Db3.snd"),
    LoadSound('3', "C:\\Projects\\Sounds\\Piano\\D3.snd"),
    LoadSound('4', "C:\\Projects\\Sounds\\Piano\\Eb3.snd"),
    LoadSound('5', "C:\\Projects\\Sounds\\Piano\\E3.snd"),
    LoadSound('6', "C:\\Projects\\Sounds\\Piano\\F3.snd"),
    LoadSound('7', "C:\\Projects\\Sounds\\Piano\\Gb3.snd"),
    LoadSound('8', "C:\\Projects\\Sounds\\Piano\\G3.snd"),
    LoadSound('9', "C:\\Projects\\Sounds\\Piano\\Ab3.snd"),
    LoadSound('0', "C:\\Projects\\Sounds\\Piano\\A3.snd"),
    LoadSound('-', "C:\\Projects\\Sounds\\Piano\\Bb3.snd"),
    LoadSound('=', "C:\\Projects\\Sounds\\Piano\\B3.snd"),
    LoadSound('q', "C:\\Projects\\Sounds\\Piano\\C4.snd"),
    LoadSound('w', "C:\\Projects\\Sounds\\Piano\\Db4.snd"),
    LoadSound('e', "C:\\Projects\\Sounds\\Piano\\D4.snd"),
    LoadSound('r', "C:\\Projects\\Sounds\\Piano\\Eb4.snd"),
    LoadSound('t', "C:\\Projects\\Sounds\\Piano\\E4.snd"),
    LoadSound('y', "C:\\Projects\\Sounds\\Piano\\F4.snd"),
    LoadSound('u', "C:\\Projects\\Sounds\\Piano\\Gb4.snd"),
    LoadSound('i', "C:\\Projects\\Sounds\\Piano\\G4.snd"),
    LoadSound('o', "C:\\Projects\\Sounds\\Piano\\Ab4.snd"),
    LoadSound('p', "C:\\Projects\\Sounds\\Piano\\A4.snd"),
    LoadSound('[', "C:\\Projects\\Sounds\\Piano\\Bb4.snd"),
    LoadSound(']', "C:\\Projects\\Sounds\\Piano\\B4.snd"),
    LoadSound('\\', "C:\\Projects\\Sounds\\Piano\\C5.snd"),
};

var wavFile = new WavFile("boing.wav");
keyboard.Key('a').Pressed += (c) => speaker.PlayPcmSamples(wavFile.PcmData);

AddSound('s', new Siren());
AddSound('d', new WaaWaa());
AddSound('f', new WarGamesTicTacToeLow());
AddSound('g', new WarGamesTicTacToeHigh());
AddSound('h', new DeniedSound());

LoadSound('k', "C:\\Projects\\Sounds\\Clarinet\\C4.snd");
LoadSound('l', "C:\\Projects\\Sounds\\ThaiGong\\C4.snd");
LoadSound(';', "C:\\Projects\\Sounds\\Cymbals\\13crash.mallet.snd", 0.18);

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
OnOffSound LoadSound(char key, string fileName, double scale = 0.25)
{
    var jsonString = File.ReadAllText(fileName);
    EnvelopeSound soundModel = JsonSerializer.Deserialize<EnvelopeSound>(jsonString) ?? throw new JsonException("Deserialization returned null.");

    var func = soundModel.ToWaveform(scale);
    var onOffSound = new OnOffSound(func);
    var soundSourceId = speaker.CreateSoundSource(onOffSound.TryGetSample, onOffSound.Reset, () => new short[bufferSize]);
    keyboard.Key(key).Pressed += (c) => { onOffSound.On(); speaker.PlaySoundSource(soundSourceId); };
    keyboard.Key(key).Released += (c) => onOffSound.Off();
    return onOffSound;
}

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