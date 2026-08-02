using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using DirtyMagic.Hooks;
using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Input;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Input
{
    public class GlobalKeyboardInput : IKeyboardInput
    {
        public override void KeyPress(VirtualKey key, Modifiers modifiers = Modifiers.None,
            TimeSpan keyPressTime = default(TimeSpan), int extraInfo = 0)
        {
            SendKey(key, modifiers, false, extraInfo);
            if (!DefaultKeypressTime.IsEmpty())
                Thread.Sleep((int) DefaultKeypressTime.TotalMilliseconds);
            SendKey(key, modifiers, true, extraInfo);
        }

        public override void SendChar(char c)
        {
            var inp = new INPUT {Type = InputType.KEYBOARD};
            inp.Union.ki.dwFlags = KeyEventFlags.UNICODE;
            inp.Union.ki.wVk = 0;
            inp.Union.ki.wScan = Convert.ToInt16(c);
            inp.Union.ki.time = 0;
            inp.Union.ki.dwExtraInfo = IntPtr.Zero;

            if (User32.SendInput(1, new INPUT[] {inp}, INPUT.Size) != 1)
                throw new Win32Exception();
        }

        public override void SendKey(VirtualKey key, Modifiers modifiers, bool up, int extraInfo = 0)
        {
            if (KeyboardHook.ModifierToKeyMap.TryGetValue(key, out var val))
                modifiers &= ~val;

            var inputs = BuildModifiersInput(modifiers, up, extraInfo);

            if (key != VirtualKey.None)
            {
                var inp = new INPUT {Type = InputType.KEYBOARD};
                inp.Union.ki.dwFlags = up ? KeyEventFlags.KEYUP : KeyEventFlags.NONE;
                inp.Union.ki.wVk = (short) key;
                inp.Union.ki.wScan = 0;
                inp.Union.ki.time = 0;
                inp.Union.ki.dwExtraInfo = new IntPtr(extraInfo);

                inputs.Add(inp);
            }

            if (inputs.Count == 0)
                return;

            if (up)
                inputs.Reverse();

            if (User32.SendInput(inputs.Count, inputs.ToArray(), INPUT.Size) != inputs.Count)
                throw new Win32Exception();
        }

        private List<INPUT> BuildModifiersInput(Modifiers modifiers, bool up, int extraInfo)
        {
            var keys = new List<VirtualKey>();
            foreach (var pair in KeyboardHook.ModifierToKeyMap)
            {
                if (modifiers.HasFlag(pair.Value))
                    keys.Add(pair.Key);
            }

            return keys.Select(key =>
            {
                var input = new INPUT();
                input.Type = InputType.KEYBOARD;
                input.Union.ki.dwFlags = up ? KeyEventFlags.KEYUP : KeyEventFlags.NONE;
                input.Union.ki.wVk = (short)key;
                input.Union.ki.wScan = 0;
                input.Union.ki.time = 0;
                input.Union.ki.dwExtraInfo = new IntPtr(extraInfo);

                return input;
            }).ToList();
        }

        public void SendScanCode(ScanCodeShort scanCode, bool up = false)
        {
            var inp = new INPUT {Type = InputType.KEYBOARD};
            inp.Union.ki.dwFlags = (up ? KeyEventFlags.KEYUP : KeyEventFlags.NONE) | KeyEventFlags.SCANCODE;
            inp.Union.ki.wVk = 0;
            inp.Union.ki.wScan = (short) scanCode;
            inp.Union.ki.time = 0;
            inp.Union.ki.dwExtraInfo = IntPtr.Zero;

            if (User32.SendInput(1, new[] {inp}, INPUT.Size) != 1)
                throw new Win32Exception();
        }
    }
}
