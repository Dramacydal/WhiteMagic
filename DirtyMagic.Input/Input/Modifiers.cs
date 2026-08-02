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
        RShift = 0x20
    }

    public static class ModifiersExtension
    {
        public static bool AltPressed(this Modifiers modifiers) => (modifiers & (Modifiers.LAlt | Modifiers.RAlt)) != Modifiers.None;
        public static bool CtrlPressed(this Modifiers modifiers) => (modifiers & (Modifiers.LCtrl | Modifiers.RCtrl)) != Modifiers.None;
        public static bool ShiftPressed(this Modifiers modifiers) => (modifiers & (Modifiers.LShift | Modifiers.RShift)) != Modifiers.None;
    }
}
