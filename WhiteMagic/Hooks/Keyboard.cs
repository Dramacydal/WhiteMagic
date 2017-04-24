using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WhiteMagic.WinAPI.Structures.Hooks;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Hooks
{
    public class KeyInfo
    {
        private KBDLLHOOKSTRUCT Raw;

        public KeyInfo(KBDLLHOOKSTRUCT raw)
        {
            Raw = raw;
        }

        public VirtualKeyShort VirtualKey { get { return (VirtualKeyShort)Raw.vkCode; } }
        public ScanCodeShort ScanCode { get { return (ScanCodeShort)Raw.scanCode; } }
        
        public bool IsExtended { get { return Flags.HasFlag(KBDLLHOOKSTRUCT.LLFlags.LLKHF_EXTENDED); } }

        private KBDLLHOOKSTRUCT.LLFlags Flags { get { return (KBDLLHOOKSTRUCT.LLFlags)Raw.flags; } }
    }

    public delegate bool KeyboardMessageHandler(WM mEvent, KeyInfo info);

    public class Keyboard : HookBase<KeyboardMessageHandler>
    {
        public Keyboard() : base(HookType.WH_KEYBOARD_LL)
        {
        }

        public bool LAltPressed { get; private set; }
        public bool RAltPressed { get; private set; }
        public bool LControlPressed { get; private set; }
        public bool RControlPressed { get; private set; }
        public bool LShiftPressed { get; private set; }
        public bool RShiftPressed { get; private set; }

        public bool AltPressed { get { return LAltPressed || RAltPressed; } }
        public bool ControlPressed { get { return LControlPressed || RControlPressed; } }
        public bool ShiftPressed { get { return LShiftPressed || RShiftPressed; } }

        private Dictionary<VirtualKeyShort, bool> SpecialKeyStates = new Dictionary<VirtualKeyShort, bool>();
        void StoreSpecialKeyState(WM Event, KeyInfo info)
        {
            var toggle = Event == WM.KEYDOWN || Event == WM.SYSKEYDOWN;
            switch (info.VirtualKey)
            {
                case VirtualKeyShort.LMENU: LAltPressed = toggle; break;
                case VirtualKeyShort.RMENU: RAltPressed = toggle; break;
                case VirtualKeyShort.LCONTROL: LControlPressed = toggle; break;
                case VirtualKeyShort.RCONTROL: RControlPressed = toggle; break;
                case VirtualKeyShort.LSHIFT: LShiftPressed = toggle; break;
                case VirtualKeyShort.RSHIFT: RShiftPressed = toggle; break;
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
                var keyInfo = new KeyInfo(str);

                StoreSpecialKeyState(ev, keyInfo);

                foreach (var Handler in Handlers)
                    if (!Handler(ev, keyInfo))
                        return false;
            }
            catch (Exception)
            {
            }

            return true;
        }
    }
}
