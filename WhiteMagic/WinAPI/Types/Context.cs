using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI
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


    public enum CONTEXT_FLAGS_x64 : uint
    {
        CONTEXT_AMD64 = 0x00100000,
        CONTEXT_CONTROL = (CONTEXT_AMD64 | 0x00000001),
        CONTEXT_INTEGER = (CONTEXT_AMD64 | 0x00000002),
        CONTEXT_SEGMENTS = (CONTEXT_AMD64 | 0x00000004),
        CONTEXT_FLOATING_POINT = (CONTEXT_AMD64 | 0x00000008),
        CONTEXT_DEBUG_REGISTERS = (CONTEXT_AMD64 | 0x00000010),
        CONTEXT_FULL = (CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_FLOATING_POINT),
        CONTEXT_ALL = (CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS | CONTEXT_FLOATING_POINT | CONTEXT_DEBUG_REGISTERS),
        CONTEXT_XSTATE = (CONTEXT_AMD64 | 0x00000040),
        CONTEXT_EXCEPTION_ACTIVE = 0x08000000,
        CONTEXT_SERVICE_ACTIVE = 0x10000000,
        CONTEXT_EXCEPTION_REQUEST = 0x40000000,
        CONTEXT_EXCEPTION_REPORTING = 0x80000000,
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
    public struct M128A
    {
        public ulong Low;
        public ulong High;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FLOATING_SAVE_AREA_x64
    {
        public short ControlWord;
        public short StatusWord;
        public byte TagWord;
        public byte Reserved1;
        public short ErrorOpcode;
        public uint ErrorOffset;
        public short ErrorSelector;
        public short Reserved2;
        public uint DataOffset;
        public short DataSelector;
        public short Reserved3;
        public uint MxCsr;
        public uint MxCsr_Mask;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public M128A[] FloatRegisters;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public M128A[] XmmRegisters;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
        public byte[] Reserved4;

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

        public int GetFreeBreakpointSlot()
        {
            for (var index = 0; index < Kernel32.MaxHardwareBreakpoints; ++index)
                if ((Dr7 & (1 << (index * 2))) == 0)
                    return index;

            return -1;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONTEXT_x64
    {
        //
        // Register parameter home addresses.
        //
        // N.B. These fields are for convience - they could be used to extend the
        //      context record in the future.
        //

        public ulong P1Home;
        public ulong P2Home;
        public ulong P3Home;
        public ulong P4Home;
        public ulong P5Home;
        public ulong P6Home;

        //
        // Control flags.
        //

        public uint ContextFlags;
        public uint MxCsr;

        //
        // Segment Registers and processor flags.
        //

        public short SegCs;
        public short SegDs;
        public short SegEs;
        public short SegFs;
        public short SegGs;
        public short SegSs;
        public uint EFlags;

        //
        // Debug registers
        //

        public ulong Dr0;
        public ulong Dr1;
        public ulong Dr2;
        public ulong Dr3;
        public ulong Dr6;
        public ulong Dr7;

        //
        // Integer registers.
        //

        public ulong Rax;
        public ulong Rcx;
        public ulong Rdx;
        public ulong Rbx;
        public ulong Rsp;
        public ulong Rbp;
        public ulong Rsi;
        public ulong Rdi;
        public ulong R8;
        public ulong R9;
        public ulong R10;
        public ulong R11;
        public ulong R12;
        public ulong R13;
        public ulong R14;
        public ulong R15;

        //
        // Program counter.
        //

        public ulong Rip;

        //
        // Floating point state.
        //

        public FLOATING_SAVE_AREA_x64 FltSave;

        //
        // Vector registers.
        //

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
        public M128A[] VectorRegister;
        public ulong VectorControl;

        //
        // Special debug control registers.
        //

        public ulong DebugControl;
        public ulong LastBranchToRip;
        public ulong LastBranchFromRip;
        public ulong LastExceptionToRip;
        public ulong LastExceptionFromRip;

        public int GetFreeBreakpointSlot()
        {
            for (var index = 0; index < Kernel32.MaxHardwareBreakpoints; ++index)
                if ((Dr7 & (1uL << (index * 2))) == 0)
                    return index;

            return -1;
        }
    }
}
