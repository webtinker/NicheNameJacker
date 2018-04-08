using Newtonsoft.Json;

namespace NicheNameJacker.Schema.BackLinks
{
    public class YouTubeBackLink : BaseBackLink
    {
        [JsonIgnore]
        public YouTubeSearchResult SearchResult { get; set; }
    }
}
