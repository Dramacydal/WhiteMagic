using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Input
{
    public class GlobalKeyboardInput : IKeyboardInput
    {
        public override void KeyPress(Keys Key, TimeSpan KeyPressTime)
        {
            SendKey(Key, false);
            if (!KeypressTime.IsEmpty())
                Thread.Sleep((int)KeypressTime.TotalMilliseconds);
            SendKey(Key, true);
        }

        public override void SendChar(char c)
        {
            var inp = new INPUT();
            inp.Type = InputType.KEYBOARD;
            inp.Union.ki.dwFlags = KeyEventFlags.UNICODE;
            inp.Union.ki.wVk = 0;
            inp.Union.ki.wScan = Convert.ToInt16(c);
            inp.Union.ki.time = 0;
            inp.Union.ki.dwExtraInfo = IntPtr.Zero;

            if (User32.SendInput(1, ref inp, INPUT.Size) != 1)
                throw new Win32Exception();
        }

        public override void SendKey(Keys Key, bool Up = false)
        {
            var inp = new INPUT();
            inp.Type = InputType.KEYBOARD;
            inp.Union.ki.dwFlags = Up ? KeyEventFlags.KEYUP : KeyEventFlags.NONE;
            inp.Union.ki.wVk = (short)Key;
            inp.Union.ki.wScan = 0;
            inp.Union.ki.time = 0;
            inp.Union.ki.dwExtraInfo = IntPtr.Zero;

            if (User32.SendInput(1, ref inp, INPUT.Size) != 1)
                throw new Win32Exception();
        }

        public void SendScanCode(ScanCodeShort ScanCode, bool Up = false)
        {
            var inp = new INPUT();
            inp.Type = InputType.KEYBOARD;
            inp.Union.ki.dwFlags = (Up ? KeyEventFlags.KEYUP : KeyEventFlags.NONE) | KeyEventFlags.SCANCODE;
            inp.Union.ki.wVk = 0;
            inp.Union.ki.wScan = (short)ScanCode;
            inp.Union.ki.time = 0;
            inp.Union.ki.dwExtraInfo = IntPtr.Zero;

            if (User32.SendInput(1, ref inp, INPUT.Size) != 1)
                throw new Win32Exception();
        }
    }
}
