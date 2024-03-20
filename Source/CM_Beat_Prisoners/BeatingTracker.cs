using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace CM_Beat_Prisoners;

public class BeatingTracker(World world) : WorldComponent(world)
{
    private const int minimumBeatingInterval = 30000;

    private List<BeatingCounter> beatingCounters = [];

    private List<BeatingInProgress> beatingsInProgress = [];

    public override void ExposeData()
    {
        base.ExposeData();

        if (Scribe.mode == LoadSaveMode.Saving)
        {
            var currentTick = Find.TickManager.TicksGame;

            beatingsInProgress = beatingsInProgress.Where(beating => beating.HasValidBeatee).ToList();
            beatingCounters = beatingCounters
                .Where(beating => beating.HasValidBeater && beating.nextBeatingTick > currentTick).ToList();
        }

        Scribe_Collections.Look(ref beatingsInProgress, "beatingsInProgress", LookMode.Deep);
        Scribe_Collections.Look(ref beatingCounters, "beatingCounters", LookMode.Deep);
    }

    public bool CanGiveBeating(Pawn beater)
    {
        var counter = beatingCounters.Find(bc => bc.beater == beater);

        if (counter == null)
        {
            return true;
        }

        var ticksUntilNextBeating = counter.nextBeatingTick - Find.TickManager.TicksGame;

        if (ticksUntilNextBeating > 0)
        {
            Logger.MessageFormat(this, "{0} cannot give beating for another {1} ticks", beater, ticksUntilNextBeating);
        }

        return ticksUntilNextBeating <= 0;
    }

    public BeatingInProgress GetBeatingInProgress(Pawn beatee)
    {
        return beatingsInProgress.Find(bting => bting.beatee == beatee);
    }

    public BeatingInProgress GetOrStartBeatingInProgress(Pawn beatee, Pawn beater = null)
    {
        var beating = GetBeatingInProgress(beatee);

        if (beating == null)
        {
            beating = new BeatingInProgress
            {
                beatee = beatee
            };
            beatingsInProgress.Add(beating);

            Logger.MessageFormat(this, "Started a new beating for {0}", beatee);
        }

        if (beater == null)
        {
            return beating;
        }

        beating.AddBeater(beater);
        var counter = beatingCounters.Find(bc => bc.beater == beater);
        if (counter != null)
        {
            return beating;
        }

        beatingCounters.Add(new BeatingCounter
        {
            beater = beater,
            nextBeatingTick = Find.TickManager.TicksGame + minimumBeatingInterval
        });

        return beating;
    }

    public void BeaterDowned(Pawn beatee, Pawn beater)
    {
        GetBeatingInProgress(beatee)?.TryPrisonBreak();
    }

    public void StopBeating(Pawn beatee, Pawn beater)
    {
        var beating = GetBeatingInProgress(beatee);

        if (beating == null)
        {
            return;
        }

        beating.RemoveBeater(beater);
        if (beating.beaters.Count != 0)
        {
            return;
        }

        beatingsInProgress.Remove(beating);
        Logger.MessageFormat(this, "Ended beating for {0}", beatee);
    }
}