using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CM_Beat_Prisoners;

public class WorkGiver_Warden_Break : WorkGiver_Warden
{
    public override Job JobOnThing(Pawn pawn, Thing target, bool forced = false)
    {
        if (!ShouldTakeCareOfPrisoner(pawn, target, forced))
        {
            return null;
        }

        var pawn2 = (Pawn)target;

        var interactionMode = pawn2.guest.interactionMode;
        var beatingTracker = Current.Game.World.GetComponent<BeatingTracker>();

        var canGiveBeating = (beatingTracker?.CanGiveBeating(pawn) ?? true) &&
                             pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
        var initiatorHealty = Mathf.Abs(1.0f - (pawn.health?.summaryHealth?.SummaryHealthPercent ?? 1.0f)) <= 0.01f;
        var targetHealthy = Mathf.Abs(1.0f - (pawn2.health?.summaryHealth?.SummaryHealthPercent ?? 1.0f)) <= 0.01f;
        var prisonerCanReceiveBeating =
            interactionMode == PrisonerInteractionModeDefOf.ReduceResistance && !pawn2.Downed;

        if (!canGiveBeating || !prisonerCanReceiveBeating || !initiatorHealty ||
            !targetHealthy && beatingTracker?.GetBeatingInProgress(pawn2) == null)
        {
            return null;
        }

        var breakJob = JobMaker.MakeJob(BeatPrisonersDefOf.CM_Beat_Prisoners_Job_Break_Resistance, target);
        breakJob.maxNumMeleeAttacks = Rand.RangeInclusive(3, 9);
        return breakJob;
    }
}