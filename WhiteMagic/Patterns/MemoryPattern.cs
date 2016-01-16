using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace WhiteMagic.Patterns
{
    public class MemoryPattern : Regex
    {
        protected MemoryPattern(string Pattern, RegexOptions Options = RegexOptions.None)
            : base(Pattern, Options)
        {
        }

        public static MemoryPattern FromRegex(string Pattern, RegexOptions Options = RegexOptions.None)
        {
            return new MemoryPattern(Pattern, Options);
        }

        public static MemoryPattern FromBinary(string Pattern)
        {
            return FromBinary(
                Pattern.Split(new char[] { '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(_ =>
                    {
                        if (_.Contains('?'))
                            return (byte)0x90;

                        return Convert.ToByte(_, 16);
                    }).ToArray());
        }

        public static MemoryPattern FromBinary(byte[] Pattern)
        {
            return new MemoryPattern(string.Concat(Pattern.Select(_ => _ == 0x90 ? "." : @"\x" + string.Format("{0:X2}", _))));
        }
    }
}
