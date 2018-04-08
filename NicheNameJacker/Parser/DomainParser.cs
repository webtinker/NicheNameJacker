using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NicheNameJacker.ViewModels;
using System;
using NicheNameJacker.Common;

namespace NicheNameJacker.Parser
{
    public delegate IEnumerable<SingleDomain> ParserFunction(string input, string source, string keyword );

    public static class DomainParser
    {
        private static IReadOnlyList<string> _blockedDomains = new List<string>()
        {
            "vk.com",
            "facebook.com",
            "twitter.com",
            "youtube.com",
            "google.com",
            "goo.gl",
            "bit.ly",
            "adf.ly",
            "youtu.be",
            "instagram.com",
            "plus.google.com",
            "play.google.com",
            "itunes.apple.com"
        };

        public static IReadOnlyList<string> SubdomainPostfixes = new List<string>
        {
            ".livejournal.com",
            ".tumblr.com",
            ".weebly.com",
            ".overblog.com",
            ".webnode.com",
            ".jigsy.com",
            ".beep.com",
            ".webgarden.com",
            ".spruz.com",
            ".wallinside.com",
            ".snack.ws",
            ".wikidot.com",
            ".webstarts.com"
        };

        //public static IEnumerable<SingleDomain> Parse(string input, string source)
        //{
        //    //(? (?= regex)then |else). 
        //    // Copyright Shubham Sharma ***RE***
        //    var re = @"(?x)(?:https?://|//)                                    # http(s):// or protocol-independent URL
        //                   (?<WWW>[^\.]+?\.)?
        //                   (?<FDN>[A-Za-z0-9-]+?)
        //                   \. 
        //                   (?<TLD>[A-Za-z0-9-]+?)
        //                   (?:/|\s+|$|"")                                         # match any domain";

        //    var domains = new List<SingleDomain>();
        //    var regex = new Regex(re);
        //    var matchCollection = regex.Matches(input);

        //    foreach (Match match in matchCollection)
        //    {
        //        var domain = match.Groups["FDN"].Value;
        //        var topLevelDomain = match.Groups["TLD"].Value;

        //        if (topLevelDomain == "gov" || topLevelDomain == "edu" || topLevelDomain == "mil")
        //        {
        //            continue;
        //        }

        //        var address = (domain + "." + topLevelDomain).Trim();
        //        var isBlockedDomain = _blockedDomains.Any(x => address.Contains(x));
        //        var isDuplicateDomain = domains.Any(x => address.Contains(x.Address));

        //        if (isBlockedDomain || isDuplicateDomain)
        //        {
        //            continue;
        //        }

        //        var singleDomain = new SingleDomain
        //        {
        //            DomainName = domain,
        //            TopLevelDomainName = topLevelDomain,
        //            Address = address,
        //            DomainSource = source
        //        };
        //        domains.Add(singleDomain);
        //    }
        //    return domains;
        //}

        static IEnumerable<Match> GetMatches(string input)
        {
            var pattern = @"(http|https)://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?";
            var regex = new Regex(pattern);
            return regex.Matches(input).OfType<Match>();
        }

        static Uri CreateUri(string possibleUri)
        {
            Uri uri;
            if (Uri.TryCreate(possibleUri, UriKind.Absolute, out uri) && uri.IsDefaultPort)
            {
                return uri;
            }
            return null;
        }

        static IEnumerable<Uri> GetUris(string input) => GetMatches(input).Select(m => CreateUri(m.Value)).Where(u => u != null);

        public static List<SingleDomain> Parse(string input, string source, string keyword  )
        {
            var matches = GetMatches(input);
            var domains = new List<SingleDomain>();
            foreach (var match in matches)
            {
                Uri uri;
                if (Uri.TryCreate(match.Value, UriKind.Absolute, out uri) && uri.IsDefaultPort)
                {
                    var address = uri.Host.Replace("www.", "");
                    var addressParts = address.Split('.');

                    if (addressParts.Length > 2) continue; //no subdomains

                    var domain = addressParts.First();
                    var topLevelDomain = addressParts.Last();
                    if (topLevelDomain == "gov" || topLevelDomain == "edu" || topLevelDomain == "mil") continue;

                    var isBlockedDomain = _blockedDomains.Any(x => address.Contains(x));
                    var isDuplicateDomain = domains.Any(x => address.Contains(x.Address));

                    if (isBlockedDomain || isDuplicateDomain) continue;

                    var singleDomain = new SingleDomain
                    {
                        DomainName = domain,
                        Keyword = keyword,
                        TopLevelDomainName = topLevelDomain,
                        Address = address,
                        FullAddress = uri.OriginalString,
                        DomainSource = source,
                    };
                    domains.Add(singleDomain);
                }
            }
            return domains;
        }

        public static List<SingleDomain> ParseSubdomain(string input, string source, string keyword )
        {
            var domains = new List<SingleDomain>();
            foreach (var uri in GetUris(input).Where(u => SubdomainPostfixes.Any(s => u.Host.Contains(s))))
            {
                var address = uri.Host.Replace("www.", "");
                if (domains.Any(x => address.Contains(x.Address))) continue;

                var addressParts = address.Split('.');
                var domain = addressParts.Take(addressParts.Length - 1).JoinStrings(".");
                var topLevelDomain = addressParts.Last();

                var singleDomain = new SingleDomain
                {
                    DomainName = domain,
                    Keyword = keyword,
                    TopLevelDomainName = topLevelDomain,
                    Address = address,
                    FullAddress = uri.OriginalString,
                    DomainSource = source
                };
                domains.Add(singleDomain);
            }
            return domains;
        }

        public static List<SingleDomain> ParseSubdomainUnfiltered(string input, string source, string keyword)
        {
            var domains = new List<SingleDomain>();
            foreach (var uri in GetUris(input))
            {
                var address = uri.Host.Replace("www.", "");
                if (domains.Any(x => address.Contains(x.Address))) continue;

                var addressParts = address.Split('.');
                var domain = addressParts.Take(addressParts.Length - 1).JoinStrings(".");
                var topLevelDomain = addressParts.Last();

                var singleDomain = new SingleDomain
                {
                    DomainName = domain,
                    Keyword = keyword,
                    TopLevelDomainName = topLevelDomain,
                    Address = address,
                    FullAddress = uri.OriginalString,
                    DomainSource = source
                };
                domains.Add(singleDomain);
            }
            return domains;
        }
    }
}
