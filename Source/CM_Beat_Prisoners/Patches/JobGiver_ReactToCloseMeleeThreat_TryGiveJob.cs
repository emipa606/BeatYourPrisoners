using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Beat_Prisoners.Patches;

[HarmonyPatch(typeof(JobGiver_ReactToCloseMeleeThreat), "TryGiveJob", MethodType.Normal)]
public static class JobGiver_ReactToCloseMeleeThreat_TryGiveJob
{
    public static void Postfix(Pawn pawn, ref Job __result)
    {
        if (!pawn.IsPrisoner || pawn.mindState.meleeThreat == null || __result == null)
        {
            return;
        }

        var beating = Current.Game.World.GetComponent<BeatingTracker>()?.GetBeatingInProgress(pawn);

        if (beating == null)
        {
            return;
        }

        // If the perceived threat is not someone already beating us, then act as if they were
        if (!beating.IsBeating(pawn.mindState.meleeThreat))
        {
            beating.AddBeater(pawn.mindState.meleeThreat);
            beating.TryFightBack();
        }

        // If we aren't fighting back, cower
        if (!beating.FightingBack)
        {
            __result = JobMaker.MakeJob(BeatPrisonersDefOf.CM_Beat_Prisoners_Job_Cower,
                pawn.mindState.meleeThreat);
        }
    }
}