using log4net;
using Newtonsoft.Json.Linq;
using NicheNameJacker.Commands;
using NicheNameJacker.Common;
using NicheNameJacker.Common.Membership;
using NicheNameJacker.Controls;
using NicheNameJacker.DataProviders;
using NicheNameJacker.Extensions;
using NicheNameJacker.Properties;
using NicheNameJacker.Schema;
using NicheNameJacker.Utilities;
using NicheNameJacker.ViewModels.SubViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace NicheNameJacker.ViewModels
{
    public class MultiDomainModel : ObservableBase
    {
        private int m_count;
        public string Keyword { get; set; }
        public int Count
        {
            get { return m_count; }
            set { SetProperty(ref m_count, value); }
        }
    }
    public sealed class MainViewModel : ObservableBase, IDisposable
    {
        private string m_favouritesFileName = "FavoriteDomainsSaved.xml";
        private string m_blackListedFileName = "BlacklistedDomainsSaved.xml";
        private string m_permanentBlackListedFileName = "PermBlacklistedDomainsSaved.xml";
        private CancellationTokenSource _ctsReddit;
        private CancellationTokenSource _ctsTumblr;
        private CancellationTokenSource _ctsYouTube;
        private CancellationTokenSource _ctsWikipedia;
        private Action _completeDialogPopup;
       
        private CancellationTokenSource _ctsHuffpost;
        private CancellationTokenSource _ctsMashable;
        private CancellationTokenSource _ctsYahoo;

        private CancellationTokenSource _ctsBulk;

        private System.Windows.Forms.Timer _resultsTimer;
        private static readonly ILog _log =
        LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MainViewModel()
        {

            _resultsTimer = new System.Windows.Forms.Timer();
            _resultsTimer.Interval = 60000;  // 1 minute interval if no results are obiained for 1 minute we will halt the search
            _resultsTimer.Tick += _resultsTimer_Tick;
            _ctsReddit = new CancellationTokenSource();
            _ctsTumblr = new CancellationTokenSource();
            _ctsYouTube = new CancellationTokenSource();
            _ctsWikipedia = new CancellationTokenSource();
            _ctsHuffpost = new CancellationTokenSource();
            _ctsBulk = new CancellationTokenSource();
            _ctsMashable = new CancellationTokenSource();
            _ctsYahoo = new CancellationTokenSource();
            ActiveDomains = new ObservableCollection<SingleDomain>();

            ActiveDomains.CollectionChanged += (s, e) => OnPropertyChanged(nameof(AvailableCount));
            

            Observable.FromEvent(a => FavoriteDomainsUpdated += a, a => FavoriteDomainsUpdated -= a)
                .Throttle(TimeSpan.FromSeconds(2))
                .ObserveOn(Scheduler.Default)
                .Subscribe(_ => LocalStorage.SaveDataToFile(FavoriteDomains, m_favouritesFileName));

            Observable.FromEvent(a => BlacklistedDomainsUpdated += a, a => BlacklistedDomainsUpdated -= a)
                .Throttle(TimeSpan.FromSeconds(2))
                .ObserveOn(Scheduler.Default)
                .Subscribe(_ => LocalStorage.SaveDataToFile(BlacklistedDomains, m_blackListedFileName));

            Observable.FromEvent(a => PermBlacklistedDomainsUpdated += a, a => PermBlacklistedDomainsUpdated -= a)
                .Throttle(TimeSpan.FromSeconds(2))
                .ObserveOn(Scheduler.Default)
                .Subscribe(_ => LocalStorage.SaveDataToFile(PermBlacklistedDomains, m_permanentBlackListedFileName));

            Selection = new SelectionSubViewModel(ActiveDomains);
            FavoriteSelection = new SelectionSubViewModel(FavoriteDomains);
            DomainsDomDetailer = new DomDetailerSubViewModel(ActiveDomains, Permissions.Nnj, Permissions.NnjPlanName, () => { });
            DomDetailer = new DomDetailerSubViewModel(FavoriteDomains, Permissions.Nnj, Permissions.NnjPlanName, () => FavoriteDomainsUpdated?.Invoke());

            SelectedMetrics = MetricsTypeEnum.None;
            SelectedWebsite = AllWebSites.None;
            _searchInYouTube = IsYouTubeAvailable();
            _searchWiki = Permissions.Nnj.CanScrapeWiki;
            _searchHuffpost = Permissions.Nnj.CanScrapeHuffPost;
            _searchMashable = Permissions.Nnj.CanScrapeMashable;
            _searchYahoo = Permissions.Nnj.CanScrapeYahoo;
            _showUpdateLink = Permissions.Nnj.ShowUpgradeLink;
            Pbn.ScrappingNow += Pbn_ScrappingNow;
            if (PermissionAssistant.CurrentPermissions.MembershipPlan != MembershipPlan.Lite)
                Settings.Default.IsDomDetailerAvailable = true;
            //if ((DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())))
            //{
            bool isDesignMode = (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()));
            bool isDesignValues = Environment.GetCommandLineArgs()?.Contains("WithDesignValues") == true;

            if (isDesignValues || isDesignMode)
            {
                Domains.AddRange(new ObservableCollection<SingleDomain>()
                {
                    new SingleDomain{IsVisible = true, Address = "asdfasdsdfxcxc.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsVisible = true,IsAvailable = false, IsFavorite = true, Address = "asdfsafdsd.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsVisible = true,IsBlacklisted = true, Address = "asdfsafdsd.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsVisible = true,  IsChecking = false,Address = "asdwercxvgfhf.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsVisible = false,IsAvailable = true,Address = "asdgherwtsfvasdf.com", StatsLoaded = true, DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsVisible = true,Address = "bdfsdweghaerthshbadfv.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsVisible = true,Address = "asdf4aw45fsdf.com", IsGettingStats = false,DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsVisible = true,Address = "dsf4fsdf4sfsa3sadf3.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsVisible = true,Address = "adsf4afa2afsdf.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsVisible = true,Address = "fdsgdf5adf3af.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsVisible = true,Address = "adsf7sad5r3sfas4.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") }
                });


                BlacklistedDomains.AddRange(new ObservableCollection<SingleDomain>()
                {
                    new SingleDomain{ Address = "asdfasdsdfxcxc.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsFavorite = true, Address = "asdfsafdsd.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsBlacklisted = true, Address = "asdfsafdsd.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{IsAvailable = true,Address = "asdwercxvgfhf.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "asdgherwtsfvasdf.com", StatsLoaded = true, DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "bdfsdweghaerthshbadfv.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "asdf4aw45fsdf.com", IsGettingStats = false,DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "dsf4fsdf4sfsa3sadf3.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "adsf4afa2afsdf.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "fdsgdf5adf3af.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new SingleDomain{Address = "adsf7sad5r3sfas4.com", DomainSource = "Reddit", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") }
                });

                FavoriteDomains.AddRange(new ObservableCollection<SingleDomain>()
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
                });

            }
        }

        private void _resultsTimer_Tick(object sender, EventArgs e)
        {

            this._ctsReddit.Cancel();
            this._ctsTumblr.Cancel();
            this._ctsYouTube.Cancel();
            _ctsWikipedia.Cancel();
            _ctsHuffpost.Cancel();
            _ctsMashable.Cancel();
            _ctsYahoo.Cancel();
            //  _ctsBulk.Cancel();
        }

        private void Pbn_ScrappingNow(bool showScrapButtons)
        {
            CanSearchNow = showScrapButtons;
        }

        private bool IsYouTubeAvailable()
        {
            return Permissions.Nnj.CanScrapeYoutube && !Settings.Default.YouTubeApiKey.IsNullOrEmpty() &&
                   !Settings.Default.YouTubeClientId.IsNullOrEmpty();
        }


        public string Title => $"{Permissions.MembershipPlanFullname} v{AssemblyInfo.VersionNumber}";

        public SelectionSubViewModel FavoriteSelection { get; }

        public ObservableCollection<BaseSearchResult> Results { get; } = new ObservableCollection<BaseSearchResult>();

        public ObservableCollection<SingleDomain> Domains { get; } = new ObservableCollection<SingleDomain>();
        public ObservableCollection<DomainData> DomainsData { get; } = new ObservableCollection<DomainData>();
        private ObservableCollection<SingleDomain> _activeDomains;
        public ObservableCollection<SingleDomain> ActiveDomains {
            get { return _activeDomains; }
            set { SetProperty(ref _activeDomains, value); }
        }
        public string SelectedQuery { get; set; }
        public int TotalDomainsCount => DomainsData.Sum(d => d.DomainsCount);
        public int AvailableCount => 
            DomainsData.Sum(d=>d.Domains.
            Where(x=>x.IsAvailable==true).Count());//Domains.Count(d => d.IsAvailable == true);

        public Action FavoriteDomainsUpdated;
        public ObservableCollection<SingleDomain> FavoriteDomains { get; } = new ObservableCollection<SingleDomain>();

        public Action BlacklistedDomainsUpdated;
        public Action PermBlacklistedDomainsUpdated;
        public ObservableCollection<SingleDomain> BlacklistedDomains { get; } = new ObservableCollection<SingleDomain>();
        public ObservableCollection<SingleDomain> PermBlacklistedDomains { get; } = new ObservableCollection<SingleDomain>();

        public PbnViewModel Pbn { get; } = new PbnViewModel();

        public BulkCheckerViewModel BulkChecker { get; } = new BulkCheckerViewModel();

        public DomDetailerSubViewModel DomainsDomDetailer { get; }

        public DomDetailerSubViewModel DomDetailer { get; }

        public SelectionSubViewModel Selection { get; }

        public PermissionSet Permissions => PermissionAssistant.GetCurrentPermissionsAndSubscribe(() =>
        {
            OnPropertyChanged(nameof(Permissions));
            OnPropertyChanged(nameof(Title));

            if (!IsYouTubeAvailable())
                SearchInYouTube = false;

            BulkChecker.IsDomDetailerAvailable = Settings.Default.IsDomDetailerAvailable;
            OnPropertyChanged(nameof(CanScrapeYouTube));
        });

        public bool CanScrapeYouTube => IsYouTubeAvailable();

        public string YouTubeToolTipText => !Permissions.Nnj.CanScrapeYoutube
            ? $"This option is not available in the {Permissions.MembershipPlanName} version"
            : "YouTube API key or ClientId is not set";

        public AppUpdaterSubViewModel AppUpdater { get; } = new AppUpdaterSubViewModel();

        private bool _domainsVisible = false;
        public bool DomainsVisible
        {
            get { return _domainsVisible; }
            set { SetProperty(ref _domainsVisible, value); }
        }

       // public ObservableCollection<MultiDomainModel> MultiKeywordCounts { get; set; } = new ObservableCollection<MultiDomainModel>();
        public Dictionary<string, int> MultiDomainDictionary { get; set; } = new Dictionary<string, int>();
        public ICommand SearchCommand => new RelayCommand(async () => await SearchAsync());

        public ICommand SearchBulkCommand => new RelayCommand(async () => await SearchManyAsync());

        public ICommand TogglePopupCommand => new RelayCommand<Popup>(p => p.IsOpen = !p.IsOpen);

        public ICommand ClosePopupCommand => new RelayCommand<Popup>(p => p.IsOpen = false);

        public ICommand CheckCommand => new RelayCommand<SingleDomain>(async domain =>
        {
            await CheckSingle(domain);
            OnPropertyChanged(nameof(AvailableCount));
        });

        public ICommand CheckAllCommand =>
            new RelayCommand(async () => await CheckMultiple(ActiveDomains.Where(d => d.IsAvailable == null).ToList()));

        public ICommand CheckSelectedCommand =>
            new RelayCommand(async () => await CheckMultiple(ActiveDomains.Where(d => d.IsAvailable == null && d.IsSelected).ToList()));

        public ICommand DeleteSelectedKeywordCommand =>
            new RelayCommand<DomainData>((_) => DeleteSelectedKeyword(_));

        public ICommand ShowFilteredCommand =>
                    new RelayCommand<DomainData>((_) => ShowFiltered(_));

        private void DeleteSelectedKeyword(DomainData domainData)
        {
            //if user is deleting active list also clear the domains
            if (domainData.SearchQuery.Equals(SearchQuery))
                ActiveDomains.Clear();
            DomainsData.Remove(domainData);
            
            OnPropertyChanged(nameof(TotalDomainsCount));
                //MultiKeywordCounts?.Remove(domainData);
        }

        private void ShowFiltered(DomainData domainData)
        {
            // ActiveDomains.Clear();
            SelectedQuery = domainData.SearchQuery;
             var relatedDomains = DomainsData.Where(d => d.SearchQuery.Equals(domainData.SearchQuery))
               .FirstOrDefault();
            ActiveDomains = relatedDomains.Domains;
            Selection.ChangeDomainsReference(ActiveDomains);
            
        }

        async Task CheckSingle(SingleDomain domain)
        {
            domain.IsChecking = true;

            var json = await ExpiredDomainAssistant.CheckAsync(domain.Address);
            SetDomainAvailability(domain, json);
            domain.IsChecking = false;
        }

        async Task CheckMultipleWithHttp(List<SingleDomain> domains)
        {
            _log.Info($"Inside Check multiple, total domains to check : {domains?.Count}");
            foreach (var item in domains)
            {
                item.IsChecking = true;
            }

            foreach (var chunk in domains.ToChunks(20).ToList())
            {
                _log.Info("chunking into 20 items");

                var result = await Task.WhenAll(chunk.Select(async x => new
                {
                    Domain = x,
                    Result = await SubdomainAssistant.IsAvailableTimeout(x.Address, TimeSpan.FromSeconds(7))
                }));

                var @checked = result.Where(x => x.Result.HasValue && !x.Result.Value).ToList();
                _log.Info($"total items that returned unavailable {@checked.Count}");
                var other = result.Except(@checked).ToList();

                foreach (var pair in @checked)
                {
                    _log.Info($"Domain : {pair.Domain.Address} , Result: {pair.Result} in checked");
                    SetDomainAvailability(pair.Domain, pair.Result);
                    pair.Domain.IsChecking = false;
                }
                _log.Info($"Total domains sent to PHP script : {other.Count}");
                var json = await ExpiredDomainAssistant.CheckAsync(other.Select(d => d.Domain.Address).ToArray());
                foreach (var domain in other)
                {
                    SetDomainAvailability(domain.Domain, json);
                    _log.Info($"Domain : {domain.Domain.Address} , Result: {domain.Domain.IsAvailable} in others from PHP script");

                    domain.Domain.IsChecking = false;
                }

                OnPropertyChanged(nameof(AvailableCount));
            }
            if (Permissions.Nnj.CanDeleteUnavailable)
            {
                if (Settings.Default.AutoDel)
                {
                    BlacklistDomains(ActiveDomains.Where(d => d.IsAvailable == false));
                     var domainsToDel = ActiveDomains.Where(d => d.IsAvailable == false);
                    ActiveDomains.RemoveWhere(d => d.IsAvailable == false);
                   
                    foreach (var item in domainsToDel)
                    {
                        DomainsData.Where(d => d.SearchQuery.Equals(item.Keyword))
                           .FirstOrDefault().Domains.Remove(item);
                        OnPropertyChanged(nameof(TotalDomainsCount));
                    }

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
        async Task CheckMultiple(List<SingleDomain> domains)
        {
            _log.Info($"Inside Check multiple, total domains to check : {domains?.Count}");
            foreach (var item in domains)
            {
                item.IsChecking = true;
            }

            foreach (var chunk in domains.ToChunks(20).ToList())
            {
                _log.Info("chunking into 20 items");
                var result = await Task.WhenAll(chunk.Select(async x => new
                {
                    Domain = x,
                    IsAvailable = await GoogleDNSLookup.CheckAvailability(x.Address)
                }
                ));
                var notAvailable = result.Where(x => x.IsAvailable.HasValue && x.IsAvailable == false).ToList();
                _log.Info($"total items that returned unavailable {notAvailable.Count}");
                foreach (var domain in notAvailable)
                {
                    SetDomainAvailability(domain.Domain, false);
                    domain.Domain.IsChecking = false;
                }
                var availableFromGoogleDNS = result.Except(notAvailable).ToList();

                _log.Info($"Total domains sent to PHP script : {availableFromGoogleDNS.Count}");
                var json = await ExpiredDomainAssistant.CheckAsync(availableFromGoogleDNS.Select(d => d.Domain.Address).ToArray());
                foreach (var domain in availableFromGoogleDNS)
                {
                    SetDomainAvailability(domain.Domain, json);
                    _log.Info($"Domain : {domain.Domain.Address} , Result: {domain.Domain.IsAvailable} in others from PHP script");

                    domain.Domain.IsChecking = false;
                }

                OnPropertyChanged(nameof(AvailableCount));
            }
            if (Permissions.Nnj.CanDeleteUnavailable)
            {
                if (Settings.Default.AutoDel)
                {
                    BlacklistDomains(ActiveDomains.Where(d => d.IsAvailable == false));
                    RemoveDomainsFromDomainData(ActiveDomains.Where(d => d.IsAvailable == false));
                    ActiveDomains.RemoveWhere(d => d.IsAvailable == false);
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
                DomainsData.Where(d => d.SearchQuery.Equals(item.Keyword))
                   .FirstOrDefault()?.Domains.Remove(item);
                OnPropertyChanged(nameof(TotalDomainsCount));
            }
        }

        private void RemoveDomainFromDomainData(SingleDomain domainToDelete)
        {
            
                DomainsData.Where(d => d.SearchQuery.Equals(domainToDelete.Keyword))
                   .FirstOrDefault()?.Domains.Remove(domainToDelete);
                OnPropertyChanged(nameof(TotalDomainsCount));

        }

        void SetDomainAvailability(SingleDomain domain, JArray json)
        {
            var result = json?.Children().FirstOrDefault(d => domain.Address.Contains(d.ValueOf("domain")))?.BoolValueOf("available");
            domain.IsAvailable = result;
            domain.Status = result.HasValue ? result.Value ? "Available" : "Unavailable" : "Unknown";
            var favoriteDomain = FavoriteDomains.FirstOrDefault(x => x.Address == domain.Address);
            if (favoriteDomain != null)
            {
                favoriteDomain.IsAvailable = result;
            }
        }

        void SetDomainAvailability(SingleDomain domain, bool? result)
        {
            domain.IsAvailable = result;
            domain.Status = result.HasValue ? result.Value ? "Available" : "Unavailable" : "Unknown";
            var favoriteDomain = FavoriteDomains.FirstOrDefault(x => x.Address == domain.Address);
            if (favoriteDomain != null)
            {
                favoriteDomain.IsAvailable = result;
            }
        }

        public ICommand OpenInBrowserCommand => new RelayCommand<string>(s => Process.Start(s));

        public ICommand FindRefrencesCommand => new RelayCommand<string>(s =>
        {
            if (!Permissions.Nnj.CanUseExplore)
            {
                PermissionAssistant.ShowPermissionDeniedMessage(Permissions.NnjPlanName);
                return;
            }

            new RefrencesDialog(s).ShowDialog();
        });

        public ICommand SaveAllDomainsCommand => new RelayCommand(() =>
        {
            var domains = ActiveDomains.Where(d => d.IsAvailable == true).ToList();
            if (domains.Any())
            {
                SaveDomains(domains);
            }
            else
            {
                MessageBox.Show("No domains are available");
            }
        });

        public ICommand SaveSelectedDomainsCommand => new RelayCommand(() =>
        {
            var domains = ActiveDomains.Where(x => x.IsSelected);
            if (domains?.Count() > 0)
            {
                SaveDomains(domains);
            }
        });

        public ICommand SaveFavoritesCommand => new RelayCommand(() => SaverSubViewModel.SaveFavorites(FavoriteDomains));

        public ICommand DeleteAllDomainsCommand => new RelayCommand(() =>
        {
            ActiveDomains?.Clear();
            DomainsData?.Clear();
            //todo check with greg..
           // MultiKeywordCounts.Clear();
        });

        public ICommand DeleteSelectedDomainsCommand => new RelayCommand(() =>
        {
            var selectedDomains = ActiveDomains.Where(d => d.IsSelected);
            RemoveDomainsFromDomainData(ActiveDomains.Where(d => d.IsSelected));
            ActiveDomains.RemoveWhere(d=>d.IsSelected);
        });

        public ICommand DeleteBlacklistedDomainsCommand => new RelayCommand(() =>
        {
            var blackListedDomainsList = ActiveDomains.Where(d => d.IsBlacklisted == true).ToList();
            RemoveDomainsFromDomainData(ActiveDomains.Where(d => d.IsBlacklisted == true));
            ActiveDomains.RemoveWhere(d => d.IsBlacklisted == true);
        });

        public ICommand DeleteUnavailableDomainsCommand => new RelayCommand(() =>
        {
            RemoveDomainsFromDomainData(ActiveDomains.Where(d => d.IsAvailable == false));
            ActiveDomains.RemoveWhere(d => d.IsAvailable == false);
        });
        #region BlackListCommands
        public ICommand BlacklistAllDomainsCommand => new RelayCommand(() => BlacklistDomains(ActiveDomains));

        public ICommand UnlacklistAllDomainsCommand => new RelayCommand(() => UnblacklistDomains(ActiveDomains));

        public ICommand BlacklistUnavailableDomainsCommand => new RelayCommand(() => BlacklistDomains(ActiveDomains.Where(d => d.IsAvailable == false)));

        public ICommand BlacklistUnknownDomainsCommand => new RelayCommand(() => BlacklistDomains(ActiveDomains.Where(d => d.IsAvailable == null)));

        public ICommand BlacklistSelectedDomainsCommand => new RelayCommand(() => BlacklistDomains(ActiveDomains.Where(d => d.IsSelected)));

        public ICommand ToggleBlacklistDomainCommand => new RelayCommand<SingleDomain>((d) => ToggleBlacklistDomian(d));
        #endregion

        #region PermanentBlacklistCommands
        public ICommand AddPermBlacklistDomainCommand =>
        new RelayCommand<SingleDomain>((d) =>
        {
            //_decrementDomainCount?.Invoke(d);
            PermantlyBlackListDomain(d, ActiveDomains);
            OnPropertyChanged(nameof(TotalDomainsCount));
        });
        public ICommand PermBlacklistUnavailableDomainsCommand => new RelayCommand(
        async () =>
        {
            var unavialableDomains = ActiveDomains.Where(d => d.IsAvailable == false);
            var unavailableList = unavialableDomains.ToList();
            if (await PermantlyBlackListDomains(unavialableDomains, ActiveDomains))
                OnPropertyChanged(nameof(TotalDomainsCount));

        });

        public ICommand PermBlacklistUnknownDomainsCommand => new RelayCommand(
        async () => 
        {
            var unknownDomains = ActiveDomains.Where(d => d.IsAvailable == null);
            var unknownDomainsList = unknownDomains.ToList();
            if (await PermantlyBlackListDomains(unknownDomains, ActiveDomains))
                OnPropertyChanged(nameof(TotalDomainsCount));
        });

        public ICommand PermBlacklistSelectedDomainsCommand => new RelayCommand(
        async() =>
        {
            var selectedDomains = ActiveDomains.Where(d => d.IsSelected);
            var selectedDomainsList = selectedDomains.ToList();
            if (await PermantlyBlackListDomains(selectedDomains, ActiveDomains))
                OnPropertyChanged(nameof(TotalDomainsCount));


        });

        public ICommand PermBlackListSelectedFavCommand => new RelayCommand(
        async () =>
        {
            await PermantlyBlackListDomains(FavoriteDomains.Where(d => d.IsSelected), FavoriteDomains);
            FavoriteDomainsUpdated?.Invoke();
        }
        );
        public ICommand AddPermBlacklistFavDomainCommand =>
            new RelayCommand<SingleDomain>((d) =>
            {
                PermantlyBlackListDomain(d, FavoriteDomains);
                FavoriteDomainsUpdated?.Invoke();
            });

        public ICommand SelectedBlackListToPermCommand =>
            new RelayCommand(
            async() =>
            {
               await PermantlyBlackListDomains(BlacklistedDomains.Where(d => d.IsSelected), BlacklistedDomains);
                BlacklistedDomainsUpdated?.Invoke();
            });
        public ICommand AddBlackListToPermBlacklistCommand =>
            new RelayCommand<SingleDomain>((d) =>
            {
                PermantlyBlackListDomain(d, BlacklistedDomains);
                BlacklistedDomainsUpdated?.Invoke();
            });
        #endregion

        private void PermantlyBlackListDomain(SingleDomain domain, IList<SingleDomain> domainList)
        {
            domain.IsBlacklistedPermanent = true;
            PermBlacklistedDomains.Add(domain);
            RemoveDomainFromDomainData(domain);
            domainList.Remove(domain);
            PermBlacklistedDomainsUpdated?.Invoke();
        }
        public ICommand ToggleSelectAllBlacklistDomainsCommand => new RelayCommand<bool>((b) =>
        {
            foreach (var domain in BlacklistedDomains)
            {
                domain.IsSelected = b;
            }
        });


        async Task<bool> PermantlyBlackListDomains(IEnumerable<SingleDomain> domains, IList<SingleDomain> sourceDomains)
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
                PermBlacklistedDomains.Add(item);
                RemoveDomainFromDomainData(item);
                sourceDomains.Remove(item);
            }

            PermBlacklistedDomainsUpdated?.Invoke();
            return true;
        }

        void ToggleBlacklistDomian(SingleDomain domain)
        {
            if (domain.IsBlacklisted != null && (bool)domain.IsBlacklisted)
            {
                domain.IsBlacklisted = false;
                BlacklistedDomains.Remove(domain);
            }
            else
            {
                domain.IsBlacklisted = true;
                BlacklistedDomains.Add(domain);
            }
            BlacklistedDomainsUpdated?.Invoke();
        }

        void BlacklistDomains(IEnumerable<SingleDomain> domains)
        {
            foreach (var item in domains.Where(d => d.IsBlacklisted != true))
            {
                item.IsBlacklisted = true;
                BlacklistedDomains.Add(item);
            }
            BlacklistedDomainsUpdated?.Invoke();
        }


        void UnblacklistDomains(IEnumerable<SingleDomain> domains)
        {
            foreach (var item in domains.Where(d => d.IsBlacklisted == true))
            {
                item.IsBlacklisted = false;
                BlacklistedDomains.Remove(item);
            }
            BlacklistedDomainsUpdated?.Invoke();
        }

        public ICommand ClearBlacklistCommand => new RelayCommand(() =>
        {
            while (BlacklistedDomains.Count > 0)
            {
                BlacklistedDomains.First().RemoveFromBlacklistCommand.Execute(this);
            }
        });

        public ICommand ClearFavoritesCommand => new RelayCommand(() =>
        {
            while (FavoriteDomains.Count > 0)
            {
                FavoriteDomains.First().RemoveFromFavoritesCommand.Execute(this);
            }
        });

        public ICommand AddAvailableToFavoritesCommand => new RelayCommand(() =>
        {
            foreach (var domain in ActiveDomains.Where(d => d.IsAvailable == true && d.IsFavorite != true))
            {
                domain.IsFavorite = true;
                FavoriteDomains.Add(domain);
            }
            FavoriteDomainsUpdated?.Invoke();
        });

        public ICommand FavoriteSelectedDomainsCommand => new RelayCommand(() =>
        {
            foreach (var domain in ActiveDomains)
            {
                if (domain.IsSelected && (domain.IsFavorite == null || !(bool)domain.IsFavorite))
                {
                    domain.IsFavorite = true;
                    FavoriteDomains.Add(domain);
                }
            }

            FavoriteDomainsUpdated?.Invoke();

        });

        public ICommand AddSelectedToFavoritesCommand => new RelayCommand(() =>
        {
            bool isfavouritesUpdated = false;
            if (_selectedWebsite.Equals(AllWebSites.None))
            {
                isfavouritesUpdated = AddDomainsToFavourites();
            }
            else
            {
                isfavouritesUpdated = AddDomainsToFavourites(_selectedWebsite);
            }
            if (isfavouritesUpdated)
                FavoriteDomainsUpdated?.Invoke();
        });

        public ICommand ToggleAddToFavoriteCommand => new RelayCommand<SingleDomain>(sd =>
        {
            if (FavoriteDomains.Contains(sd))
            {
                FavoriteDomains.Remove(sd);
                sd.IsFavorite = false;
            }
            else
            {
                FavoriteDomains.Add(sd);
                sd.IsFavorite = true;
            }

            FavoriteDomainsUpdated?.Invoke();
        });

        public ICommand RemoveFromFavoriteCommand => new RelayCommand<SingleDomain>(sd =>
        {
            if (FavoriteDomains.Contains(sd))
            {
                FavoriteDomains.Remove(sd);
                sd.IsFavorite = false;

                FavoriteDomainsUpdated?.Invoke();
            }
        });

        public ICommand RemoveSelectedFavoriteFromFavoriteCommand => new RelayCommand(() =>
        {
            FavoriteDomains.Where(d => d.IsSelected).ToList()
            .ForEach(d => FavoriteDomains.Remove(d));

            FavoriteDomainsUpdated?.Invoke();
        });

        public ICommand RemoveBlacklistedFavoritesFromFavoritesCommand => new RelayCommand(() =>
        {
            FavoriteDomains.Where(d => d.IsBlacklisted != null && (bool)d.IsBlacklisted).ToList()
            .ForEach(d => FavoriteDomains.Remove(d));

            FavoriteDomainsUpdated?.Invoke();
        });

        public ICommand AddSelectedFavoritesToBlacklistCommand => new RelayCommand(() =>
        {

            FavoriteDomains.Where(d => d.IsBlacklisted != true && d.IsSelected).ToList()
                        .ForEach(x =>
                        {
                            x.IsBlacklisted = true;
                            BlacklistedDomains.Add(x);
                        });

            FavoriteDomainsUpdated?.Invoke();
        });


        public ICommand SortCommand => new RelayCommand<object>(Sort);
        public ICommand SortByMetricsCommand => new RelayCommand<object>(SortbyMetrics);

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

        public ICommand DeleteDomainCommand => new RelayCommand<SingleDomain>((d) =>
        {
            //  _decrementDomainCount?.Invoke(d);
            RemoveDomainFromDomainData(d);

            ActiveDomains.Remove(d);
            OnPropertyChanged(nameof(TotalDomainsCount));

        });


        public ICommand RemoveFromBlacklistCommand => new RelayCommand<SingleDomain>(domain =>
        {
            domain.IsBlacklisted = false;
            BlacklistedDomains.Remove(domain);
            BlacklistedDomainsUpdated?.Invoke();
        });

        public ICommand RemoveSelectedFromBlacklistCommand => new RelayCommand(() =>
        {
            BlacklistedDomains.
            Where(d => d.IsSelected).ToList().
            ForEach(d =>
            {
                d.IsBlacklisted = false;
                BlacklistedDomains.Remove(d);
            });

            BlacklistedDomainsUpdated?.Invoke();
        });

        public static IReadOnlyList<dynamic> MetricsTypes = Enum.GetValues(typeof(MetricsTypeEnum)).OfType<MetricsTypeEnum>().Select(x => new
        {
            Value = x,
            Text = x == MetricsTypeEnum.None ? "Sort by Metrics" : x.ToString()
        }).ToList();

        public static IReadOnlyList<dynamic> WebsiteTypes = Enum.GetValues(typeof(AllWebSites)).
            OfType<AllWebSites>().Select(
            x => new
            {
                Value = x,
                Text = x == AllWebSites.None ? "Sort by Web2.0" : x.ToString()
            }).ToList();


        void Sort(object source)
        {
            var domains = source as ObservableCollection<SingleDomain>;
            if (domains != null)
            {

                if (_selectedWebsite.Equals(AllWebSites.None))
                {
                    IsSomeSiteSelected = false;
                    domains?.All(j => j.IsVisible = true);

                    return;
                }

                IsSomeSiteSelected = true;
                foreach (var item in FavoriteDomains)
                {
                    if (item.DomainSource != _selectedWebsite.ToString())
                    {
                        item.IsVisible = false;
                        continue;
                    }
                    item.IsVisible = true;
                }
            }

        }

        void SortbyMetrics(object source)
        {
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

        public MetricsTypeEnum SelectedMetrics
        {
            get { return _selectedMetrics; }
            set
            {
                _selectedMetrics = value;
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

        public AllWebSites SelectedWebsite
        {
            get { return _selectedWebsite; }
            set
            {
                _selectedWebsite = value;
            }
        }

        #region Left search result stuff

        //public ICommand SelectAllSearchResultsCommand => new RelayCommand(() => ChangeSelection(Results, true));
        //public ICommand DeSelectAllSearchResultsCommand => new RelayCommand(() => ChangeSelection(Results, false));

        //static void ChangeSelection(IEnumerable<BaseSearchResult> results, bool isSelected)
        //{
        //    foreach (var item in results)
        //    {
        //        item.IsSelected = isSelected;
        //    }
        //}

        //public ICommand SaveAllSearchResultsCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(() => SaveDomains(Results));
        //    }
        //}

        //public ICommand SaveSelectedSearchResultsCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            IEnumerable<BaseSearchResult> results = this.Results.Where((x) => x.IsSelected);
        //            if (results?.Count() > 0)
        //            {
        //                this.SaveDomains(results);
        //            }
        //        });
        //    }
        //}

        //public ICommand DeleteAllSearchResultsCommand => new RelayCommand(() =>
        //{
        //    _ctsReddit.Cancel();
        //    _ctsTumblr.Cancel();
        //    _ctsYouTube.Cancel();

        //    Results.Clear();
        //});

        //public ICommand DeleteSelectedSearchResultsCommand => new RelayCommand(() => Results.RemoveWhere(x => x.IsSelected));

        #endregion

        bool _isSearching;
        public bool IsSearching
        {
            get { return _isSearching; }
            set { SetProperty(ref _isSearching, value); }
        }

        bool _searchInReddit = true;
        public bool SearchInReddit
        {
            get { return _searchInReddit; }
            set { SetProperty(ref _searchInReddit, value); }
        }

        bool _searchInTumblr = true;
        public bool SearchInTumblr
        {
            get { return _searchInTumblr; }
            set { SetProperty(ref _searchInTumblr, value); }
        }

        bool _searchInYouTube;
        private MetricsTypeEnum _selectedMetrics;
        private AllWebSites _selectedWebsite = AllWebSites.None;
        private bool _searchWiki;
        private bool _searchHuffpost;
        private bool _searchBulk;

        private string _bulkInfo;
        private bool _searchMashable;
        private bool _groupBySource;
        private bool _searchYahoo;
        private bool _showUpdateLink;
        private bool _deleteUnavailable;
        private bool _showSelectedToFav = false;
        private bool _canSearchNow = true;
        public bool CanSearchNow
        {
            get { return _canSearchNow; }
            set
            {
                SetProperty(ref _canSearchNow, value);
            }
        }
        public bool GroupBySource
        {
            get { return _groupBySource; }
            set { SetProperty(ref _groupBySource, value); }
        }

        private bool _favoritesGroupBySource;

        public bool FavoritesGroupBySource
        {
            get { return _favoritesGroupBySource; }
            set { SetProperty(ref _favoritesGroupBySource, value); }
        }

        public bool SearchInYouTube
        {
            get { return _searchInYouTube; }
            set { SetProperty(ref _searchInYouTube, value); }
        }

        public bool SearchWiki
        {
            get { return _searchWiki; }
            set { SetProperty(ref _searchWiki, value); }
        }

        public bool SearchHuffpost
        {
            get { return _searchHuffpost; }
            set { SetProperty(ref _searchHuffpost, value); }
        }

        public bool SearchMashable
        {
            get { return _searchMashable; }
            set { SetProperty(ref _searchMashable, value); }
        }

        public bool SearchYahoo
        {
            get { return _searchYahoo; }
            set { SetProperty(ref _searchYahoo, value); }
        }

        public bool DeleteUnavailable
        {
            get { return _deleteUnavailable; }
            set { SetProperty(ref _deleteUnavailable, value); }
        }

        public bool ShowUpdateLink
        {
            get { return _showUpdateLink; }
            set { SetProperty(ref _showUpdateLink, value); }
        }

        public bool SearchBulk
        {
            get { return _searchBulk; }
            set { SetProperty(ref _searchBulk, value); }
        }

        public string BulkInfo
        {
            get { return _bulkInfo; }
            set { SetProperty(ref _bulkInfo, value); }
        }

        public string SearchQuery { get; set; }

        public uint MaxResults { get; set; } = 25;


        public async Task<bool> SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery)) return false;

            SearchBulk = false;

            DomainsVisible = true;
            Pbn.CanSearchNow = false;
            if (this.IsSearching)
            {
                this.Results.Clear();
                // Cancel Searching and clear results
                this._ctsReddit.Cancel();
                this._ctsTumblr.Cancel();
                this._ctsYouTube.Cancel();
                _ctsWikipedia.Cancel();
                _ctsHuffpost.Cancel();
                _ctsMashable.Cancel();
                _ctsYahoo.Cancel();

                if (!_ctsBulk.IsCancellationRequested)
                    _ctsBulk.Cancel();

                while (this.IsSearching)
                {
                    await Task.Delay(500);
                }

                this.Dispose();
            }

            Logger.LogStatus($"Searching started for query {SearchQuery}...");

            this._ctsReddit = new CancellationTokenSource();
            this._ctsTumblr = new CancellationTokenSource();
            this._ctsYouTube = new CancellationTokenSource();
            _ctsWikipedia = new CancellationTokenSource();
            _ctsHuffpost = new CancellationTokenSource();
            _ctsMashable = new CancellationTokenSource();
            _ctsYahoo = new CancellationTokenSource();

            this.IsSearching = true;
            this.Results.CollectionChanged += Results_CollectionChanged;
            //AddToKeywordToCountListIfRequired(SearchQuery);
            try
            {

                List<Task> tasks = new List<Task>();
                _resultsTimer.Start();
                if (this.SearchInYouTube)
                {
                    _log.Info("Added youtube search task");
                    tasks.Add(await Task.Factory.StartNew(() => SearchInYouTubeAsync(SearchQuery)));
                }

                if (SearchWiki)
                {
                    _log.Info("Added wiki search task");
                    tasks.Add(await Task.Factory.StartNew(() => SearchInWikipediaAsync(SearchQuery)));
                }

                if (SearchHuffpost)
                {
                    _log.Info("Added Huffpost search task");
                    tasks.Add(await Task.Factory.StartNew(() => SearchInHuffpostAsync(SearchQuery)));
                }

                if (this.SearchInReddit)
                {
                    _log.Info("Added reddit search task");
                    tasks.Add(await Task.Factory.StartNew(() => SearchInRedditAsync(SearchQuery)));
                }

                if (SearchMashable)
                {
                    _log.Info("Added Mashable search task");
                    tasks.Add(await Task.Factory.StartNew(() => SearchInMashableAsync(SearchQuery)));
                }

                if (this.SearchInTumblr)
                {
                    _log.Info("Added tumblr search task");
                    tasks.Add(await Task.Factory.StartNew(() => SearchInTumblrAsync(SearchQuery)));
                }
                if (this.SearchYahoo)
                {
                    tasks.Add(await Task.Factory.StartNew(() => SearchInYahooAnswersAsync(SearchQuery)));
                }
                _log.Info("Waiting for all tasks to finish");
                // Wait for all tasks to finish completion
                await Task.WhenAll(tasks);
                _resultsTimer.Stop();

                GC.Collect();
                Pbn.CanSearchNow = true;
                this.IsSearching = false;
                _completeDialogPopup();
                Logger.LogStatus($"Searching completed for query {SearchQuery}...");

                //this.CheckAvailabilityAndDeleteCommand.Execute(this.Domains);
                _log.Info("Success : SearchAsync() MainViewModel");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Exception caught while searchasync()", ex);
                return false;
            }
        }

        private Action _popUp;
        public void SetupMessageBox(Action popUp)
        {
            popUp = _popUp;
        }

        public async Task<bool> SearchManyAsync()
        {
            string[] keywords = KeywordsProvider.GetFromFile();
            if (keywords == null)
                return await Task.FromResult(true);

            SearchBulk = true;
            BulkInfo = "";

            Pbn.CanSearchNow = false;
            DomainsVisible = true;

            if (this.IsSearching)
            {
                this.Results.Clear();

                // Cancel Searching and clear results
                this._ctsReddit.Cancel();
                this._ctsTumblr.Cancel();
                this._ctsYouTube.Cancel();
                _ctsWikipedia.Cancel();
                _ctsHuffpost.Cancel();
                _ctsMashable.Cancel();
                _ctsYahoo.Cancel();

                if (!_ctsBulk.IsCancellationRequested)
                    _ctsBulk.Cancel();

                while (this.IsSearching)
                {
                    await Task.Delay(500);
                }

                this.Dispose();
            }

            Logger.LogStatus($"Searching started for query {string.Join(",", keywords)}...");

            this.IsSearching = true;
            this.Results.CollectionChanged += Results_CollectionChanged;

            _ctsBulk = new CancellationTokenSource();

            int index = 0;
            _resultsTimer.Start();
            foreach (var query in keywords)
            {
                if (_ctsBulk.IsCancellationRequested)
                    break;

                index++;

                SearchQuery = query;

                this._ctsReddit = new CancellationTokenSource();
                this._ctsTumblr = new CancellationTokenSource();
                this._ctsYouTube = new CancellationTokenSource();
                _ctsWikipedia = new CancellationTokenSource();
                _ctsHuffpost = new CancellationTokenSource();
                _ctsMashable = new CancellationTokenSource();
                _ctsYahoo = new CancellationTokenSource();

                List<Task> tasks = new List<Task>();
               // MultiKeywordCounts.Add(new MultiDomainModel() { Keyword = query, Count = 0 });
             //   AddToKeywordToCountListIfRequired(query);
                if (this.SearchInYouTube)
                {
                    tasks.Add(await Task.Factory.StartNew(() => SearchInYouTubeAsync(query)));
                }

                if (SearchWiki)
                {
                    tasks.Add(await Task.Factory.StartNew(() => SearchInWikipediaAsync(query)));
                }

                if (SearchHuffpost)
                {
                    tasks.Add(await Task.Factory.StartNew(() => SearchInHuffpostAsync(query)));
                }

                if (this.SearchInReddit)
                {
                    tasks.Add(await Task.Factory.StartNew(() => SearchInRedditAsync(query)));
                }

                if (SearchMashable)
                {
                    tasks.Add(await Task.Factory.StartNew(() => SearchInMashableAsync(query)));
                }

                if (this.SearchInTumblr)
                {
                    tasks.Add(await Task.Factory.StartNew(() => SearchInTumblrAsync(query)));
                }
                if (this.SearchYahoo)
                {
                    tasks.Add(await Task.Factory.StartNew(() => SearchInYahooAnswersAsync(SearchQuery)));
                }

                BulkInfo = $"Searching for \"{query}\" {index}/{keywords.Length}";


                // Wait for all tasks to finish completion
                await Task.WhenAll(tasks);
                _resultsTimer.Stop();
            }

            GC.Collect();

            this.IsSearching = false;
            Pbn.CanSearchNow = true;

            _completeDialogPopup();

            Logger.LogStatus($"Searching completed for query {string.Join(",", keywords)}...");

            //this.CheckAvailabilityAndDeleteCommand.Execute(this.Domains);

            return true;
        }

        private void Results_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var currentThread = Thread.CurrentThread.ManagedThreadId;
                var timeSpent = _resultsTimer;

                var result = e.NewItems[0] as BaseSearchResult;
                if (result?.Domains != null && result?.Domains.Count() > 0)
                    _resultsTimer.Stop();


                foreach (var domain in result.Domains)
                {
                    domain.SourceURL = result.SourceAddress;
                    if (result.Source.Equals("Reddit"))
                    {
                        //if (this.Domains.Where(x => x.DomainSource.Equals("Reddit") && x.Keyword.Equals(SearchQuery)).Count() >= this.MaxResults)
                        //{
                        //    this._ctsReddit.Cancel();
                        //    break;
                        //}
                        var domains = GetQuerySpecificDomains(SearchQuery);
                        if (domains != null)
                        {
                            if (domains.Where(x => x.DomainSource.Equals("Reddit")).Count() >= this.MaxResults)
                            {
                                this._ctsReddit.Cancel();
                                break;
                            }
                        }

                        AddToDomainsIfRequired(domain);
                    }
                    else if (result.Source.Equals("Wikipedia"))
                    {
                        //if (this.Domains.Where(x => x.DomainSource.Equals("Wikipedia") && x.Keyword.Equals(SearchQuery)).Count() >= this.MaxResults)
                        //{
                        //    _ctsWikipedia.Cancel();
                        //    break;
                        //}

                        var domains = GetQuerySpecificDomains(SearchQuery);
                        if (domains != null)
                        {
                            if (domains.Where(x => x.DomainSource.Equals("Wikipedia")).Count() >= this.MaxResults)
                            {
                                this._ctsWikipedia.Cancel();
                                break;
                            }
                        }

                        AddToDomainsIfRequired(domain);
                    }
                    else if (result.Source.Equals("HuffPost"))
                    {
                        //if (this.Domains.Where(x => x.DomainSource.Equals("HuffPost") && x.Keyword.Equals(SearchQuery)).Count() >= this.MaxResults)
                        //{
                        //    _ctsHuffpost.Cancel();
                        //    break;
                        //}
                        var domains = GetQuerySpecificDomains(SearchQuery);
                        if (domains != null)
                        {
                            if (domains.Where(x => x.DomainSource.Equals("HuffPost")).Count() >= this.MaxResults)
                            {
                                this._ctsHuffpost.Cancel();
                                break;
                            }
                        }
                        AddToDomainsIfRequired(domain);
                    }
                    else if (result.Source.Equals("YouTube"))
                    {
                        //if (this.Domains.Where(x => x.DomainSource.Equals("YouTube") && x.Keyword.Equals(SearchQuery)).Count() >= this.MaxResults)
                        //{
                        //    this._ctsYouTube.Cancel();
                        //    break;
                        //}
                        var domains = GetQuerySpecificDomains(SearchQuery);
                        if (domains != null)
                        {
                            if (domains.Where(x => x.DomainSource.Equals("YouTube")).Count() >= this.MaxResults)
                            {
                                this._ctsYouTube.Cancel();
                                break;
                            }
                        }

                        AddToDomainsIfRequired(domain);
                    }
                    else if (result.Source.Equals("Tumblr"))
                    {
                        //if (this.Domains.Where(x => x.DomainSource.Equals("Tumblr") && x.Keyword.Equals(SearchQuery)).Count() >= this.MaxResults)
                        //{
                        //    this._ctsTumblr.Cancel();
                        //    break;
                        //}

                        var domains = GetQuerySpecificDomains(SearchQuery);
                        if (domains != null)
                        {
                            if (domains.Where(x => x.DomainSource.Equals("Tumblr")).Count() >= this.MaxResults)
                            {
                                this._ctsTumblr.Cancel();
                                break;
                            }
                        }

                        AddToDomainsIfRequired(domain);
                    }
                    else if (result.Source.Equals("Mashable"))
                    {
                        //if (this.Domains.Where(x => x.DomainSource.Equals("Mashable") && x.Keyword.Equals(SearchQuery)).Count() >= this.MaxResults)
                        //{
                        //    _ctsMashable.Cancel();
                        //    break;
                        //}
                        var domains = GetQuerySpecificDomains(SearchQuery);
                        if (domains != null)
                        {
                            if (domains.Where(x => x.DomainSource.Equals("Mashable")).Count() >= this.MaxResults)
                            {
                                this._ctsMashable.Cancel();
                                break;
                            }
                        }
                        AddToDomainsIfRequired(domain);
                    }
                    else if (result.Source.Equals("YahooAnswers"))
                    {
                        //if (this.Domains.Where(x => x.DomainSource.Equals("YahooAnswers") && x.Keyword.Equals(SearchQuery)).Count() >= this.MaxResults)
                        //{
                        //    _ctsYahoo.Cancel();
                        //    break;
                        //}
                        var domains = GetQuerySpecificDomains(SearchQuery);
                        if (domains != null)
                        {
                            if (domains.Where(x => x.DomainSource.Equals("YahooAnswers")).Count() >= this.MaxResults)
                            {
                                this._ctsYahoo.Cancel();
                                break;
                            }
                        }
                        AddToDomainsIfRequired(domain);
                    }
                }

                _resultsTimer.Start();

            }
        }

        private ObservableCollection<SingleDomain> GetQuerySpecificDomains(string searchQuery)
        {
            var domainData =
                DomainsData.Where(x => x.SearchQuery.Equals(searchQuery)).FirstOrDefault();
            var domains = domainData?.Domains;
            return domains;
        }

        private void AddToDomainsIfRequired(SingleDomain domain)
        {
            //if (!DoesDomainAlreadyExists(domain.Address) &&
            if(!IsDomainBlacklisted(domain.Address) &&
               !IsDomainFavourite(domain.Address)&&
               !DoesDomainAlreadyExistsInAll(domain.Address))
            {
                //var multiListItem = MultiKeywordCounts.Where(x => x.Keyword.Equals(domain.Keyword)).FirstOrDefault();//
                //if (multiListItem != null)
                //{
                //    multiListItem.Count++;
                //}
             
                    DomainData domainData = null;
                    if (KeywordDomainExists(domain.Keyword, ref domainData))
                    {
                        //if (!DoesDomainAlreadyExists(domain.Address, domainData.Domains))
                        {
                            domainData.Domains.Add(domain);
                            AddDomainToActiveList(domain);
                            // Domains.Add(domain);
                        }
                    }
                    else
                    {
                        domainData = new DomainData() { SearchQuery = domain.Keyword };
                        domainData.Domains.Add(domain);
                        DomainsData.Add(domainData);
                        AddDomainToActiveList(domain);
                        // Domains.Add(domain);
                    }

                    OnPropertyChanged(nameof(TotalDomainsCount)); 
                
                //this.Domains.Add(domain);
            }

        }

        private void AddDomainToActiveList(SingleDomain domain)
        {
            if(String.IsNullOrWhiteSpace(SelectedQuery))
            {
                ActiveDomains.Add(domain);
                return;
            }
            //if(domain.Keyword.Equals(SelectedQuery))
            //{
            //    ActiveDomains.Add(domain);
            //}
        }

        private bool KeywordDomainExists(string query,  ref DomainData domainData)
        {
            domainData = DomainsData.Where(d => d.SearchQuery.Equals(query)).FirstOrDefault();
            if (domainData == null)
                return false;
            return true;
        }

        #region KeywordCount_UtilityMethods
        //private void AddToKeywordToCountListIfRequired(string searchTerm)
        //{
        //    var existsAlready = MultiKeywordCounts.Where
        //           (x => x.Keyword.ToLowerInvariant().Equals
        //           (searchTerm.ToLowerInvariant())).FirstOrDefault();
        //    if (existsAlready == null)
        //    {
        //        MultiKeywordCounts.Add(new MultiDomainModel() { Keyword = searchTerm, Count = 0 });
        //    }
        //}
        //private void DecrementCountForDomain(SingleDomain d)
        //{
        //    var item = MultiKeywordCounts.Where(x => x.Keyword.Equals(d.Keyword)).FirstOrDefault();
        //    if (item == null)
        //        return;
        //    item.Count--;
        //}

        #endregion
        private bool IsDomainFavourite(string domainAddress)
        {
            return FavoriteDomains.Any(d => d.Address.ToLowerInvariant() == domainAddress.ToLowerInvariant());
            //return false;
        }
        
        private bool DoesDomainAlreadyExists(string domainAddress,ObservableCollection<SingleDomain> keywordDomains)
        {
            return keywordDomains.Any((x) => x.Address.ToLowerInvariant().Equals(domainAddress.ToLowerInvariant()));
        }
        private bool DoesDomainAlreadyExistsInAll(string domainAddress)
        {
            var domains = DomainsData.Select(d => d.Domains);
            var allDomains = domains.SelectMany(d => d.Select(x => x)).ToList();
            var @bool= allDomains.Any((x) => x.Address.ToLowerInvariant().Equals(domainAddress.ToLowerInvariant()));
            return @bool;
        }
        private bool IsDomainBlacklisted(string domainAddress)
        {
            if (BlacklistedDomains.Any((x) => x.Address.ToLowerInvariant().Equals(domainAddress.ToLowerInvariant())))
                return true;
            if (PermBlacklistedDomains.Any((x) => x.Address.ToLowerInvariant().Equals(domainAddress.ToLowerInvariant())))
                return true;
            return false;
        }

        private async Task SearchInYahooAnswersAsync(string query)
        {
            Logger.LogStatus("Searching in YahooAnswers...");

            var yahooDataProvider = new YahooDataProvider(query, Results, _ctsReddit.Token, maxResults: this.MaxResults);
            await yahooDataProvider.LoadMoreItemsAsync(int.MaxValue);
            Console.WriteLine("Searching in YahooAnswers completed.");

            Logger.LogStatus("Searching in YahooAnswers completed...");
        }
        private async Task SearchInMashableAsync(string query)
        {
            Logger.LogStatus("Searching in Mashable...");

            var mashableDataProvider = new MashableDataProvider(query, Results, _ctsMashable.Token);
            await mashableDataProvider.LoadMoreItemsAsync(int.MaxValue);
            Console.WriteLine("Searching in Mashable completed.");
            Logger.LogStatus("Searching in Mashable completed.");
        }

        private async Task SearchInRedditAsync(string query)
        {
            Logger.LogStatus("Searching in reddit...");

            var redditDataProvider = new RedditDataProvider(query, Results, _ctsReddit.Token);
            await redditDataProvider.LoadMoreItemsAsync(int.MaxValue);
            Console.WriteLine("Searching in reddit completed.");
            Logger.LogStatus("Searching in reddit completed.");
        }

        private async Task SearchInWikipediaAsync(string query)
        {
            Logger.LogStatus("Searching in wikipedia...");

            var provider = new WikiDataProvider(query, Results, _ctsWikipedia.Token);
            await Task.Run(async () => { await provider.LoadMoreItemsAsync(int.MaxValue); });

            Logger.LogStatus("Searching in wikipedia completed.");
            Console.WriteLine("Searching in wikipedia completed.");
        }

        private async Task SearchInHuffpostAsync(string query)
        {
            Logger.LogStatus("Searching in huffingtonpost...");

            var provider = new HuffingtonpostDataProvider(query, Results, _ctsHuffpost.Token);
            await Task.Run(async () => { await provider.LoadMoreItemsAsync(int.MaxValue); });
            Console.WriteLine("Searching in huffingtonpost completed.");

            Logger.LogStatus("Searching in huffingtonpost completed.");
        }

        private async Task SearchInTumblrAsync(string query)
        {
            Logger.LogStatus("Searching in tumblr...");

            var tumblrDataProvider = new TumblrDataProvider(query, Results, _ctsTumblr.Token);
            await tumblrDataProvider.LoadMoreItemsAsync(int.MaxValue);
            Console.WriteLine("Searching in tumblr completed.");
            Logger.LogStatus("Searching in tumblr completed.");
        }

        private async Task SearchInYouTubeAsync(string query)
        {
            Logger.LogStatus("Searching in youtube...");

            var youtubeDataProvider = new YouTubeDataProvider(query, Results, _ctsYouTube.Token);
            await youtubeDataProvider.LoadMoreItemsAsync(int.MaxValue);
            Console.WriteLine("Searching in youtube completed.");
            Logger.LogStatus("Searching in youtube completed.");
        }

        public void SaveDomains(IEnumerable<BaseSearchResult> searchResults)
        {
            // Configure open file dialog box
            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Domains"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "CSV Files (.csv)|*.csv"; // Filter files by extension

            // Process open file dialog box results
            if (dlg.ShowDialog() == true)
            {
                // Open document
                string filename = dlg.FileName;

                try
                {
                    using (StreamWriter writer = new StreamWriter(filename))
                        ServiceStack.Text.CsvSerializer.SerializeToWriter(searchResults, writer);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message);
                }
            }
        }

        public void SaveDomains(IEnumerable<SingleDomain> domains)
        {
            // Configure open file dialog box
            var dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.FileName = "Domains"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "CSV Files (.csv)|*.csv"; // Filter files by extension

            // Process open file dialog box results
            if (dlg.ShowDialog() == true)
            {
                // Open document
                string filename = dlg.FileName;

                domains.ToList().ForEach(x =>
                {
                    //x.IsBlacklisted = null;
                    x.IsBlacklisted = x.IsBlacklisted == null ? false : true;
                    x.DomainSourceAddress = string.IsNullOrEmpty(x.DomainSourceAddress) ? "" : x.DomainSourceAddress;
                });

                try
                {
                    using (StreamWriter writer = new StreamWriter(filename))
                        ServiceStack.Text.CsvSerializer.SerializeToWriter(domains, writer);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message);
                }
            }

        }

        public async Task ReadDataFromFilesAsync()
        {
            var favorites = default(List<SingleDomain>);
            var blacklisted = default(List<SingleDomain>);
            var permBlacklisted = default(List<SingleDomain>);

            await Task.Run(() =>
            {
                favorites = LocalStorage.GetDataFromFile<List<SingleDomain>>(m_favouritesFileName) ?? new List<SingleDomain>();
                blacklisted = LocalStorage.GetDataFromFile<List<SingleDomain>>(m_blackListedFileName) ?? new List<SingleDomain>();
                permBlacklisted = LocalStorage.GetDataFromFile<List<SingleDomain>>(m_permanentBlackListedFileName) ?? new List<SingleDomain>();
            });


            //todo - look if we have to do this with permblacklist too
            if (blacklisted.Any())
            {
                for (int i = 0; i < favorites.Count; i++)
                {
                    var item = favorites[i];
                    var blacklistedDomain = blacklisted.FirstOrDefault(d => d.Address == item.Address);
                    if (blacklistedDomain != null)
                    {
                        blacklistedDomain.IsFavorite = true;
                        favorites[i] = blacklistedDomain;
                    }
                }
            }

            FavoriteDomains.AddRange(favorites);
            BlacklistedDomains.AddRange(blacklisted);
            PermBlacklistedDomains.AddRange(permBlacklisted);
            // PermBlacklistedDomains.AddRange()

        }

        public async Task OnStartAsync()
        {
            await ReadDataFromFilesAsync();
            await AppUpdater.CheckForTheNewVersion();
        }

        public void SetupCompleteDialog(Action popup)
        {
            _completeDialogPopup = popup;
            Pbn?.SetupCompleteDialog(popup);
        }

        private bool AddDomainsToFavourites()
        {
            bool isUpdated = false;
            foreach (var domain in ActiveDomains.Where(d => d.IsFavorite != true))
            {
                domain.IsFavorite = true;
                FavoriteDomains.Add(domain);
                isUpdated = true;
            }
            return isUpdated;
        }

        private bool AddDomainsToFavourites(AllWebSites siteType)
        {
            bool isUpdated = false;
            foreach (var domain in ActiveDomains.Where(d => d.IsFavorite != true
            && d.DomainSource == siteType.ToString() && d.IsSelected == true

            ))
            {
                domain.IsFavorite = true;
                FavoriteDomains.Add(domain);
                isUpdated = true;
            }
            return isUpdated;
        }

        public void Dispose()
        {
            _ctsReddit.Dispose();
            _ctsTumblr.Dispose();
            _ctsYouTube.Dispose();
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




    public class BoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
