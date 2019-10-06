using System;
using System.Threading;
using System.Windows.Forms;
using DirtyMagic.Hooks.Events;

namespace DirtyMagic.Input
{
    public abstract class IMouseInput
    {
        public abstract void Move(int x, int y, bool absolute);
        public abstract void SendButton(MouseButtons button, bool up = false);

        public void Click(MouseButtons button, TimeSpan keyPressTime)
        {
            SendButton(button, false);
            if (!keyPressTime.IsEmpty())
                Thread.Sleep((int)keyPressTime.TotalMilliseconds);
            SendButton(button, true);
        }

        public void Click(MouseButtons button) => Click(button, new TimeSpan());

        public abstract void SendScroll(ScrollDirection direction);
    }
}
