using NicheNameJacker.Commands;
using NicheNameJacker.Common;
using NicheNameJacker.DataProviders;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using System;
using System.Threading;
using NicheNameJacker.Parser;
using System.Collections.ObjectModel;
using System.Linq;
using NicheNameJacker.ViewModels.SubViewModels;
using System.Collections.Generic;
using NicheNameJacker.Schema;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Windows.Data;
using NicheNameJacker.Common.Membership;
using NicheNameJacker.Controls;
using NicheNameJacker.Extensions;
using NicheNameJacker.Properties;
using log4net;
using System.Windows.Controls.Primitives;

namespace NicheNameJacker.ViewModels
{
    public class PbnViewModel : ObservableBase
    {
        private string m_favouritesFileName = "PbnFavoriteDomainsSaved.xml";
        private string m_blackListedFileName = "PbnBlacklistedDomainsSaved.xml";
        private string m_permBlackListedFileName = "PbnPermBlacklistedDomainsSaved.xml";
        CancellationTokenSource _youtubeTokenSource = new CancellationTokenSource();
        CancellationTokenSource _tumblrTokenSource = new CancellationTokenSource();
        CancellationTokenSource _redditTokenSource = new CancellationTokenSource();
        CancellationTokenSource _yahooTokenSource = new CancellationTokenSource();
        CancellationTokenSource _bulkTokenSource = new CancellationTokenSource();
        private System.Windows.Forms.Timer _resultsTimer;
        Subject<VideoDto> _youtubeSubject = new Subject<VideoDto>();
        private static readonly ILog _log =
        LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public PbnViewModel()
        {
            _resultsTimer = new System.Windows.Forms.Timer();
            _resultsTimer.Interval = 300000;  // 5 minute interval if no results are obiained for 1 minute we will halt the search
            _resultsTimer.Tick += _resultsTimer_Tick;
            ActiveDomains = new ObservableCollection<SingleDomain>();
            _youtubeSubject.ObserveOnDispatcher().Subscribe(OnVideoLoad);
            Selection = new SelectionSubViewModel(ActiveDomains);
            FavoriteSelection = new SelectionSubViewModel(Favorites);
            Saver = new SaverSubViewModel(ActiveDomains, Favorites);

            ActiveDomains.CollectionChanged += (s, e) => OnPropertyChanged(nameof(AvailableCount));

            DomainsDomDetailer = new DomDetailerSubViewModel(ActiveDomains, Permissions.Pbn, Permissions.PbnPlanName, () => { });
            DomDetailer = new DomDetailerSubViewModel(Favorites, Permissions.Pbn, Permissions.PbnPlanName, () => FavoritesUpdated?.Invoke());

            Observable.FromEvent(a => FavoritesUpdated += a, a => FavoritesUpdated -= a)
                .Throttle(TimeSpan.FromSeconds(2))
                .ObserveOn(Scheduler.Default)
                .Subscribe(_ => LocalStorage.SaveDataToFile(Favorites, m_favouritesFileName));

            Observable.FromEvent(a => BlacklistUpdated += a, a => BlacklistUpdated -= a)
                .Throttle(TimeSpan.FromSeconds(2))
                .ObserveOn(Scheduler.Default)
                .Subscribe(_ => LocalStorage.SaveDataToFile(Blacklist, m_blackListedFileName));

            Observable.FromEvent(a => PermanentBlacklistUpdated += a, a => PermanentBlacklistUpdated -= a)
                .Throttle(TimeSpan.FromSeconds(2))
                .ObserveOn(Scheduler.Default)
                .Subscribe(_ => LocalStorage.SaveDataToFile(PermanentBlacklist, m_permBlackListedFileName));

            _searchYouTube = IsYouTubeAvailable();
            _searchYahoo = Permissions.Nnj.CanScrapeYahoo;
            _canSearchNow = true;

            bool isDesignMode = (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()));
            bool isDesignValues = Environment.GetCommandLineArgs()?.Contains("WithDesignValues") == true;

            if (isDesignValues || isDesignMode)
            {
                ActiveDomains.AddRange(new List<SingleDomain>() {
                    new SingleDomain { IsVisible = true, Address = "asdfasdsdfxcxc.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5", "5", "6", "2", "1", "3", "6", "3", "8") },
                    new SingleDomain { IsVisible = true, IsAvailable = false, IsFavorite = true, Address = "asdfsafdsd.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5", "5", "6", "2", "1", "3", "6", "3", "8") },
                    new SingleDomain { IsVisible = true, IsBlacklisted = true, Address = "asdfsafdsd.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5", "5", "6", "2", "1", "3", "6", "3", "8") },
                    new SingleDomain { IsVisible = true, IsChecking = false, Address = "asdwercxvgfhf.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5", "5", "6", "2", "1", "3", "6", "3", "8") },
                    new SingleDomain { IsVisible = true, IsAvailable = true, Address = "asdgherwtsfvasdf.com", StatsLoaded = true, DomainSource = "Reddit", StatsData = new DomDetailerData("5", "5", "6", "2", "1", "3", "6", "3", "8") },
                    new SingleDomain { IsVisible = true, Address = "bdfsdweghaerthshbadfv.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5", "5", "6", "2", "1", "3", "6", "3", "8") },
                    new SingleDomain { IsVisible = true, Address = "asdf4aw45fsdf.com", IsGettingStats = false, DomainSource = "Reddit", StatsData = new DomDetailerData("5", "5", "6", "2", "1", "3", "6", "3", "8") },
                    new SingleDomain { IsVisible = false,Address = "hidden.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5", "5", "6", "2", "1", "3", "6", "3", "8") },
                    new SingleDomain { IsVisible = true, Address = "adsf4afa2afsdf.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5", "5", "6", "2", "1", "3", "6", "3", "8") },
                    new SingleDomain { IsVisible = true, Address = "fdsgdf5adf3af.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5", "5", "6", "2", "1", "3", "6", "3", "8") },
                    new SingleDomain { IsVisible = true, Address = "adsf7sad5r3sfas4.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5", "5", "6", "2", "1", "3", "6", "3", "8") }
                });

                Favorites = new ObservableCollection<SingleDomain>()
                {
                    new SingleDomain{ Address = "asdfasdsdfxcxc.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsFavorite = true, Address = "asdfsafdsd.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsBlacklisted = true, Address = "asdfsafdsd.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{ IsAvailable = true,Address = "asdwercxvgfhf.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "asdgherwtsfvasdf.com", StatsLoaded = true, DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "bdfsdweghaerthshbadfv.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "asdf4aw45fsdf.com", IsGettingStats = false,DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "dsf4fsdf4sfsa3sadf3.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "adsf4afa2afsdf.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "fdsgdf5adf3af.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "adsf7sad5r3sfas4.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") }
                };


              
                //Blacklist = new ObservableCollection<SingleDomain>()
                //{
                //    new SingleDomain{ Address = "asdfasdsdfxcxc.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                //    new SingleDomain{IsFavorite = true, Address = "asdfsafdsd.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                //    new SingleDomain{IsBlacklisted = true, Address = "asdfsafdsd.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                //    new SingleDomain{IsAvailable = true,Address = "asdwercxvgfhf.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                //    new SingleDomain{Address = "asdgherwtsfvasdf.com", StatsLoaded = true, DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                //    new SingleDomain{Address = "bdfsdweghaerthshbadfv.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                //    new SingleDomain{Address = "asdf4aw45fsdf.com", IsGettingStats = false,DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                //    new SingleDomain{Address = "dsf4fsdf4sfsa3sadf3.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                //    new SingleDomain{Address = "adsf4afa2afsdf.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                //    new SingleDomain{Address = "fdsgdf5adf3af.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                //    new SingleDomain{Address = "adsf7sad5r3sfas4.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") }
                //};
            }


        }

        private void _resultsTimer_Tick(object sender, EventArgs e)
        {
            _youtubeTokenSource?.Cancel();
            _tumblrTokenSource?.Cancel();
            _redditTokenSource?.Cancel();
            _yahooTokenSource?.Cancel();
        }

        private bool IsYouTubeAvailable()
        {
            return Permissions.Pbn.CanScrapeYoutube && !Settings.Default.YouTubeApiKey.IsNullOrEmpty() &&
                   !Settings.Default.YouTubeClientId.IsNullOrEmpty();
        }

        public static IReadOnlyList<dynamic> WebsiteTypes = Enum.GetValues(typeof(AllWebSites)).
          OfType<AllWebSites>().Select(
          x => new
          {
              Value = x,
              Text = x == AllWebSites.None ? "Sort by Web2.0" : x.ToString()
          }).ToList();
        public AllWebSites SelectedWebsite
        {
            get { return _selectedWebsite; }
            set
            {
                _selectedWebsite = value;
            }
        }

        public bool IsSomeSiteSelected
        {
            get
            {
                return _showSelectedToFav;
            }
            set { SetProperty(ref _showSelectedToFav, value); }
        }

        //void Sort(object source)
        //{
        //    var domains = source as ObservableCollection<SingleDomain>;
        //    if (domains != null)
        //    {

        //        if (_selectedWebsite.Equals(AllWebSites.None))
        //        {
        //            IsSomeSiteSelected = false;
        //            domains?.All(j => j.IsVisible = true);

        //            return;
        //        }

        //        IsSomeSiteSelected = true;
        //        foreach (var item in domains)
        //        {
        //            if (item.DomainSource != _selectedWebsite.ToString())
        //            {
        //                item.IsVisible = false;
        //                continue;
        //            }
        //            item.IsVisible = true;
        //        }
        //    }

        //}
        public string SelectedQuery { get; set; }
        public ICommand SortByMetricsCommand => new RelayCommand<object>(SortbyMetrics);


        void SortbyMetrics(object source)
        {
            if (source == null)
                return;
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(source);
            var sortDirection = ListSortDirection.Ascending;
            if ((view.SortDescriptions.Any() && view.SortDescriptions.First().Direction == ListSortDirection.Ascending))
            {
                sortDirection = ListSortDirection.Descending;
            }

            var oldComparer = view.CustomSort as MetricsComparer;
            if (oldComparer != null && oldComparer.SortDirection == ListSortDirection.Ascending)
            {
                sortDirection = ListSortDirection.Descending;
            }

            if (SelectedMetrics > 0)
            {
                view.CustomSort = new MetricsComparer(_selectedMetrics, sortDirection);
            }
            else
            {
                view.CustomSort = null;
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(nameof(SingleDomain.Address), sortDirection));
            }
        }
        public DomDetailerSubViewModel DomainsDomDetailer { get; }

        public DomDetailerSubViewModel DomDetailer { get; }

        public PermissionSet Permissions => PermissionAssistant.GetCurrentPermissionsAndSubscribe(() =>
        {
            OnPropertyChanged(nameof(Permissions));
            if (!IsYouTubeAvailable())
                SearchYouTube = false;

            OnPropertyChanged(nameof(CanScrapeYouTube));
        });


        public bool CanSearchNow
        {
            get { return _canSearchNow; }
            set
            {
                SetProperty(ref _canSearchNow, value);
            }
        }
        public bool CanScrapeYouTube => IsYouTubeAvailable();

        public ICommand TogglePopupCommand => new RelayCommand<Popup>(p => p.IsOpen = !p.IsOpen);

        public string YouTubeToolTipText => !Permissions.Pbn.CanScrapeYoutube
            ? $"This option is not available in the {Permissions.MembershipPlanName} version"
            : "YouTube API key or ClientId is not set";

        public string YahooToolTipText => !Permissions.Pbn.CanScrapYahoo
            ? $"This option is not available in the {Permissions.MembershipPlanName} version"
            : "YouTube API key or ClientId is not set";

        private bool _groupBySource;

        public bool GroupBySource
        {
            get { return _groupBySource; }
            set { SetProperty(ref _groupBySource, value); }
        }

        private bool _favoritesGroupBySource;
        private bool _canSearchNow;

        public bool FavoritesGroupBySource
        {
            get { return _favoritesGroupBySource; }
            set { SetProperty(ref _favoritesGroupBySource, value); }
        }

        string _searchTerm;
        public string SearchTerm
        {
            get { return _searchTerm; }
            set { SetProperty(ref _searchTerm, value); }
        }

        bool _searchActive;
        public bool SearchActive
        {
            get { return _searchActive; }
            set { SetProperty(ref _searchActive, value); }
        }

        bool _searchYouTube;
        public bool SearchYouTube
        {
            get { return _searchYouTube; }
            set { SetProperty(ref _searchYouTube, value); }
        }

        bool _searchTumblr = true;
        public bool SearchTumblr
        {
            get { return _searchTumblr; }
            set { SetProperty(ref _searchTumblr, value); }
        }

        bool _searchYahoo;
        public bool SearchYahoo
        {
            get { return _searchYahoo; }
            set { SetProperty(ref _searchYahoo, value); }
        }


        bool _searchBulk;
        public bool SearchBulk
        {
            get { return _searchBulk; }
            set { SetProperty(ref _searchBulk, value); }
        }

        string _bulkInfo;
        public string BulkInfo
        {
            get { return _bulkInfo; }
            set { SetProperty(ref _bulkInfo, value); }
        }

        bool _searchReddit = true;
        private MetricsTypeEnum _selectedMetrics;
        private AllWebSites _selectedWebsite = AllWebSites.None;
        private bool _showSelectedToFav = false;


        private Action _completeDialogPopup;

        public bool SearchReddit
        {
            get { return _searchReddit; }
            set { SetProperty(ref _searchReddit, value); }
        }
        public ObservableCollection<DomainData> DomainsData { get; } = new ObservableCollection<DomainData>();

        private ObservableCollection<SingleDomain> _activeDomains;
        public ObservableCollection<SingleDomain> ActiveDomains
        {
            get { return _activeDomains; }
            set { SetProperty(ref _activeDomains, value); }
        }
        public int TotalDomainsCount => DomainsData.Sum(d => d.DomainsCount);
        public int AvailableCount =>
            DomainsData.Sum(d => d.Domains.
            Where(x => x.IsAvailable == true).Count());
        public ObservableCollection<SingleDomain> Blacklist { get; } = new ObservableCollection<SingleDomain>();
        event Action BlacklistUpdated;
        public ObservableCollection<SingleDomain> PermanentBlacklist { get; } = new ObservableCollection<SingleDomain>();
        event Action PermanentBlacklistUpdated;
        public event Action<bool> ScrappingNow;
        public ObservableCollection<SingleDomain> Favorites { get; } = new ObservableCollection<SingleDomain>();
        event Action FavoritesUpdated;

        public SelectionSubViewModel Selection { get; }
        public SelectionSubViewModel FavoriteSelection { get; }
        public SaverSubViewModel Saver { get; }

        public ICommand SearchCommand => new RelayCommand(async () => await SearchAsync());

        public ICommand SearchBulkCommand => new RelayCommand(async () => await SearchBulkAsync());


        async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm)) return;

            await CancelSearchAsync();
            _resultsTimer.Start();

            SearchBulk = false;
            SearchActive = true;
            ScrappingNow(false);// false means the IsEnalbled will be set to false on Find Domains Scrapp 
            var youtube = SearchYoutubeAsync(SearchTerm);
            var tumblr = SearchTumblrAsync(SearchTerm);
            var reddit = SearchRedditAsync(SearchTerm);
            var yahoo = SearchYahooAsync(SearchTerm);

            await Task.WhenAll(youtube, tumblr, reddit, yahoo);
            _resultsTimer.Stop();
            SearchActive = false;
            ScrappingNow(true);// true means the IsEnalbled will be set to true on Find Domains Scrapp buttons
            _completeDialogPopup();
        }

     
        async Task SearchBulkAsync()
        {
            string[] keywords = KeywordsProvider.GetFromFile();
            if (keywords == null)
                return;
            var distinctKeywords = keywords.Distinct().ToList();

            await CancelSearchAsync();

            SearchBulk = true;
            SearchActive = true;
            ScrappingNow(false); // false means the IsEnalbled will be set to false on Find Domains Scrapp buttons
            int index = 0;
            _resultsTimer.Start();
            foreach (var query in distinctKeywords)
            {
                if (_bulkTokenSource.IsCancellationRequested)
                    break;
                index++;
                SearchTerm = query;
                var youtube = SearchYoutubeAsync(query);
                var tumblr = SearchTumblrAsync(query);
                var reddit = SearchRedditAsync(query);
                var yahoo = SearchYahooAsync(query);
                BulkInfo = $"Searching for \"{query}\" {index}/{distinctKeywords.Count}";

                await Task.WhenAll(youtube, tumblr, reddit, yahoo);
                _resultsTimer.Stop();
            }
            SearchActive = false;
            ScrappingNow(true);// true means the IsEnalbled will be set to true on Find Domains Scrapp buttons
            _completeDialogPopup();
        }

        async Task SearchYoutubeAsync(string query)
        {
            if (SearchYouTube)
            {
                try
                {
                    _youtubeTokenSource = new CancellationTokenSource();

                    //var constructedSearchTerm = $"*.*.* +{SearchTerm}";
                    var youtubeScraper = new YouTubeScraper(query, _youtubeSubject, _youtubeTokenSource);
                    var scrapeDataTask = youtubeScraper.SearchListAsync();
                    var youtubeApiAssistant = new YouTubeAssistant(query, false, _youtubeSubject, _youtubeTokenSource);
                    var searchApiTask = youtubeApiAssistant.LoadDataAsync();

                    await Task.WhenAll(scrapeDataTask, searchApiTask);
                    //foreach (var postfix in DomainParser.SubdomainPostfixes)
                    //{
                    //    var constructedSearchTerm = $"*{postfix} +{SearchTerm}";
                    //    var youtube = new YouTubeAssistant(constructedSearchTerm, false, _youtubeSubject, _youtubeTokenSource.Token);
                    //    await youtube.LoadDataAsync();
                    //    if (_youtubeTokenSource.Token.IsCancellationRequested)
                    //    {
                    //        break;
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    _log.Error("Exception while executing SearchYoutubeAsync()", ex);
                    throw;
                }
            }
        }

        async Task SearchTumblrAsync(string query)
        {
            if (SearchTumblr)
            {
                _tumblrTokenSource = new CancellationTokenSource();
                foreach (var postfix in DomainParser.SubdomainPostfixes)
                {
                    var constructedSearchTerm = $@"""http://*{postfix} ""{query}""""";
                    var results = new ObservableCollection<BaseSearchResult>();
                    results.CollectionChanged += (s, e) =>
                    {
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            var result = e.NewItems[0] as BaseSearchResult;
                            AddDomains(result.Domains, result.SourceAddress, query);
                        }
                    };
                    var provider = new TumblrDataProvider(constructedSearchTerm, results, _tumblrTokenSource.Token,
                        DomainParser.ParseSubdomain);
                    await Task.Run(async () => { await provider.LoadMoreItemsAsync(int.MaxValue); });
                    if (_tumblrTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
        }

        async Task SearchRedditAsync(string query)
        {
            if (SearchReddit)
            {
                _redditTokenSource = new CancellationTokenSource();
                foreach (var postfix in DomainParser.SubdomainPostfixes)
                {
                    var constructedSearchTerm = $"(site:{postfix.TrimStart('.')} AND {query})";
                    var results = new ObservableCollection<BaseSearchResult>();
                    results.CollectionChanged += (s, e) =>
                    {
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            var result = e.NewItems[0] as BaseSearchResult;
                            AddDomains(result.Domains, result.SourceAddress, query);
                        }
                    };
                    var provider = new RedditDataProvider(constructedSearchTerm, results, _redditTokenSource.Token, DomainParser.ParseSubdomain);
                    await Task.Run(async () => { await provider.LoadMoreItemsAsync(int.MaxValue); });
                    if (_redditTokenSource.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
        }

        async Task SearchYahooAsync(string query)
        {
            if (SearchYahoo)
            {
                _yahooTokenSource = new CancellationTokenSource();
                // foreach (var postfix in DomainParser.SubdomainPostfixes)
                {
                    // var constructedSearchTerm = $"(site:{postfix.TrimStart('.')} AND {query})";
                    var results = new ObservableCollection<BaseSearchResult>();
                    results.CollectionChanged += (s, e) =>
                    {
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            var result = e.NewItems[0] as BaseSearchResult;

                            AddDomains(result.Domains, result.SourceAddress, query);
                        }
                    };
                    var provider = new YahooDataProvider(query, results, _yahooTokenSource.Token, DomainParser.ParseSubdomain);
                    await Task.Run(async () => { await provider.LoadMoreItemsAsync(int.MaxValue); });

                }
            }
        }
        async Task CancelSearchAsync()
        {
            if (SearchActive)
            {
                _youtubeTokenSource.Cancel();
                _tumblrTokenSource.Cancel();
                _redditTokenSource.Cancel();
                _yahooTokenSource.Cancel();
                if (!_bulkTokenSource.IsCancellationRequested)
                    _bulkTokenSource.Cancel();

                if (SearchActive)
                {
                    await Task.Delay(500);
                }
            }

            ActiveDomains.Clear();
        }

        void OnVideoLoad(VideoDto video)
        {
            try
            {
                var domains = DomainParser.ParseSubdomain(video.Description, "YouTube", video.Term);

                //var domains = DomainParser.ParseSubdomain(video.Snippet.Description, "YouTube");

                //foreach (var domain in domains)
                //{
                //    if (Domains.Count(d => d.IsYouTube) >= MaxDomains)
                //    {
                //        _youtubeTokenSource.Cancel();
                //        break;
                //    }

                //    if (Domains.None(domain.Isomorphic) && _blacklist.None(domain.Isomorphic))
                //    {
                //        var favorite = _favorites.FirstOrDefault(domain.Isomorphic);
                //        Domains.Add(favorite ?? domain);
                //    }
                //}
                string sourceURL = "youtube.com";
                if (!string.IsNullOrWhiteSpace(video.SourceURL))
                    sourceURL = video.SourceURL;
                AddDomains(domains, sourceURL, video?.Term);
            }
            catch (Exception)
            {
            }
        }

        void AddDomains(IEnumerable<SingleDomain> domains, string sourceURL, string query)
        {
            foreach (var domain in domains)
            {
                domain.SourceURL = sourceURL;
                domain.BaseQuery = query;
                domain.Address = domain.Address.CleanDomainUrl();
                if (!DoesDomainAlreadyExistsInAll(domain.Address)
                    && Blacklist.None(domain.Isomorphic)
                    && Favorites.None(domain.Isomorphic)
                    )
                {
                    _resultsTimer.Stop();//new domain added
                                         //todo - Umar apply the regex on {domain.Address}
                                         //umar: comment, since the domains are scraped, 
                                         //we cannot be exactly sure of what extension to extract so we will only use the most commonly used domains



                    //umar update: completed, please confirm at your end and remove from task list if tests satisfied
                    //this also results in duplicates for address in list e.g. abc.tumblr.com shows twice for different sources
                    
                 //   ActiveDomains.Add(domain);
                    DomainData domainData = null;
                    if (KeywordDomainExists(domain.BaseQuery, ref domainData))
                    {
                       domainData.Domains.Add(domain);
                       AddDomainToActiveList(domain);
                    }
                    else
                    {
                        domainData = new DomainData() { SearchQuery = domain.BaseQuery };
                        domainData.Domains.Add(domain);
                        DomainsData.Add(domainData);
                        AddDomainToActiveList(domain);
                        // Domains.Add(domain);
                    }
                    _resultsTimer.Start();
                    OnPropertyChanged(nameof(TotalDomainsCount));

                }
            }

        }
        private bool DoesDomainAlreadyExistsInAll(string domainAddress)
        {
            //var stopWatch = Stopwatch.StartNew();
            var domains = DomainsData.Select(d => d.Domains);
            var allDomains = domains.SelectMany(d => d.Select(x => x)).ToList();
            var @bool = allDomains.Any((x) => x.Address.ToLowerInvariant().Equals(domainAddress.ToLowerInvariant()));
           // Debug.WriteLine(stopWatch.ElapsedMilliseconds);
            return @bool;
            // return keywordDomains.Any((x) => x.Address.ToLowerInvariant().Equals(domainAddress.ToLowerInvariant()));
        }
        private void AddDomainToActiveList(SingleDomain domain)
        {
            if (String.IsNullOrWhiteSpace(SelectedQuery))
            {
                ActiveDomains.Add(domain);
                return;
            }
        }
        private bool KeywordDomainExists(string query, ref DomainData domainData)
        {
            domainData = DomainsData.Where(d => d.SearchQuery.Equals(query)).FirstOrDefault();
            if (domainData == null)
                return false;
            return true;
        }

        public ICommand CheckCommand => new RelayCommand<SingleDomain>(async d => await CheckSingle(d));

        async Task CheckSingle(SingleDomain domain)
        {
            domain.IsChecking = true;

            var result = await SubdomainAssistant.IsAvailableAsync(domain.Address);
            domain.IsAvailable = result;
            domain.Status = result.HasValue ? result.Value ? "Available" : "Unavailable" : "Unknown";
            if (domain.IsAvailable == true)
            {
                OnPropertyChanged(nameof(AvailableCount));
            }

            domain.IsChecking = false;
        }

        public ICommand CheckAllCommand =>
            new RelayCommand(async () => await CheckMultiple(ActiveDomains.Where(d => d.IsAvailable == null).ToList()));

        public ICommand CheckSelectedCommand =>
            new RelayCommand(async () => await CheckMultiple(ActiveDomains.Where(d => d.IsAvailable == null && d.IsSelected).ToList()));

        async Task CheckMultiple(List<SingleDomain> domains)
        {
            foreach (var item in domains)
            {
                item.IsChecking = true;
            }

            foreach (var chunk in domains.ToChunks(5).ToList())
            {
                await chunk.Select(CheckSingle).WhenAll();
            }

            if (Permissions.Nnj.CanDeleteUnavailable)
            {
                if (Settings.Default.AutoDel)
                {
                    BlacklistDomains(ActiveDomains.Where(d => d.IsAvailable == false));
                    RemoveDomainsFromDomainData(ActiveDomains.Where(d => d.IsAvailable == false));
                    ActiveDomains.RemoveWhere(d => d.IsAvailable == false);
                    OnPropertyChanged(nameof(TotalDomainsCount));

                }
            }
            if (Permissions.Nnj.ShowAutoStats) //if plan is elite or trail
            {
                if (Settings.Default.AutoStats) // user selected to get stats automatically
                {
                    await DomainsDomDetailer.GetsStatsForDomains(ActiveDomains.Where(d => d.IsAvailable == true));//.GetStatsForMultipleCommand.Execute(Domains.Where(d=>d.IsAvailable == true));
                }
            }
        }

        private void RemoveDomainsFromDomainData(IEnumerable<SingleDomain> domainsToDelete)
        {
            var domainsToBeRemoved = domainsToDelete.ToList();
            foreach (var item in domainsToBeRemoved)
            {
                DomainsData.Where(d => d.SearchQuery.Equals(item.BaseQuery))
                   .FirstOrDefault()?.Domains.Remove(item);
                OnPropertyChanged(nameof(TotalDomainsCount));
            }
        }

        private void RemoveDomainFromDomainData(SingleDomain domainToDelete)
        {

            DomainsData.Where(d => d.SearchQuery.Equals(domainToDelete.BaseQuery))
               .FirstOrDefault()?.Domains.Remove(domainToDelete);
            OnPropertyChanged(nameof(TotalDomainsCount));

        }
        public ICommand BlacklistDomainCommand => new RelayCommand<SingleDomain>(BlacklistDomain);

        void BlacklistDomain(SingleDomain domain)
        {
            domain.IsBlacklisted = true;
            Blacklist.Add(domain);
            BlacklistUpdated?.Invoke();
        }

        public ICommand UnblacklistDomainCommand => new RelayCommand<SingleDomain>(UnblacklistDomain);

        void UnblacklistDomain(SingleDomain domain)
        {
            domain.IsBlacklisted = false;
            Blacklist.Remove(domain);
            BlacklistUpdated?.Invoke();
        }

        public ICommand ToggleBlacklistDomainCommand => new RelayCommand<SingleDomain>(d =>
        {
            if (d.IsBlacklisted == null || !(bool)d.IsBlacklisted)
            {
                BlacklistDomain(d);
            }
            else
            {
                UnblacklistDomain(d);
            }
        });

        public ICommand ClearBlacklistCommand => new RelayCommand(() =>
        {
            while (Blacklist.Count > 0)
            {
                UnblacklistDomain(Blacklist.First());
            }
        });

        public ICommand BlacklistAllDomainsCommand => new RelayCommand(() => BlacklistDomains(ActiveDomains));
        public ICommand UnlacklistAllDomainsCommand => new RelayCommand(() => UnblacklistDomains(ActiveDomains));
        public ICommand BlacklistUnavailableDomainsCommand => new RelayCommand(() => BlacklistDomains(ActiveDomains.Where(d => d.IsAvailable == false)));
        public ICommand BlacklistUnknownDomainsCommand => new RelayCommand(() => BlacklistDomains(ActiveDomains.Where(d => d.IsAvailable == null)));
        public ICommand BlacklistSelectedDomainsCommand => new RelayCommand(() => BlacklistDomains(ActiveDomains.Where(d => d.IsSelected)));

        public ICommand ToggleSelectAllBlacklistDomainsCommand => new RelayCommand<bool>((b) =>
        {
            foreach (var domain in Blacklist)
            {
                domain.IsSelected = b;
            }
        });

        void BlacklistDomains(IEnumerable<SingleDomain> domains)
        {
            foreach (var domain in domains.Where(d => d.IsBlacklisted != true))
            {
                BlacklistDomain(domain);
            }
        }

        void UnblacklistDomains(IEnumerable<SingleDomain> domains)
        {
            foreach (var domain in domains.Where(d => d.IsBlacklisted == true))
            {
                domain.IsBlacklisted = false;
                Blacklist.Remove(domain);
            }

            BlacklistUpdated?.Invoke();
        }

        public ICommand AddToFavoritesCommand => new RelayCommand<SingleDomain>(AddToFavorites);

        void AddToFavorites(SingleDomain domain)
        {
            domain.IsFavorite = true;
            Favorites.Add(domain);
            FavoritesUpdated?.Invoke();
        }

        public ICommand RemoveFromFavoritesCommand => new RelayCommand<SingleDomain>(RemoveFromFavorites);

        void RemoveFromFavorites(SingleDomain domain)
        {
            domain.IsFavorite = false;
            Favorites.Remove(domain);
            FavoritesUpdated?.Invoke();
        }
        public ICommand AddSelectedFavoritesToBlacklistCommand => new RelayCommand(() =>
        {

            Favorites.Where(d => d.IsBlacklisted != true && d.IsSelected).ToList()
                        .ForEach(x =>
                        {
                            x.IsBlacklisted = true;
                            Blacklist.Add(x);
                        });

            FavoritesUpdated?.Invoke();
        });
        public ICommand RemoveSelectedFavoriteFromFavoriteCommand => new RelayCommand(() =>
        {
            Favorites.Where(d => d.IsSelected).ToList()
            .ForEach(d => Favorites.Remove(d));

            FavoritesUpdated?.Invoke();
        });

        public ICommand RemoveBlacklistedFavoritesFromFavoritesCommand => new RelayCommand(() =>
        {
            Favorites.Where(d => d.IsBlacklisted != null && (bool)d.IsBlacklisted).ToList()
            .ForEach(d => Favorites.Remove(d));

            FavoritesUpdated?.Invoke();
        });

        public ICommand ToggleAddToFavoriteCommand => new RelayCommand<SingleDomain>(domain =>
        {
            if (domain.IsFavorite == null || !(bool)domain.IsFavorite)
            {
                domain.IsFavorite = true;
                Favorites.Add(domain);
                FavoritesUpdated?.Invoke();
            }
            else
            {
                domain.IsFavorite = false;
                Favorites.Remove(domain);
                FavoritesUpdated?.Invoke();
            }
        });

        public ICommand ClearFavoritesCommand => new RelayCommand(() =>
        {
            while (Favorites.Count > 0)
            {
                RemoveFromFavorites(Favorites.First());
            }
        });

        public ICommand AddAvailableToFavoritesCommand => new RelayCommand(() =>
        {
            foreach (var domain in ActiveDomains.Where(d => d.IsAvailable == true && d.IsFavorite != true))
            {
                AddToFavorites(domain);
            }
        });

        public ICommand FavoriteSelectedDomainsCommand => new RelayCommand(() =>
        {
            foreach (var domain in ActiveDomains)
            {
                if (domain.IsSelected && (domain.IsFavorite == null || !(bool)domain.IsFavorite))
                {
                    domain.IsFavorite = true;
                    Favorites.Add(domain);
                }
            }

            FavoritesUpdated?.Invoke();

        });

        //public ICommand SortCommand => new RelayCommand<object>(Sort);

        public ICommand GroupCommand => new RelayCommand<object>(p =>
        {
            GroupBySource = !GroupBySource;
            Group(p, GroupBySource);
        });

        public ICommand GroupFavoritesCommand => new RelayCommand<object>(p =>
        {
            FavoritesGroupBySource = !FavoritesGroupBySource;
            Group(p, FavoritesGroupBySource);
        });

        public MetricsTypeEnum SelectedMetrics
        {
            get { return _selectedMetrics; }
            set
            {
                _selectedMetrics = value;
            }
        }

        public ICommand DeleteAllDomainsCommand => new RelayCommand(() =>
        {
            ActiveDomains?.Clear();
            DomainsData?.Clear();
        });

        public ICommand DeleteDomainCommand => new RelayCommand<SingleDomain>((d) =>
        {
            RemoveDomainFromDomainData(d);
            ActiveDomains.Remove(d);
            OnPropertyChanged(nameof(TotalDomainsCount));
        });

        public ICommand RemoveFromBlacklistCommand => new RelayCommand<SingleDomain>(domain =>
        {
            domain.IsBlacklisted = false;
            Blacklist.Remove(domain);
            BlacklistUpdated?.Invoke();
        });

        public ICommand RemoveSelectedFromBlacklistCommand => new RelayCommand(() =>
        {
            Blacklist.
            Where(d => d.IsSelected).ToList().
            ForEach(d =>
            {
                d.IsBlacklisted = false;
                Blacklist.Remove(d);
            });

            BlacklistUpdated?.Invoke();
        });

        public ICommand DeleteSelectedDomainsCommand => new RelayCommand(() =>
        {
            var selectedDomains = ActiveDomains.Where(x => x.IsSelected).ToList();
            RemoveDomainsFromDomainData(ActiveDomains.Where(d => d.IsSelected));
            ActiveDomains.RemoveWhere(d => d.IsSelected);
            OnPropertyChanged(nameof(TotalDomainsCount));

        });



        public ICommand DeleteBlacklistedDomainsCommand => new RelayCommand(() =>
        {
            var blacklisted = ActiveDomains.Where(d => d.IsBlacklisted == true).ToList();
            RemoveDomainsFromDomainData(ActiveDomains.Where(d => d.IsBlacklisted ==true));
            ActiveDomains.RemoveWhere(d => d.IsBlacklisted == true);
            OnPropertyChanged(nameof(TotalDomainsCount));

        });
        public ICommand DeleteUnavailableDomainsCommand => new RelayCommand(() =>
        {
            var unavailable = ActiveDomains.Where(d => d.IsAvailable == false).ToList();
            RemoveDomainsFromDomainData(ActiveDomains.Where(d => d.IsAvailable == false));
            ActiveDomains.RemoveWhere(d => d.IsAvailable == false);
            OnPropertyChanged(nameof(TotalDomainsCount));

        });

        public ICommand FindRefrencesCommand => new RelayCommand<string>(s =>
        {
            if (!Permissions.Pbn.CanUseExplore)
            {
                PermissionAssistant.ShowPermissionDeniedMessage(Permissions.PbnPlanName);
                return;
            }

            new RefrencesDialog(s).ShowDialog();
        });



        #region PermanentBlacklistCommands
        public ICommand AddPermBlacklistDomainCommand =>
            new RelayCommand<SingleDomain>((d) =>
            {
                PermantlyBlackListDomain(d, ActiveDomains);
                OnPropertyChanged(nameof(TotalDomainsCount));
            });

        public ICommand PermBlacklistUnavailableDomainsCommand => new RelayCommand(
            async () =>
            {
                var unavailableDomains = ActiveDomains.Where(d => d.IsAvailable == false);
                var unavialableList = unavailableDomains.ToList();
                if (await PermantlyBlackListDomains(unavailableDomains, ActiveDomains))
                    OnPropertyChanged(nameof(TotalDomainsCount));

            });

        public ICommand PermBlacklistUnknownDomainsCommand => new RelayCommand(
            async () =>
            {
                var unKnownDomains = ActiveDomains.Where(d => d.IsAvailable == null);
                var unKnownList = unKnownDomains.ToList();
                if (await PermantlyBlackListDomains(unKnownDomains, ActiveDomains))
                    OnPropertyChanged(nameof(TotalDomainsCount));

            });

        public ICommand PermBlacklistSelectedDomainsCommand => new RelayCommand(
            async () =>
            {
                var selectedDomains = ActiveDomains.Where(d => d.IsSelected);
                var selectedList = selectedDomains.ToList();
                if (await PermantlyBlackListDomains(selectedDomains, ActiveDomains))
                    OnPropertyChanged(nameof(TotalDomainsCount));

            });

        public ICommand PermBlackListSelectedFavCommand => new RelayCommand(
            async () =>
        {
            await PermantlyBlackListDomains(Favorites.Where(d => d.IsSelected), Favorites);
            FavoritesUpdated?.Invoke();
        }
        );
        public ICommand AddPermBlacklistFavDomainCommand =>
            new RelayCommand<SingleDomain>((d) =>
            {
                PermantlyBlackListDomain(d, Favorites);
                FavoritesUpdated?.Invoke();
            });

        public ICommand SelectedBlackListToPermCommand =>
            new RelayCommand(async () =>
            {
                await PermantlyBlackListDomains(Blacklist.Where(d => d.IsSelected), Blacklist);
                BlacklistUpdated?.Invoke();
            });
        public ICommand AddBlackListToPermBlacklistCommand =>
            new RelayCommand<SingleDomain>((d) =>
            {
                PermantlyBlackListDomain(d, Blacklist);
                BlacklistUpdated?.Invoke();
            });



        #endregion
        public ICommand DeleteSelectedKeywordCommand =>
        new RelayCommand<DomainData>((_) => DeleteSelectedKeyword(_));

        public ICommand FilterDomainCommand =>
                    new RelayCommand<DomainData>((_) => ShowFilteredDomains(_));
      

        private void DeleteSelectedKeyword(DomainData domainData)
        {
            if (domainData.SearchQuery.Equals(SearchTerm))
                ActiveDomains.Clear();
            DomainsData.Remove(domainData);

            OnPropertyChanged(nameof(TotalDomainsCount));
        }

        private void ShowFilteredDomains(DomainData domainData)
        {
            SelectedQuery = domainData.SearchQuery;
             var relatedDomains = DomainsData.Where(d => d.SearchQuery.Equals(domainData.SearchQuery))
               .FirstOrDefault();
            ActiveDomains = relatedDomains.Domains;
            Selection.ChangeDomainsReference(ActiveDomains);
        }

        private void PermantlyBlackListDomain(SingleDomain domain, IList<SingleDomain> domainList)
        {
            domain.IsBlacklistedPermanent = true;
            PermanentBlacklist.Add(domain);
            RemoveDomainFromDomainData(domain);
            domainList.Remove(domain);
            PermanentBlacklistUpdated?.Invoke();
        }

        private async Task<bool> PermantlyBlackListDomains(IEnumerable<SingleDomain> domains, ObservableCollection<SingleDomain> sourceDomains)
        {
            bool confirmation = await ConfirmUser("Confirmation", "Are you sure permanently blacklisting these items? The items will never be shown again.");

            if (!confirmation)
            {
                return false;
            }

            var domainsList = domains?.ToList();
            if (domainsList == null)
                return false;
            for (int i = domainsList.Count - 1; i >= 0; i--)
            {
                var item = domainsList[i];
                if (item == null)
                    continue;
                item.IsBlacklistedPermanent = true;
                PermanentBlacklist.Add(item);
                RemoveDomainFromDomainData(item);
                sourceDomains.Remove(item);
            }

            PermanentBlacklistUpdated?.Invoke();
            return true;
        }
        public async Task LoadDataAsync()
        {
            var domains = await Task.Run(() =>
            {
                return new
                {
                    favorites = LocalStorage.GetDataFromFile<List<SingleDomain>>(m_favouritesFileName) ?? new List<SingleDomain>(),
                    blacklist = LocalStorage.GetDataFromFile<List<SingleDomain>>(m_blackListedFileName) ?? new List<SingleDomain>(),
                    permBlacklist = LocalStorage.GetDataFromFile<List<SingleDomain>>(m_permBlackListedFileName) ?? new List<SingleDomain>()
                };
            });
            //todo - look if we have to do this with permblacklist too
            if (domains.blacklist.Any())
            {
                for (int i = 0; i < domains.favorites.Count; i++)
                {
                    var item = domains.favorites[i];
                    var blacklistedDomain = domains.blacklist.FirstOrDefault(d => d.Address == item.Address);
                    if (blacklistedDomain != null)
                    {
                        blacklistedDomain.IsFavorite = true;
                        domains.favorites[i] = blacklistedDomain;
                    }
                }
            }

            Favorites.AddRange(domains.favorites);
            Blacklist.AddRange(domains.blacklist);
            PermanentBlacklist.AddRange(domains.permBlacklist);
        }

        private bool IsDomainBlacklisted(string domainAddress)
        {
            if (Blacklist.Any((x) => x.Address.ToLowerInvariant().Equals(domainAddress.ToLowerInvariant())))
                return true;
            if (PermanentBlacklist.Any((x) => x.Address.ToLowerInvariant().Equals(domainAddress.ToLowerInvariant())))
                return true;
            return false;
        }

        private void Group(object source, bool flag)
        {
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(source);
            if (flag)
            {
                if (view.GroupDescriptions != null)
                {
                    view.GroupDescriptions.Clear();
                    view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SingleDomain.DomainSource)));
                }
            }
            else
            {
                view.GroupDescriptions?.Clear();
            }
        }

        //void Sort(object source)
        //{
        //    var view = (ListCollectionView)CollectionViewSource.GetDefaultView(source);
        //    var sortDirection = ListSortDirection.Ascending;
        //    if ((view.SortDescriptions.Any() && view.SortDescriptions.First().Direction == ListSortDirection.Ascending))
        //    {
        //        sortDirection = ListSortDirection.Descending;
        //    }

        //    var oldComparer = view.CustomSort as MetricsComparer;
        //    if (oldComparer != null && oldComparer.SortDirection == ListSortDirection.Ascending)
        //    {
        //        sortDirection = ListSortDirection.Descending;
        //    }

        //    if (SelectedMetrics > 0)
        //    {
        //        view.CustomSort = new MetricsComparer(_selectedMetrics, sortDirection);
        //    }
        //    else
        //    {
        //        view.CustomSort = null;
        //        view.SortDescriptions.Add(new SortDescription(nameof(SingleDomain.Address), sortDirection));
        //    }
        //}
        public void SetupCompleteDialog(Action popup)
        {
            _completeDialogPopup = popup;
        }


        private async Task<bool> ConfirmUser(string title, string message)
        {
            return await System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                var result = new ConfirmationDialog(title, message).ShowDialog();

                return (bool)result;
            });
        }

    }
}
