using ConsoleApp;
using NAudio.Midi;
using SoundSynthesis;
using SoundSynthesis.Sounds;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Timers;

List<MidiIn> inputs = new List<MidiIn>();

ManagementEventWatcher insertWatcher;
ManagementEventWatcher removeWatcher;

// The debounce timer
System.Timers.Timer debounceTimer;
object lockObject = new object();

var keyboard = new Keyboard();
var speaker = new Speaker();

var bufferSize = (int)(0.1 * 44100);

var pianoKeys = new List<OnOffSound>()
{
    LoadSound('1', ".\\Sounds\\Piano\\C3.snd"),
    LoadSound('2', ".\\Sounds\\Piano\\Db3.snd"),
    LoadSound('3', ".\\Sounds\\Piano\\D3.snd"),
    LoadSound('4', ".\\Sounds\\Piano\\Eb3.snd"),
    LoadSound('5', ".\\Sounds\\Piano\\E3.snd"),
    LoadSound('6', ".\\Sounds\\Piano\\F3.snd"),
    LoadSound('7', ".\\Sounds\\Piano\\Gb3.snd"),
    LoadSound('8', ".\\Sounds\\Piano\\G3.snd"),
    LoadSound('9', ".\\Sounds\\Piano\\Ab3.snd"),
    LoadSound('0', ".\\Sounds\\Piano\\A3.snd"),
    LoadSound('-', ".\\Sounds\\Piano\\Bb3.snd"),
    LoadSound('=', ".\\Sounds\\Piano\\B3.snd"),
    LoadSound('q', ".\\Sounds\\Piano\\C4.snd"),
    LoadSound('w', ".\\Sounds\\Piano\\Db4.snd"),
    LoadSound('e', ".\\Sounds\\Piano\\D4.snd"),
    LoadSound('r', ".\\Sounds\\Piano\\Eb4.snd"),
    LoadSound('t', ".\\Sounds\\Piano\\E4.snd"),
    LoadSound('y', ".\\Sounds\\Piano\\F4.snd"),
    LoadSound('u', ".\\Sounds\\Piano\\Gb4.snd"),
    LoadSound('i', ".\\Sounds\\Piano\\G4.snd"),
    LoadSound('o', ".\\Sounds\\Piano\\Ab4.snd"),
    LoadSound('p', ".\\Sounds\\Piano\\A4.snd"),
    LoadSound('[', ".\\Sounds\\Piano\\Bb4.snd"),
    LoadSound(']', ".\\Sounds\\Piano\\B4.snd"),
    LoadSound('\\', ".\\Sounds\\Piano\\C5.snd")
};

var wavFile = new WavFile("boing.wav");
keyboard.Key('a').Pressed += (c) => speaker.PlayPcmSamples(wavFile.PcmData);

AddSound('s', new Siren());
AddSound('d', new WaaWaa());
AddSound('f', new WarGamesTicTacToeLow());
AddSound('g', new WarGamesTicTacToeHigh());
AddSound('h', new DeniedSound());

LoadSound('k', ".\\Sounds\\Clarinet\\C4.snd");
LoadSound('l', ".\\Sounds\\ThaiGong\\C4.snd");
LoadSound(';', ".\\Sounds\\Cymbals\\13crash.mallet.snd", 0.18);

keyboard.KeyPressed += Console.Write;

StartMidiListening();
StartMidiDeviceChangeMonitoring();

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

#region MIDI
void StartMidiDeviceChangeMonitoring()
{
    // Initialize a 500ms timer that only fires ONCE per burst
    debounceTimer = new System.Timers.Timer(500);
    debounceTimer.AutoReset = false;
    debounceTimer.Elapsed += OnDebounceTimerElapsed;

    // Query string looking for USB/Hardware device configuration changes
    string query = "SELECT * FROM __InstanceOperationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'";

    // 1. Listen for device connections
    insertWatcher = new ManagementEventWatcher(query.Replace("__InstanceOperationEvent", "__InstanceCreationEvent"));
    insertWatcher.EventArrived += (s, e) => ResetDebounceTimer();
    insertWatcher.Start();

    // 2. Listen for device disconnections
    removeWatcher = new ManagementEventWatcher(query.Replace("__InstanceOperationEvent", "__InstanceDeletionEvent"));
    removeWatcher.EventArrived += (s, e) => ResetDebounceTimer();
    removeWatcher.Start();

    //Console.WriteLine("Monitoring hardware changes... Press any key to exit.");
    //Console.ReadKey();

    // Cleanup
    //insertWatcher.Stop();
    //removeWatcher.Stop();
}

void ResetDebounceTimer()
{
    lock (lockObject)
    {
        // Stop the running timer and start it over.
        // This constantly pushes the execution forward until the burst of OS events stops.
        debounceTimer.Stop();
        debounceTimer.Start();
    }
}

void OnDebounceTimerElapsed(object sender, ElapsedEventArgs e)
{
    // This block runs EXACTLY ONCE, 500ms after the very last USB event finishes.
    Console.WriteLine("\n[Hardware Settled] Refreshing MIDI device list...");

    StartMidiListening();
}

void StopMidiListening()
{
    inputs.Clear();
}

void StartMidiListening()
{
    int inputDevices = MidiIn.NumberOfDevices;
    if (inputDevices == 0)
        Console.WriteLine($"No Devices Connected");

    for (int device = 0; device < inputDevices; device++)
    {
        // Retrieve details for each device
        MidiInCapabilities caps = MidiIn.DeviceInfo(device);
        Console.WriteLine($"Device ID {device}: {caps.ProductName}");

        var midiIn = new MidiIn(device);
        inputs.Add(midiIn);

        // 2. Attach the event handler for incoming messages
        midiIn.MessageReceived += OnMidiMessageReceived;
        midiIn.ErrorReceived += OnMidiErrorReceived;

        // 3. Start monitoring the incoming stream
        midiIn.Start();
    }
}

void OnMidiMessageReceived(object sender, MidiInMessageEventArgs e)
{
    // e.MidiEvent contains details about the note or control action
    MidiEvent midiEvent = e.MidiEvent;

    // Extract basic data or cast to specific event types
    Console.WriteLine($"Event: {midiEvent.CommandCode} | Channel: {midiEvent.Channel} | Absolute Time: {e.Timestamp}");

    if (e.MidiEvent is NoteEvent noteEvent)
    {
        bool isNoteOff = noteEvent.CommandCode == MidiCommandCode.NoteOff ||
                         (noteEvent.CommandCode == MidiCommandCode.NoteOn && noteEvent.Velocity == 0);

        var index = noteEvent.NoteNumber - 48;
        if (index < 0)
            return; // sanity check
        var key = pianoKeys[index];

        if (isNoteOff)
        {
            // Key was released
            Console.WriteLine($"[NOTE OFF] Ch: {noteEvent.Channel} | Note: {noteEvent.NoteNumber}");
            key.Off();
        }
        else
        {
            // Key was pressed down (Velocity > 0)
            Console.WriteLine($"[NOTE ON ] Ch: {noteEvent.Channel} | Note: {noteEvent.NoteNumber} | Vel: {noteEvent.Velocity}");

            key.On();
        }
    }
}

void OnMidiErrorReceived(object sender, MidiInMessageEventArgs e)
{
    Console.WriteLine($"Error received: {e.RawMessage}");
}
#endregion

#region Helpers
OnOffSound LoadSound(char key, string fileName, double scale = 0.25)
{
    var jsonString = File.ReadAllText(fileName);
    EnvelopeSound soundModel = JsonSerializer.Deserialize<EnvelopeSound>(jsonString) ?? throw new JsonException("Deserialization returned null.");

    var func = soundModel.ToWaveform(scale);
    var onOffSound = new OnOffSound(func);
    var soundSourceId = speaker.CreateSoundSource(onOffSound.TryGetSample, onOffSound.OnStopped, () => new short[bufferSize]);
    onOffSound.onAction = () => speaker.PlaySoundSource(soundSourceId);

    keyboard.Key(key).Pressed += (c) => { onOffSound.On();  };
    keyboard.Key(key).Released += (c) => onOffSound.Off();
    return onOffSound;
}

OnOffSound MakeSound(char key, GetSample getSample)
{
    var onOffSound = new OnOffSound(getSample);
    var soundSourceId = speaker.CreateSoundSource(onOffSound.TryGetSample, onOffSound.OnStopped, () => new short[bufferSize]);
    onOffSound.onAction = () => speaker.PlaySoundSource(soundSourceId);

    keyboard.Key(key).Pressed += (c) => onOffSound.On(); 
    keyboard.Key(key).Released += (c) => onOffSound.Off();
    return onOffSound;
}

OnOffSound AddSound(char key, OnOffSound sound)
{
    var soundSourceId = speaker.CreateSoundSource(sound.TryGetSample, sound.OnStopped, () => new short[bufferSize]);
    sound.onAction = () => speaker.PlaySoundSource(soundSourceId);

    keyboard.Key(key).Pressed += (c) => sound.On(); speaker.PlaySoundSource(soundSourceId);
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