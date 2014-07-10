using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Win32HWBP
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public UInt32 LowPart;
        public Int32 HighPart;
    }

    public enum DebugEventType : uint
    {
        EXCEPTION_DEBUG_EVENT = 1,
        CREATE_THREAD_DEBUG_EVENT = 2,
        CREATE_PROCESS_DEBUG_EVENT = 3,
        EXIT_THREAD_DEBUG_EVENT = 4,
        EXIT_PROCESS_DEBUG_EVENT = 5,
        LOAD_DLL_DEBUG_EVENT = 6,
        UNLOAD_DLL_DEBUG_EVENT = 7,
        OUTPUT_DEBUG_STRING_EVENT = 8,
        RIP_EVENT = 9
    }

    public enum ExceptonStatus : uint
    {
        STATUS_WAIT_0 = 0x00000000,
        STATUS_ABANDONED_WAIT_0 = 0x00000080,
        STATUS_USER_APC = 0x000000C0,
        STATUS_TIMEOUT = 0x00000102,
        STATUS_SEGMENT_NOTIFICATION = 0x40000005,
        STATUS_GUARD_PAGE_VIOLATION = 0x80000001,
        STATUS_DATATYPE_MISALIGNMENT = 0x80000002,
        STATUS_BREAKPOINT = 0x80000003,
        STATUS_SINGLE_STEP = 0x80000004,
        STATUS_ACCESS_VIOLATION = 0xC0000005,
        STATUS_IN_PAGE_ERROR = 0xC0000006,
        STATUS_INVALID_HANDLE = 0xC0000008,
        STATUS_NO_MEMORY = 0xC0000017,
        STATUS_ILLEGAL_INSTRUCTION = 0xC000001D,
        STATUS_NONCONTINUABLE_EXCEPTION = 0xC0000025,
        STATUS_INVALID_DISPOSITION = 0xC0000026,
        STATUS_ARRAY_BOUNDS_EXCEEDED = 0xC000008C,
        STATUS_FLOAT_DENORMAL_OPERAND = 0xC000008D,
        STATUS_FLOAT_DIVIDE_BY_ZERO = 0xC000008E,
        STATUS_FLOAT_INEXACT_RESULT = 0xC000008F,
        STATUS_FLOAT_INVALID_OPERATION = 0xC0000090,
        STATUS_FLOAT_OVERFLOW = 0xC0000091,
        STATUS_FLOAT_STACK_CHECK = 0xC0000092,
        STATUS_FLOAT_UNDERFLOW = 0xC0000093,
        STATUS_INTEGER_DIVIDE_BY_ZERO = 0xC0000094,
        STATUS_INTEGER_OVERFLOW = 0xC0000095,
        STATUS_PRIVILEGED_INSTRUCTION = 0xC0000096,
        STATUS_STACK_OVERFLOW = 0xC00000FD,
        STATUS_CONTROL_C_EXIT = 0xC000013A,
        STATUS_FLOAT_MULTIPLE_FAULTS = 0xC00002B4,
        STATUS_FLOAT_MULTIPLE_TRAPS = 0xC00002B5,
        STATUS_ILLEGAL_VLM_REFERENCE = 0xC00002C0,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EXCEPTION_DEBUG_INFO
    {
        public EXCEPTION_RECORD ExceptionRecord;
        public uint dwFirstChance;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EXCEPTION_RECORD
    {
        public uint ExceptionCode;
        public uint ExceptionFlags;
        public IntPtr ExceptionRecord;
        public IntPtr ExceptionAddress;
        public uint NumberParameters;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15, ArraySubType = UnmanagedType.U4)]
        public uint[] ExceptionInformation;
    }

    public delegate uint PTHREAD_START_ROUTINE(IntPtr lpThreadParameter);

    [StructLayout(LayoutKind.Sequential)]
    public struct CREATE_THREAD_DEBUG_INFO
    {
        public IntPtr hThread;
        public IntPtr lpThreadLocalBase;
        public PTHREAD_START_ROUTINE lpStartAddress;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CREATE_PROCESS_DEBUG_INFO
    {
        public IntPtr hFile;
        public IntPtr hProcess;
        public IntPtr hThread;
        public IntPtr lpBaseOfImage;
        public uint dwDebugInfoFileOffset;
        public uint nDebugInfoSize;
        public IntPtr lpThreadLocalBase;
        public PTHREAD_START_ROUTINE lpStartAddress;
        public IntPtr lpImageName;
        public ushort fUnicode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EXIT_THREAD_DEBUG_INFO
    {
        public uint dwExitCode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EXIT_PROCESS_DEBUG_INFO
    {
        public uint dwExitCode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LOAD_DLL_DEBUG_INFO
    {
        public IntPtr hFile;
        public IntPtr lpBaseOfDll;
        public uint dwDebugInfoFileOffset;
        public uint nDebugInfoSize;
        public IntPtr lpImageName;
        public ushort fUnicode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UNLOAD_DLL_DEBUG_INFO
    {
        public IntPtr lpBaseOfDll;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OUTPUT_DEBUG_STRING_INFO
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpDebugStringData;
        public ushort fUnicode;
        public ushort nDebugStringLength;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RIP_INFO
    {
        public uint dwError;
        public uint dwType;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_EVENT
    {
        public DebugEventType dwDebugEventCode;
        public int dwProcessId;
        public int dwThreadId;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 86, ArraySubType = UnmanagedType.U1)]
        byte[] debugInfo;

        public EXCEPTION_DEBUG_INFO Exception
        {
            get { return GetDebugInfo<EXCEPTION_DEBUG_INFO>(); }
        }

        public CREATE_THREAD_DEBUG_INFO CreateThread
        {
            get { return GetDebugInfo<CREATE_THREAD_DEBUG_INFO>(); }
        }

        public CREATE_PROCESS_DEBUG_INFO CreateProcessInfo
        {
            get { return GetDebugInfo<CREATE_PROCESS_DEBUG_INFO>(); }
        }

        public EXIT_THREAD_DEBUG_INFO ExitThread
        {
            get { return GetDebugInfo<EXIT_THREAD_DEBUG_INFO>(); }
        }

        public EXIT_PROCESS_DEBUG_INFO ExitProcess
        {
            get { return GetDebugInfo<EXIT_PROCESS_DEBUG_INFO>(); }
        }

        public LOAD_DLL_DEBUG_INFO LoadDll
        {
            get { return GetDebugInfo<LOAD_DLL_DEBUG_INFO>(); }
        }

        public UNLOAD_DLL_DEBUG_INFO UnloadDll
        {
            get { return GetDebugInfo<UNLOAD_DLL_DEBUG_INFO>(); }
        }

        public OUTPUT_DEBUG_STRING_INFO DebugString
        {
            get { return GetDebugInfo<OUTPUT_DEBUG_STRING_INFO>(); }
        }

        public RIP_INFO RipInfo
        {
            get { return GetDebugInfo<RIP_INFO>(); }
        }

        private T GetDebugInfo<T>() where T : struct
        {
            var structSize = Marshal.SizeOf(typeof(T));
            var pointer = Marshal.AllocHGlobal(structSize);
            Marshal.Copy(debugInfo, 0, pointer, structSize);

            var result = Marshal.PtrToStructure(pointer, typeof(T));
            Marshal.FreeHGlobal(pointer);
            return (T)result;
        }
    }

    public enum DebugContinueStatus : uint
    {
        DBG_CONTINUE = 0x00010002,
        DBG_EXCEPTION_NOT_HANDLED = 0x80010001,
    }

    public static partial class WinApi
    {
        public enum TokenObject : uint
        {
            TOKEN_QUERY = 0x0008,
            TOKEN_QUERY_SOURCE = 0x0010,
            TOKEN_ADJUST_PRIVILEGES = 0x0020,
        }

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

        public static string SE_DEBUG_NAME = "SeDebugPrivilege";

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle,
            TokenObject DesiredAccess, out IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName,
            out LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
           [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
           ref TOKEN_PRIVILEGES NewState,
           UInt32 Zero,
           IntPtr Null1,
           IntPtr Null2);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern uint GetProcAddress(uint hModule, string procName);

        [DllImport("kernel32", SetLastError = true, EntryPoint = "GetProcAddress")]
        public static extern uint GetProcAddressOrdinal(uint hModule, uint procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DebugActiveProcess(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DebugActiveProcessStop(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DebugSetProcessKillOnExit(bool KillOnExit);

        [DllImport("kernel32.dll")]
        public static extern bool ContinueDebugEvent(int dwProcessId, int dwThreadId, uint dwContinueStatus);

        public static bool SetDebugPrivileges()
        {
            IntPtr hToken;
            LUID luidSEDebugNameValue;
            TOKEN_PRIVILEGES tkpPrivileges;

            if (!OpenProcessToken(GetCurrentProcess(), TokenObject.TOKEN_ADJUST_PRIVILEGES | TokenObject.TOKEN_QUERY, out hToken))
                return false;

            if (!LookupPrivilegeValue(null, SE_DEBUG_NAME, out luidSEDebugNameValue))
            {
                WinApi.CloseHandle(hToken);
                return false;
            }

            tkpPrivileges.PrivilegeCount = 1;
            tkpPrivileges.Luid = luidSEDebugNameValue;
            tkpPrivileges.Attributes = WinApi.PrivilegeAttributes.SE_PRIVILEGE_ENABLED;

            if (!AdjustTokenPrivileges(hToken, false, ref tkpPrivileges, 0, IntPtr.Zero, IntPtr.Zero))
            {
                CloseHandle(hToken);
                return false;
            }

            CloseHandle(hToken);
            return true;
        }
    }
}
