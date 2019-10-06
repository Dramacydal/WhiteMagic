using System;
using System.Runtime.InteropServices;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.WinAPI
{
    public static class Advapi32
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken(IntPtr processHandle,
            TokenObject DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName,
            out LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AdjustTokenPrivileges(IntPtr tokenHandle,
           [MarshalAs(UnmanagedType.Bool)]bool disableAllPrivileges,
           ref TOKEN_PRIVILEGES newState,
           uint zero,
           IntPtr null1,
           IntPtr null2);
    }
}
