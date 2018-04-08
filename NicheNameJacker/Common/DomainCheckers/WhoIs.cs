using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NicheNameJacker.Common.DomainCheckers
{
    abstract class WhoIsBase
    {
        public abstract Task<bool?> IsAvailableAsync(string domain);
    }

    class Mashape : WhoIsBase
    {
        public override async Task<bool?> IsAvailableAsync(string domain)
        {
            string baseUrl = "https://whois-v0.p.mashape.com";

            bool? result = null;
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (var client = new HttpClient(httpClientHandler, true))
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
                    client.DefaultRequestHeaders.Add("X-Mashape-Key", "LA35aKLB3Rmsh6O2HR2JugpxFRNlp1ak1C0jsnLouEprhD1DXy");

                    var response = await client.GetAsync($"{baseUrl}/check?domain={domain}");
                    string content = await response.Content.ReadAsStringAsync();
                    var json = JsonConvert.DeserializeAnonymousType(content, new { available = default(bool) });
                    result = json.available;
                }
                catch (Exception)
                {
                }
            }

            return result;
        }
    }

    class NameCheap : WhoIsBase
    {
        public override async Task<bool?> IsAvailableAsync(string domain)
        {
            string baseUrl = "https://www.namecheap.com";
            bool? result = null;

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (var client = new HttpClient(httpClientHandler, true))
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Accept",
                        "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");


                    var response = await client.GetAsync($"{baseUrl}/domains/whois/results.aspx?domain={domain}");
                    string content = response.Content.ReadAsStringAsync().Result;
                    var match = Regex.Match(content, @"var url\s*=\s*""(?<url>.*)""");
                    if (match.Success)
                    {
                        var group = match.Groups["url"];
                        var apiResponse = await client.GetAsync($"{baseUrl}{group.Value}");

                        string ajaxResult = apiResponse.Content.ReadAsStringAsync().Result;
                        result = Regex.IsMatch(ajaxResult, "No match for domain");
                    }
                }
                catch (Exception)
                {
                }
            }

            return result;
        }
    }

    class WhoIsCom : WhoIsBase
    {
        public override async Task<bool?> IsAvailableAsync(string domain)
        {
            string baseUrl = "http://www.whois.com";

            bool? result = null;

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (var client = new HttpClient(httpClientHandler, true))
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Accept",
                        "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");

                    var response = await client.GetAsync($"{baseUrl}/whois/{domain}");
                    string content = response.Content.ReadAsStringAsync().Result;

                    var match = Regex.Match(content, @"\<div\s*id=""availableBlk""\s*style=""display: block"">");
                    if (match.Success)
                    {
                        result = true;
                    }
                }
                catch (Exception)
                {
                }
            }

            return result;
        }
    }

    class WhoIs : WhoIsBase
    {
        public override async Task<bool?> IsAvailableAsync(string domain)
        {
            string baseUrl = "https://who.is";

            bool? result = null;

            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Accept",
                        "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");


                    var response = await client.GetAsync($"{baseUrl}/whois/{domain}");
                    string content = response.Content.ReadAsStringAsync().Result;
                    var match = Regex.Match(content, @"var pendingSiteAnalysis\s*=\s*""(?<hash>.*)""");
                    if (match.Success)
                    {
                        var group = match.Groups["hash"];
                        var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("pendingSiteAnalysis", group.Value), });
                        var apiResponse = await client.PostAsync($"{baseUrl}/api/whois/getDomainSiteAnalysis/{domain}", formContent);

                        string json = apiResponse.Content.ReadAsStringAsync().Result;
                        var status = JsonConvert.DeserializeAnonymousType(json, new { active = default(string) });
                        int isActive;
                        if (int.TryParse(status.active, out isActive))
                        {
                            result = isActive == 0;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            return result;
        }
    }
}
