using System.Runtime.InteropServices;

namespace ConsoleApp
{
    public class Keyboard
    {
        // Import GetAsyncKeyState from the Windows API
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public event Action<char> KeyPressed;

        private Dictionary<char, KeyTracker> _keys = new Dictionary<char, KeyTracker>();
       
        public KeyTracker Key(char key)
        {
            if (!_keys.ContainsKey(key))
            {
                _keys.Add(key, new KeyTracker(key));
            }

            return _keys[key];
        }

        public void Poll()
        {
            foreach (var key in _keys)
            {
                bool keyDown = (GetAsyncKeyState(key.Value.VirtualKey) & 0x8000) != 0;
                if (key.Value.IsPressed != keyDown)
                {
                    if (keyDown)
                        key.Value.OnPressed(key.Key);
                    else
                        key.Value.OnReleased(key.Key);
                }
            }
        }
    }

    public class KeyTracker
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern short VkKeyScan(char ch);

        public KeyTracker(char key)
        {
            Key = key;

            short scanResult = VkKeyScan(key);

            // The low-order byte contains the virtual key code
            VirtualKey = scanResult & 0xFF;
        }

        public event Action<char> Pressed;
        public event Action<char> Released;

        public bool IsPressed { get; private set; }
        public char Key { get; private set; }
        public int VirtualKey { get; private set; }

        public void OnPressed(char key)
        {
            IsPressed = true;
            Pressed?.Invoke(key);
        }

        public void OnReleased(char key)
        {
            IsPressed = false;
            Released?.Invoke(key);
        }
    }
}
