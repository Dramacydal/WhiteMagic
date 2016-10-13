using System.Runtime.InteropServices;

namespace WhiteMagic.WinAPI.Structures
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
    public class CONTEXT
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
}
