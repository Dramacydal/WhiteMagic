using System;

namespace WhiteMagic.WinAPI
{
    [Flags]
    public enum FreeType : uint
    {
        Decommit = 0x4000,
        Release = 0x8000,
    }
}
