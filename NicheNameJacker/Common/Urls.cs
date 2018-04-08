using System;

namespace NicheNameJacker.Common
{
    public static class Urls
    {
        static readonly string VersionNumber = Guid.NewGuid().ToString();

        public static readonly string LeftAds = "http://www.nichenamejacker.com/left-ads.html?v=" + VersionNumber;
        public static readonly string StartPage = "http://nichenamejacker.com/startpage.html?v=" + VersionNumber;
        public const string Expyred = "http://www.expyred.com";
        public const string NicheReaper = "http://niche-reaper.nichenamejacker.com";
        public const string ContentReaper = "http://content-reaper.nichenamejacker.com";
        public const string Weird = "http://weird-niche-content.nichenamejacker.com";
        public const string VideoMaker = "http://video-maker-fx.nichenamejacker.com";
        public const string AutoBlog = "http://autoblog-samurai.nichenamejacker.com";

        public const string Wayback = "http://waybackmachinedownload.com";
        public const string ArhivePrefix = "https://web.archive.org/web/*/";

        public const string MembershipPage = "http://nichejacker.com/members/";

        public const string DomDetailerPage = "http://nichejacker.com/domdetailer";

        public const string Support = "http://nichenamejacker.com/members/support";

        public const string WaybackDownloader = "http://waybackmachinedownload.com";
        public const string VideoJeet = "http://nichejacker.com/video-jeet";
        public const string ExpiredDomainScraper = "http://nichejacker.com/expired-domain-scraper";
        public const string FlipBidz = "http://flipbidz.com";
        public const string RankCipher = "http://nichejacker.com/rank-cipher";
    }
}
