using NicheNameJacker.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicheNameJacker.ViewModels
{
    /// <summary>
    /// This class holds all the domains against a particular search query
    /// </summary>
    public class DomainData : ObservableBase
    {
        private string _searchQuery;
        private ObservableCollection<SingleDomain> _domains;
        private int _domainsCount;
        public DomainData()
        {
            Domains = new ObservableCollection<SingleDomain>();
            Domains.CollectionChanged += Domains_CollectionChanged;

        }

        private void Domains_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DomainsCount = Domains.Count;
        }
        /// <summary>
        /// The term that was searched
        /// </summary>
        public string SearchQuery
        {
            get
            {
                return _searchQuery;
            }
            set
            {
                SetProperty(ref _searchQuery, value);
            }
        }
        
        /// <summary>
        /// the count of domains that have been added. (This is just for showing on UI
        /// </summary>
        public int DomainsCount
        {
            set { SetProperty(ref _domainsCount, value); }
            get
            {
                return _domainsCount;
            }
        }

        /// <summary>
        /// The list of domian against a particular query
        /// </summary>
        public ObservableCollection<SingleDomain> Domains
        {
            get
            {
                return _domains;
            }

            set
            {
                SetProperty(ref _domains, value);
            }
        }
    }
}
