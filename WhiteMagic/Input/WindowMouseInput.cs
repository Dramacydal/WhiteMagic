using System;
using System.Windows.Forms;
using WhiteMagic.Hooks.Events;
using WhiteMagic.Processes;

namespace WhiteMagic.Input
{
    public class WindowMouseInput : IMouseInput
    {
        public RemoteWindow Window { get; }
        public bool Recursive { get; set; }

        public WindowMouseInput(RemoteWindow Window, bool Recursive = true)
        {
            this.Window = Window;
            this.Recursive = Recursive;
        }

        public WindowMouseInput SetRecursive(bool On)
        {
            Recursive = On;
            return this;
        }

        public override void Move(int X, int Y, bool Absolute)
        {
            throw new NotImplementedException();
        }

        public override void SendButton(MouseButtons Button, bool Up = false)
        {
            throw new NotImplementedException();
        }

        public override void SendScroll(ScrollDirection Direction)
        {
            throw new NotImplementedException();
        }
    }
}
