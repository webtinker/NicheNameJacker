using Newtonsoft.Json;

namespace NicheNameJacker.Schema.BackLinks
{
    public class WikipediaBackLink : BaseBackLink
    {
        [JsonIgnore]
        public WikipediaSearchResult SearchResult { get; set; }
    }
}
