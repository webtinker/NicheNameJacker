using log4net;
using NicheNameJacker.Utilities;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NicheNameJacker.Common
{
    public static class SubdomainAssistant
    {
        private static readonly ILog _log =
        LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Lazy<HttpClient> _httpClientFetcher = new Lazy<HttpClient>(() => new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false
        }));

        private static HttpClient Client => _httpClientFetcher.Value;

        public static async Task<bool?> IsAvailableTimeout(string address, TimeSpan? timeout = null)
        {
            try
            {

                if (!address.StartsWith("http://"))
                {
                    address = "http://" + address;
                }
                _log.Info($"Inside IsAvailableTimeOut for domain : {address}");

                HttpResponseMessage result;
                using (var client = new HttpClient(new HttpClientHandler
                {
                    AllowAutoRedirect = false
                }))
                {
                    if (timeout.HasValue)
                        client.Timeout = timeout.Value;
                    _log.Info($"Inside IsAvailableTimeOut called GetAsync for domain : {address}");
                    result = await client.GetAsync(address);
                }
                _log.Info($"Inside IsAvailableTimeOut called GetAsync for domain : {address} ,resulted in {result?.StatusCode} ");
                return result.StatusCode == HttpStatusCode.NotFound || result.StatusCode == HttpStatusCode.BadGateway;
            }
            catch (Exception e)
            {
                _log.Info($"Inside IsAvailableTimeOut Exception Caught for Domain : {address} , Reason {e.Message} , Inner Exception : {e?.InnerException?.Message}");
                Logger.LogError(e.Message);
                return null;
            }
        }

        public static async Task<bool?> IsAvailableAsync(string address)
        {
            try
            {
                if (!address.StartsWith("http://"))
                {
                    address = "http://" + address;
                }

                var result = await Client.GetAsync(address);
                return result.StatusCode == HttpStatusCode.NotFound;
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                return true;
            }
        }
    }
}
