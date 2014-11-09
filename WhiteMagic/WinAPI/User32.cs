using System;
using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI
{
    public static class User32
    {
        [DllImport("user32.dll")]
        public static extern WaitResult MsgWaitForMultipleObjects(uint nCount, IntPtr[] pHandles,
            bool bWaitAll, uint dwMilliseconds, WakeFlags dwWakeMask);
    }
}
