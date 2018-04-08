using System;
using NicheNameJacker.Extensions;
using NicheNameJacker.Properties;
using NicheNameJacker.Schema.Trial;
using NicheNameJacker.Common.Membership;

namespace NicheNameJacker.Common
{
    public enum CreditsIncrementState
    {
        DomainExists = 1,
        QuotaExhausted = 2,
        DomainAdded = 3
    }

    public class CreditsUsedManager
    {
        static CreditsUsedManager()
        {
            var currentPlan = PermissionAssistant.CurrentPermissions.MembershipPlan;

            if (currentPlan == MembershipPlan.Elite ||
                currentPlan == MembershipPlan.Lifetime ||
                currentPlan == MembershipPlan.Yearly)
                MaxCredits = 1000;
            else if (currentPlan == MembershipPlan.Pro||
                     currentPlan == MembershipPlan.Trial)
                MaxCredits = 100;
            else if (currentPlan == MembershipPlan.Lite)
                MaxCredits = 50;
            else
                MaxCredits = 0;
        }

        const string CreditsUsedFileName = "Credits.json";

        private static readonly object _lock = new object();

        private static readonly Lazy<CreditsUsed> CreditsUsed = new Lazy<CreditsUsed>(() =>
            LocalStorage.GetDataFromFile<CreditsUsed>(CreditsUsedFileName) ?? new CreditsUsed());
         
        private static readonly int MaxCredits = 100;
        
        public static bool CreditsQuotaExhausted()
        {
            return CreditsUsed.Value.CheckedDomains.Count >= MaxCredits;
        }

        public static int Credits => CreditsUsed.Value.CheckedDomains.Count;

        //public static string CreditsLeft => !Settings.Default.DomDetailerKey.IsNullOrEmpty() ? null : $"You have {MaxCredits - Credits} free credits left";

        public static string DefaultToolTip => $"Get detailer stats";

        public static void SaveCredits()
        {
            if (!CreditsUsed.IsValueCreated)
                return;

            LocalStorage.SaveDataToFile(CreditsUsed.Value, CreditsUsedFileName);
        }

        public static CreditsIncrementState IncreaseUsage(string domain)
        {
            lock (_lock)
            {
                if (CreditsUsed.Value.CheckedDomains.Contains(domain))
                    return CreditsIncrementState.DomainExists;

                if (CreditsQuotaExhausted())
                    return CreditsIncrementState.QuotaExhausted;
                
                CreditsUsed.Value.CheckedDomains.Add(domain);
                
                return CreditsIncrementState.DomainAdded;
            }
        }
    }
}
