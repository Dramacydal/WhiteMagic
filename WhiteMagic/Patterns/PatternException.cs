using System;

namespace WhiteMagic.Patterns
{
    public class PatternException : Exception
    {
        public PatternException(string message)
            : base(message) { }
    }
}
