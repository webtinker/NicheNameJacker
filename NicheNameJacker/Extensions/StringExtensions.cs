using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NicheNameJacker.Extensions
{
    static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static int ToInt(this string source, int def = default(int))
        {
            int res;
            return int.TryParse(source, out res) ? res : def;
        }

        public static string Match(this string source, string pattern, RegexOptions options = RegexOptions.None)
        {
            if (source == null)
                return string.Empty;

            Match match = Regex.Match(source, pattern, options);
            return match.Success ? match.Value : String.Empty;
        }

        public static bool IsMatch(this string source, string pattern, RegexOptions options = RegexOptions.None)
        {
            if (source == null)
                return false;

            Match match = Regex.Match(source, pattern, options);
            return match.Success;
        }

        public static string Match(this string source, string pattern, string groupName, RegexOptions options = RegexOptions.None)
        {
            if (source == null)
                return string.Empty;

            Match match = Regex.Match(source, pattern, options);
            if (!match.Success)
                return String.Empty;

            Group group = match.Groups[groupName];
            if (group == null)
                return String.Empty;

            return group.Value;
        }

        public static List<string> MatchMany(this string source, string pattern, string groupName, RegexOptions options = RegexOptions.None)
        {
            List<string> result = new List<string>();
            if (source == null)
                return result;

            MatchCollection matches = Regex.Matches(source, pattern, options);
            result.AddRange(from match in matches.OfType<Match>().Where(x => x.Success)
                            select match.Groups[groupName] into @group
                            where @group != null
                            select @group.Value);

            return result;
        }
    }
}
