using DirtyMagic.Exceptions;

namespace DirtyMagic
{
    public class DebuggerException : MagicException
    {
        public DebuggerException(string Message) : base(Message) { }
    }
}
