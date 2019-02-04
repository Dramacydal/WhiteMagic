using System;
using WhiteMagic.Input;

namespace WhiteMagic.Processes
{
    public class RemoteWindow
    {
        public IntPtr Handle { get; }
        public RemoteProcess Process { get; }

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
