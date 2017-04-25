using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WhiteMagic.WinAPI.Structures;
using WhiteMagic.WinAPI.Structures.Hooks;
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

        public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpFn, IntPtr hMod, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(IntPtr handle, int nCode, IntPtr wParam, IntPtr lParam);

        public static void MoveMouse(int dx, int dy)
        {
            var inp = new INPUT();
            inp.Type = InputType.MOUSE;
            inp.Union.Mouse.dx = dx;
            inp.Union.Mouse.dy = dy;
            inp.Union.Mouse.mouseData = 0;
            inp.Union.Mouse.dwFlags = MouseEventFlag.MOVE;
            inp.Union.Mouse.time = 0;
            inp.Union.Mouse.dwExtraInfo = IntPtr.Zero;

            if (SendInput(1, ref inp, INPUT.Size) != 1)
                throw new Win32Exception();
        }

        public static void SendKey(ScanCodeShort sc, Keys vk, bool up = false)
        {
            var inp = new INPUT();
            inp.Type = InputType.KEYBOARD;
            inp.Union.Keyboard.dwFlags = up ? KeyEventFlags.KEYUP : KeyEventFlags.NONE;
            inp.Union.Keyboard.wVk = (short)vk;
            inp.Union.Keyboard.wScan = (short)sc;
            inp.Union.Keyboard.time = 0;
            inp.Union.Keyboard.dwExtraInfo = IntPtr.Zero;

            if (SendInput(1, ref inp, INPUT.Size) != 1)
                throw new Win32Exception();
        }

        public static void SendKeyToWindow(IntPtr Window, Keys Key, bool Up, bool Recursive = false)
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
