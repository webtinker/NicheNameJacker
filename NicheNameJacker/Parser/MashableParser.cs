﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NicheNameJacker.Extensions;
using NicheNameJacker.Utilities;

namespace NicheNameJacker.Parser
{
    public class MashableParser
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

        public static async Task<List<string>> ParsePage(string url)
        {
            List<string> links = new List<string>();

            string response = null;
            response = await Downloader.DownloadTextAsync(url, proxy: null, note: "Requesting webpage from Mashable...");
            if (string.IsNullOrEmpty(response))
            {
                return links;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(response);
            try
            {
                var main = doc.DocumentNode.SelectSingleNode("//article[@data-id]");
                if (main == null)
                    return links;

                links.AddRange(main.SelectNodes(".//a[contains(@href,'http')]").Select(a => a.GetAttributeValue("href", string.Empty)).Where(x => !x.IsNullOrEmpty()));
            }
            catch (Exception)
            {
            }

            return links;
        }
    }
}
