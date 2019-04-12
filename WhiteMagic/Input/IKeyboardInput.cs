using System;
using System.Windows.Forms;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Input
{
    public abstract class IKeyboardInput
    {
        public static readonly TimeSpan DefaultKeypressTime = TimeSpan.FromMilliseconds(50);

        public abstract void SendKey(Keys Key, Modifiers Modifiers = Modifiers.None, bool Up = false, int ExtraInfo = 0);
        public abstract void KeyPress(Keys Key, Modifiers Modifiers = Modifiers.None, TimeSpan KeyPressTime = default(TimeSpan), int ExtraInfo = 0);
        public abstract void SendChar(char c);

        public void KeyPress(Keys Key, Modifiers Modifiers = Modifiers.None) => KeyPress(Key, Modifiers);

        public void SendText(string Text)
        {
            foreach (var c in Text)
                SendChar(c);
        }
    }
}
