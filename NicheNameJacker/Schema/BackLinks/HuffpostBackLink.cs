using Newtonsoft.Json;

namespace NicheNameJacker.Schema.BackLinks
{
    public class HuffpostBackLink : BaseBackLink
    {
        [JsonIgnore]
        public HuffpostSearchResult SearchResult { get; set; }
    }
}
