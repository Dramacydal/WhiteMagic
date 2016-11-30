using System;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using WhiteMagic.WinAPI;

namespace WhiteMagic
{
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public sealed class SafeMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeMemoryHandle() : base(true)
        {
        }
        public SafeMemoryHandle(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            // Check whether the handle is set AND whether the handle has been successfully closed
            return handle != IntPtr.Zero && Kernel32.CloseHandle(handle);
        }
    }
}