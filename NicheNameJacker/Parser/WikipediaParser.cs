using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NicheNameJacker.Utilities;

namespace NicheNameJacker.Parser
{
    public class WikipediaParser
    {
        public static List<string> ParseBingResults(string response, List<string> usedUrls)
        {
            List<string> links = new List<string>();

            var matches = Regex.Matches(response, @"\<li\s*class=""b_algo""\s*\>\s*(?:\<div\s*class=""b_title""\s*\>)?\<h2\>\<a\s*href=""(?<href>[^""]*)""");
            foreach (var match in matches.OfType<Match>())
            {
                Group group = match.Groups["href"];
                if (group == null)
                    continue;

                links.Add(WebUtility.UrlDecode(group.Value));
            }

            return links.Except(usedUrls).ToList();
        }

        public static string ParseNextPage(string response, string baseUrl)
        {
            try
            {
                var match = Regex.Match(response, @"<a\s*class=""sb_pagN""\s*title=""[^""]*""\s*href=""(?<href>[^""]*)""");
                if (match.Success)
                {
                    var group = match.Groups["href"];
                    if (group == null)
                        return null;

                    return $"{baseUrl}{WebUtility.HtmlDecode(group.Value)}";
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        public static async Task<List<string>> ParseWiki(string url)
        {
            //Random rand = new Random(DateTime.UtcNow.Millisecond);
            //string[] proxies = null;
            //if (File.Exists("fastproxies-new.txt"))
            //{
            //    proxies = File.ReadAllLines("fastproxies-new.txt");
            //}

            //WebProxy proxy = proxies == null
            //    ? null
            //    : new WebProxy(new Uri($"http://{proxies[rand.Next(0, proxies.Length - 1)]}"));

            List<string> links = new List<string>();

            string response = null;
            response = await Downloader.DownloadTextAsync(url, proxy: null, note: "Requesting webpage from Wiki...");
            if (string.IsNullOrEmpty(response))
            {
                return links;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(response);
            try
            {
                var items = doc.DocumentNode.SelectNodes("//a[@title='Wikipedia:Link rot']").ToList();
                foreach (var node in items)
                {
                    var parent = node.ParentNode;
                    while (parent != null && parent.Name != "li")
                    {
                        parent = parent.ParentNode;
                    }

                    if (parent != null)
                    {
                        var link = parent.SelectSingleNode(".//a[contains(@class, 'external')]");
                        if (link != null)
                        {
                            links.Add(link.Attributes["href"].Value);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return links;
        }
    }
}
