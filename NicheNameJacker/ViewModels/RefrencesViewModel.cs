using Microsoft.Win32;
using NicheNameJacker.Commands;
using NicheNameJacker.Common;
using NicheNameJacker.DataProviders;
using NicheNameJacker.Parser;
using NicheNameJacker.Schema;
using NicheNameJacker.Schema.BackLinks;
using NicheNameJacker.Utilities;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NicheNameJacker.ViewModels
{
    public class RefrencesViewModel : ObservableBase
    {
        private bool _isBusy;
        private bool _isBackLinkCountVisible;
        private bool _isRedditReferencesAvailable;
        private bool _isTumblrReferencesAvailable;
        private bool _isWikipediaReferencesAvailable;
        private bool _isYouTubeReferencesAvailable;
        private bool _isYahooReferencesAvailable;

        private string _backLinkCount;

        private readonly string _fqdn;
        private readonly CancellationToken _cancellationToken;
        private readonly ParserFunction _domainParser;

        public RefrencesViewModel()
        {

        }

        public RefrencesViewModel(string fqdn, CancellationToken cancellationToken)
        {
            _fqdn = fqdn;
            _cancellationToken = cancellationToken;
            _domainParser = _fqdn.Count(c => c == '.') > 1 ? (ParserFunction)DomainParser.ParseSubdomain : DomainParser.Parse;
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public bool IsBackLinkCountVisible
        {
            get { return _isBackLinkCountVisible; }
            set { SetProperty(ref _isBackLinkCountVisible, value); }
        }
        public bool IsRedditReferencesAvailable
        {
            get { return _isRedditReferencesAvailable; }
            set { SetProperty(ref _isRedditReferencesAvailable, value); }
        }
        public bool IsTumblrReferencesAvailable
        {
            get { return _isTumblrReferencesAvailable; }
            set { SetProperty(ref _isTumblrReferencesAvailable, value); }
        }

        public bool IsWikipediaReferencesAvailable
        {
            get { return _isWikipediaReferencesAvailable; }
            set { SetProperty(ref _isWikipediaReferencesAvailable, value); }
        }

        public bool IsHuffpostReferencesAvailable
        {
            get { return _isHuffPostReferencesAvailable; }
            set { SetProperty(ref _isHuffPostReferencesAvailable, value); }
        }

        public bool IsMashableReferencesAvailable
        {
            get { return _isMashableReferencesAvailable; }
            set { SetProperty(ref _isMashableReferencesAvailable, value); }
        }

        public bool IsYouTubeReferencesAvailable
        {
            get { return _isYouTubeReferencesAvailable; }
            set { SetProperty(ref _isYouTubeReferencesAvailable, value); }
        }
        public bool IsYahooReferencesAvailable
        {
            get { return _isYahooReferencesAvailable; }
            set { SetProperty(ref _isYahooReferencesAvailable, value); }
        }

        public string BackLinkCount
        {
            get { return _backLinkCount; }
            set { SetProperty(ref _backLinkCount, value); }
        }

        ulong _totalVideoViews;
        public ulong TotalVideoViews
        {
            get { return _totalVideoViews; }
            set { SetProperty(ref _totalVideoViews, value); }
        }

        public ObservableCollection<RedditBackLink> RedditRefrences { get; } = new ObservableCollection<RedditBackLink>();

        public ObservableCollection<TumblrBackLink> TumblrRefrences { get; } = new ObservableCollection<TumblrBackLink>();

        public ObservableCollection<YouTubeBackLink> YouTubeRefrences { get; } = new ObservableCollection<YouTubeBackLink>();

        public ObservableCollection<WikipediaBackLink> WikipediaRefrences { get; } = new ObservableCollection<WikipediaBackLink>();

        public ObservableCollection<HuffpostBackLink> HuffpostRefrences { get; } = new ObservableCollection<HuffpostBackLink>();

        public ObservableCollection<MashableBackLink> MashableRefrences { get; } = new ObservableCollection<MashableBackLink>();

        public ObservableCollection<YahooBackLink> YahooReferences { get; } = new ObservableCollection<YahooBackLink>();

        private string _statusText;
        private bool _isSearchEnabled;
        private bool _isHuffPostReferencesAvailable;
        private bool _isMashableReferencesAvailable;

        public string StatusText
        {
            get { return _statusText; }
            set { SetProperty(ref _statusText, value); }
        }

        public ICommand BuyCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //string GODADDY_REQUEST_BASE_URL = "https://www.godaddy.com/domains/searchresults.aspx?checkAvail=1&domainToCheck={0}";
                    string GODADDY_REQUEST_BASE_URL = "http://www.expyred.com";
                    string GODADDY_REQUEST_BASE_REQUEST_URL = string.Format(GODADDY_REQUEST_BASE_URL, this._fqdn);

                    ProcessStartInfo startInfo = new ProcessStartInfo(GODADDY_REQUEST_BASE_REQUEST_URL);
                    using (Process process = Process.Start(startInfo))
                    {

                    }
                });
            }
        }

        public bool IsSearchEnabled
        {
            get { return _isSearchEnabled; }
            set
            {
                SetProperty(ref _isSearchEnabled, value);
            }
        }

        public ICommand SaveCommand => new RelayCommand(() => SaveBacklinks());

        private void SaveBacklinks()
        {
            var dialog = new SaveFileDialog { FileName = "BackLinks", DefaultExt = "csv", Filter = "CSV Files (.csv)|*.csv" };
            var result = dialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    using (var writer = new StreamWriter(dialog.FileName))
                    {
                        IEnumerable<BaseBackLink> xs = YouTubeRefrences;
                        IEnumerable<BaseBackLink> ys = RedditRefrences;
                        IEnumerable<BaseBackLink> zs = TumblrRefrences;
                        IEnumerable<BaseBackLink> ks = WikipediaRefrences;
                        IEnumerable<BaseBackLink> ls = HuffpostRefrences;
                        IEnumerable<BaseBackLink> ms = MashableRefrences;
                        IEnumerable<BaseBackLink> yas = YahooReferences;
                        var data = xs.Concat(ys).Concat(zs).Concat(ks).Concat(ls).Concat(ms).Concat(yas);

                        CsvSerializer.SerializeToWriter(data, writer);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message);
                }
            }
        }

        public ICommand SearchCommand => new RelayCommand<int>(async i => await SearchAsync(i));

        private async Task SearchAsync(int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 0:
                    StatusText = $"Getting Backlinks in Youtube for {_fqdn}";
                    await GetYouTubeRefrencesAsync();
                    break;
                case 1:
                    StatusText = $"Getting Backlinks in Reddit for {_fqdn}";
                    await GetRedditRefrencesAsync();
                    break;
                case 2:
                    StatusText = $"Getting Backlinks in Tumblr for {_fqdn}";
                    await GetTumblrRefrencesAsync();
                    break;
                case 3:
                    StatusText = $"Getting Backlinks in Wikipedia for {_fqdn}";
                    await GetWikipediaRefrencesAsync();
                    break;
                case 4:
                    StatusText = $"Getting Backlinks in Huffingtonpost for {_fqdn}";
                    await GetHuffpostRefrencesAsync();
                    break;
                case 5:
                    StatusText = $"Getting Backlinks in Mashable for {_fqdn}";
                    await GetMashableRefrencesAsync();
                    break;
                case 6:
                    StatusText = $"Getting Backlinks in Yahoo answers for {_fqdn}";
                    await GetYahooRefrencesAsync();
                    break;
                default:
                    break;
            }
        }

        public ICommand ViewInBrowserCommand => new RelayCommand<string>(s => Process.Start(s));

        public ICommand CopyYoutubeEmbedCodeCommand => new RelayCommand<string>(s =>
            Clipboard.SetText($@"<iframe src=""http://www.youtube.com/embed/{s}"" frameborder=""0"" allowfullscreen></iframe>"));
        
        private async Task GetRedditRefrencesAsync()
        {
            await Task.Factory.StartNew(async () =>
            {
                if (this.RedditRefrences.Count != 0 || this.IsBusy)
                {
                    return;
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = true;
                    }));

                List<BaseSearchResult> results = new List<BaseSearchResult>();
                var redditDataProvider = new RedditDataProvider(_fqdn, results, _cancellationToken, _domainParser);
                await redditDataProvider.LoadMoreItemsAsync(Int32.MaxValue);

                var items = results.Where((x) => x.Source.Equals("Reddit"));
                var fdqn = _fqdn.ToLowerInvariant();
                foreach (var item in items)
                {
                    var domain = item.Domains.FirstOrDefault(x => x.Address.ToLowerInvariant().Equals(fdqn));
                    if (domain != null)
                    {
                        var backRefrence = new RedditBackLink();
                        backRefrence.Source = "Reddit";
                        backRefrence.SourceAddress = item.SourceAddress;
                        backRefrence.FullAddress = domain.FullAddress;
                        backRefrence.SearchResult = (RedditSearchResult)item;

                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                this.RedditRefrences.Add(backRefrence);
                            }));
                    }
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = false;
                        this.IsRedditReferencesAvailable = this.RedditRefrences.Count > 0 ? true : false;
                        if (this.IsRedditReferencesAvailable)
                        {
                            this.IsBackLinkCountVisible = true;
                            this.BackLinkCount = this.RedditRefrences.Count.ToString("N0");
                        }
                    }));
            });
        }

        private async Task GetTumblrRefrencesAsync()
        {
            await Task.Factory.StartNew(async () =>
            {
                if (this.TumblrRefrences.Count != 0 || this.IsBusy)
                {
                    return;
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = true;
                    }));

                List<BaseSearchResult> results = new List<BaseSearchResult>();
                var youtubeDataProvider = new TumblrDataProvider(_fqdn, results, _cancellationToken, _domainParser);
                await youtubeDataProvider.LoadMoreItemsAsync(Int32.MaxValue);

                var items = results.Where((x) => x.Source.Equals("Tumblr"));
                var fdqn = _fqdn.ToLowerInvariant();
                foreach (var item in items)
                {
                    var domain = item.Domains.FirstOrDefault(x => x.Address.ToLowerInvariant().Equals(fdqn));
                    if (domain != null)
                    {
                        var backRefrence = new TumblrBackLink();
                        backRefrence.Source = "Tumblr";
                        backRefrence.SourceAddress = item.SourceAddress;
                        backRefrence.FullAddress = domain.FullAddress;
                        backRefrence.SearchResult = (TumblrSearchResult)item;

                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                this.TumblrRefrences.Add(backRefrence);
                            }));
                    }
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = false;
                        this.IsTumblrReferencesAvailable = this.TumblrRefrences.Count > 0 ? true : false;
                        if (this.IsTumblrReferencesAvailable)
                        {
                            this.IsBackLinkCountVisible = true;
                            this.BackLinkCount = this.TumblrRefrences.Count.ToString("N0");
                        }
                    }));
            });
        }

        private async Task GetWikipediaRefrencesAsync()
        {
            await Task.Factory.StartNew(async () =>
            {
                if (WikipediaRefrences.Count != 0 || this.IsBusy)
                {
                    return;
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = true;
                    }));

                List<BaseSearchResult> results = new List<BaseSearchResult>();
                var wikiDataProvider = new WikiDataProvider(_fqdn, results, _cancellationToken, _domainParser);
                await wikiDataProvider.LoadMoreItemsAsync(Int32.MaxValue);

                var items = results.Where((x) => x.Source.Equals("Wikipedia"));
                var fdqn = _fqdn.ToLowerInvariant();
                foreach (var item in items)
                {
                    var domain = item.Domains.FirstOrDefault(x => x.Address.ToLowerInvariant().Equals(fdqn));
                    if (domain != null)
                    {
                        var backRefrence = new WikipediaBackLink();
                        backRefrence.Source = "Wikipedia";
                        backRefrence.SourceAddress = item.SourceAddress;
                        backRefrence.FullAddress = domain.FullAddress;
                        backRefrence.SearchResult = (WikipediaSearchResult)item;

                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                WikipediaRefrences.Add(backRefrence);
                            }));
                    }
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = false;
                        IsWikipediaReferencesAvailable = WikipediaRefrences.Count > 0 ? true : false;
                        if (IsWikipediaReferencesAvailable)
                        {
                            this.IsBackLinkCountVisible = true;
                            this.BackLinkCount = WikipediaRefrences.Count.ToString("N0");
                        }
                    }));
            });
        }

        private async Task GetHuffpostRefrencesAsync()
        {
            await Task.Factory.StartNew(async () =>
            {
                if (HuffpostRefrences.Count != 0 || this.IsBusy)
                {
                    return;
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = true;
                    }));

                List<BaseSearchResult> results = new List<BaseSearchResult>();
                var huffpostDataProvider = new HuffingtonpostDataProvider(_fqdn, results, _cancellationToken, _domainParser);
                await huffpostDataProvider.LoadMoreItemsAsync(Int32.MaxValue);

                var items = results.Where((x) => x.Source.Equals("HuffPost"));
                var fdqn = _fqdn.ToLowerInvariant();
                foreach (var item in items)
                {
                    var domain = item.Domains.FirstOrDefault(x => x.Address.ToLowerInvariant().Equals(fdqn));
                    if (domain != null)
                    {
                        var backRefrence = new HuffpostBackLink();
                        backRefrence.Source = "HuffPost";
                        backRefrence.SourceAddress = item.SourceAddress;
                        backRefrence.FullAddress = domain.FullAddress;
                        backRefrence.SearchResult = (HuffpostSearchResult)item;

                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                HuffpostRefrences.Add(backRefrence);
                            }));
                    }
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = false;
                        IsHuffpostReferencesAvailable = HuffpostRefrences.Count > 0 ? true : false;
                        if (IsHuffpostReferencesAvailable)
                        {
                            this.IsBackLinkCountVisible = true;
                            this.BackLinkCount = HuffpostRefrences.Count.ToString("N0");
                        }
                    }));
            });
        }

        private async Task GetMashableRefrencesAsync()
        {
            await Task.Factory.StartNew(async () =>
            {
                if (MashableRefrences.Count != 0 || this.IsBusy)
                {
                    return;
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = true;
                    }));

                List<BaseSearchResult> results = new List<BaseSearchResult>();
                var mashableDataProvider = new MashableDataProvider(_fqdn, results, _cancellationToken, _domainParser);
                await mashableDataProvider.LoadMoreItemsAsync(Int32.MaxValue);

                var items = results.Where((x) => x.Source.Equals("Mashable"));
                var fdqn = _fqdn.ToLowerInvariant();
                foreach (var item in items)
                {
                    var domain = item.Domains.FirstOrDefault(x => x.Address.ToLowerInvariant().Equals(fdqn));
                    if (domain != null)
                    {
                        var backRefrence = new MashableBackLink();
                        backRefrence.Source = "Mashable";
                        backRefrence.SourceAddress = item.SourceAddress;
                        backRefrence.FullAddress = domain.FullAddress;
                        backRefrence.SearchResult = (MashableSearchResult)item;

                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                MashableRefrences.Add(backRefrence);
                            }));
                    }
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = false;
                        IsMashableReferencesAvailable = MashableRefrences.Count > 0 ? true : false;
                        if (IsMashableReferencesAvailable)
                        {
                            this.IsBackLinkCountVisible = true;
                            this.BackLinkCount = MashableRefrences.Count.ToString("N0");
                        }
                    }));
            });
        }
        private async Task GetYahooRefrencesAsync()
        {
            await Task.Factory.StartNew(async () =>
            {
                if (YahooReferences.Count != 0 || this.IsBusy)
                {
                    return;
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = true;
                    }));
                List<BaseSearchResult> results = new List<BaseSearchResult>();
                var yahooDataProvider = new YahooDataProvider(_fqdn, results, _cancellationToken, _domainParser);
                await yahooDataProvider.LoadMoreItemsAsync(Int32.MaxValue);

                var items = results.Where((x) => x.Source.Equals("Yahoo"));
                var fdqn = _fqdn.ToLowerInvariant();
                foreach (var item in items)
                {
                    var domain = item.Domains.FirstOrDefault(x => x.Address.ToLowerInvariant().Equals(fdqn));
                    if (domain != null)
                    {
                        var backRefrence = new YahooBackLink();
                        backRefrence.Source = "Yahoo";
                        backRefrence.SourceAddress = item.SourceAddress;
                        backRefrence.FullAddress = domain.FullAddress;
                        backRefrence.SearchResult = (YahooSearchResult)item;

                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                YahooReferences.Add(backRefrence);
                            }));
                    }
                }
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                   System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                   {
                       this.IsBusy = false;
                       IsYahooReferencesAvailable = YahooReferences.Count > 0 ? true : false;
                       if (IsYahooReferencesAvailable)
                       {
                           this.IsBackLinkCountVisible = true;
                           this.BackLinkCount = YahooReferences.Count.ToString("N0");
                       }
                   }));
            });
        }
        private async Task GetYouTubeRefrencesAsync()
        {
            await Task.Factory.StartNew(async () =>
            {
                if (this.YouTubeRefrences.Count != 0 || this.IsBusy)
                {
                    return;
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = true;
                    }));

                List<BaseSearchResult> results = new List<BaseSearchResult>();
                var youtubeDataProvider = new YouTubeDataProvider(_fqdn, results, _cancellationToken, true, _domainParser);
                await youtubeDataProvider.LoadMoreItemsAsync(Int32.MaxValue);

                var items = results.Where(x => x.Source.Equals("YouTube"));
                var fdqn = _fqdn.ToLowerInvariant();

                foreach (var item in items)
                {
                    var domain = item.Domains.FirstOrDefault(x => x.Address.ToLowerInvariant().Equals(fdqn));
                    if (domain != null)
                    {
                        var backRefrence = new YouTubeBackLink();
                        backRefrence.Source = "YouTube";
                        backRefrence.SourceAddress = item.SourceAddress;
                        backRefrence.FullAddress = domain.FullAddress;
                        backRefrence.SearchResult = (YouTubeSearchResult)item;

                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                this.YouTubeRefrences.Add(backRefrence);
                            }));
                    }
                }

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.IsBusy = false;
                        this.IsYouTubeReferencesAvailable = this.YouTubeRefrences.Count > 0 ? true : false;
                        if (this.IsYouTubeReferencesAvailable)
                        {
                            this.IsBackLinkCountVisible = true;
                            this.BackLinkCount = this.YouTubeRefrences.Count.ToString("N0");
                            TotalVideoViews = YouTubeRefrences.Select(x => x.SearchResult.ViewCount)
                                                              .Where(x => x.HasValue)
                                                              .Select(x => x.Value)
                                                              .Aggregate((x, y) => x + y);
                        }
                    }));
            });
        }
    }
}
