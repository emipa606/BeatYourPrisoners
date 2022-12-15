using HarmonyLib;
using Verse;

namespace CM_Beat_Prisoners;

public class BeatPrisonersMod : Mod
{
    public BeatPrisonersMod(ModContentPack content) : base(content)
    {
        var harmony = new Harmony("CM_Beat_Prisoners");
        harmony.PatchAll();

        Instance = this;
    }

    public static BeatPrisonersMod Instance { get; private set; }
}