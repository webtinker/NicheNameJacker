using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NicheNameJacker.ViewModels;
using NicheNameJacker.Common;
using System.Windows.Input;
using NicheNameJacker.Commands;
using System.Diagnostics;

namespace NicheNameJacker.Schema
{
    public class BaseSearchResult : ObservableBase
    {
        private bool _isSelected;
        public BaseSearchResult() { }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public string Description { get; set; }
        public string SourceAddress { get; set; }
        public IEnumerable<SingleDomain> Domains { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public string DomainsHighlight
        {
            get
            {
                string output = string.Empty;
                if (this.Domains != null)
                {
                    foreach (var item in this.Domains)
                    {
                        output += (item.Address + ", ");
                    }
                }

                return output;
            }
        }
            /*
            get
            {
                return new RelayCommand(() =>
                {
                    //xxx

                    The below will stupidly load the selected page into the user's default browser.
                    ProcessStartInfo startInfo = new ProcessStartInfo(this.SourceAddress);
                    using (Process process = Process.Start(startInfo))
                    {

                    }
                });
            }*/
        //}
    }
}
