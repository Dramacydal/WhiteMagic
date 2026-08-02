using System;
using DirtyMagic.WinAPI.Input;

namespace DirtyMagic.Input
{
    public abstract class KeyboardInput
    {
        public static readonly TimeSpan DefaultKeypressTime = TimeSpan.FromMilliseconds(50);

        public abstract void SendKey(VirtualKey key, Modifiers modifiers, bool up, int extraInfo = 0);
        public abstract void KeyPress(VirtualKey key, Modifiers modifiers, TimeSpan keyPressTime, int extraInfo = 0);
        public abstract void SendChar(char c);

        public void KeyPress(VirtualKey key, Modifiers modifiers = Modifiers.None) => KeyPress(key, modifiers, TimeSpan.Zero);

        public void SendText(string text)
        {
            foreach (var c in text)
                SendChar(c);
        }
    }
}
