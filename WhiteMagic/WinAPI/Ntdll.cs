using System;
using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI
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
