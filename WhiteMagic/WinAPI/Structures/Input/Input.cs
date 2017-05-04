using System;
using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI.Structures.Input
{
    [Flags]
    internal enum KeyEventFlags : uint
    {
        NONE = 0x0,
        EXTENDEDKEY = 0x0001,
        KEYUP = 0x0002,
        SCANCODE = 0x0008,
        UNICODE = 0x0004
    }

    [Flags]
    public enum MouseEventFlag : uint
    {
        ABSOLUTE = 0x8000,
        HWHEEL = 0x01000,
        MOVE = 0x0001,
        MOVE_NOCOALESCE = 0x2000,
        LEFTDOWN = 0x0002,
        LEFTUP = 0x0004,
        RIGHTDOWN = 0x0008,
        RIGHTUP = 0x0010,
        MIDDLEDOWN = 0x0020,
        MIDDLEUP = 0x0040,
        VIRTUALDESK = 0x4000,
        WHEEL = 0x0800,
        XDOWN = 0x0080,
        XUP = 0x0100
    }

    public enum InputType : uint
    {
        MOUSE = 0,
        KEYBOARD = 1,
        HARDWARE = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public InputType Type;
        public InputUnion Union;

        public static int Size
        {
            get { return Marshal.SizeOf(typeof(INPUT)); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KBDLLHOOKSTRUCT
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;

        [Flags]
        public enum Flags : int
        {
            KF_EXTENDED = 0x0100,
            KF_DLGMODE = 0x0800,
            KF_MENUMODE = 0x1000,
            KF_ALTDOWN = 0x2000,
            KF_REPEAT = 0x4000,
            KF_UP = 0x8000,
        }

        [Flags]
        public enum LLFlags : int
        {
            LLKHF_EXTENDED = (Flags.KF_EXTENDED >> 8),
            LLKHF_INJECTED = 0x00000010,
            LLKHF_ALTDOWN = (Flags.KF_ALTDOWN >> 8),
            LLKHF_UP = (Flags.KF_UP >> 8),
            LLKHF_LOWER_IL_INJECTED = 0x00000002,
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public int ptX;
        public int ptY;
        public int mouseData;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)]
        internal MOUSEINPUT mi;
        [FieldOffset(0)]
        internal KEYBDINPUT ki;
        //[FieldOffset(0)]
        //internal HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public int mouseData;
        public MouseEventFlag dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        internal short wVk;
        internal short wScan;
        internal KeyEventFlags dwFlags;
        internal int time;
        internal IntPtr dwExtraInfo;
    }
}
