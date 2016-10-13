using System;
using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public UInt32 LowPart;
        public Int32 HighPart;
    }
}
