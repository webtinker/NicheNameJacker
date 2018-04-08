using System.Collections.Generic;

namespace NicheNameJacker.Schema.Trial
{
    class CreditsUsed
    {
        public CreditsUsed()
        {
            CheckedDomains = new List<string>();
        }

        public List<string> CheckedDomains { get; set; }
    }
}
