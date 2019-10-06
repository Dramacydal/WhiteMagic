using System;

namespace DirtyMagic.Input
{
    [Flags]
    public enum Modifiers
    {
        None = 0x0,
        LAlt = 0x1,
        RAlt = 0x2,
        LCtrl = 0x4,
        RCtrl = 0x8,
        LShift = 0x10,
        RShift = 0x20,

        Alt = LAlt | RAlt,
        Ctrl = LCtrl | RCtrl,
        Shift = LShift | RShift
    }

    public static class ModifiersExtension
    {
        public static bool AltPressed(this Modifiers modifiers) => (modifiers & Modifiers.Alt) != Modifiers.None;
        public static bool CtrlPressed(this Modifiers modifiers) => (modifiers & Modifiers.Ctrl) != Modifiers.None;
        public static bool ShiftPressed(this Modifiers modifiers) => (modifiers & Modifiers.Shift) != Modifiers.None;
    }
}
