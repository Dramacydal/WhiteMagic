using System;

namespace DirtyMagic.WinAPI.Structures
{
    [Flags]
    public enum WakeFlags : uint
    {
        QS_ALLEVENTS = 0x04BF,
    }
}
