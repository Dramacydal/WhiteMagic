using System;
using System.Threading;
using System.Windows.Forms;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Input
{
    public abstract class IMouseInput
    {
        public abstract void Move(int X, int Y, bool Absolute);
        public abstract void SendButton(MouseButtons Button, bool Up = false);

        public void Click(MouseButtons Button, TimeSpan PressTime)
        {
            SendButton(Button, false);
            if (!PressTime.IsEmpty())
                Thread.Sleep((int)PressTime.TotalMilliseconds);
            SendButton(Button, true);
        }

        public void Click(MouseButtons Button)
        {
            Click(Button, new TimeSpan());
        }

        public abstract void SendScroll(ScrollDirection Direction);
    }
}
