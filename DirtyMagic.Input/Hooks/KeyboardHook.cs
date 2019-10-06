using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DirtyMagic.Hooks.Events;
using DirtyMagic.Input;
using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Hooks
{
    public class KeyboardHook : HookBase
    {
        public KeyboardHook() : base(HookType.WH_KEYBOARD_LL)
        {
        }

        public Modifiers ModifiersState { get; private set; } = Modifiers.None;

        private void StoreSpecialKeyState(WM @event, KeyboardEvent info)
        {
            var toggle = @event == WM.KEYDOWN || @event == WM.SYSKEYDOWN;
            Modifiers flag;
            switch (info.VirtualKey)
            {
                case Keys.LMenu: flag = Modifiers.LAlt; break;
                case Keys.RMenu: flag = Modifiers.RAlt; break;
                case Keys.LControlKey: flag = Modifiers.LCtrl; break;
                case Keys.RControlKey: flag = Modifiers.RCtrl; break;
                case Keys.LShiftKey: flag = Modifiers.LShift; break;
                case Keys.RShiftKey: flag = Modifiers.RShift; break;
                default: return;
            }

            if (toggle)
                ModifiersState |= flag;
            else
                ModifiersState &= ~flag;
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
