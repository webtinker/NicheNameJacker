using Newtonsoft.Json;
using NicheNameJacker.Commands;
using System.Windows;
using System.Windows.Input;

namespace NicheNameJacker.Schema.BackLinks
{
    public class BaseBackLink
    {
        public string Source { get; set; }
        public string SourceAddress { get; set; }
        public string FullAddress { get; set; }

        [JsonIgnore]
        public ICommand CopyCommand => new RelayCommand(() => Clipboard.SetText(FullAddress));
    }
}
