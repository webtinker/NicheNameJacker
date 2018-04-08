using log4net;
using Newtonsoft.Json.Linq;
using NicheNameJacker.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static NicheNameJacker.Common.Assistant;

namespace NicheNameJacker.Common.Membership
{
    public class PermissionAssistant
    {
        private static readonly ILog _log;

        static PermissionAssistant()
        {
            log4net.Config.XmlConfigurator.Configure();
            _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
        static MembershipPlan _currentPlan;
        static event Action MembershipPlanUpdated;

        public static void SetMembershipPlan(MembershipPlan plan)
        {
            _currentPlan = plan;
            MembershipPlanUpdated?.Invoke();
        }

        public static void InvalidateYouTubeSettingsState()
        {
            MembershipPlanUpdated?.Invoke();
        }

        public static PermissionSet GetCurrentPermissionsAndSubscribe(Action membershipPlanUpdated)
        {
            MembershipPlanUpdated += membershipPlanUpdated;
            return CurrentPermissions;
        }

        public static PermissionSet CurrentPermissions => PermissionSet.PlanDictionary[_currentPlan];

        public static async Task<MembershipPlan?> VerifyKey(string registrationKey)
        {
            try
            {
                _log.Info("Inside VerifyKeyAsync");
                var url = $"http://nichenamejacker.com/members/return-plan-name/?userID={registrationKey}";
                //
                _log.Info($"The Url Constructed was : {url}");
                var resp = await HttpClient.GetStringAsync(url);
                _log.Info($"Web response : {resp}");
                if (resp == null)
                    return null;

                var planname = JObject.Parse(resp).ValueOf("planname");
                _log.Info($"Plan name: {planname}");

                foreach (var plan in planname.Split(','))
                {
                    var membership = ParseMembershipPlan(plan);
                    if (membership != null)
                    {
                        return membership;
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                return null;
            }
        }
        static MembershipPlan? ParseMembershipPlan(string planname)
        {
            switch (planname)
            {
                case "nnj_lite_free"://"NNJ Lite Free":
                    return MembershipPlan.Lite;

                case "nnj_pro_monthly":// "NNJ Pro":
                    return MembershipPlan.Pro;

                case "nnj_elite_1_day_trial":// "NNJ Elite 1 Day Trial":
                    return MembershipPlan.Trial;

                case "nnj_elite_1_week_trial":// "NNJ Elite 1 Week Trial":
                    return MembershipPlan.Trial;

                case "nnj_elite_1_month_trial":
                    return MembershipPlan.Trial;

                case "nnj_elite_monthly":// "NNJ Elite":
                    return MembershipPlan.Elite;

                case "nnj_elite_yearly":// "NNJ Yearly":
                    return MembershipPlan.Yearly;

                case "nnj_elite_lifetime"://"NNJ Elite Lifetime":
                    return MembershipPlan.Lifetime;
                }

            return null;
        }

        public static IReadOnlyList<MembershipPlan> MembershipPlans = Enum.GetValues(typeof(MembershipPlan)).OfType<MembershipPlan>().ToList();
        
        public static void ShowPermissionDeniedMessage(string membershipPlanName)
        {
            var result = MessageBox.Show($@"This functionality is not available in the {membershipPlanName} version. Would you like to visit the membership page to upgrade membership?", "Alert", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Process.Start(Urls.MembershipPage);
            }
        }
    }
}
