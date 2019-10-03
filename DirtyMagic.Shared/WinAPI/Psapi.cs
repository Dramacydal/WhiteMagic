using System;
using System.Runtime.InteropServices;

namespace DirtyMagic.WinAPI
{
    public static class Psapi
    {
        [DllImport("Psapi.dll", SetLastError = true)]
        public static extern bool EnumProcesses(
           [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In][Out] int[] processIds,
             uint arraySizeBytes,
             [MarshalAs(UnmanagedType.U4)] out uint bytesCopied
          );
    }
}
