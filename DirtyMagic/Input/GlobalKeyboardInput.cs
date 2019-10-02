using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Structures.Input;

namespace DirtyMagic.Input
{
    public class GlobalKeyboardInput : IKeyboardInput
    {
        public override void KeyPress(Keys Key, Modifiers Modifiers = Modifiers.None, TimeSpan KeyPressTime = default(TimeSpan), int ExtraInfo = 0)
        {
            SendKey(Key, Modifiers, false, ExtraInfo);
            if (!DefaultKeypressTime.IsEmpty())
                Thread.Sleep((int)DefaultKeypressTime.TotalMilliseconds);
            SendKey(Key, Modifiers, true, ExtraInfo);
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

            if (User32.SendInput(1, new INPUT[] { inp }, INPUT.Size) != 1)
                throw new Win32Exception();
        }

        public override void SendKey(Keys Key, Modifiers Modifiers = Modifiers.None, bool Up = false, int ExtraInfo = 0)
        {
            var KeyMod = KeyToModifier(Key);
            if (KeyMod != Modifiers.None)
                Modifiers &= ~KeyMod;

            var inputs = BuildModifiersInput(Modifiers, Up, ExtraInfo);

            if (Key != Keys.None)
            {
                var inp = new INPUT();
                inp.Type = InputType.KEYBOARD;
                inp.Union.ki.dwFlags = Up ? KeyEventFlags.KEYUP : KeyEventFlags.NONE;
                inp.Union.ki.wVk = (short)Key;
                inp.Union.ki.wScan = 0;
                inp.Union.ki.time = 0;
                inp.Union.ki.dwExtraInfo = new IntPtr(ExtraInfo);

                inputs.Add(inp);
            }

            if (inputs.Count == 0)
                return;

            if (Up)
                inputs.Reverse();

            if (User32.SendInput(inputs.Count, inputs.ToArray(), INPUT.Size) != inputs.Count)
                throw new Win32Exception();
        }

        private Modifiers KeyToModifier(Keys Key)
        {
            switch (Key)
            {
                case Keys.LMenu:
                case Keys.RMenu:
                    return Modifiers.Alt;
                case Keys.LControlKey:
                case Keys.RControlKey:
                    return Modifiers.Ctrl;
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    return Modifiers.Shift;
                default:
                    break;
            }

            return Modifiers.None;
        }

        private List<INPUT> BuildModifiersInput(Modifiers Modifiers, bool Up, int ExtraInfo)
        {
            var keys = new List<Keys>();
            if (Modifiers.CtrlPressed())
                keys.Add(Keys.ControlKey);
            if (Modifiers.AltPressed())
                keys.Add(Keys.Menu);
            if (Modifiers.ShiftPressed())
                keys.Add(Keys.ShiftKey);

            return keys.Select(key =>
            {
                var input = new INPUT();
                input.Type = InputType.KEYBOARD;
                input.Union.ki.dwFlags = Up ? KeyEventFlags.KEYUP : KeyEventFlags.NONE;
                input.Union.ki.wVk = (short)key;
                input.Union.ki.wScan = 0;
                input.Union.ki.time = 0;
                input.Union.ki.dwExtraInfo = new IntPtr(ExtraInfo);

                return input;
            }).ToList();
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

            if (User32.SendInput(1, new INPUT[] { inp }, INPUT.Size) != 1)
                throw new Win32Exception();
        }
    }
}
