using NicheNameJacker.Parser;
using NicheNameJacker.Schema;
using NicheNameJacker.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NicheNameJacker.DataProviders
{
    public class TumblrDataProvider
    {
        private readonly string _query;
        private readonly string _initialQuery;
        private readonly IList<BaseSearchResult> _store;
        private readonly CancellationToken _cancellationToken;
        private readonly ParserFunction _parser;
        /// <summary>
        /// Initializes the new instance of <see cref="TumblrDataProvider"/> class.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="store"></param>
        /// <param name="cancellationToken"></param>
        public TumblrDataProvider(
             string query, IList<BaseSearchResult> store, CancellationToken cancellationToken,
            ParserFunction parser = null)
        {
            this._initialQuery = query;
            this._query = System.Net.WebUtility.UrlEncode(query.Replace("-", " ").Replace(".", " "));
            this._store = store;
            this._cancellationToken = cancellationToken;
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
            string baseRequestUrl = string.Format("https://www.tumblr.com/search/{0}", this._query);
            string requestUrl = string.Format("https://www.tumblr.com/search/{0}", this._query);

            string tumblrFormKey = string.Empty;
            TumblrSearchModel tumblrSearchModel = null;
            while (!this._cancellationToken.IsCancellationRequested && requestUrl != null)
            {
                string response;
                if (tumblrSearchModel == null)
                {
                    response = await Downloader.DownloadTextAsync(requestUrl, note: "Requesting webpage: Tumblr");
                }
                else
                {
                    IEnumerable<KeyValuePair<string, string>> nameValueCollection = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("ad_placement_id", tumblrSearchModel.ad_placement_id),
                        new KeyValuePair<string, string>("before", tumblrSearchModel.before),
                        new KeyValuePair<string, string>("blogs_before", tumblrSearchModel.blogs_before),
                        new KeyValuePair<string, string>("post_page", tumblrSearchModel.post_page),
                        new KeyValuePair<string, string>("num_posts_shown", tumblrSearchModel.num_posts_shown),
                        new KeyValuePair<string, string>("post_view", tumblrSearchModel.post_view),
                        new KeyValuePair<string, string>("sort", "top"),
                        new KeyValuePair<string, string>("q", tumblrSearchModel.query),
                        new KeyValuePair<string, string>("more_posts", "true")
                    };

                    FormUrlEncodedContent urlEncodedContent = new FormUrlEncodedContent(nameValueCollection);
                    response = await PostAsync(
                        requestUri: requestUrl, authKey: tumblrFormKey, referer: baseRequestUrl, content: urlEncodedContent);
                }

                if (response == null)
                {
                    // Brute Force Algorithm
                    break;
                }

                if (tumblrSearchModel == null)
                {
                    tumblrFormKey = TumblrParser.ParseFormKey(response);
                    tumblrSearchModel = TumblrParser.ParseSearchModel(response);
                }

                IEnumerable<TumblrSearchResult> results = TumblrParser.ParseResults
                    (System.Net.WebUtility.HtmlDecode(response), _query, _parser);
                if (response == null || results?.Count() == 0)
                {
                    break;
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                     System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                     {
                         foreach (var result in results)
                         {
                             if (!this._cancellationToken.IsCancellationRequested)
                             {
                                 result.Domains.ToList().ForEach(d => d.Keyword = _initialQuery);
                                 this._store.Add(result);
                             }
                         }
                     }));

                tumblrSearchModel.post_page = (int.Parse(tumblrSearchModel.post_page) + 1).ToString();
                tumblrSearchModel.before = (int.Parse(tumblrSearchModel.before) + results.Count()).ToString();
                tumblrSearchModel.num_posts_shown = (int.Parse(tumblrSearchModel.num_posts_shown) + results.Count()).ToString();
                requestUrl = $"https://www.tumblr.com/search/{this._query}/post_page/{tumblrSearchModel.post_page}";
            }

            return true;
        }

        private async Task<string> PostAsync(string requestUri, string authKey, string referer, FormUrlEncodedContent content)
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;

            var httpClient = new HttpClient(httpClientHandler, true);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:10.0) Gecko/20150101 Firefox/44.0 (Chrome)");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US, en-IN; q=0.7, en; q=0.3");
            httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            httpClient.DefaultRequestHeaders.Add("X-tumblr-form-key", authKey);
            httpClient.DefaultRequestHeaders.Add("Referer", referer);
            HttpResponseMessage responseMessage;
            try
            {
                responseMessage = await httpClient.PostAsync(requestUri, content);
                if (responseMessage.IsSuccessStatusCode)
                {
                    return await responseMessage.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }
    }
}
