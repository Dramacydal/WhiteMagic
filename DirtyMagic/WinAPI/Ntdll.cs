using System;
using System.Runtime.InteropServices;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.WinAPI
{
    public static class Ntdll
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtQueryInformationThread(
            IntPtr threadHandle,
            ThreadInfoClass threadInformationClass,
            byte[] threadInformation,
            int threadInformationLength,
            IntPtr returnLengthPtr);
    }
}
