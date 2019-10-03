using System;
using System.Runtime.InteropServices;

namespace DirtyMagic.WinAPI.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemInfo
    {
        public ProcessorArchitecture ProcessorArchitecture; // WORD
        public uint PageSize; // DWORD
        public IntPtr MinimumApplicationAddress; // (long)void*
        public IntPtr MaximumApplicationAddress; // (long)void*
        public IntPtr ActiveProcessorMask; // DWORD*
        public uint NumberOfProcessors; // DWORD (WTF)
        public uint ProcessorType; // DWORD
        public uint AllocationGranularity; // DWORD
        public ushort ProcessorLevel; // WORD
        public ushort ProcessorRevision; // WORD
    }
}
