using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteMagic.Input;

namespace WhiteMagic.Processes
{
    public class RemoteWindow
    {
        public IntPtr Handle { get; private set; }
        public RemoteProcess Process { get; private set; }

        public WindowKeyboardInput KeyboardInput { get; }
        public WindowMouseInput MouseInput { get; }


        public RemoteWindow(RemoteProcess Process, IntPtr WindowHandle)
        {
            Handle = WindowHandle;
            this.Process = Process;

            KeyboardInput = new WindowKeyboardInput(this);
            MouseInput = new WindowMouseInput(this);
        }
    }
}
