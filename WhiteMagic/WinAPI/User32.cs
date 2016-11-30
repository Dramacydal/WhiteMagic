using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using WhiteMagic.WinAPI.Structures;

namespace WhiteMagic.WinAPI
{
    public static class User32
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetSystemMetrics(SystemMetrics metric);
 
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, out WindowPlacement lpwndpl);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FlashInfo pwfi);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint key, TranslationTypes translation);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SendInput(int nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
  
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
 
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WindowPlacement lpwndpl);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr hwnd, string lpString);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, WindowStates nCmdShow);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, int msg, UIntPtr wParam, UIntPtr lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true), SuppressUnmanagedCodeSecurity]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        internal static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(HookType code,
            HookProc func,
            IntPtr hInstance,
            int threadId);

        [DllImport("user32.dll", EntryPoint = "SetWindowsHookEx")]
        public static extern IntPtr SetWindowsHookLowLevel(HookType code,
            LowLevelProc func,
            IntPtr hInstance,
            int threadId);

        public static IntPtr SetWindowsHook(HookType hookType, LowLevelProc callback)
        {
            IntPtr hookId;
            using (var currentProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var currentModule = currentProcess.MainModule)
            {
                var handle = Kernel32.GetModuleHandle(currentModule.ModuleName);
                hookId = SetWindowsHookLowLevel(hookType, callback, handle, 0);
            }
            return hookId;
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        internal static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        internal static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr newValue);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        internal static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr newValue);

        [DllImport("user32.dll")]
        internal static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int msg, int wParam,
            IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern int RegisterWindowMessage(string lpString);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);
    }
}
