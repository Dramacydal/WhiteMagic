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
            return new MemoryPattern(
                string.Concat(
                Pattern.Split(new char[] { '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(_ =>
                    {
                        if (_.Contains('?'))
                            return ".";

                        return @"\x" + string.Format("{0:X2}", Convert.ToByte(_, 16));
                    })));
        }

        public static MemoryPattern FromBinary(byte[] Pattern)
        {
            return new MemoryPattern(string.Concat(Pattern.Select(_ =>  @"\x" + string.Format("{0:X2}", _))));
        }
    }
}
