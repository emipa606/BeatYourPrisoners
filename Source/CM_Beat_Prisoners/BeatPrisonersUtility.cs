using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CM_Beat_Prisoners;

public static class BeatPrisonersUtility
{
    public static void GiveThoughtsForPrisonerBeaten(Pawn victim, Pawn perpetrator)
    {
        if (victim == null || perpetrator == null)
        {
            return;
        }

        var otherBeatenThoughts = new List<ThoughtDef>
        {
            BeatPrisonersDefOf.CM_Beat_Prisoners_Thought_Prisoner_Beaten,
            BeatPrisonersDefOf.CM_Beat_Prisoners_Thought_Prisoner_Beaten_Mild,
            BeatPrisonersDefOf.CM_Beat_Prisoners_Thought_Prisoner_Beaten_Spicy
        };


        var selfBeatenThoughts = new List<ThoughtDef>
        {
            BeatPrisonersDefOf.CM_Beat_Prisoners_Thought_Self_Beaten,
            BeatPrisonersDefOf.CM_Beat_Prisoners_Thought_Self_Beaten_Mild,
            BeatPrisonersDefOf.CM_Beat_Prisoners_Thought_Self_Beaten_Kinky
        };

        var gaveBeatingThoughts = new List<ThoughtDef>
        {
            BeatPrisonersDefOf.CM_Beat_Prisoners_Thought_Give_Beating_Mild,
            BeatPrisonersDefOf.CM_Beat_Prisoners_Thought_Give_Beating_Spicy
        };

        TryGiveThoughts(victim, selfBeatenThoughts);
        TryGiveThoughts(perpetrator, gaveBeatingThoughts);

        foreach (var humanlikePawnOnMap in perpetrator.MapHeld.mapPawns.AllPawns.Where(pawn =>
                     pawn != victim && pawn != perpetrator && !pawn.NonHumanlikeOrWildMan() && pawn.needs.mood != null))
        {
            TryGiveThoughts(humanlikePawnOnMap, otherBeatenThoughts);
        }
    }

    private static void TryGiveThoughts(Pawn pawn, List<ThoughtDef> thoughts)
    {
        foreach (var thought in thoughts)
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
        }
    }
}