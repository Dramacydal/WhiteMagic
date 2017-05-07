using System;
using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public int bInheritHandle;
    }
}
