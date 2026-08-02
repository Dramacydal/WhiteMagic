using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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

        public static readonly ConcurrentDictionary<Keys, Modifiers> ModifierToKeyMap = new ConcurrentDictionary<Keys, Modifiers>()
        {
            [Keys.LMenu] = Modifiers.LAlt,
            [Keys.RMenu] = Modifiers.RAlt,
            [Keys.LControlKey] = Modifiers.LCtrl,
            [Keys.RControlKey] = Modifiers.RCtrl,
            [Keys.LShiftKey] = Modifiers.LShift,
            [Keys.RShiftKey] = Modifiers.RShift,
        };

        private void StoreSpecialKeyState(KeyboardEvent info)
        {
            if (ModifierToKeyMap.TryGetValue(info.VirtualKey, out var mod))
            {
                if (info.IsKeyDown)
                    ModifiersState |= mod;
                else
                    ModifiersState &= ~mod;
            }
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

                StoreSpecialKeyState(Event);

                OnKey?.Invoke(Event);

                if (Event.Cancel)
                    return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"KeyboardHook.Dispatch: unhandled exception from OnKey subscriber: {ex}");
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
