namespace WhiteMagic.Breakpoints
{
    public class BreakPointException : MagicException
    {
        public BreakPointException(string Message, params object[] Arguments) : base(Message, Arguments) { }
    }
}
