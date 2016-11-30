using System;
using System.Windows.Forms;

namespace WhiteMagic.Windows
{
    public interface IKeyboard
    {
        void Press(Keys key, TimeSpan interval);
        void PressRelease(Keys key);
        void Write(string text, params object[] args);
        void Press(Keys key);
        void Write(char character);
    }
}