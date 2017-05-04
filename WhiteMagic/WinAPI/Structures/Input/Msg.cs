using System;
using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI.Structures.Input
{
    [StructLayout(LayoutKind.Sequential)]
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
