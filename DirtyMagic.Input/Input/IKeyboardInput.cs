using System;
using DirtyMagic.WinAPI.Input;

namespace DirtyMagic.Input
{
    public abstract class KeyboardInput
    {
        public static readonly TimeSpan DefaultKeypressTime = TimeSpan.FromMilliseconds(50);

        public abstract void SendKey(VirtualKey key, Modifiers modifiers, bool up, int extraInfo = 0);

        public abstract void SendChar(char c);

        public abstract void KeyPress(VirtualKey key, TimeSpan keyPressTime, Modifiers modifiers = Modifiers.None, int extraInfo = 0);

        public void KeyPress(VirtualKey key, Modifiers modifiers = Modifiers.None) => KeyPress(key, DefaultKeypressTime, modifiers);

        public void SendText(string text)
        {
            foreach (var c in text)
                SendChar(c);
        }
    }
}
