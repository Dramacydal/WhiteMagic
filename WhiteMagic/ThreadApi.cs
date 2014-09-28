using System;
using System.Runtime.InteropServices;

namespace WhiteMagic
{
    public enum CONTEXT_FLAGS : uint
    {
        CONTEXT_i386 = 0x10000,
        CONTEXT_i486 = 0x10000,   //  same as i386
        CONTEXT_CONTROL = CONTEXT_i386 | 0x01, // SS:SP, CS:IP, FLAGS, BP
        CONTEXT_INTEGER = CONTEXT_i386 | 0x02, // AX, BX, CX, DX, SI, DI
        CONTEXT_SEGMENTS = CONTEXT_i386 | 0x04, // DS, ES, FS, GS
        CONTEXT_FLOATING_POINT = CONTEXT_i386 | 0x08, // 387 state
        CONTEXT_DEBUG_REGISTERS = CONTEXT_i386 | 0x10, // DB 0-3,6,7
        CONTEXT_EXTENDED_REGISTERS = CONTEXT_i386 | 0x20, // cpu specific extensions
        CONTEXT_FULL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS,
        CONTEXT_ALL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS | CONTEXT_FLOATING_POINT | CONTEXT_DEBUG_REGISTERS | CONTEXT_EXTENDED_REGISTERS
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FLOATING_SAVE_AREA
    {
         public uint ControlWord;
         public uint StatusWord;
         public uint TagWord;
         public uint ErrorOffset;
         public uint ErrorSelector;
         public uint DataOffset;
         public uint DataSelector;
         [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
         public byte[] RegisterArea;
         public uint Cr0NpxState;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct CONTEXT
    {
        // Eax parts
        public ushort Ax
        {
            get { return (ushort)(Eax & 0xFFFF); }
            set { Eax = Eax & 0xFFFF0000 | value; }
        }
        public byte Al
        {
            get { return (byte)(Ax & 0xFF); }
            set { Ax = (ushort)(Ax & 0xFF00 | value); }
        }
        public byte Ah
        {
            get { return (byte)(Ax >> 16); }
            set { Ax = (ushort)(Ax & 0xFF | (value << 16)); }
        }

        // Ecx parts
        public ushort Cx
        {
            get { return (ushort)(Ecx & 0xFFFF); }
            set { Ecx = Ecx & 0xFFFF0000 | value; }
        }
        public byte Cl
        {
            get { return (byte)(Cx & 0xFF); }
            set { Cx = (ushort)(Cx & 0xFF00 | value); }
        }
        public byte Ch
        {
            get { return (byte)(Cx >> 16); }
            set { Cx = (ushort)(Cx & 0xFF | (value << 16)); }
        }

        // Edx parts
        public ushort Dx
        {
            get { return (ushort)(Edx & 0xFFFF); }
            set { Edx = Edx & 0xFFFF0000 | value; }
        }
        public byte Dl
        {
            get { return (byte)(Dx & 0xFF); }
            set { Dx = (ushort)(Dx & 0xFF00 | value); }
        }
        public byte Dh
        {
            get { return (byte)(Dx >> 16); }
            set { Dx = (ushort)(Dx & 0xFF | (value << 16)); }
        }

        // Ebx parts
        public ushort Bx
        {
            get { return (ushort)(Ebx & 0xFFFF); }
            set { Ebx = Ebx & 0xFFFF0000 | value; }
        }
        public byte Bl
        {
            get { return (byte)(Bx & 0xFF); }
            set { Bx = (ushort)(Bx & 0xFF00 | value); }
        }
        public byte Bh
        {
            get { return (byte)(Bx >> 16); }
            set { Bx = (ushort)(Bx & 0xFF | (value << 16)); }
        }

        // Esp parts
        public ushort Sp
        {
            get { return (ushort)(Esp & 0xFFFF); }
            set { Esp = Esp & 0xFFFF0000 | value; }
        }

        // Ebp parts
        public ushort Bp
        {
            get { return (ushort)(Ebp & 0xFFFF); }
            set { Ebp = Ebp & 0xFFFF0000 | value; }
        }

        // Esi parts
        public ushort Si
        {
            get { return (ushort)(Esi & 0xFFFF); }
            set { Esi = Esi & 0xFFFF0000 | value; }
        }

        // Edi parts
        public ushort Di
        {
            get { return (ushort)(Edi & 0xFFFF); }
            set { Edi = Edi & 0xFFFF0000 | value; }
        }

        public uint ContextFlags; //set this to an appropriate value
        // Retrieved by CONTEXT_DEBUG_REGISTERS
        public uint Dr0;
        public uint Dr1;
        public uint Dr2;
        public uint Dr3;
        public uint Dr6;
        public uint Dr7;
        // Retrieved by CONTEXT_FLOATING_POINT
        public FLOATING_SAVE_AREA FloatSave;
        // Retrieved by CONTEXT_SEGMENTS
        public uint SegGs;
        public uint SegFs;
        public uint SegEs;
        public uint SegDs;
        // Retrieved by CONTEXT_INTEGER
        public uint Edi;
        public uint Esi;
        public uint Ebx;
        public uint Edx;
        public uint Ecx;
        public uint Eax;
        // Retrieved by CONTEXT_CONTROL
        public uint Ebp;
        public uint Eip;
        public uint SegCs;
        public uint EFlags;
        public uint Esp;
        public uint SegSs;
        // Retrieved by CONTEXT_EXTENDED_REGISTERS
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] ExtendedRegisters;
    }

    [Flags]
    public enum ThreadAccess : int
    {
        TERMINATE = (0x0001),
        SUSPEND_RESUME = (0x0002),
        GET_CONTEXT = (0x0008),
        SET_CONTEXT = (0x0010),
        SET_INFORMATION = (0x0020),
        QUERY_INFORMATION = (0x0040),
        SET_THREAD_TOKEN = (0x0080),
        IMPERSONATE = (0x0100),
        DIRECT_IMPERSONATION = (0x0200),
        STANDARD_RIGHTS_REQUIRED = (0x000F0000),
        SYNCHRONIZE = (0x00100000),
        // vista and later
        THREAD_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFFF)
    }

    public enum WaitResult : uint
    {
        WAIT_OBJECT_0 = 0x00000000,
        WAIT_ABANDONED = 0x00000080,
        WAIT_TIMEOUT = 0x00000102,
        WAIT_FAILED = 0xFFFFFFFF,
    }

    public enum ThreadInfoClass : int
    {
        ThreadQuerySetWin32StartAddress = 9
    }

    [Flags]
    public enum WakeFlags : uint
    {
        QS_ALLEVENTS = 0x04BF,
    }

    public static partial class WinApi
    {
        public static uint MAX_BREAKPOINTS = 4;
        public static uint INFINITE = 0xFFFFFFFF;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle,
           int dwThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT lpContext);

        [DllImport("kernel32.dll")]
        public static extern bool SetThreadContext(IntPtr hThread, [In] ref CONTEXT lpContext);

        [DllImport("kernel32.dll")]
        public static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", EntryPoint = "WaitForDebugEvent")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WaitForDebugEvent(ref DEBUG_EVENT lpDebugEvent, uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess,
            IntPtr lpThreadAttributes, uint dwStackSize, uint lpStartAddress,
            IntPtr lpParameter, uint dwCreationFlags, out int lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern WaitResult WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("user32.dll")]
        public static extern WaitResult MsgWaitForMultipleObjects(uint nCount, IntPtr[] pHandles,
            bool bWaitAll, uint dwMilliseconds, WakeFlags dwWakeMask);

        [DllImport("kernel32.dll")]
        public static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtQueryInformationThread(
            IntPtr threadHandle,
            ThreadInfoClass threadInformationClass,
            byte[] threadInformation,
            int threadInformationLength,
            IntPtr returnLengthPtr);
    }
}
