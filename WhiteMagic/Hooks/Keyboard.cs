using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WhiteMagic.WinAPI.Structures.Hooks;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Hooks
{
    public class KeyEventInfo
    {
        private KBDLLHOOKSTRUCT Raw;
        private Keyboard Hook;
        private WM Event;

        public KeyEventInfo(WM Event, KBDLLHOOKSTRUCT Raw, Keyboard Hook)
        {
            this.Event = Event;
            this.Hook = Hook;
            this.Raw = Raw;
        }

        public Keys VirtualKey { get { return (Keys)Raw.vkCode; } }
        public ScanCodeShort ScanCode { get { return (ScanCodeShort)Raw.scanCode; } }
        public bool Up { get { return Event == WM.KEYUP || Event == WM.SYSKEYUP; } }

        public Modifiers ModifiersState { get { return Hook.ModifiersState; } }
        
        public override string ToString()
        {
            return string.Format($"VirtualKey: {VirtualKey} Scancode: {ScanCode} Up: {Up}");
        }
    }

    public delegate bool KeyboardMessageHandler(KeyEventInfo Info);

    public class Keyboard : HookBase<KeyboardMessageHandler>
    {
        public Keyboard() : base(HookType.WH_KEYBOARD_LL)
        {
        }

        private bool LAltPressed { get; set; } = false;
        private bool RAltPressed { get; set; } = false;
        private bool LControlPressed { get; set; } = false;
        private bool RControlPressed { get; set; } = false;
        private bool LShiftPressed { get; set; } = false;
        private bool RShiftPressed { get; set; } = false;

        public Modifiers ModifiersState
        {
            get
            {
                if (!Installed)
                    throw new Win32Exception("Keyboard hooks are not installed");

                Modifiers state = Modifiers.None;
                if (LAltPressed || RAltPressed)
                    state |= Modifiers.Alt;
                if (LControlPressed || RControlPressed)
                    state |= Modifiers.Ctrl;
                if (LShiftPressed || RShiftPressed)
                    state |= Modifiers.Shift;

                return state;
            }
        }

        private void StoreSpecialKeyState(WM Event, KeyEventInfo info)
        {
            var toggle = Event == WM.KEYDOWN || Event == WM.SYSKEYDOWN;
            switch (info.VirtualKey)
            {
                case Keys.LMenu: LAltPressed = toggle; break;
                case Keys.RMenu: RAltPressed = toggle; break;
                case Keys.LControlKey: LControlPressed = toggle; break;
                case Keys.RControlKey: RControlPressed = toggle; break;
                case Keys.LShiftKey: LShiftPressed = toggle; break;
                case Keys.RShiftKey: RShiftPressed = toggle; break;
                default: break;
            }
        }

        public override bool Dispatch(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code != 0)
                return true;

            var ev = (WM)wParam;

            try
            {
                var str = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                var keyInfo = new KeyEventInfo(ev, str, this);

                StoreSpecialKeyState(ev, keyInfo);

                foreach (var Handler in Handlers)
                    if (!Handler(keyInfo))
                        return false;
            }
            catch (Exception)
            {
            }

            return true;
        }
    }
}
