using System;
using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI.Structures
{
    public enum PrivilegeAttributes : uint
    {
        SE_PRIVILEGE_ENABLED = 0x00000002,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TOKEN_PRIVILEGES
    {
        public UInt32 PrivilegeCount;
        public LUID Luid;
        public PrivilegeAttributes Attributes;
    }
}
