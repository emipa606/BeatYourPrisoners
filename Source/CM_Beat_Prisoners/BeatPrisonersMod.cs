using System.Reflection;
using HarmonyLib;
using Verse;

namespace CM_Beat_Prisoners;

public class BeatPrisonersMod : Mod
{
    public BeatPrisonersMod(ModContentPack content) : base(content)
    {
        new Harmony("CM_Beat_Prisoners").PatchAll(Assembly.GetExecutingAssembly());
    }
}