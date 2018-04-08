using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NicheNameJacker.Common;
using NicheNameJacker.Extensions;
using NicheNameJacker.Parser;
using NicheNameJacker.Utilities;

namespace NicheNameJacker.DataProviders
{
    class VideosPageInfo
    {
        public IList<string> Links { get; set; }

        public string NextPage { get; set; }
    }

    public class VideoDto
    {
        public string Description { get; set; }

        public string Term { get; set; }

        public string SourceURL { get; set; }
    }

    sealed class YouTubeScraper
    {
        readonly string _term;
        readonly IObserver<VideoDto> _observer;
        readonly CancellationToken _token;
        readonly CancellationTokenSource _tokenSource;

        public YouTubeScraper(string term, IObserver<VideoDto> observer, CancellationTokenSource token)
        {
            _term = term;
            _token = token.Token;
            _tokenSource = token;
            _observer = observer;
        }

        public async Task SearchListAsync()
        {
            DownloaderQueue queue = new DownloaderQueue(_token);
            try
            {
                string[] proxies = new string[] {null}; // File.ReadAllLines("goodproxies.txt");

                Random rand = new Random(DateTime.UtcNow.Millisecond);

                string baseUrl = @"https://www.youtube.com";
                string requestUrl = string.Format("{0}/results?search_query={1}", baseUrl, WebUtility.UrlEncode(_term));

                while (!_token.IsCancellationRequested && !string.IsNullOrEmpty(requestUrl))
                {
                    int maxRetries = 5;
                    int retry = 0;
                    string response = null;
                    await Task.Delay(TimeSpan.FromMilliseconds(rand.Next(0, 1000)));

                    while (retry < maxRetries)
                    {
                        WebProxy proxy = null;
                        string address = proxies[rand.Next(0, proxies.Length)];
                        if (!address.IsNullOrEmpty())
                            proxy = new WebProxy(String.Format("http://{0}", address))
                            {
                                UseDefaultCredentials = true
                            };

                        response = await queue.Enqueu(Downloader.DownloadTextAsync(requestUrl, proxy: proxy,
                            note: "Requesting webpage from YouTube..."));
                            //await
                            //    Downloader.DownloadTextAsync(requestUrl, proxy: proxy,
                            //        note: "Requesting webpage from YouTube...");
                        if (response.IsNullOrEmpty())
                        {
                            retry++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (response.IsNullOrEmpty())
                        break;

                    VideosPageInfo videosPageInfo = YouTubeParser.ParseVideosPage(response);
                    if (videosPageInfo.Links.Count == 0)
                        break;

                    await videosPageInfo.Links.Select(link => String.Format("{0}{1}", baseUrl, link))
                        .Select(link => SearchVideoAsync(link, _term, rand, proxies, queue))
                        .WhenAll();

                    if (videosPageInfo.NextPage.IsNullOrEmpty())
                        break;

                    requestUrl = string.Format("{0}{1}", baseUrl, videosPageInfo.NextPage);
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                if (!_tokenSource.IsCancellationRequested)
                    _tokenSource.Cancel();
            }
        }
        
        async Task SearchVideoAsync(string videoLink, string query, Random rand, string[] proxies, DownloaderQueue queue)
        {
            if (_token.IsCancellationRequested) return;

            int maxRetries = 5;
            int retry = 0;
            string response = null;
            await Task.Delay(TimeSpan.FromMilliseconds(rand.Next(100, 300)));

            while (retry < maxRetries)
            {
                WebProxy proxy = null;
                string address = proxies[rand.Next(0, proxies.Length)];
                if (!address.IsNullOrEmpty())
                    proxy = new WebProxy(String.Format("http://{0}", address))
                    {
                        UseDefaultCredentials = true
                    };

                response = await queue.Enqueu(Downloader.DownloadTextAsync(videoLink, proxy: proxy, note: String.Format("Requesting {0} from YouTube...", videoLink)));
                if (response.IsNullOrEmpty())
                {
                    retry++;
                }
                else
                {
                    break;
                }
            }
            
            if (response.IsNullOrEmpty())
                return;
            
            string description = YouTubeParser.ParseVideoPage(response);
            _observer.OnNext(new VideoDto() { Description = description, Term = query , SourceURL = videoLink });
        }
    }
}
