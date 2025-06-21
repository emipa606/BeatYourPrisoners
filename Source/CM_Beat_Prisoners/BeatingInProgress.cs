using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CM_Beat_Prisoners;

public class BeatingInProgress : IExposable
{
    private const float BaseFightBackChance = 0.05f;
    private const float FightBackChanceMeleeFactor = 0.02f;

    private const float BasePrisonBreakChance = 0.25f;

    private static readonly List<Pair<string, float>> fightBackTraitFactors =
    [
        new("Wimp", 0.5f),
        new("Kind", 0.5f),
        new("Masochist", 0.0f),
        new("Brawler", 2.0f),
        new("Bloodlust", 2.0f)
    ];

    public Pawn Beatee;
    public List<Pawn> Beaters = [];

    public bool FightingBack;

    private float startingPainLevel;

    public bool HasValidBeatee => Beatee is { Spawned: true, IsPrisonerOfColony: true } &&
                                  Beatee.IsPrisonerInPrisonCell();

    public void ExposeData()
    {
        Scribe_References.Look(ref Beatee, "beatee");
        Scribe_Collections.Look(ref Beaters, "beaters", LookMode.Reference);

        Scribe_Values.Look(ref FightingBack, "fightingBack");
        Scribe_Values.Look(ref startingPainLevel, "startingPainLevel");
    }

    public float GetAndResetPainInflicted(float currentPain)
    {
        var painInflicted = Mathf.Max(0.0f, currentPain - startingPainLevel);
        startingPainLevel = currentPain;

        return painInflicted;
    }

    public void AddBeater(Pawn newBeater)
    {
        if (Beaters.Contains(newBeater))
        {
            return;
        }

        Beaters.Add(newBeater);
        Logger.MessageFormat(this, "{0} joined in on {1}'s beating", newBeater, Beatee);
    }

    public void RemoveBeater(Pawn oldBeater)
    {
        if (!Beaters.Contains(oldBeater))
        {
            return;
        }

        Beaters.Remove(oldBeater);
        Logger.MessageFormat(this, "{0} no longer beating {1}", oldBeater, Beatee);
    }

    public bool IsBeating(Pawn beater)
    {
        return Beaters.Any(originalBeater => originalBeater == beater);
    }

    public void TryFightBack()
    {
        if (!HasValidBeatee)
        {
            FightingBack = false;
            return;
        }

        if (FightingBack)
        {
            return;
        }

        var fightBackChance = BaseFightBackChance;

        if (Beatee.story?.traits != null && Beatee.skills != null)
        {
            fightBackChance += FightBackChanceMeleeFactor * Beatee.skills.GetSkill(SkillDefOf.Melee).Level;
            fightBackChance = factorInFightingBackTraits(Beatee, fightBackChance);
        }

        FightingBack = Rand.Chance(fightBackChance);

        Logger.MessageFormat(this, "{0} fighting back: {1}", Beatee, FightingBack);
    }

    public void TryPrisonBreak()
    {
        if (!HasValidBeatee || Beatee.Downed)
        {
            return;
        }

        if (!FightingBack)
        {
            return;
        }

        Logger.MessageFormat(this, "{0} prison break chance: {1}", Beatee, BasePrisonBreakChance);

        if (Rand.Chance(BasePrisonBreakChance))
        {
            PrisonBreakUtility.StartPrisonBreak(Beatee);
        }
    }

    private float factorInFightingBackTraits(Pawn pawn, float initialValue)
    {
        var traits = pawn.story.traits;

        Logger.StartMessage(this, "{0} base fightback chance = {1}", pawn, initialValue);

        foreach (var traitFactor in fightBackTraitFactors)
        {
            if (!traits.allTraits.Any(trait => trait.def.defName == traitFactor.First))
            {
                continue;
            }

            initialValue *= traitFactor.Second;

            Logger.AddToMessage("{0} *= {1}", traitFactor.First, traitFactor.Second);
        }

        Logger.AddToMessage("Final fightback chance = {0}", initialValue);
        Logger.DisplayMessage();

        return initialValue;
    }
}