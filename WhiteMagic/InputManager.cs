using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhiteMagic.Input;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic
{
    public static class InputManager
    {
        public static class Global
        {
            public static GlobalKeyboardInput Keyboard { get; } = new GlobalKeyboardInput();

            public static GlobalMouseInput Mouse { get; } = new GlobalMouseInput();
        }

        public static WindowKeyboardInput CreateWindowInput(IntPtr Handle, bool Recursive = true)
        {
            return new WindowKeyboardInput(Handle, Recursive);
        }
    }
}
