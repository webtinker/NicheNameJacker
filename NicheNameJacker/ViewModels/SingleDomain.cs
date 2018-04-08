using NicheNameJacker.Commands;
using NicheNameJacker.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using NicheNameJacker.Interfaces;

namespace NicheNameJacker.ViewModels
{
    public class SingleDomain : ObservableBase, IDomDetailerStats
    {
        private bool? _isFavorite;
        private bool? _isAvailable;
        private bool? _isBlacklisted;
        private bool? _isBlacklistedPermanent;
        private bool _isVisible = true;
        private string _sourceURL;
        private string _baseQuery;

        private bool _isSelected;
        private string _status;

        public SingleDomain() { }

        public string Address { get; set; }

        [JsonIgnore]
        public string FullAddress { get; set; }

        public string Keyword { get; set; }

        public string DomainName { get; set; }
        public string TopLevelDomainName { get; set; }
        public string DomainSource { get; set; }

        [JsonIgnore]
        public bool IsYouTube => DomainSource == "YouTube";


        private string _stats;
        //[JsonIgnore]
        public string Stats
        {
            get { return _stats; }
            set
            {
                SetProperty(ref _stats, value);
                OnPropertyChanged(nameof(StatsArray));
            }
        }

        [JsonIgnore]
        public string[] StatsArray => Regex.Split(Stats ?? "", @"(?<=\d)\s");

        [JsonIgnore]

        bool _statsLoaded;
        //[JsonIgnore]
        public bool StatsLoaded
        {
            get { return _statsLoaded; }
            set { SetProperty(ref _statsLoaded, value); }
        }

        public string DomainSourceAddress { get; set; }

        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }
        public string BaseQuery
        {
            get { return _baseQuery; }
            set { SetProperty(ref _baseQuery, value); }
        }

        [JsonIgnore]
        public bool IsBusy => IsChecking || IsGettingStats;

        bool _isChecking;
        [JsonIgnore]
        public bool IsChecking
        {
            get { return _isChecking; }
            set
            {
                if (_isChecking == value) return;
                _isChecking = value;
                OnPropertyChanged(nameof(IsChecking));
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        bool _isGettingStats;
        private DomDetailerData _statsData;

        [JsonIgnore]
        public bool IsGettingStats
        {
            get { return _isGettingStats; }
            set
            {
                if (_isGettingStats == value) return;
                SetProperty(ref _isGettingStats, value);
                //_isGettingStats = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        [JsonIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        //[JsonIgnore]
        public DomDetailerData StatsData
        {
            get { return _statsData; }
            set { SetProperty(ref _statsData, value); }
        }

        public bool? IsFavorite
        {
            get { return _isFavorite; }
            set { SetProperty(ref _isFavorite, value); }
        }
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        public bool? IsAvailable
        {
            get { return _isAvailable; }
            set { SetProperty(ref _isAvailable, value); }
        }

        public bool? IsBlacklisted
        {
            get { return _isBlacklisted; }
            set { SetProperty(ref _isBlacklisted, value); }
        }

        [JsonIgnore]
        public ICommand DeleteDomainCommand
        {
            get
            {
                return new RelayCommand<ListView>(
                    (listView) =>
                    {
                        var items = listView.ItemsSource as ObservableCollection<SingleDomain>;
                        items.Remove(this);
                    });
            }
        }

        [JsonIgnore]
        public ICommand AddToFavoritesCommand => new RelayCommand<MainViewModel>(vm =>
        {
            IsFavorite = true;
            vm.FavoriteDomains.Add(this);
            vm.FavoriteDomainsUpdated?.Invoke();
            //vm.SaveDomainsToFileAsync(vm.FavoriteDomains, "FavoriteDomains.xml").ConfigureAwait(false);
            //vm.LoadDataAsync().ConfigureAwait(false);
        });

        [JsonIgnore]
        public ICommand RemoveFromFavoritesCommand => new RelayCommand<MainViewModel>(vm =>
        {
            IsFavorite = false;

            var favoriteDomain = vm.FavoriteDomains.FirstOrDefault(x => x.Address == Address);
            if (favoriteDomain != null)
            {
                favoriteDomain.IsFavorite = false;
                vm.FavoriteDomains.Remove(favoriteDomain);
                vm.FavoriteDomainsUpdated?.Invoke();
            }

            var domain = vm.Domains.FirstOrDefault(x => x.Address == Address);
            if (domain != null)
            {
                domain.IsFavorite = false;
            }
            //vm.SaveDomainsToFileAsync(vm.FavoriteDomains, "FavoriteDomains.xml").ConfigureAwait(false);
            //vm.LoadDataAsync().ConfigureAwait(false);
        });

        [JsonIgnore]
        public ICommand AddToBlacklistCommand => new RelayCommand<MainViewModel>(vm =>
        {
            IsBlacklisted = true;
            vm.BlacklistedDomains.Add(this);
            vm.BlacklistedDomainsUpdated?.Invoke();
            //vm.SaveDomainsToFileAsync(vm.BlacklistedDomains, "BlacklistedDomains.xml").ConfigureAwait(false);
        });

        [JsonIgnore]
        public ICommand RemoveFromBlacklistCommand => new RelayCommand<MainViewModel>(vm =>
        {
            IsBlacklisted = false;

            var blacklistedDomain = vm.BlacklistedDomains.FirstOrDefault(x => x.Address == Address);
            if (blacklistedDomain != null)
            {
                blacklistedDomain.IsBlacklisted = false;
                vm.BlacklistedDomains.Remove(blacklistedDomain);
                vm.BlacklistedDomainsUpdated?.Invoke();
            }

            var domain = vm.Domains.FirstOrDefault(x => x.Address == Address);
            if (domain != null)
            {
                domain.IsBlacklisted = false;
            }
            //vm.SaveDomainsToFileAsync(vm.BlacklistedDomains, "BlacklistedDomains.xml").ConfigureAwait(false);
        });
        [JsonIgnore]
        public ICommand OpenInBrowserCommand => new RelayCommand<string>(s => Process.Start(s));
        [JsonIgnore]
        public ICommand BuyCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //string GODADDY_REQUEST_BASE_URL = "https://www.godaddy.com/domains/searchresults.aspx?checkAvail=1&domainToCheck={0}";
                    string GODADDY_REQUEST_BASE_URL = "http://www.expyred.com";
                    string GODADDY_REQUEST_BASE_REQUEST_URL = string.Format(GODADDY_REQUEST_BASE_URL, this.DomainName);

                    ProcessStartInfo startInfo = new ProcessStartInfo(GODADDY_REQUEST_BASE_REQUEST_URL);
                    using (Process process = Process.Start(startInfo))
                    {

                    }
                });
            }
        }

        public string SourceURL
        {
            get
            {
                return _sourceURL;
            }

            set
            {
               SetProperty(ref _sourceURL , value);
            }
        }

        public bool? IsBlacklistedPermanent
        {
            get
            {
                return _isBlacklistedPermanent;
            }

            set
            {
               SetProperty(ref _isBlacklistedPermanent ,value);
            }
        }
    }
}
