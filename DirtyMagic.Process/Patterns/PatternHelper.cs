using System.Linq;
using System.Text;

namespace DirtyMagic.Patterns
{
    public static class PatternHelper
    {
        private static readonly Encoding ConversionEncoding = Encoding.GetEncoding("iso-8859-1"); // to encode all chars in byte range

        public static string BytesToString(byte[] data) => ConversionEncoding.GetString(data, 0, data.Length);

        public static string ToBinaryRegex(string source) => ToBinaryRegex(ConversionEncoding.GetBytes(source));

        public static string ToBinaryRegex(byte[] source) => string.Concat(source.Select(_ => @"\x" + string.Format((string) "{0:X2}", (object) _)));
    }
}
