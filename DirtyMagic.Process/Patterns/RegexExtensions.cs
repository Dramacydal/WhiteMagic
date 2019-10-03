using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DirtyMagic.Patterns
{
    public static class RegexExtensions
    {
        public static Match Match(this IEnumerable<byte> Data, Regex Pattern)
            => Pattern.Match(PatternHelper.BytesToString(Data.ToArray()));

        public static MatchCollection Matches(this IEnumerable<byte> Data, Regex Pattern)
            => Pattern.Matches(PatternHelper.BytesToString(Data.ToArray()));

        public static bool IsMatch(this IEnumerable<byte> Data, Regex Pattern)
            => Pattern.IsMatch(PatternHelper.BytesToString(Data.ToArray()));
    }
}
