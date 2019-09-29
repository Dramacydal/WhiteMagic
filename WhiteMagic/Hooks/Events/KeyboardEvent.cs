using System.Windows.Forms;
using WhiteMagic.Input;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Hooks.Events
{
    public class KeyboardEvent : HookEvent
    {
        private KBDLLHOOKSTRUCT Raw;
        private KeyboardHook Hook;
        private WM Event;

        public KeyboardEvent(WM Event, KBDLLHOOKSTRUCT Raw, KeyboardHook Hook, bool WasPressed)
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
}
