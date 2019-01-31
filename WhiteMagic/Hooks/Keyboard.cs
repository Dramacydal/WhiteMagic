using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Structures;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Hooks
{
    public class KeyboardEvent : HookEvent
    {
        private KBDLLHOOKSTRUCT Raw;
        private Keyboard Hook;
        private WM Event;

        public KeyboardEvent(WM Event, KBDLLHOOKSTRUCT Raw, Keyboard Hook, bool WasPressed)
        {
            this.Event = Event;
            this.Hook = Hook;
            this.Raw = Raw;
            this.PreviouslyPressed = WasPressed;
        }

        public Keys VirtualKey => (Keys)Raw.vkCode;
        public ScanCodeShort ScanCode => (ScanCodeShort)Raw.scanCode;
        public bool IsKeyUp => Event == WM.KEYUP || Event == WM.SYSKEYUP;
        public bool IsKeyDown => !IsKeyUp;
        public bool IsExtended => ((KBDLLHOOKSTRUCT.LLFlags)Raw.flags & KBDLLHOOKSTRUCT.LLFlags.LLKHF_EXTENDED) != 0;
        public bool IsInjected => ((KBDLLHOOKSTRUCT.LLFlags)Raw.flags & (KBDLLHOOKSTRUCT.LLFlags.LLKHF_INJECTED)) != 0;
        public int ExtraInfo => Raw.dwExtraInfo.ToInt32();
        public bool PreviouslyPressed { get; } = false;

        public Modifiers ModifiersState => Hook.ModifiersState;
        
        public override string ToString() => string.Format($"VirtualKey: {VirtualKey} Scancode: {ScanCode} Extended: {IsExtended} Up: {IsKeyUp} WasPressed: {PreviouslyPressed} Injected: {IsInjected} ExtraInfo: {Raw.dwExtraInfo.ToInt32()}");
    }

    public class Keyboard : HookBase<KeyboardEvent>
    {
        public Keyboard() : base(HookType.WH_KEYBOARD_LL)
        {
        }

        public Modifiers ModifiersState { get; private set; } = Modifiers.None;

        private void StoreSpecialKeyState(WM Event, KeyboardEvent info)
        {
            var toggle = Event == WM.KEYDOWN || Event == WM.SYSKEYDOWN;
            Modifiers Flag = Modifiers.None;
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

                Dispatch(Event);
                if (Event.Cancel)
                    return false;
            }
            catch (Exception)
            {
            }

            return true;
        }
    }
}
