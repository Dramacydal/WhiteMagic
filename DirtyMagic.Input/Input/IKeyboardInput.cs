using System;
using System.Windows.Forms;

namespace DirtyMagic.Input
{
    public abstract class IKeyboardInput
    {
        public static readonly TimeSpan DefaultKeypressTime = TimeSpan.FromMilliseconds(50);

        public abstract void SendKey(Keys key, Modifiers modifiers, bool up, int extraInfo = 0);
        public abstract void KeyPress(Keys key, Modifiers modifiers, TimeSpan keyPressTime, int extraInfo = 0);
        public abstract void SendChar(char c);

        public void KeyPress(Keys key, Modifiers modifiers = Modifiers.None) => KeyPress(key, modifiers, default(TimeSpan));

        public void SendText(string text)
        {
            foreach (var c in text)
                SendChar(c);
        }
    }
}
