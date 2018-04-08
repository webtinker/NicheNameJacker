using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NicheNameJacker.Utilities
{
    internal static class Downloader
    {
        internal static async Task<Stream> DownloadAsync(string requestUri, string id = null, string note = null, string errnote = null, bool fatal = false, bool verbose = true)
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;

            var httpClient = new HttpClient(httpClientHandler, true);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:10.0) Gecko/20150101 Firefox/44.0 (Chrome)");
            httpClient.DefaultRequestHeaders.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-us,en;q=0.5");

            HttpResponseMessage responseMessage;
            try
            {
                if (verbose)
                {
                    if (note == null)
                    {
                        Console.WriteLine("Downloading...");
                    }
                    else
                    {
                        if (id == null)
                        {
                            Console.WriteLine(note);
                        }
                        else
                        {
                            Console.WriteLine($"{id}: {note}");
                        }
                    }
                }

                responseMessage = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead);
                if (responseMessage.IsSuccessStatusCode)
                {
                    return await responseMessage.Content.ReadAsStreamAsync();
                }
            }
            catch (Exception ex)
            {
                if (errnote == null)
                {
                    errnote = "Unable to download.";
                }

                var errmsg = $"{errnote}: {ex.Message}";
                if (fatal)
                {
                    throw new Exception(message: errnote, innerException: ex);
                }
                else
                {
                    if (verbose)
                    {
                        Console.WriteLine(errmsg);
                    }
                }
            }

            return null;
        }

        internal static async Task<String> DownloadTextAsync(string requestUri, WebProxy proxy = null,  string id = null, string note = null, string errnote = null, bool fatal = false, bool verbose = true)
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (proxy != null)
            {
                httpClientHandler.UseDefaultCredentials = true;
                httpClientHandler.UseProxy = true;
                httpClientHandler.Proxy = proxy;
            }

            var httpClient = new HttpClient(httpClientHandler, true);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:10.0) Gecko/20150101 Firefox/44.0 (Chrome)");
            httpClient.DefaultRequestHeaders.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-us,en;q=0.5");

            HttpResponseMessage responseMessage;
            try
            {
                if (verbose)
                {
                    if (note == null)
                    {
                        Console.WriteLine("Downloading...");
                    }
                    else
                    {
                        if (id == null)
                        {
                            Console.WriteLine(note);
                        }
                        else
                        {
                            Console.WriteLine($"{id}: {note}");
                        }
                    }
                }

                responseMessage = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
                if (responseMessage.IsSuccessStatusCode)
                {
                    return await responseMessage.Content.ReadAsStringAsync();
                }
                else if(responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    return "Forbidden";
                }
            }
            catch (Exception ex)
            {
                if (errnote == null)
                {
                    errnote = "Unable to download webpage";
                }

                var errmsg = $"{errnote}: {ex.Message}";
                if (fatal)
                {
                    throw new Exception(message: errnote, innerException: ex);
                }
                else
                {
                    if (verbose)
                    {
                        Console.WriteLine(errmsg);
                    }
                }
            }

            return null;
        }
    }
}
