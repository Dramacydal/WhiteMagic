using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DirtyMagic.Patterns
{
    public static class RegexExtensions
    {
        public static Match Match(this IEnumerable<byte> data, Regex pattern)
            => pattern.Match(PatternHelper.BytesToString(data.ToArray()));

        public static MatchCollection Matches(this IEnumerable<byte> data, Regex pattern)
            => pattern.Matches(PatternHelper.BytesToString(data.ToArray()));

        public static bool IsMatch(this IEnumerable<byte> data, Regex pattern)
            => pattern.IsMatch(PatternHelper.BytesToString(data.ToArray()));
    }
}
