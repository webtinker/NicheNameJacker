using NicheNameJacker.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace NicheNameJacker.ViewModels.SubViewModels
{
    public class SelectionSubViewModel
    {
        private IEnumerable<SingleDomain> _domains;

        public SelectionSubViewModel(IEnumerable<SingleDomain> domains)
        {
            _domains = domains;
        }
        public void ChangeDomainsReference(IEnumerable<SingleDomain> domains)
        {
            _domains = domains;
        }

        public ICommand AllCommand => new RelayCommand(() => ChangeSelection(_domains));

        public ICommand ToggleAllCommand => new RelayCommand<bool>((b) => ChangeSelection(_domains, b));

        public ICommand UnavailableCommand => new RelayCommand(() => ChangeSelection(_domains.Where(d => d.IsAvailable == false)));
        public ICommand ClearCommand => new RelayCommand(() => ChangeSelection(_domains, false));

        void ChangeSelection(IEnumerable<SingleDomain> domains, bool isSelected = true)
        {
            foreach (var item in domains)
            {
                item.IsSelected = isSelected;
            }
        }
    }
}
