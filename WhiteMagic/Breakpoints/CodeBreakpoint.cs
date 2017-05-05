using WhiteMagic.Pointers;
using WhiteMagic.WinAPI.Structures;

namespace WhiteMagic.Breakpoints
{
    public class CodeBreakpoint : HardwareBreakPoint
    {
        public CodeBreakpoint(ModulePointer Pointer) : base(Pointer, BreakpointCondition.Code, 1) { }
    }
}
