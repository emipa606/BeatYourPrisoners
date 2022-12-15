using Verse;

namespace CM_Beat_Prisoners;

public class BeatingCounter : IExposable
{
    public Pawn beater;
    public int nextBeatingTick;

    public bool HasValidBeater => beater is { Dead: false };

    public void ExposeData()
    {
        Scribe_References.Look(ref beater, "beater");
        Scribe_Values.Look(ref nextBeatingTick, "nextBeatingTick");
    }
}