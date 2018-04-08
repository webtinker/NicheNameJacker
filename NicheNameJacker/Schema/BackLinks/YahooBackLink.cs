using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicheNameJacker.Schema.BackLinks
{
   public class YahooBackLink : BaseBackLink
    {
        [JsonIgnore]
        public YahooSearchResult SearchResult { get; set; }
    }
    
}
