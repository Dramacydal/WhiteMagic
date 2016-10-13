using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WhiteMagic.Patterns
{
    public static class RegexExtensions
    {
        public static Match Match(this IEnumerable<byte> Data, MemoryPattern Pattern)
        {
            return Pattern.Match(PatternHelper.BytesToString(Data.ToArray()));
        }

        public static MatchCollection Matches(this IEnumerable<byte> Data, MemoryPattern Pattern)
        {
            return Pattern.Matches(PatternHelper.BytesToString(Data.ToArray()));
        }
    }
}
