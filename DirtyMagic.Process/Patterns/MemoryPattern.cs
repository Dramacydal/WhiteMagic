using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DirtyMagic.Patterns
{
    public class MemoryPattern : Regex
    {
        protected MemoryPattern(string pattern, RegexOptions options = RegexOptions.None)
            : base(pattern, options | RegexOptions.Singleline)
        {
        }

        public static MemoryPattern FromRegex(string pattern, RegexOptions options = RegexOptions.None)
            => new MemoryPattern(pattern, options);

        public static MemoryPattern FromBinary(string pattern)
        {
            return new MemoryPattern(
                string.Concat(
                pattern.Split(new char[] { '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(_ =>
                    {
                        if (_.Contains('?'))
                            return ".";

                        return @"\x" + string.Format("{0:X2}", Convert.ToByte(_, 16));
                    })));
        }

        public static MemoryPattern FromBinary(byte[] pattern) => new MemoryPattern(PatternHelper.ToBinaryRegex(pattern));
    }
}
