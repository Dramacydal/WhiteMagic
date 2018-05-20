using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhiteMagic.Processes;
using WhiteMagic.WinAPI.Structures.Input;

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
