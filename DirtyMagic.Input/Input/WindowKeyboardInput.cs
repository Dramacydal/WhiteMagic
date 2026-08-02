using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Input;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Input
{
    public class WindowKeyboardInput : IKeyboardInput
    {
        public IntPtr Window { get; }
        public bool Recursive { get; set; }

        public WindowKeyboardInput(IntPtr window, bool recursive = true)
        {
            this.Window = window;
            this.Recursive = recursive;
        }

        public WindowKeyboardInput SetRecursive(bool on)
        {
            Recursive = on;
            return this;
        }

        private static void SendKeyToWindow(IntPtr window, VirtualKey key, bool up, bool recursive = false)
        {
            var lParam = 0u;
            lParam |= up ? 1u : 0;

            var scanCode = User32.MapVirtualKey((uint)key, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC);
            lParam |= (uint)scanCode << 16;

            if (up)
            {
                lParam |= 1u << 30;
                lParam |= 1u << 31;
            }

            if (!User32.PostMessage(window, up ? WM.KEYUP : WM.KEYDOWN, (uint)key, lParam))
                throw new Win32Exception();

            if (recursive)
            {
                User32.EnumChildWindows(window, (hwnd, param) =>
                {
                    SendKeyToWindow(hwnd, key, up, false);
                    return true;
                }, IntPtr.Zero);
            }
        }

        public override void SendKey(VirtualKey key, Modifiers modifiers, bool up, int extraInfo = 0)
        {
            var keyPresses = new List<VirtualKey>();
            if (modifiers.CtrlPressed())
                keyPresses.Add(VirtualKey.ControlKey);
            if (modifiers.AltPressed())
                keyPresses.Add(VirtualKey.Menu);
            if (modifiers.ShiftPressed())
                keyPresses.Add(VirtualKey.ShiftKey);
            if (key != VirtualKey.None)
                keyPresses.Add(key);

            if (up)
                keyPresses.Reverse();

            foreach (var keyPress in keyPresses)
                SendKeyToWindow(Window, keyPress, up, Recursive);
        }

        public override void KeyPress(VirtualKey key, Modifiers modifiers, TimeSpan keyPressTime, int extraInfo = 0)
        {
            SendKey(key, modifiers, false, 0);
            if (!DefaultKeypressTime.IsEmpty())
                Thread.Sleep((int)DefaultKeypressTime.TotalMilliseconds);
            SendKey(key, modifiers, true, 0);
        }

        public override void SendChar(char c)
        {
            if (!User32.PostMessage(Window, WM.CHAR, Convert.ToUInt32(c), 0))
                throw new Win32Exception();
        }
    }
}
