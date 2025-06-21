using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace CM_Beat_Prisoners;

public class BeatingTracker(World world) : WorldComponent(world)
{
    private const int MinimumBeatingInterval = 30000;

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
                .Where(beating => beating.HasValidBeater && beating.NextBeatingTick > currentTick).ToList();
        }

        Scribe_Collections.Look(ref beatingsInProgress, "beatingsInProgress", LookMode.Deep);
        Scribe_Collections.Look(ref beatingCounters, "beatingCounters", LookMode.Deep);
    }

    public bool CanGiveBeating(Pawn beater)
    {
        var counter = beatingCounters.Find(bc => bc.Beater == beater);

        if (counter == null)
        {
            return true;
        }

        var ticksUntilNextBeating = counter.NextBeatingTick - Find.TickManager.TicksGame;

        if (ticksUntilNextBeating > 0)
        {
            Logger.MessageFormat(this, "{0} cannot give beating for another {1} ticks", beater, ticksUntilNextBeating);
        }

        return ticksUntilNextBeating <= 0;
    }

    public BeatingInProgress GetBeatingInProgress(Pawn beatee)
    {
        return beatingsInProgress.Find(bting => bting.Beatee == beatee);
    }

    public BeatingInProgress GetOrStartBeatingInProgress(Pawn beatee, Pawn beater = null)
    {
        var beating = GetBeatingInProgress(beatee);

        if (beating == null)
        {
            beating = new BeatingInProgress
            {
                Beatee = beatee
            };
            beatingsInProgress.Add(beating);

            Logger.MessageFormat(this, "Started a new beating for {0}", beatee);
        }

        if (beater == null)
        {
            return beating;
        }

        beating.AddBeater(beater);
        var counter = beatingCounters.Find(bc => bc.Beater == beater);
        if (counter != null)
        {
            return beating;
        }

        beatingCounters.Add(new BeatingCounter
        {
            Beater = beater,
            NextBeatingTick = Find.TickManager.TicksGame + MinimumBeatingInterval
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
        if (beating.Beaters.Count != 0)
        {
            return;
        }

        beatingsInProgress.Remove(beating);
        Logger.MessageFormat(this, "Ended beating for {0}", beatee);
    }
}