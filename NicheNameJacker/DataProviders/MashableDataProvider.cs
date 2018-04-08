using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NicheNameJacker.Extensions;
using NicheNameJacker.Parser;
using NicheNameJacker.Schema;
using NicheNameJacker.Utilities;

namespace NicheNameJacker.DataProviders
{
    class Post
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    class Posts
    {
        [JsonProperty("posts")]
        public Post[] Items { get; set; } 
    }
    
    class MashableDataProvider
    {
        private readonly string _query;
        private readonly IList<BaseSearchResult> _store;
        private readonly CancellationToken _cancellationToken;
        private readonly ParserFunction _parser;

        /// <summary>
        /// Initializes the new instance of <see cref="MashableDataProvider"/> class.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="store"></param>
        /// <param name="cancellationToken"></param>
        public MashableDataProvider(
            string query, IList<BaseSearchResult> store, CancellationToken cancellationToken, ParserFunction parser = null)
        {
            this._query = query;
            this._store = store;
            this._cancellationToken = cancellationToken;
            _parser = parser ?? DomainParser.Parse;
        }

        /// <summary>
        /// Initializes incremental loading from the view.
        /// </summary>
        /// <param name="count">The number of items to load.</param>
        /// <returns><see cref="System.Boolean"/></returns>
        public async Task<bool> LoadMoreItemsAsync(uint count)
        {
            return await LoadMoreItemsInternalAsync(_cancellationToken);
        }

        /// <summary>
        /// Initializes incremental loading from the view.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token</param>
        /// <returns><see cref="System.Boolean"/></returns>
        private async Task<bool> LoadMoreItemsInternalAsync(CancellationToken cancellationToken)
        {
            string baseUrl = "http://mashable.com";

            int page = 1;

            string requestUrl = $"{baseUrl}/search.json?page={page}&q={WebUtility.UrlEncode(_query)}";
            
            while (!_cancellationToken.IsCancellationRequested && !cancellationToken.IsCancellationRequested && !requestUrl.IsNullOrEmpty())
            {
                try
                {
                    string response = await Downloader.DownloadTextAsync(requestUrl, note: "Requesting webpage from Mashable...");
                    if (string.IsNullOrEmpty(response))
                    {
                        // Brute Force Algorithm
                        continue;
                    }

                    var posts = JsonConvert.DeserializeObject<Posts>(response);
                    if (posts?.Items == null || posts.Items.Length == 0)
                        break;
                    
                    IEnumerable<MashableSearchResult> results = await Task.WhenAll(posts.Items.Select(item => $"{baseUrl}/{item.Id}").Select(async s =>
                        new MashableSearchResult()
                        {
                            Source = "Mashable",
                            SourceAddress = s,
                            Domains = (await MashableParser.ParsePage(s)).SelectMany(d => _parser.Invoke(d, "Mashable", _query))
                        }));

                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            foreach (var result in results)
                            {
                                if (!_cancellationToken.IsCancellationRequested)
                                {
                                    _store.Add(result);
                                }
                            }
                        }));

                    requestUrl = MashableParser.ParseNextPage(response, baseUrl);
                }
                catch (Exception)
                {
                    requestUrl = null;
                }
            }

            return true;
        }
    }
}
