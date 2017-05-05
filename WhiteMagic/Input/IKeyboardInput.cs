using System;
using System.Windows.Forms;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Input
{
    public abstract class IKeyboardInput
    {
        public TimeSpan KeypressTime = TimeSpan.FromMilliseconds(50);

        public abstract void SendKey(Keys Key, bool Up = false);
        public abstract void KeyPress(Keys Key, TimeSpan KeyPressTime);
        public abstract void SendChar(char c);

        public void KeyPress(Keys Key) => KeyPress(Key, KeypressTime);

        public void SendText(string Text)
        {
            foreach (var c in Text)
                SendChar(c);
        }

        public ModifierToggler SetModifiers(Modifiers Mask) => new ModifierToggler(this, Mask);
    }
}
