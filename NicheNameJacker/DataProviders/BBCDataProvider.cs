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
    public sealed class BBCDataProvider
    {
        private readonly string _query;
        private readonly IList<BaseSearchResult> _store;
        private readonly CancellationToken _cancellationToken;

        private int _itemsRequired;
        private int _itemsAvailable;

        private string _nextPageToken = null;
        private string _prevPageToken = null;

        public BBCDataProvider(
            string query, IList<BaseSearchResult> store, CancellationToken cancellationToken)
        {
            this._query = query;
            this._store = store;
            this._cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Initializes incremental loading from the view.
        /// </summary>
        /// <param name="count">The number of items to load.</param>
        /// <returns>The wrapped results of the load operation.</returns>
        public async Task<uint> LoadMoreItemsAsync(uint count)
        {
            return await LoadMoreItemsInternalAsync(count, this._cancellationToken);
        }

        private async Task<uint> LoadMoreItemsInternalAsync(uint count, CancellationToken cancellationToken)
        {
            var REDDIT_API_BASE = "http://www.bbc.co.uk/search/more?page={0}&q={1}";
            var REDDIT_API_BASE_REQUEST = string.Format(REDDIT_API_BASE, "1", Uri.EscapeDataString(this._query));

            var requestUrl = REDDIT_API_BASE_REQUEST;
            int page = 1;
            while (!this._cancellationToken.IsCancellationRequested && requestUrl != null)
            {
                string response = await Downloader.DownloadTextAsync(requestUrl);
                if (response == null)
                {
                    // Brute Force Algorithm
                    continue;
                }

                IEnumerable<BaseSearchResult> results = BBCParser.ParseSearchResults(response, _query);
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        foreach (var result in results)
                        {
                            if (_itemsAvailable > _itemsRequired)
                            {
                               break;
                            }

                            if (!this._cancellationToken.IsCancellationRequested)
                            {
                                this._store.Add(result);
                            }
                        }
                    }));

                page += 1;
                requestUrl = string.Format(REDDIT_API_BASE, page, Uri.EscapeDataString(this._query));
            }

            return (uint)_itemsAvailable;
        }
    }
}
