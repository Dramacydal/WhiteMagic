using System;
using System.Text;
//using DirtyMagic.Input;
using DirtyMagic.WinAPI;

namespace DirtyMagic.Processes
{
    public class RemoteWindow
    {
        public IntPtr Handle { get; }
        public RemoteProcess Process { get; }

        //public WindowKeyboardInput KeyboardInput { get; }
        //public WindowMouseInput MouseInput { get; }

        public string Title
        {
            get
            {
                // Allocate correct string length first
                var length = User32.GetWindowTextLength(Handle);
                if (length <= 0)
                    return "";

                var sb = new StringBuilder(length + 1);
                User32.GetWindowText(Handle, sb, sb.Capacity);

                return sb.ToString();
            }
        }

        public RemoteWindow(RemoteProcess process, IntPtr windowHandle)
        {
            Handle = windowHandle;
            this.Process = process;

            //KeyboardInput = new WindowKeyboardInput(this);
            //MouseInput = new WindowMouseInput(this);
        }
    }
}
