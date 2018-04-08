using NicheNameJacker.Schema;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NicheNameJacker.Parser
{
    public class BBCParser
    {
        public static IEnumerable<BaseSearchResult> ParseSearchResults(string input, string query)
        {
            List<BaseSearchResult> results = new List<BaseSearchResult>();
            if (string.IsNullOrEmpty(input))
            {
                return results;
            }

            // Copyright Shubham Sharma
            Regex regex = new Regex(@"(?x)\<li[^\>]+?\>.*?\<\/li\>", RegexOptions.Singleline);
            MatchCollection matchCollection = regex.Matches(input);
            foreach (Match match in matchCollection)
            {
                // Copyright Shubham Sharma
                Match header = Regex.Match(match.Value, @"\<h1\s+itemprop=""headline"".*?\>.*?\<a[^\>]+\>(?<headline>.*?)\<\/a\>");

                BaseSearchResult result = new BaseSearchResult();
                result.Title = header.Groups["headline"].Value;
                result.Source = "BBC";
                result.Domains = DomainParser.Parse(match.Value, "BBC", query);
                results.Add(result);
            }

            return results;
        }
    }
}
