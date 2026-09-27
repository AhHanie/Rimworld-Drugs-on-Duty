using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Drugs_on_Duty.Patches
{
    /// <summary>
    /// Caches whether the last scheduled-drug search during Work hours found anything, so the
    /// priority postfix can skip re-searching for an unavailable drug on every work job. Outside
    /// Work hours the giver runs at its normal (lower) priority and isn't a hot path, so we leave
    /// its cache alone there.
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_TakeDrugsForDrugPolicy), "TryGiveJob")]
    public static class Patch_JobGiver_TakeDrugsForDrugPolicy_TryGiveJob
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Job __result)
        {
            if (pawn.timetable == null || pawn.timetable.CurrentAssignment != TimeAssignmentDefOf.Work)
            {
                return;
            }

            if (__result == null)
            {
                ScheduledDrugRetry.RecordMiss(pawn);
            }
            else
            {
                ScheduledDrugRetry.RecordHit(pawn);
            }
        }
    }
}
