﻿using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Beat_Prisoners.Patches;

[HarmonyPatch(typeof(Pawn_WorkSettings), "EnableAndInitialize", MethodType.Normal)]
public static class Pawn_WorkSettings_EnableAndInitialize
{
    public static void Postfix(Pawn_WorkSettings __instance, DefMap<WorkTypeDef, int> ___priorities)
    {
        if (___priorities != null)
        {
            __instance.SetPriority(BeatPrisonersDefOf.CM_Beat_Prisoners_WorkType_Break, 0);
        }
    }
}