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
        extension(Modifiers modifiers)
        {
            public bool AltPressed() => (modifiers & (Modifiers.LAlt | Modifiers.RAlt)) != Modifiers.None;
            public bool CtrlPressed() => (modifiers & (Modifiers.LCtrl | Modifiers.RCtrl)) != Modifiers.None;
            public bool ShiftPressed() => (modifiers & (Modifiers.LShift | Modifiers.RShift)) != Modifiers.None;
        }
    }
}
