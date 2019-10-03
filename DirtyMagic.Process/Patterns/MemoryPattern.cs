using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DirtyMagic.Patterns
{
    public static class PatternHelper
    {
        private static readonly Encoding ConversionEncoding = Encoding.GetEncoding("iso-8859-1"); // to encode all chars in byte range

        public static string BytesToString(byte[] Data) => ConversionEncoding.GetString(Data, 0, Data.Length);

        public static string ToBinaryRegex(string Source) => ToBinaryRegex(ConversionEncoding.GetBytes(Source));

        public static string ToBinaryRegex(byte[] Source) => string.Concat(Source.Select(_ => @"\x" + string.Format("{0:X2}", _)));
    }

    public class MemoryPattern : Regex
    {
        protected MemoryPattern(string Pattern, RegexOptions Options = RegexOptions.None)
            : base(Pattern, Options | RegexOptions.Singleline)
        {
        }

        public static MemoryPattern FromRegex(string Pattern, RegexOptions Options = RegexOptions.None)
            => new MemoryPattern(Pattern, Options);

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

        public static MemoryPattern FromBinary(byte[] Pattern) => new MemoryPattern(PatternHelper.ToBinaryRegex(Pattern));
    }
}
