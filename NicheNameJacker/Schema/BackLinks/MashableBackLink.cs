using Newtonsoft.Json;

namespace NicheNameJacker.Schema.BackLinks
{
    public class MashableBackLink : BaseBackLink
    {
        [JsonIgnore]
        public MashableSearchResult SearchResult { get; set; }
    }
}
