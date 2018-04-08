using NicheNameJacker.Common;

namespace NicheNameJacker.Interfaces
{
    public interface IDomDetailerStats
    {
        string Address { get; set; }
        bool IsGettingStats { get; set; }
        bool StatsLoaded { get; set; }
        string Stats { get; set; }
        bool IsSelected { get; set; }

        bool? IsAvailable { get; set; } //binding availability to dom detailer as mentioned in issue 187

        DomDetailerData  StatsData { get; set;}
    }
}
