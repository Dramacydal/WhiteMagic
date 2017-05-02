using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteMagic.WinAPI.Structures.Input
{
    public struct MSG
    {
        public IntPtr hwnd;
        public WM message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }
}
