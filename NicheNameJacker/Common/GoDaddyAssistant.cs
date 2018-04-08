using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NicheNameJacker.Common
{
    public class GoDaddyAssistant
    {
        private const string _key = "2s946pZ9FV_W5hLDpFGWmcbHrJ9n1PoH9";
        private const string _secret = "UFqhpeEXcBnk981A2wvVGs";

        private static Lazy<HttpClient> _httpClientFetcher =
            new Lazy<HttpClient>(() =>
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"sso-key {_key}:{_secret}");
                return client;
            });

        private static HttpClient Client => _httpClientFetcher.Value;

        public static async Task<bool?> IsDomainAvailable(string domain)
        {
            try
            {
                domain = WebUtility.UrlEncode(domain);
                var result = await Client.GetStringAsync($"https://api.godaddy.com/v1/domains/available?domain={domain}&checktype=FULL");
                var json = JObject.Parse(result);
                return json.Value<bool>("available");
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
