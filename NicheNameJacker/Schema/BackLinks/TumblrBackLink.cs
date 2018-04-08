using Newtonsoft.Json;

namespace NicheNameJacker.Schema.BackLinks
{
    public class TumblrBackLink : BaseBackLink
    {
        [JsonIgnore]
        public TumblrSearchResult SearchResult { get; set; }
    }
}
