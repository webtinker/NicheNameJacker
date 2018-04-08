using Newtonsoft.Json;

namespace NicheNameJacker.Schema.BackLinks
{
    public class RedditBackLink : BaseBackLink
    {
        [JsonIgnore]
        public RedditSearchResult SearchResult { get; set; }
    }
}
