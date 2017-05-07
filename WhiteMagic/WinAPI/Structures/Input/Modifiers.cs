using System;

namespace WhiteMagic.WinAPI.Structures.Input
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
}
