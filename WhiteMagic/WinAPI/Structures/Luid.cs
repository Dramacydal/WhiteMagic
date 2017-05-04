using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public uint HighPart;
    }
}
