using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TimeTracker
{
    static class Extensions
    {
        public static string MakeReadable(this string s)
        {
            return new string(MakeReadableInternal(Regex.Replace(s, @"\s+", "").Trim()).ToArray());
        }

        public static IEnumerable<char> MakeReadableInternal(string s)
        {
            char? last = null;
            foreach (var c in s)
            {
                if (last.HasValue && !c.IsSameKind(last.Value) && char.IsLower(last.Value))
                    yield return ' ';
                yield return c;
                last = c;
            }
        }

        public static bool IsSameKind(this char c1, char c2)
        {
            return char.IsUpper(c1) == char.IsUpper(c2)
                   && char.IsDigit(c1) == char.IsDigit(c2);
        }

        public static string RemoveWhitespace(this string s)
        {
            return Regex.Replace(s, @"\s+", "");
        }
    }
}