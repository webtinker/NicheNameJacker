using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NicheNameJacker.Schema;

namespace NicheNameJacker.Parser
{
    public static class RedditParser
    {
        public static IEnumerable<RedditSearchResult> ParseResults(string input, string query, ParserFunction parser)
        {
            if (input == null)
            {
                yield break;
            }

            // Copyright
            Regex regex = new Regex(@"(?x)
                                      \<div\s+class=""\s+search-result[^""]+?""[^\>]+?\>
                                      .*?
                                      \</div\>.*?\</div\>.*?\</div\>", RegexOptions.Singleline);
            MatchCollection matches = regex.Matches(input);
            foreach (Match match in matches)
            {
                // Copyright
                Match header = Regex.Match(match.Value, @"\<header\s*class=""\s*search-result-header\s*""\s*\>.*?\<a.*?href=""(?<url>[^""]+)"".*?\>(?<header>.*?)\</a\>");

                RedditSearchResult result = new RedditSearchResult();
                result.Title = header.Groups["header"].Value;
                result.Source = "Reddit";
                result.SourceAddress = header.Groups["url"].Value;
                result.Domains = parser.Invoke(match.Value, "Reddit", query);
                yield return result;
            }
        }

        public static string ParseNextPage(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            try
            {
                string re = @"(?x)
                              \<div\s*class=""nav-buttons""\s*\>
                              .*?
                              \<span\s*class=""nextprev""\s*\>
                              .*?
                              \<a.*?href=""(?<next>[^""]+)""\s*rel=""nofollow\snext""\s*\>
                              .*?
                              next
                              .*?
                              \<\/a\>
                              .*?
                              \<\/span\>
                              .*?
                              \<\/div\>";
                Regex regex = new Regex(re, RegexOptions.Singleline, TimeSpan.FromSeconds(2));
                MatchCollection matches = regex.Matches(input);
                foreach (Match item in matches)
                {
                    if (!item.Value.Contains("type=sr"))
                    {
                        return System.Net.WebUtility.HtmlDecode(item.Groups["next"].Value);
                    }
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return null;
            }

            return null;
        }
    }
}
