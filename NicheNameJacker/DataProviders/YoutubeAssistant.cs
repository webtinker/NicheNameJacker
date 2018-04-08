using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using NicheNameJacker.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NicheNameJacker.Properties;
using System.Net.Http;
using System.Net.Sockets;
using log4net;
using System.Net;

namespace NicheNameJacker.DataProviders
{
    public class YouTubeAssistant
    {
        readonly string _term;
        readonly string _part;
        readonly string _fields;
        readonly IObserver<VideoDto> _observer;
        readonly CancellationToken _token;
        readonly CancellationTokenSource _tokenSource;
        private static readonly ILog _log =
       LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public YouTubeAssistant(string term, bool extendedSearch, IObserver<VideoDto> observer, CancellationTokenSource tokenSource)
        {
            _term = term;
            _token = tokenSource.Token;
            _tokenSource = tokenSource;
            _observer = observer;
            if (extendedSearch)
            {
                _part = "snippet, statistics, contentDetails";
                _fields = "items(id, snippet(title, channelTitle, channelId, publishedAt, description, thumbnails), statistics(viewCount, likeCount, dislikeCount), contentDetails(duration))";
            }
            else
            {
                _part = "snippet";
                _fields = "items(snippet(description))";

                // TODO looks like we don't need title and publishedAt fields, they aren't used anywhere
                //_fields = "items(id, snippet(title, description, publishedAt))";
            }
        }

        YouTubeService CreateService() => new YouTubeService(new BaseClientService.Initializer
        {
            ApiKey = Settings.Default.YouTubeApiKey, // "AIzaSyBvZfGSa9NUidAMyvT73Kja3ShotaI9VO0",
            ApplicationName = Settings.Default.YouTubeClientId // "YouTubeClient"
        });

        SearchResource.ListRequest CreateListRequest(YouTubeService service, DateTime? publishedBefore, DateTime? publishedAfter)
        {
            var request = service.Search.List("snippet");
            request.Q = _term;
            request.Type = "video";
            request.Fields = "items/id, pageInfo, nextPageToken";
            request.MaxResults = 25;
            request.PublishedBefore = publishedBefore;
            request.PublishedAfter = publishedAfter;
            request.Order = SearchResource.ListRequest.OrderEnum.Date;
            return request;
        }

        public async Task LoadDataAsync() => await SearchListAsync();

        async Task SearchListAsync()
        {
            try
            {
                DateTime now = DateTime.UtcNow;

                Random rand = new Random(now.Millisecond);

                List<DateTime> dates = new List<DateTime>();
                for (DateTime from = now; from >= now.AddYears(-4); from = from.AddMonths(-1))
                {
                    dates.Add(from);
                }
                //make youtube requests for videos in each month for 4 previous years makes a total of 48 requests
                // we execute 6 requests in parallel
                await dates.ForEachAsync(6, (d) => RequestYoutubeAPI(d, rand));
            }
            catch (Exception e)
            {
            }
        }


        public async Task RequestYoutubeAPI(DateTime publishedBefore,Random rand)
        {
            using (var service = CreateService())
            {
                try
                {
                    var request = CreateListRequest(service, publishedBefore,
                        publishedBefore.AddMonths(-1));

                    while (!_token.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(rand.Next(0, 3000)), _token);

                        var response = await request.ExecuteAsync(_token);

                        if (_token.IsCancellationRequested) break;

                        if (response.Items.Count == 0)
                            break;

                        await Task.Delay(TimeSpan.FromMilliseconds(rand.Next(0, 1000)), _token);

                        await response.Items.Select(i => SearchVideoAsync(i.Id.VideoId, rand, _term)).WhenAll();
                        if (response.NextPageToken == null) break;

                        request.PageToken = response.NextPageToken;
                    }
                }
                catch (HttpRequestException ex) //in case the internet is down
                {
                   // _tokenSource.Cancel();
                    return;
                }
                catch (Exception e)
                {
                    await Task.Delay(1000);
                }
            }
        }

        VideosResource.ListRequest CreateVideoRequest(YouTubeService service, string videoId)
        {
            var request = service.Videos.List(_part);
            request.Id = videoId;
            request.Fields = _fields;
            return request;
        }

        async Task SearchVideoAsync(string videoId, Random rand, string term)
        {
           
            if (_token.IsCancellationRequested) return;
            using (var service = CreateService())
            {
                try
                {
                    
                    var request = CreateVideoRequest(service, videoId);

                    await Task.Delay(TimeSpan.FromMilliseconds(rand.Next(0, 200)), _token);
                    var response = await request.ExecuteAsync(_token);

                    if (_token.IsCancellationRequested) return;

                    var item = response.Items[0];
                    _observer.OnNext(new VideoDto() { Description = item.Snippet.Description, Term = term,SourceURL =$"https://www.youtube.com/watch?v={videoId}" });
                }
              
                catch(HttpRequestException ex)
                {
                    _tokenSource.Cancel(); //cancelling because internet is not available
                    _log.Debug("Cancelled from src");
                }
               
                catch (Exception)
                {
                    if (_token.IsCancellationRequested) return;
                    await Task.Delay(1000);
                }
            }
        }
    }
}
