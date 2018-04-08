using System;
using System.Collections.Generic;
using NicheNameJacker.Schema;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NicheNameJacker.ViewModels;

namespace NicheNameJacker.Parser
{
    public static class TumblrParser
    {
        public static IEnumerable<TumblrSearchResult> ParseResults(string input, string query, ParserFunction parser
            )
        {
            if (input == null)
            {
                yield break;
            }

            // Copyright

            string unEscaped = "";
            //Had to add this because in some cases there are invalid characters inside the input and it crashes the program while unescaping 
            //e.g input keyword : affiliate network
            if (!TryUnEscapeSequence(input, out unEscaped))
            {
                yield break;
            }
            Regex regex = new Regex(@"(?x)\<article[^\>]+?\>.*?\</article\>", RegexOptions.Singleline);
            MatchCollection matches = regex.Matches(unEscaped);

            foreach (Match match in matches)
            {
                string id = null;
                string title = null;
                string description = null;
                string sourceAddress = null;

                // Copyright
                Match mobj_title = Regex.Match(match.Value, @"\<div\s+class=""post_title""[^\>]*?\>(?<title>.*?)\<\/div\>");
                if (mobj_title.Success)
                {
                    title = mobj_title.Groups["title"].Value;
                }

                Match mobj_body = Regex.Match(match.Value, @"\<div\s+class=""post_content_inner""[^\>]*?\>(?<body>.*?)\<\/section\>", RegexOptions.Singleline);
                if (mobj_body.Success)
                {
                    description = mobj_body.Groups["body"].Value;
                }

                Match mobj_data = Regex.Match(match.Value, @"data-json='(\{[^']+\})'");
                if (mobj_data.Success)
                {
                    try
                    {
                        JObject jObject = JObject.Parse(mobj_data.Groups[1].Value);
                        JToken jToken = jObject.Value<JToken>("tumblelog-data");

                        if (title == null)
                        {
                            title = jToken.Value<string>("title");
                        }

                        if (description == null)
                        {
                            description = jToken.Value<string>("description");
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

                Match mobj_id = Regex.Match(match.Value, @"""\s*post_id\s*""\s*:\s*""\s*(.*?)\s*""");
                if (mobj_id.Success)
                {
                    if (id == null)
                    {
                        id = mobj_id.Groups[1].Value;
                    }
                }

                Match mobj_address = Regex.Match(match.Value, @"""\s*post_url\s*""\s*:\s*""\s*(.*?)\s*""");
                if (mobj_address.Success)
                {
                    if (sourceAddress == null)
                    {
                        sourceAddress = Regex.Unescape(mobj_address.Groups[1].Value);
                    }
                }

                TumblrSearchResult result = new TumblrSearchResult();
                result.Id = id;
                result.Title = title;
                result.Source = "Tumblr";
                result.SourceAddress = sourceAddress;
                result.Domains = Parse(description, query, parser); 
                yield return result;
            }
        }
        public static bool TryUnEscapeSequence(string input, out string unEscapedOutput)
        {
            unEscapedOutput = string.Empty;
            try
            {
                unEscapedOutput = Regex.Unescape(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string ParseFormKey(string input)
        {
            try
            {
                Regex regex = new Regex(@"\<input type=""hidden"" name=""form_key"" value=""(?<key>[^""]+?)""\/\>");
                Match match = regex.Match(input);
                if (match.Success)
                {
                    return match.Groups["key"].Value;
                }
            }
            catch (Exception)
            {

            }

            return null;
        }

        public static IEnumerable<SingleDomain> Parse(string input, string query, ParserFunction parser
            )
        {
            List<SingleDomain> domains = new List<SingleDomain>();
            try
            {
                var re = @"http:\/\/t\.umblr\.com\/redirect\?z=([^""]+?)""";
                Regex regex = new Regex(re, RegexOptions.Singleline);
                MatchCollection matchCollection = regex.Matches(input);

                string tumblrDomains = string.Empty;
                foreach (Match item in matchCollection)
                {
                    tumblrDomains += (@"""" + item.Groups[1].Value);
                }

                //var domains1 = DomainParser.Parse(input, "Tumblr");
                //var domains2 = DomainParser.Parse(Uri.UnescapeDataString(tumblrDomains), "Tumblr");

                var domains1 = parser.Invoke(input, "Tumblr", query);
                var domains2 = parser.Invoke(Uri.UnescapeDataString(tumblrDomains), "Tumblr", query);
                
                domains.AddRange(domains1);
                domains.AddRange(domains2);
            }
            catch (Exception)
            {

            }

            return domains;
        }

        internal static TumblrSearchModel ParseSearchModel(string response)
        {
            try
            {
                TumblrSearchModel model = new TumblrSearchModel();
                var re = @"(?x)var\s*model\s*=\s*new\s*Tumblr.SearchResultsModel\s*\(\s*\{.*?\}\s*\);";
                Match match = Regex.Match(response, re, RegexOptions.Singleline);
                if (match.Success)
                {
                    model.ad_placement_id = Regex.Match(match.Value, @"ad_placement_id\s*\s*:.*?([A-Za-z0-9]+)").Groups[1].Value;
                    model.before = Regex.Match(match.Value, @"before\s*:.*?([A-Za-z0-9]+)").Groups[1].Value;
                    model.blogs_before = Regex.Match(match.Value, @"blogs_before\s*:.*?([A-Za-z0-9]+)").Groups[1].Value;
                    model.filter_post_type = Regex.Match(match.Value, @"filter_post_type\s*:.*?([A-Za-z0-9]+)").Groups[1].Value;
                    model.has_pages = Regex.Match(match.Value, @"has_pages\s*:.*?([A-Za-z0-9]+)").Groups[1].Value;
                    model.next_ad_offset = Regex.Match(match.Value, @"next_ad_offset\s*:.*?([A-Za-z0-9]+)").Groups[1].Value;
                    model.num_posts_shown = Regex.Match(match.Value, @"num_posts_shown\s*:.*?([A-Za-z0-9]+)").Groups[1].Value;
                    model.post_mode = Regex.Match(match.Value, @"post_mode\s*:.*?([A-Za-z0-9]+)").Groups[1].Value;
                    model.post_view = Regex.Match(match.Value, @"post_view\s*:.*?([A-Za-z0-9]+)").Groups[1].Value;
                    model.query = Regex.Match(match.Value, @"query\s*:.*?([A-Za-z0-9]+)").Groups[1].Value;
                    model.results = Regex.Match(match.Value, @"results\s*:.*?([A-Za-z0-9]+)").Groups[1].Value;
                }

                return model;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class TumblrSearchModel
    {
        public TumblrSearchModel()
        {

        }

        public string query { get; set; }
        public string blogs_before { get; set; }
        public string next_ad_offset { get; set; }
        public string ad_placement_id { get; set; }
        public string results { get; set; }
        public string has_pages { get; set; }
        public string before { get; set; }
        public string num_posts_shown { get; set; }
        public string filter_post_type { get; set; }
        public string post_mode { get; set; }
        public string post_view { get; set; }
        public string post_page { get; set; } = "1";
    }
}
