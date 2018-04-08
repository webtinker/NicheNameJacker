namespace NicheNameJacker.Common.Membership
{
    public class PbnPermissionSet : IDomDetailerPermissionSet
    {
        private PbnPermissionSet(
            bool canScrapeYoutube = false,
            bool canBulkSearch = false,
            bool canUseDomDetailerStatsForSingle = false,
            bool canUseDomDetailerStatsForMultiple = false,
            bool canUseExplore = false,
            bool canScrapYahoo = false,
            bool canSeeSourceUrl = false)
        {
            CanBulkSearch = canBulkSearch;
            CanScrapeYoutube = canScrapeYoutube;
            CanUseDomDetailerStatsForSingle = canUseDomDetailerStatsForSingle;
            CanUseDomDetailerStatsForMultiple = canUseDomDetailerStatsForMultiple;
            CanUseExplore = canUseExplore;
            CanScrapYahoo = canScrapYahoo;
            CanSeeSourceUrl = canSeeSourceUrl;
        }

        public bool CanScrapYahoo { get; }
        public bool CanBulkSearch { get; }

        public bool CanScrapeYoutube { get; }
        public bool CanUseDomDetailerStatsForSingle { get; }
        public bool CanUseDomDetailerStatsForMultiple { get; }
        public bool CanUseExplore { get; }

        public bool CanSeeSourceUrl { get; }


        public static readonly PbnPermissionSet Lite = new PbnPermissionSet();

        public static readonly PbnPermissionSet Pro = new PbnPermissionSet
        (
            canScrapeYoutube: true,
            canUseDomDetailerStatsForSingle: true,
            canUseDomDetailerStatsForMultiple: true,
            canUseExplore: true
        );

        public static readonly PbnPermissionSet Elite = new PbnPermissionSet
        (
            canScrapeYoutube: true,
            canBulkSearch: true,
            canUseDomDetailerStatsForSingle: true,
            canUseDomDetailerStatsForMultiple: true,
            canUseExplore: true,
            canScrapYahoo: true,
            canSeeSourceUrl: true
        ); 

        public static readonly PbnPermissionSet Yearly = new PbnPermissionSet //similar to Elite
        (
            canScrapeYoutube: true,
            canBulkSearch: true,
            canUseDomDetailerStatsForSingle: true,
            canUseDomDetailerStatsForMultiple: true,
            canUseExplore: true,
            canScrapYahoo: true,
            canSeeSourceUrl: true
        );

        public static readonly PbnPermissionSet LifeTime = new PbnPermissionSet //similar to Elite
        (
            canScrapeYoutube: true,
            canBulkSearch: true,
            canUseDomDetailerStatsForSingle: true,
            canUseDomDetailerStatsForMultiple: true,
            canUseExplore: true,
            canScrapYahoo: true,
            canSeeSourceUrl: true
        );
        public static readonly PbnPermissionSet Trail = new PbnPermissionSet //similar to Elite
        (
            canScrapeYoutube: true,
            canBulkSearch: true,
            canUseDomDetailerStatsForSingle: true,
            canUseDomDetailerStatsForMultiple: true,
            canUseExplore: true,
            canScrapYahoo: true,
            canSeeSourceUrl: true
        );
    }
}
