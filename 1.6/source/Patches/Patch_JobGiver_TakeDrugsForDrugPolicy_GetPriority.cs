using HarmonyLib;
using RimWorld;
using Verse;

namespace Drugs_on_Duty.Patches
{
    /// <summary>
    /// Lets a due, allowed scheduled drug outrank ordinary work (but not food, chemical needs,
    /// neural supercharge or hemogen, all priority 9.1+) while a colonist's timetable says Work.
    /// Outside Work hours this is a no-op and vanilla's 7.5 stands. Per-drug settings
    /// (ModSettings) can opt individual drugs out of the boost, leaving them at vanilla behavior.
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_TakeDrugsForDrugPolicy), "GetPriority")]
    public static class Patch_JobGiver_TakeDrugsForDrugPolicy_GetPriority
    {
        // 9 (JobGiver_Work during Work) < 9.01 < 9.1 (hemogen), so this only beats ordinary work.
        public const float WorkHoursPriority = 9.01f;

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref float __result)
        {
            // Vanilla already decided no scheduled drug is due/allowed; nothing to raise.
            if (__result <= 0f)
            {
                return;
            }

            if (!pawn.IsColonist || pawn.timetable == null || pawn.timetable.CurrentAssignment != TimeAssignmentDefOf.Work)
            {
                return;
            }

            // A recent failed stock search stands until the retry delay elapses (or the policy
            // changes), so we don't send the giver hunting for the same unavailable drug every
            // time a short work job finishes.
            if (!ScheduledDrugRetry.CanRetryNow(pawn))
            {
                return;
            }

            // Re-walk the same due-check vanilla's GetPriority/TryGiveJob use, in the same policy
            // order, so the entry gated on here is the same one TryGiveJob will actually try first.
            // This is the only way to honor a per-drug setting: __result is just an aggregate float
            // and doesn't say which schedule entry made the drug due.
            DrugPolicy policy = pawn.drugs?.CurrentPolicy;
            if (policy == null)
            {
                return;
            }

            for (int i = 0; i < policy.Count; i++)
            {
                if (pawn.drugs.ShouldTryToTakeScheduledNow(policy[i].drug))
                {
                    if (!ModSettings.IsEnabledForDrug(policy[i].drug))
                    {
                        return;
                    }
                    break;
                }
            }

            if (__result < WorkHoursPriority)
            {
                __result = WorkHoursPriority;
            }
        }
    }
}
