namespace NicheNameJacker.Common.Membership
{
    public class NnjPermissionSet : IDomDetailerPermissionSet
    {
        private NnjPermissionSet(
            bool canScrapeYoutube = false,
            bool canScrapeWiki = false,
            bool canScrapeMashable = false,
            bool canBulkSearch = false,
            bool canScrapeHuffPost = false,
            bool canUseMoreOptions = false,
            bool canUseDomDetailerStatsForSingle = false,
            bool canUseDomDetailerStatsForMultiple = false,
            bool canUseExplore = false,
            bool canScrapeYahoo = false,
            bool showUpgradeLink = false,
            bool canDeleteUnavailable = false,
            bool showAutoStatOption = false,
            bool canSeeSourceUrl = false)
        {
            CanScrapeWiki = canScrapeWiki;
            CanBulkSearch = canBulkSearch;
            CanScrapeMashable = canScrapeMashable;
            CanScrapeYahoo = canScrapeYahoo;
            CanScrapeHuffPost = canScrapeHuffPost;
            CanScrapeYoutube = canScrapeYoutube;
            CanUseMoreOptions = canUseMoreOptions;
            CanUseDomDetailerStatsForSingle = canUseDomDetailerStatsForSingle;
            CanUseDomDetailerStatsForMultiple = canUseDomDetailerStatsForMultiple;
            CanUseExplore = canUseExplore;
            ShowUpgradeLink = showUpgradeLink;
            CanDeleteUnavailable = canDeleteUnavailable;
            ShowAutoStats = showAutoStatOption;
            CanSeeSourceUrl = canSeeSourceUrl;
        }

        public bool CanDeleteUnavailable { get; set; }
        public bool CanScrapeWiki { get; }

        public bool CanScrapeHuffPost { get; }

        public bool CanScrapeMashable { get; }

        public bool CanScrapeYahoo { get; }

        public bool CanBulkSearch { get; }

        public bool ShowUpgradeLink { get; }
        public bool CanScrapeYoutube { get; }
        public bool CanUseMoreOptions { get; }
        public bool CanUseDomDetailerStatsForSingle { get; }
        public bool CanUseDomDetailerStatsForMultiple { get; }

        public bool CanUseExplore { get; }
        public bool ShowAutoStats { get; }

        public bool CanSeeSourceUrl { get; }

        public static readonly NnjPermissionSet Lite = new NnjPermissionSet(
            canUseDomDetailerStatsForSingle: false,
            canUseDomDetailerStatsForMultiple: false,
            showUpgradeLink: true
        );

        public static readonly NnjPermissionSet Pro = new NnjPermissionSet
        (
            canUseMoreOptions: true,
            canUseDomDetailerStatsForSingle: true,
            canUseDomDetailerStatsForMultiple: true,
            canUseExplore: true,
            showUpgradeLink: true,
            canScrapeWiki: true
        );

        public static readonly NnjPermissionSet Trail = new NnjPermissionSet
        (
            canScrapeWiki: true,
            canScrapeHuffPost: true,
            canScrapeYoutube: true,
            canScrapeMashable: true,
            canScrapeYahoo: true,
            canUseMoreOptions: true,
            canBulkSearch: true,
            canUseDomDetailerStatsForSingle: true,
            canUseDomDetailerStatsForMultiple: true,
            canUseExplore: true,
            showUpgradeLink: true,
            canDeleteUnavailable: true,
            showAutoStatOption: true,
            canSeeSourceUrl: true
        );

        public static readonly NnjPermissionSet Elite = new NnjPermissionSet
        (
            canScrapeWiki: true,
            canScrapeHuffPost: true,
            canScrapeYoutube: true,
            canScrapeMashable: true,
            canScrapeYahoo: true,
            canUseMoreOptions: true,
            canBulkSearch: true,
            canUseDomDetailerStatsForSingle: true,
            canUseDomDetailerStatsForMultiple: true,
            canUseExplore: true,
            canDeleteUnavailable: true,
            showAutoStatOption: true,
            showUpgradeLink: true,
            canSeeSourceUrl: true
        );

        public static readonly NnjPermissionSet Lifetime = new NnjPermissionSet
        (
            canScrapeWiki: true,
            canScrapeHuffPost: true,
            canScrapeMashable: true,
            canScrapeYoutube: true,
            canScrapeYahoo: true,
            canUseMoreOptions: true,
            canBulkSearch: true,
            canUseDomDetailerStatsForSingle: true,
            canUseDomDetailerStatsForMultiple: true,
            canUseExplore: true,
            canDeleteUnavailable: true,
            showAutoStatOption: true,
            canSeeSourceUrl: true
        );

        public static readonly NnjPermissionSet Yearly = new NnjPermissionSet //Kept similar to lifetime
       (
            canScrapeWiki: true,
            canScrapeHuffPost: true,
            canScrapeMashable: true,
            canScrapeYoutube: true,
            canScrapeYahoo: true,
            canUseMoreOptions: true,
            canBulkSearch: true,
            canUseDomDetailerStatsForSingle: true,
            canUseDomDetailerStatsForMultiple: true,
            canUseExplore: true,
            canDeleteUnavailable: true,
            showAutoStatOption: true,
            showUpgradeLink: true,
            canSeeSourceUrl:true
       );
    }
}
