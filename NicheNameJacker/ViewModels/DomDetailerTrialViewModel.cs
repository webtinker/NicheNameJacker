using NicheNameJacker.Common;

namespace NicheNameJacker.ViewModels
{
    public class DomDetailerTrialViewModel : ObservableBase
    {
        public string Version { get; } = AssemblyInfo.VersionNumber;

        //public string InformationText => CreditsUsedManager.CreditsQuotaExhausted() 
        //    ? "You've run out of FREE DomDetailer credits. In order to keep checking Moz and Majestic metrics, you must have a DomDetailer api key. The credits are very inexpensive and never expire."
        //    : $"All paid members receive 100 free DomDetailer credits for checking Moz and Majestic metrics. Once your credits are used up, the metrics will stop working. In order to avoid this, get a DomDetailer api key and enter it in the Settings window. {CreditsUsedManager.CreditsLeft}";
    }
}
