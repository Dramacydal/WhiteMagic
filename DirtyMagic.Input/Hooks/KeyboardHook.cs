using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DirtyMagic.Hooks.Events;
using DirtyMagic.Input;
using DirtyMagic.WinAPI;
using DirtyMagic.WinAPI.Input;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Hooks
{
    public class KeyboardHook() : HookBase(HookType.WH_KEYBOARD_LL)
    {
        public Modifiers ModifiersState { get; private set; } = Modifiers.None;

        public static readonly ConcurrentDictionary<VirtualKey, Modifiers> ModifierToKeyMap = new ConcurrentDictionary<VirtualKey, Modifiers>()
        {
            [VirtualKey.LMenu] = Modifiers.LAlt,
            [VirtualKey.RMenu] = Modifiers.RAlt,
            [VirtualKey.LControlKey] = Modifiers.LCtrl,
            [VirtualKey.RControlKey] = Modifiers.RCtrl,
            [VirtualKey.LShiftKey] = Modifiers.LShift,
            [VirtualKey.RShiftKey] = Modifiers.RShift,
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
            foreach (var d in OnKey?.GetInvocationList() ?? [])
                OnKey -= (KeyboardEventHandler) d;
        }
    }
}
