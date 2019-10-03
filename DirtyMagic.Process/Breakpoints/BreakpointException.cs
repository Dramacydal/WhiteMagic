using DirtyMagic.Exceptions;

namespace DirtyMagic.Breakpoints
{
    public class BreakPointException : MagicException
    {
        public BreakPointException(string Message) : base(Message) { }
    }
}
