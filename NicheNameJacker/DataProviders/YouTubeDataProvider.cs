using Google;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NicheNameJacker.Parser;
using NicheNameJacker.Properties;
using NicheNameJacker.Schema;
using log4net;
using System.Timers;

namespace NicheNameJacker.DataProviders
{
    public sealed class YouTubeDataProvider
    {
        private static readonly ILog _log =
           LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _query;
        private readonly IList<BaseSearchResult> _store;
        private readonly CancellationToken _cancellationToken;
        private readonly bool _extendedProps;

        private string _nextPageToken = string.Empty;
        private readonly ParserFunction _parser;

        /// <summary>
        /// Initializes the new instance of <see cref="YouTubeDataProvider"/> class.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="store"></param>
        /// <param name="cancellationToken"></param>
        public YouTubeDataProvider(
            string query, IList<BaseSearchResult> store, CancellationToken cancellationToken, bool extendedProps = false, ParserFunction parser = null)
        {
            this._query = query;
            this._store = store;
            this._cancellationToken = cancellationToken;
            this._extendedProps = extendedProps;
            _parser = parser ?? DomainParser.Parse;
        }

        /// <summary>
        /// Initializes incremental loading from the view.
        /// </summary>
        /// <param name="count">The number of items to load.</param>
        /// <returns>The wrapped results of the load operation.</returns>
        public async Task<bool> LoadMoreItemsAsync(uint count)
        {
            return await LoadMoreItemsInternalAsync(count, _cancellationToken);
        }

        /// <summary>
        /// Returns the most popular videos for the content region.
        /// </summary>
        /// <param name="cancellationToken">The Task cancellation token for the operation</param>
        /// <returns>The wrapped results of the get operation.</returns>
        private async Task<bool> LoadMoreItemsInternalAsync(uint count, CancellationToken cancellationToken)
        {
            YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Settings.Default.YouTubeApiKey, // "AIzaSyBvZfGSa9NUidAMyvT73Kja3ShotaI9VO0",
                ApplicationName = Settings.Default.YouTubeClientId // "YouTubeClient"
            });

            try
            {
                var searchListRequest = youtubeService.Search.List("snippet");

                // Set the request filters
                searchListRequest.Q = _query;
                searchListRequest.Type = "video";
                searchListRequest.Fields = "items/id, pageInfo, nextPageToken";

                while (!this._cancellationToken.IsCancellationRequested && _nextPageToken != null)
                {
                    // Set the request filters for page
                    searchListRequest.PageToken = _nextPageToken == string.Empty ? null : _nextPageToken;
                    searchListRequest.MaxResults = 25;

                    // Call the search.list method to retrieve results matching the specified query term. 
                    SearchListResponse searchListResponse;
                    try
                    {
                        
                        _log.Info("About to call searchListRequest.ExecuteAsync() method");
                        searchListResponse = await searchListRequest.ExecuteAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Exception during brute force in method: LoadMoreItemsInternalAsync", ex);
                        // Brute force algorithm
                        continue;
                    }

                    // We store the next page token to efficiently access the next page in the dataset
                    this._nextPageToken = searchListResponse.NextPageToken;

                    // We use parallel processing to process results faster and efficiently.
                    List<Task> tasks = new List<Task>();
                    _log.Info("iterating over searchListResponse and starting new tasks to get videos of each result!");

                    foreach (var result in searchListResponse.Items)
                    {
                        tasks.Add(await Task.Factory.StartNew(() => GetVideoAsync(result.Id.VideoId, cancellationToken)));
                    }
                    _log.Info("Waiting for all tasks to finish");

                    // Wait for all tasks to finish completion
                    await Task.WhenAll(tasks);
                }
            }
            catch (GoogleApiException ex)
            {
                _log.Error("Google API exception caught ,passing on the exception ", ex);
                // Pass
                return false;
            }
            catch (Exception ex)
            {
                _log.Error("General exception caught,passing on the exception ", ex);
                // Pass
                return false;
            }
            finally
            {
                youtubeService.Dispose();
                _log.Info("Dispose youtube service");
            }
            _log.Info("Success in LoadMoreItemsInternalAsync(), returing true");

            return true;
        }

        /// <summary>
        /// Retrieves the information about the YouTube video.
        /// </summary>
        /// <param name="videoId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> GetVideoAsync(string videoId, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Settings.Default.YouTubeApiKey, // "AIzaSyBvZfGSa9NUidAMyvT73Kja3ShotaI9VO0",
                ApplicationName = Settings.Default.YouTubeClientId // "YouTubeClient"
            });
            _log.Info("successfully init youtubeService in GetVideoAsync");
            try
            {
                string part = "snippet";
                string fields = "items(id, snippet(title, description))";
                if (_extendedProps)
                {
                    part = "snippet, statistics, contentDetails";
                    fields = "items(id, snippet(title, channelTitle, channelId, publishedAt, description, thumbnails), statistics(viewCount, likeCount, dislikeCount), contentDetails(duration))";
                }

                var videosListRequest = youtubeService.Videos.List(part);
                _log.Info("Got video list in GetVideoAsync()");
                // Set the request filters
                videosListRequest.Id = videoId;
                videosListRequest.Fields = fields;

                // Call the videos.list method to retrieve results matching the specified query term. 
                var videosListResponse = await videosListRequest.ExecuteAsync(cancellationToken);
                _log.Info("Called the videos.list method to retrieve results matching the specified query term. ");
                // Create the YouTubeSearchResult object from the YouTube Video.
                YouTubeSearchResult result = YouTubeParser.ParseResult(videosListResponse.Items[0], _parser, _query);

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            _store.Add(result);
                        }
                    }));

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Exception in GetVideoAsync",ex);

                return false;
            }
            finally
            {
                youtubeService.Dispose();
            }
        }
    }
}
