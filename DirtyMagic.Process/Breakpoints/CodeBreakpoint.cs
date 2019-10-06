using DirtyMagic.Pointers;
using DirtyMagic.WinAPI.Structures;

namespace DirtyMagic.Breakpoints
{
    public abstract class CodeBreakpoint : HardwareBreakPoint
    {
        protected CodeBreakpoint(ModulePointer pointer) : base(pointer, BreakpointCondition.Code, 1) { }
    }
}
