namespace NicheNameJacker.Common.Membership
{
    public interface IDomDetailerPermissionSet
    {
        bool CanUseDomDetailerStatsForSingle { get; }
        bool CanUseDomDetailerStatsForMultiple { get; }
    }
}
