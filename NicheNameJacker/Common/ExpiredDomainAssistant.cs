using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NicheNameJacker.Common
{
    public class ExpiredDomainAssistant
    {
        static Lazy<HttpClient> _httpClientFetcher = new Lazy<HttpClient>(() => new HttpClient());
        static HttpClient Client = _httpClientFetcher.Value;
        const string ServiceUrl = "http://nichejacker.com/domainchecker/index.php";

        public static async Task<JArray> CheckAsync(params string[] domains)
        {
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("domains", JsonConvert.SerializeObject(domains))
                });

                var result = await Client.PostAsync(ServiceUrl, content);
                var jsonString = await result.Content.ReadAsStringAsync();
                return JArray.Parse(jsonString);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
