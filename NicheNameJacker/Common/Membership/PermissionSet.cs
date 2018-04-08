using System.Collections.Generic;

namespace NicheNameJacker.Common.Membership
{
    public class PermissionSet
    {
        private PermissionSet(MembershipPlan plan, NnjPermissionSet nnj, PbnPermissionSet pbn)
        {
            MembershipPlan = plan;
            Nnj = nnj;
            Pbn = pbn;

            switch (MembershipPlan)
            {
                case MembershipPlan.Lite:
                    MembershipPlanName = "Lite";
                    NnjPlanName = PbnPlanName = "NicheNameJacker Lite";
                    break;
                case MembershipPlan.Pro:
                    MembershipPlanName = "Pro";
                    NnjPlanName = PbnPlanName = "NicheNameJacker Pro";
                    break;
                case MembershipPlan.Elite:
                    MembershipPlanName = "Elite";
                    NnjPlanName = PbnPlanName = "NicheNameJacker Elite";
                    break;
                case MembershipPlan.Trial:
                    MembershipPlanName = "Trial";
                    NnjPlanName = PbnPlanName = "NicheNameJacker Trial";
                    break;
                case MembershipPlan.Lifetime:
                    MembershipPlanName = "Lifetime";
                    NnjPlanName = PbnPlanName = "NicheNameJacker Lifetime";
                    break;
                case MembershipPlan.Yearly:
                    MembershipPlanName = "Yearly";
                    NnjPlanName = PbnPlanName = "NicheNameJacker Yearly";
                    break;
                default:
                    break;
            }
        }

        public MembershipPlan MembershipPlan { get; }
        public string MembershipPlanName { get; }
        public string MembershipPlanFullname => $"{NnjPlanName}";
        public string NnjPlanName { get; }
        public string PbnPlanName { get; }
        public NnjPermissionSet Nnj { get; }
        public PbnPermissionSet Pbn { get; }

        public static readonly PermissionSet Lite = new PermissionSet(MembershipPlan.Lite, NnjPermissionSet.Lite, PbnPermissionSet.Lite);
        public static readonly PermissionSet Pro = new PermissionSet(MembershipPlan.Pro, NnjPermissionSet.Pro, PbnPermissionSet.Pro);
        public static readonly PermissionSet Elite = new PermissionSet(MembershipPlan.Elite, NnjPermissionSet.Elite, PbnPermissionSet.Elite);
        public static readonly PermissionSet Trial = new PermissionSet(MembershipPlan.Trial, NnjPermissionSet.Trail, PbnPermissionSet.Trail);
        public static readonly PermissionSet Lifetime = new PermissionSet(MembershipPlan.Lifetime, NnjPermissionSet.Lifetime, PbnPermissionSet.LifeTime);
        public static readonly PermissionSet Yearly = new PermissionSet(MembershipPlan.Yearly, NnjPermissionSet.Yearly, PbnPermissionSet.Yearly);

        public static readonly IReadOnlyDictionary<MembershipPlan, PermissionSet> PlanDictionary = new Dictionary<MembershipPlan, PermissionSet>
        {
            [MembershipPlan.Lite] = Lite,
            [MembershipPlan.Pro] = Pro,
            [MembershipPlan.Elite] = Elite,
            [MembershipPlan.Trial] = Trial,
            [MembershipPlan.Lifetime] = Lifetime,
            [MembershipPlan.Yearly] = Yearly
        };
    }
}
