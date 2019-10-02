using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DirtyMagic.Hooks.Events;
using DirtyMagic.Input;
using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Structures;
using DirtyMagic.WinAPI.Structures.Input;

namespace DirtyMagic.Hooks
{
    public class KeyboardHook : HookBase
    {
        public KeyboardHook() : base(HookType.WH_KEYBOARD_LL)
        {
        }

        public Modifiers ModifiersState { get; private set; } = Modifiers.None;

        private void StoreSpecialKeyState(WM Event, KeyboardEvent info)
        {
            var toggle = Event == WM.KEYDOWN || Event == WM.SYSKEYDOWN;
            Modifiers Flag;
            switch (info.VirtualKey)
            {
                case Keys.LMenu: Flag = Modifiers.LAlt; break;
                case Keys.RMenu: Flag = Modifiers.RAlt; break;
                case Keys.LControlKey: Flag = Modifiers.LCtrl; break;
                case Keys.RControlKey: Flag = Modifiers.RCtrl; break;
                case Keys.LShiftKey: Flag = Modifiers.LShift; break;
                case Keys.RShiftKey: Flag = Modifiers.RShift; break;
                default: return;
            }

            if (toggle)
                ModifiersState |= Flag;
            else
                ModifiersState &= ~Flag;
        }

        internal override bool Dispatch(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code != 0)
                return true;

            var wmEvent = (WM)wParam.ToUInt32();

            try
            {
                var str = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                var state = User32.GetAsyncKeyState(str.vkCode);

                var Event = new KeyboardEvent(wmEvent, str, this, (state & 0x8000) != 0);

                StoreSpecialKeyState(wmEvent, Event);

                OnKey?.Invoke(Event);

                if (Event.Cancel)
                    return false;
            }
            catch (Exception)
            {
            }

            return true;
        }

        public delegate void KeyboardEventHandler(KeyboardEvent e);

        public event KeyboardEventHandler OnKey;

        public override void RemoveHandlers()
        {
            foreach (var d in OnKey?.GetInvocationList() ?? new Delegate[] { })
                OnKey -= (KeyboardEventHandler) d;
        }
    }
}
