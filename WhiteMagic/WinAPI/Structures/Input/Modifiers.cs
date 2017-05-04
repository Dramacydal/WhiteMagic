using System;

namespace WhiteMagic.WinAPI.Structures.Input
{
    [Flags]
    public enum Modifiers
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4
    }
}
