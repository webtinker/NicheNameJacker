using NicheNameJacker.Commands;
using NicheNameJacker.Common;
using NicheNameJacker.ViewModels.SubViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using NicheNameJacker.Common.Membership;
using NicheNameJacker.Properties;
using NicheNameJacker.Extensions;
using System.ComponentModel;

namespace NicheNameJacker.ViewModels
{
    public class BulkCheckerViewModel : ObservableBase
    {
        public DomDetailerSubViewModel DomDetailer { get; }
        public ObservableCollection<DomainInfo> Domains { get; } = new ObservableCollection<DomainInfo>();
        public PermissionSet Permissions => PermissionAssistant.GetCurrentPermissionsAndSubscribe(() => OnPropertyChanged(nameof(Permissions)));

        public BulkCheckerViewModel()
        {
            DomDetailer = new DomDetailerSubViewModel(Domains, Permissions.Nnj, Permissions.NnjPlanName, () => { });
            _isDomDetailerAvailable = PermissionAssistant.CurrentPermissions.MembershipPlan != MembershipPlan.Lite;

            bool isDesignMode = (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()));
            bool isDesignValues = Environment.GetCommandLineArgs()?.Contains("WithDesignValues") == true;

            if (isDesignValues || isDesignMode)
            {
                Domains = new ObservableCollection<DomainInfo>()
                {
                    new DomainInfo{ Address = "asdfasdsdfxcxc.com", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new DomainInfo{ Address = "asdfsafdsd.com", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new DomainInfo{ Address = "asdfsafdsd.com", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new DomainInfo{ Address = "asdwercxvgfhf.com",  StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new DomainInfo{ Address = "asdgherwtsfvasdf.com", StatsLoaded = true,  StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new DomainInfo{ Address = "bdfsdweghaerthshbadfv.com",  StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new DomainInfo{ Address = "asdf4aw45fsdf.com", IsGettingStats = false,StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new DomainInfo{ Address = "dsf4fsdf4sfsa3sadf3.com",  StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new DomainInfo{ Address = "adsf4afa2afsdf.com",  StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new DomainInfo{ Address = "fdsgdf5adf3af.com",  StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") },
                    new DomainInfo{ Address = "adsf7sad5r3sfas4.com", StatsData = new DomDetailerData("5","5","6","2","1","3","6","3","8") }
                };

                IsDomDetailerAvailable = true;
            }
        }

        static readonly string HttpPrefix = Uri.UriSchemeHttp + Uri.SchemeDelimiter;
        static readonly string HttpsPrefix = Uri.UriSchemeHttps + Uri.SchemeDelimiter;
        static readonly string[] ProtocolPrefixes = new[] { HttpPrefix, HttpsPrefix };
        private bool _isDomDetailerAvailable;
        string MakeAbsolute(string urlString) => ProtocolPrefixes.None(urlString.StartsWith) ? HttpPrefix + urlString : urlString;
        Uri MakeUri(string urlString)
        {
            if (!urlString.Contains('.') || urlString.None(char.IsLetter))
            {
                return null;
            }

            Uri uri;
            Uri.TryCreate(urlString, UriKind.Absolute, out uri);
            return uri;
        }

        public ICommand AddDomainCommand => new RelayCommand<string>(urlString =>
        {
            if (string.IsNullOrWhiteSpace(urlString)) return;

            var absolute = MakeAbsolute(urlString);
            var uri = MakeUri(absolute);
            if (uri == null)
            {
                MessageBox.Show($"{urlString} is not a valid url");
                return;
            }

            if (Domains.Any(d => d.Address == uri.Host))
            {
                MessageBox.Show($"Domain is already present in the list");
                return;
            }

            Domains.Add(new DomainInfo { Address = uri.Host });
        });

        public ICommand RemoveDomainCommand => new RelayCommand<DomainInfo>(domain => Domains.Remove(domain));

        public ICommand ImportCommand => new RelayCommand(() =>
        {
            var filename = StandardDialogs.OpenFile();
            if (!string.IsNullOrWhiteSpace(filename))
            {
                var lines = File.ReadAllLines(filename);
                var uris = lines?.Distinct().Select(l => MakeAbsolute(l)).Select(x => MakeUri(x)).Where(x => x != null);
                if (uris?.Any() != true)
                {
                    MessageBox.Show("No urls could be found in this file. Please provide a file with one url per line format.");
                    return;
                }

                var domains = uris.Where(u => Domains.None(d => d.Address == u.Host)).Select(u => new DomainInfo { Address = u.Host });
                Domains.AddRange(domains);
            }
        });

        public ICommand ClearCommand => new RelayCommand(Domains.Clear);

        public ICommand SaveCommand => new RelayCommand(() =>
        {
            var filename = StandardDialogs.RequestCsvSaveFilename();
            if (filename != null)
            {
                Assistant.SerializeToCsvFile(filename, Domains.Select(d => new
                {
                    d.Address,
                    Stats = d.StatsLoaded ? d.Stats : ""
                }));
            }
        });

        public ICommand DeleteDomainCommand => new RelayCommand<DomainInfo>((d) => Domains.Remove(d));

        public ICommand DeleteSelectedDomainsCommand => new RelayCommand(() => Domains.RemoveWhere(d => d.IsSelected));

        public ICommand ToggleAllCommand => new RelayCommand<bool>(b => { Domains.ToList().ForEach(d => d.IsSelected = b); });

        public bool IsDomDetailerAvailable
        {
            get     { return _isDomDetailerAvailable; }
            set
            {
                SetProperty(ref _isDomDetailerAvailable, value);
                OnPropertyChanged(nameof(CanBulkSearch));
            }
        }

        public bool CanBulkSearch => IsDomDetailerAvailable && Permissions.Nnj.CanUseDomDetailerStatsForMultiple;
    }
}
