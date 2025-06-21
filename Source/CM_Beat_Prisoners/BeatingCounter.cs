using Verse;

namespace CM_Beat_Prisoners;

public class BeatingCounter : IExposable
{
    public Pawn Beater;
    public int NextBeatingTick;

    public bool HasValidBeater => Beater is { Dead: false };

    public void ExposeData()
    {
        Scribe_References.Look(ref Beater, "beater");
        Scribe_Values.Look(ref NextBeatingTick, "nextBeatingTick");
    }
}