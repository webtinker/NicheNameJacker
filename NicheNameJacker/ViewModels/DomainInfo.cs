using System;
using NicheNameJacker.Common;
using NicheNameJacker.Interfaces;

namespace NicheNameJacker.ViewModels
{
    public class DomainInfo : ObservableBase, IDomDetailerStats
    {
        public string Address { get; set; }

        bool _statsLoaded;
        public bool StatsLoaded
        {
            get { return _statsLoaded; }
            set { SetProperty(ref _statsLoaded, value); }
        }

        bool _isGettingStats;
        public bool IsGettingStats { get { return _isGettingStats; } set { SetProperty(ref _isGettingStats, value); } }

        string _stats;
        public string Stats { get { return _stats; } set { SetProperty(ref _stats, value); } }

        bool _isSelected;
        private DomDetailerData _statsData;
        public bool IsSelected { get { return _isSelected; } set { SetProperty(ref _isSelected, value); } }

        

        public DomDetailerData StatsData
        {
            get { return _statsData; }
            set { SetProperty(ref _statsData, value); }
        }

        private bool? _isAvailable;

        public bool? IsAvailable
        {
            get{ return _isAvailable; }

            set { SetProperty(ref _isAvailable, value); }
        }
    }
}
