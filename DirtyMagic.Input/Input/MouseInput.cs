using System;
using System.Threading;
using DirtyMagic.Hooks.Events;
using DirtyMagic.WinAPI.Input;

namespace DirtyMagic.Input
{
    public abstract class MouseInput
    {
        public abstract void Move(int x, int y, bool absolute);
        public abstract void SendButton(MouseButton button, bool up = false);

        public void Click(MouseButton button, TimeSpan keyPressTime)
        {
            SendButton(button, false);
            if (!keyPressTime.IsEmpty())
                Thread.Sleep((int)keyPressTime.TotalMilliseconds);
            SendButton(button, true);
        }

        public void Click(MouseButton button) => Click(button, TimeSpan.Zero);

        public abstract void SendScroll(ScrollDirection direction);
    }
}
