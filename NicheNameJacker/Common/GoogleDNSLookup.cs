using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NicheNameJacker.Common
{


   public class GoogleDNSLookup
    {
        private static readonly ILog _log =
        LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string m_baseURI = "https://dns.google.com/resolve?name=";
        public static async Task<bool?> CheckAvailability(string address)
        {
            try
            {
               
                using (var client = new HttpClient(new HttpClientHandler
                {
                    AllowAutoRedirect = false
                }))
                { 
                    var result = await client.GetAsync($"{m_baseURI}{address}");
                    var json = await result.Content.ReadAsStringAsync();
                    if (String.IsNullOrWhiteSpace(json))
                    {
                        _log.Info($"Nothing returned for this adress: {address} returning null result");
                        return null;
                    }
                    var jsonObject = JObject.Parse(json);
                    if(jsonObject["Answer"]!=null)
                    {
                        _log.Info($"The domain: {address} is occupied in GoogleDNS Check");
                        return false; //domain is occupied
                    }
                    _log.Info($"The domain: {address} is available in GoogleDNS Check");
                    return true; //domain is available
                }
            }
            catch (Exception ex)
            {
                _log.Info($"Exception Caught in Google DNS Check for : {address}  returning null");
                _log.Error("exception message",ex);
                return null; // null means there has been an error with the API so we will not decide on whether domain is available or not and will leverage the PHP script
            }
        }
    }
}
