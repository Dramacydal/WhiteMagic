using System;
using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI
{
    public static class Psapi
    {
        [DllImport("Psapi.dll", SetLastError = true)]
        public static extern bool EnumProcesses(
           [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In][Out] Int32[] processIds,
             UInt32 arraySizeBytes,
             [MarshalAs(UnmanagedType.U4)] out UInt32 bytesCopied
          );
    }
}
