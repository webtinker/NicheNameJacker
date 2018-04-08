using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NicheNameJacker.Parser;
using NicheNameJacker.Schema;
using NicheNameJacker.Utilities;

namespace NicheNameJacker.DataProviders
{
    public sealed class RedditDataProvider
    {
        private readonly string _query;
        private readonly IList<BaseSearchResult> _store;
        private readonly CancellationToken _cancellationToken;
        private readonly ParserFunction _parser;

        /// <summary>
        /// Initializes the new instance of <see cref="RedditDataProvider"/> class.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="store"></param>
        /// <param name="cancellationToken"></param>
        public RedditDataProvider(
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
            string requestUrl = string.Format("https://www.reddit.com/search?q={0}", _query);

            while (!_cancellationToken.IsCancellationRequested && !string.IsNullOrEmpty(requestUrl))
            {
                string response = await Downloader.DownloadTextAsync(requestUrl, note: "Requesting webpage from Reddit...");
                if (string.IsNullOrEmpty(response))
                {
                    // Brute Force Algorithm
                    continue;
                }

                IEnumerable<RedditSearchResult> results = RedditParser.ParseResults(response, _query, _parser);
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


                requestUrl = RedditParser.ParseNextPage(response);
            }

            return true;
        }
    }
}
