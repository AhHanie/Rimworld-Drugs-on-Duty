using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Drugs_on_Duty.Patches
{
    /// <summary>
    /// Raising the scheduled-drug priority (see Patch_JobGiver_TakeDrugsForDrugPolicy_GetPriority)
    /// only matters the next time the think tree runs. A long work job (crafting, research, ...)
    /// doesn't re-run it on its own, so without this patch a drug that becomes due mid-job would
    /// sit un-taken until the job happens to end. This staggered, low-rate recheck asks the normal
    /// think tree to reconsider - via the same path the game already uses for damage/expiry overrides -
    /// only for colonists currently doing ordinary (non-emergency) work while a scheduled drug is due.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.JobTrackerTickInterval))]
    public static class Patch_Pawn_JobTracker_JobTrackerTickInterval
    {
        private const int CheckIntervalTicks = ScheduledDrugRetry.RetryDelayTicks;

        // GetPriority only reads the pawn's own drug policy state; one shared instance avoids
        // allocating a JobGiver_TakeDrugsForDrugPolicy per pawn per check.
        private static readonly JobGiver_TakeDrugsForDrugPolicy DrugPriorityEvaluator = new JobGiver_TakeDrugsForDrugPolicy();

        [HarmonyPostfix]
        public static void Postfix(Pawn_JobTracker __instance, int delta, Pawn ___pawn)
        {
            Pawn pawn = ___pawn;
            if (pawn == null || !pawn.Spawned || pawn.Dead || !pawn.IsColonist)
            {
                return;
            }

            // Staggers the check across pawns instead of testing everyone on the same tick.
            if (!pawn.IsHashIntervalTick(CheckIntervalTicks, delta))
            {
                return;
            }

            if (pawn.timetable == null || pawn.timetable.CurrentAssignment != TimeAssignmentDefOf.Work)
            {
                return;
            }

            if (pawn.jobs != __instance || __instance.startingNewJob || __instance.DeterminingNextJob)
            {
                return;
            }

            Job curJob = __instance.curJob;
            if (curJob == null || curJob.playerForced)
            {
                return;
            }

            // Never interrupt the emergency work branch or a forced order; only ordinary background work.
            if (!(curJob.jobGiver is JobGiver_Work workGiver) || workGiver.emergency)
            {
                return;
            }

            if (!ScheduledDrugRetry.CanRetryNow(pawn))
            {
                return;
            }

            // IsHashIntervalTick(interval, delta) can fire more than once inside one 600-tick window
            // when delta is large (e.g. a pawn resuming normal ticking); dedupe so we don't run the
            // full think-tree check twice in a row for the same window.
            if (!ScheduledDrugRetry.TryClaimPeriodicCheck(pawn, Find.TickManager.TicksGame, CheckIntervalTicks))
            {
                return;
            }

            if (DrugPriorityEvaluator.GetPriority(pawn) <= 0f)
            {
                return;
            }

            // minPriority prevents a lower-priority work job from replacing curJob via this extra
            // check; only the scheduled-drug priority (or something even higher) may win here.
            pawn.jobs.CheckForJobOverride(Patch_JobGiver_TakeDrugsForDrugPolicy_GetPriority.WorkHoursPriority);
        }
    }
}
