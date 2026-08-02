using System;
using DirtyMagic.Hooks.Events;
using DirtyMagic.WinAPI.Input;

namespace DirtyMagic.Input
{
    public class WindowMouseInput : IMouseInput
    {
        public IntPtr Window { get; }
        public bool Recursive { get; set; }

        public WindowMouseInput(IntPtr window, bool recursive = true)
        {
            this.Window = window;
            this.Recursive = recursive;
        }

        public WindowMouseInput SetRecursive(bool on)
        {
            Recursive = on;
            return this;
        }

        public override void Move(int x, int y, bool absolute)
        {
            throw new NotImplementedException();
        }

        public override void SendButton(MouseButton button, bool up = false)
        {
            throw new NotImplementedException();
        }

        public override void SendScroll(ScrollDirection direction)
        {
            throw new NotImplementedException();
        }
    }
}
