using Newtonsoft.Json.Linq;
using NicheNameJacker.Utilities;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using NicheNameJacker.Interfaces;
using NicheNameJacker.ViewModels;
using Newtonsoft.Json;

namespace NicheNameJacker.Common
{
    
    public enum MetricsTypeEnum
    {
        None = 0,
        MozLinks = 1,
        MozPa = 2,
        MozDa = 3,
        MozRank = 4,
        MozTrust = 5,
        MajesticLinks = 6,
        MajesticRefDomains = 7,
        MajesticCF = 8,
        MajesticTF = 9
    }

    public class MetricsComparer : IComparer
    {
        private readonly MetricsTypeEnum _metrics;
        private readonly ListSortDirection _sortDirection;

        public MetricsComparer(MetricsTypeEnum metrics, ListSortDirection sortDirection)
        {
            _metrics = metrics;
            _sortDirection = sortDirection;
        }

        public ListSortDirection SortDirection => _sortDirection;

        public MetricsTypeEnum Metrics => _metrics;

        public int Compare(object x, object y)
        {
            var xDomain = x as IDomDetailerStats;
            var yDomain = y as IDomDetailerStats;
            if (xDomain == null || yDomain == null)
                throw new Exception("Invalid arguments");

            if (xDomain.StatsData == null)
            {
                if (yDomain.StatsData == null)
                    return 0;

                return _sortDirection == ListSortDirection.Ascending ? -1 : 1;
            }

            if (yDomain.StatsData == null)
                return _sortDirection == ListSortDirection.Ascending ? 1 : -1;

            ParameterExpression param = Expression.Parameter(typeof(IDomDetailerStats), "s");

            var method =
                Expression.Lambda<Func<IDomDetailerStats, string>>(
                    Expression.Property(Expression.Property(param, nameof(IDomDetailerStats.StatsData)),
                        _metrics.ToString()), param).Compile();

            var left = method(xDomain)?.ToDecimal() ?? 0;
            var right = method(yDomain)?.ToDecimal() ?? 0;
            if (left == right)
                return 0;

            if (left > right)
                return _sortDirection == ListSortDirection.Ascending ? 1 : -1;

            return _sortDirection == ListSortDirection.Ascending  ? - 1 : 1;
        }
    }

    public class DomDetailerAssistant
    {
        static Lazy<HttpClient> _httpClientFetcher = new Lazy<HttpClient>(() => new HttpClient());
        static HttpClient Client => _httpClientFetcher.Value;

        public static async Task<DomDetailerData> GetDetailsAsync(string domain, string key, string app = "DomDetailer")
        {
            try
            {
                var url = $"http://domdetailer.com/api/checkDomain.php?domain={domain}&apikey={key}&app={app}";
                var result = await Client.GetStringAsync(url);

                if (result.Contains("Account Not Found"))
                {
                    return new DomDetailerData(true);
                }

                var json = JObject.Parse(result);

                return new DomDetailerData(json.ValueOf("mozLinks"), json.ValueOf("mozPA"), json.ValueOf("mozDA"), json.ValueOf("mozRank"),
                    json.ValueOf("mozTrust"), json.ValueOf("majesticLinks"), json.ValueOf("majesticRefDomains"), json.ValueOf("majesticCF"), json.ValueOf("majesticTF"));
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                return null;
            }
        }
    }

    public class DomDetailerData
    {
        public DomDetailerData()
        { }
        public DomDetailerData(bool wrongCredentials)
        {
            WrongCredentials = wrongCredentials;
        }

        public DomDetailerData(string mozLinks, string mozPa, string mozDa, string mozRank, string mozTrust, string majesticLinks, string majesticRefDomains, string majesticCF, string majesticTF)
        {
            d_MozLinks = mozLinks.ToDecimal();
            MozLinks = d_MozLinks?.FormattedString();
            d_MozPa = mozPa.ToDecimal();
            MozPa = d_MozPa?.FormattedString();
            d_MozDa = mozDa.ToDecimal();
            MozDa = d_MozDa?.FormattedString();
            d_MozRank = mozRank.ToDecimal();
            MozRank = d_MozRank?.FormattedString();
            d_MozTrust = mozTrust.ToDecimal();
            MozTrust = d_MozTrust?.FormattedString();
            d_MajesticLinks = majesticLinks.ToDecimal();
            MajesticLinks = d_MajesticLinks?.FormattedString();
            d_MajesticRefDomains = majesticRefDomains.ToDecimal();
            MajesticRefDomains = d_MajesticRefDomains?.FormattedString();
            d_MajesticCF = majesticCF.ToDecimal();
            MajesticCF = d_MajesticCF?.FormattedString();
            d_MajesticTF = majesticTF.ToDecimal();
            MajesticTF = d_MajesticTF?.FormattedString();
        }
        public decimal? d_MozLinks { get; set; } = 0;
        public decimal? d_MozPa { get; set; } = 0;
        public decimal? d_MozDa { get; set; } = 0;
        public decimal? d_MozRank { get; set; } = 0;
        public decimal? d_MozTrust { get; set; } = 0;
        public decimal? d_MajesticLinks { get; set; } = 0;
        public decimal? d_MajesticRefDomains { get; set; } = 0;
        public decimal? d_MajesticCF { get; set; } = 0;
        public decimal? d_MajesticTF { get; set; } = 0;


        public bool WrongCredentials { get; set; }
        public string MozLinks { get; set; }
        public string MozPa { get; set; }
        public string MozDa { get; set; }
        public string MozRank { get; set; }
        public string MozTrust { get; set; }
        public string MajesticLinks { get; set; }
        public string MajesticRefDomains { get; set; }
        public string MajesticCF { get; set; }
        public string MajesticTF { get; set; }

        string MozData()
        {
            var result = "";
            if (!string.IsNullOrWhiteSpace(MozLinks))
            {
                result += "    MozL " + MozLinks;
            }

            if (!string.IsNullOrWhiteSpace(MozPa))
            {
                result += "    MozPA " + MozPa;
            }

            if (!string.IsNullOrWhiteSpace(MozDa))
            {
                result += "    MozDA " + MozDa;
            }

            if (!string.IsNullOrWhiteSpace(MozRank))
            {
                result += "    MozR " + MozRank;
            }

            if (!string.IsNullOrWhiteSpace(MozTrust))
            {
                result += "    MozT " + MozTrust;
            }
            return result.TrimStart();
        }
    
        string MajesticData()
        {
            var result = "";

            if (!string.IsNullOrWhiteSpace(MajesticLinks))
            {
                result += "    MajL " + MajesticLinks;
            }

            if (!string.IsNullOrWhiteSpace(MajesticRefDomains))
            {
                result += "    MajRD " + MajesticRefDomains;
            }

            if (!string.IsNullOrWhiteSpace(MajesticCF))
            {
                result += "    MajCF " + MajesticCF;
            }

            if (!string.IsNullOrWhiteSpace(MajesticTF))
            {
                result += "    MajTF " + MajesticTF;
            }

            return result.TrimStart();
        }

        public override string ToString()
        {
            return string.Join("    ", MozData(), MajesticData());
        }
    }
}
