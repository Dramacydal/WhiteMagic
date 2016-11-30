using System;

namespace WhiteMagic.WinAPI
{
        public delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}