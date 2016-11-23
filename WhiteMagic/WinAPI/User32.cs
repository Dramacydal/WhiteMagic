using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using WhiteMagic.WinAPI.Structures;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.WinAPI
{
    public static class User32
    {
        [DllImport("user32.dll")]
        public static extern WaitResult MsgWaitForMultipleObjects(uint nCount, IntPtr[] pHandles,
            bool bWaitAll, uint dwMilliseconds, WakeFlags dwWakeMask);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, WM Msg, uint wParam, uint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        public static void MoveMouse(int dx, int dy)
        {
            var inp = new INPUT();
            inp.Type = InputType.MOUSE;
            inp.Input.mi.dx = dx;
            inp.Input.mi.dy = dy;
            inp.Input.mi.mouseData = 0;
            inp.Input.mi.dwFlags = MouseEventFlag.MOVE;
            inp.Input.mi.time = 0;
            inp.Input.mi.dwExtraInfo = IntPtr.Zero;

            if (SendInput(1, ref inp, INPUT.Size) != 1)
                throw new Win32Exception();
        }

        public static void SendKey(ScanCodeShort sc, VirtualKeyShort vk, bool up = false)
        {
            var inp = new INPUT();
            inp.Type = InputType.KEYBOARD;
            inp.Input.ki.dwFlags = up ? KeyEventFlags.KEYUP : 0;
            inp.Input.ki.wVk = vk;
            inp.Input.ki.wScan = sc;
            inp.Input.ki.time = 0;
            inp.Input.ki.dwExtraInfo = IntPtr.Zero;

            if (SendInput(1, ref inp, INPUT.Size) != 1)
                throw new Win32Exception();
        }

        public static void SendKeyToWindow(IntPtr Window, VirtualKeyShort Key, bool Up, bool Recursive = false)
        {
            if (!PostMessage(Window, Up ? WM.KEYUP : WM.KEYDOWN, (uint)Key, 0))
                throw new Win32Exception();

            if (Recursive)
            {
                EnumChildWindows(Window, (IntPtr hwnd, IntPtr param) =>
                    {
                        SendKeyToWindow(hwnd, Key, Up, false);
                        return true;
                    }, IntPtr.Zero);
            }
        }
    }
}
