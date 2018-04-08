using NicheNameJacker.Commands;
using NicheNameJacker.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NicheNameJacker.ViewModels.SubViewModels
{
    public class SaverSubViewModel
    {
        readonly IEnumerable<SingleDomain> _domains;
        readonly IEnumerable<SingleDomain> _favorites;

        public SaverSubViewModel(IEnumerable<SingleDomain> domains, IEnumerable<SingleDomain> favorites)
        {
            _domains = domains;
            _favorites = favorites;
        }

        public ICommand AvailableCommand => new RelayCommand(() => SaveDomains(_domains.Where(d => d.IsAvailable == true).ToList(), "No domains are available"));
        public ICommand SelectedCommand => new RelayCommand(() => SaveDomains(_domains.Where(d => d.IsSelected).ToList(), "No domains are selected"));
        public ICommand FavoritesCommand => new RelayCommand(() => SaveFavorites(_favorites.ToList()));

        public static void SaveFavorites(ICollection<SingleDomain> domains, string noDomainsMessage = "No favorites")
        {
            if (domains.Any())
            {
                var filename = StandardDialogs.RequestCsvSaveFilename();
                if (filename != null)
                {
                    var data = domains.Select(d => new
                    {
                        d.Address,
                        d.DomainName,
                        d.TopLevelDomainName,
                        d.DomainSource,
                        d.DomainSourceAddress,
                        d.Status,
                        d.IsFavorite,
                        d.IsAvailable,
                        d.IsBlacklisted,
                        Stats = d.StatsLoaded ? d.Stats : ""
                    });
                    Assistant.SerializeToCsvFile(filename, data);
                }
            }
            else
            {
                MessageBox.Show(noDomainsMessage);
            }
        }

        static void SaveDomains(List<SingleDomain> domains, string noDomainsMessage)
        {
            if (domains.Any())
            {
                var filename = StandardDialogs.RequestCsvSaveFilename();
                if (filename != null)
                {
                    Assistant.SerializeToCsvFile(filename, domains);
                }
            }
            else
            {
                MessageBox.Show(noDomainsMessage);
            }
        }
    }
}
