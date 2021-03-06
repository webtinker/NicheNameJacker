﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NicheNameJacker.Extensions;
using NicheNameJacker.Parser;
using NicheNameJacker.Schema;
using NicheNameJacker.Utilities;

namespace NicheNameJacker.DataProviders
{
    class WikiDataProvider
    {
        private readonly string _query;
        private readonly IList<BaseSearchResult> _store;
        private readonly CancellationToken _cancellationToken;
        private readonly ParserFunction _parser;

        /// <summary>
        /// Initializes the new instance of <see cref="WikiDataProvider"/> class.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="store"></param>
        /// <param name="cancellationToken"></param>
        public WikiDataProvider(
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
            string baseUrl = "http://www.bing.com";
            string requestUrl = $"{baseUrl}/search?q=\"{_query}\"+(site:en.wikipedia.org+inbody:\"dead+link\")";

            List<string> usedUrls = new List<string>();

            while (!_cancellationToken.IsCancellationRequested && !cancellationToken.IsCancellationRequested && !requestUrl.IsNullOrEmpty())
            {
                try
                {
                    string response = await Downloader.DownloadTextAsync(requestUrl, note: "Requesting webpage from Bing...");
                    if (string.IsNullOrEmpty(response))
                    {
                        // Brute Force Algorithm
                        continue;
                    }

                    var links = WikipediaParser.ParseBingResults(response, usedUrls);
                    usedUrls.AddRange(links);

                    IEnumerable<WikipediaSearchResult> results = await Task.WhenAll(links.Select(async s =>
                        new WikipediaSearchResult()
                        {
                            Source = "Wikipedia",
                            SourceAddress = s,
                            Domains = (await WikipediaParser.ParseWiki(s)).SelectMany(d => _parser.Invoke(d, "Wikipedia", _query))
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

                    requestUrl = WikipediaParser.ParseNextPage(response, baseUrl);
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
