// Copyright (C) 2026 Sk
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace Drugs_on_Duty.Patches
{
    /// <summary>
    /// Per-pawn retry state for the scheduled-drug-during-work feature. Weak keys let entries
    /// disappear along with the pawn instead of leaking for the rest of the save.
    /// </summary>
    internal sealed class RetryState
    {
        public int LastFailedTick = -1;
        public DrugPolicy LastFailedPolicy;
        public int LastPeriodicCheckTick = -1;
    }

    internal static class ScheduledDrugRetry
    {
        // How long a failed drug search is trusted before we let vanilla search again. This is a
        // performance/responsiveness tradeoff: newly available stock might sit unused for up to this
        // many ticks after a previous attempt found nothing.
        public const int RetryDelayTicks = 600;

        private static readonly ConditionalWeakTable<Pawn, RetryState> States = new ConditionalWeakTable<Pawn, RetryState>();

        private static RetryState GetOrCreate(Pawn pawn)
        {
            return States.GetValue(pawn, _ => new RetryState());
        }

        /// <summary>True if the priority boost / periodic recheck may attempt a fresh search now.</summary>
        public static bool CanRetryNow(Pawn pawn)
        {
            if (!States.TryGetValue(pawn, out RetryState state) || state.LastFailedTick < 0)
            {
                return true;
            }

            // A policy change can immediately allow a different drug, so don't make the pawn wait
            // out the old miss's delay against a schedule that no longer applies.
            if (!ReferenceEquals(state.LastFailedPolicy, pawn.drugs?.CurrentPolicy))
            {
                return true;
            }

            return Find.TickManager.TicksGame - state.LastFailedTick >= RetryDelayTicks;
        }

        public static void RecordMiss(Pawn pawn)
        {
            RetryState state = GetOrCreate(pawn);
            state.LastFailedTick = Find.TickManager.TicksGame;
            state.LastFailedPolicy = pawn.drugs?.CurrentPolicy;
        }

        public static void RecordHit(Pawn pawn)
        {
            if (States.TryGetValue(pawn, out RetryState state))
            {
                state.LastFailedTick = -1;
                state.LastFailedPolicy = null;
            }
        }

        /// <summary>
        /// Guards against Pawn.IsHashIntervalTick(600, delta) firing more than once inside the same
        /// 600-tick window (possible when delta is large, e.g. after the pawn was off-screen). Returns
        /// true only for the first claim within minTicksBetweenChecks.
        /// </summary>
        public static bool TryClaimPeriodicCheck(Pawn pawn, int currentTick, int minTicksBetweenChecks)
        {
            RetryState state = GetOrCreate(pawn);
            if (state.LastPeriodicCheckTick >= 0 && currentTick - state.LastPeriodicCheckTick < minTicksBetweenChecks)
            {
                return false;
            }

            state.LastPeriodicCheckTick = currentTick;
            return true;
        }
    }
}
