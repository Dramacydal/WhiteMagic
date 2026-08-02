using DirtyMagic.Input;
using DirtyMagic.WinAPI.Input;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Hooks.Events
{
    public class KeyboardEvent : HookEvent
    {
        private readonly KBDLLHOOKSTRUCT _raw;
        private readonly KeyboardHook _hook;
        private readonly WM _event;

        public KeyboardEvent(WM @event, KBDLLHOOKSTRUCT raw, KeyboardHook hook, bool wasPressed)
        {
            _event = @event;
            _hook = hook;
            _raw = raw;
            PreviouslyPressed = wasPressed;
        }

        public VirtualKey VirtualKey => (VirtualKey)_raw.vkCode;
        public ScanCodeShort ScanCode => (ScanCodeShort)_raw.scanCode;
        public bool IsKeyUp => _event == WM.KEYUP || _event == WM.SYSKEYUP;
        public bool IsKeyDown => _event == WM.KEYDOWN || _event == WM.SYSKEYDOWN;
        public bool IsExtended => ((KBDLLHOOKSTRUCT.LLFlags)_raw.flags & KBDLLHOOKSTRUCT.LLFlags.LLKHF_EXTENDED) != 0;
        public bool IsInjected => ((KBDLLHOOKSTRUCT.LLFlags)_raw.flags & (KBDLLHOOKSTRUCT.LLFlags.LLKHF_INJECTED)) != 0;
        public int ExtraInfo => _raw.dwExtraInfo.ToInt32();
        public bool PreviouslyPressed { get; } = false;

        public Modifiers ModifiersState => _hook.ModifiersState;
        
        public override string ToString() => string.Format($"VirtualKey: {VirtualKey} Scancode: {ScanCode} Extended: {IsExtended} Up: {IsKeyUp} WasPressed: {PreviouslyPressed} Injected: {IsInjected} ExtraInfo: {_raw.dwExtraInfo.ToInt32()}");
    }
}
