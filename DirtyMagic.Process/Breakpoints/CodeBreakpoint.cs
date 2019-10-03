using DirtyMagic.Pointers;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Breakpoints
{
    public abstract class CodeBreakpoint : HardwareBreakPoint
    {
        public CodeBreakpoint(ModulePointer Pointer) : base(Pointer, BreakpointCondition.Code, 1) { }
    }
}
