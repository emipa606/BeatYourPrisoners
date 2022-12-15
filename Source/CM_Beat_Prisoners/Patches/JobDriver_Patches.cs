using HarmonyLib;
using Verse;
using Verse.AI;

namespace CM_Beat_Prisoners.Patches;

[HarmonyPatch(typeof(JobDriver))]
[HarmonyPatch("Cleanup", MethodType.Normal)]
public static class JobDriver_Cleanup
{
    [HarmonyPostfix]
    public static void Postfix(JobDriver __instance, JobCondition condition)
    {
        if (__instance is not JobDriver_Break jobDriverBreak)
        {
            return;
        }

        var initiator = jobDriverBreak.pawn;
        var beatingTracker = Current.Game.World.GetComponent<BeatingTracker>();

        if (initiator.Downed || initiator.Dead)
        {
            // If this became a fight, losing might trigger a prison break
            beatingTracker?.BeaterDowned(jobDriverBreak.Victim, initiator);
        }

        beatingTracker?.StopBeating(jobDriverBreak.Victim, initiator);
    }
}