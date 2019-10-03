using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Input
{
    public class WindowKeyboardInput : IKeyboardInput
    {
        public IntPtr Window { get; }
        public bool Recursive { get; set; }

        public WindowKeyboardInput(IntPtr Window, bool Recursive = true)
        {
            this.Window = Window;
            this.Recursive = Recursive;
        }

        public WindowKeyboardInput SetRecursive(bool On)
        {
            Recursive = On;
            return this;
        }

        private static void SendKeyToWindow(IntPtr Window, Keys Key, bool Up, bool Recursive = false)
        {
            var lParam = 0u;
            lParam |= Up ? 1u : 0;

            var scanCode = User32.MapVirtualKey((uint)Key, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC);
            lParam |= (uint)scanCode << 16;

            if (Up)
            {
                lParam |= 1u << 30;
                lParam |= 1u << 31;
            }

            if (!User32.PostMessage(Window, Up ? WM.KEYUP : WM.KEYDOWN, (uint)Key, lParam))
                throw new Win32Exception();

            if (Recursive)
            {
                User32.EnumChildWindows(Window, (IntPtr hwnd, IntPtr param) =>
                {
                    SendKeyToWindow(hwnd, Key, Up, false);
                    return true;
                }, IntPtr.Zero);
            }
        }

        public override void SendKey(Keys Key, Modifiers Modifiers = Modifiers.None, bool Up = false, int ExtraInfo = 0)
        {
            var keys = new List<Keys>();
            if (Modifiers.CtrlPressed())
                keys.Add(Keys.ControlKey);
            if (Modifiers.AltPressed())
                keys.Add(Keys.Menu);
            if (Modifiers.ShiftPressed())
                keys.Add(Keys.ShiftKey);
            if (Key != Keys.None)
                keys.Add(Key);

            if (Up)
                keys.Reverse();

            foreach (var key in keys)
                SendKeyToWindow(Window, Key, Up, Recursive);
        }

        public override void KeyPress(Keys Key, Modifiers Modifiers, TimeSpan KeyPressTime, int ExtraInfo = 0)
        {
            SendKey(Key, Modifiers, false, 0);
            if (!DefaultKeypressTime.IsEmpty())
                Thread.Sleep((int)DefaultKeypressTime.TotalMilliseconds);
            SendKey(Key, Modifiers, true, 0);
        }

        public override void SendChar(char c)
        {
            if (!User32.PostMessage(Window, WM.CHAR, Convert.ToUInt32(c), 0))
                throw new Win32Exception();
        }
    }
}
