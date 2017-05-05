namespace WhiteMagic.Breakpoints
{
    public class BreakPointException : MagicException
    {
        public BreakPointException(string message, params object[] args) : base(message, args) { }
    }

}
