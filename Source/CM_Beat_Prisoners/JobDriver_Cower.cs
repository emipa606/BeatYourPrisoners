using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CM_Beat_Prisoners;

public class JobDriver_Cower : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var toil = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            defaultDuration = 1200,
            initAction = delegate { pawn.pather.StopDead(); },
            tickAction = delegate
            {
                if (!pawn.IsHashIntervalTick(35))
                {
                    return;
                }

                var beating = Current.Game.World.GetComponent<BeatingTracker>()?.GetBeatingInProgress(pawn);
                if (beating is { FightingBack: true })
                {
                    EndJobWith(JobCondition.InterruptForced);
                }
            }
        };
        yield return toil;
    }
}