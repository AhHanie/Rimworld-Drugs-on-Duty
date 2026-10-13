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
    ///
    /// This is an infix on the one <c>7.5f</c> literal in vanilla's GetPriority. That literal is only
    /// reached for the first policy entry whose <c>ShouldTryToTakeScheduledNow</c> is true, so the infix
    /// runs exactly when a drug is due and needs no due-check of its own.
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_TakeDrugsForDrugPolicy), nameof(JobGiver_TakeDrugsForDrugPolicy.GetPriority))]
    public static class Patch_JobGiver_TakeDrugsForDrugPolicy_GetPriority
    {
        // 9 (JobGiver_Work during Work) < 9.01 < 9.1 (hemogen), so this only beats ordinary work.
        public const float WorkHoursPriority = 9.01f;

        // The literal vanilla returns for a due scheduled drug. Must match the IL exactly or Harmony
        // throws at patch time ("no matching operation"), which is the loud failure we want if it changes.
        private const float VanillaDueDrugPriority = 7.5f;

        // __var_0 / __var_1 are GetPriority's locals: the current DrugPolicy and the loop index. They are
        // what vanilla's own loop was holding when it hit the 7.5f return, so policy[index].drug is exactly
        // the entry that made the drug due. This is what lets a per-drug setting be honored without
        // re-walking the policy. (Locals checked against the 1.7.4976 Assembly-CSharp IL; Harmony
        // validates their types when patching, so a reordered local fails at startup instead of misbinding.)
        [HarmonyPostfix]
        [HarmonyInfix(VanillaDueDrugPriority)]
        public static float Postfix(
            float __result,
            [HarmonyOuter] Pawn pawn,
            [HarmonyOuter] DrugPolicy __var_0,
            [HarmonyOuter] int __var_1)
        {
            if (!pawn.IsColonist || pawn.timetable == null || pawn.timetable.CurrentAssignment != TimeAssignmentDefOf.Work)
            {
                return __result;
            }

            // A recent failed stock search stands until the retry delay elapses (or the policy
            // changes), so we don't send the giver hunting for the same unavailable drug every
            // time a short work job finishes.
            if (!ScheduledDrugRetry.CanRetryNow(pawn))
            {
                return __result;
            }

            if (!ModSettings.IsEnabledForDrug(__var_0[__var_1].drug))
            {
                return __result;
            }

            return __result < WorkHoursPriority ? WorkHoursPriority : __result;
        }
    }
}
