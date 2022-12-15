using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CM_Beat_Prisoners;

public class BeatingInProgress : IExposable
{
    private const float baseFightBackChance = 0.05f;
    private const float fightBackChanceMeleeFactor = 0.02f;

    private const float basePrisonBreakChance = 0.25f;

    private static readonly List<Pair<string, float>> fightBackTraitFactors = new List<Pair<string, float>>
    {
        new Pair<string, float>("Wimp", 0.5f),
        new Pair<string, float>("Kind", 0.5f),
        new Pair<string, float>("Masochist", 0.0f),
        new Pair<string, float>("Brawler", 2.0f),
        new Pair<string, float>("Bloodlust", 2.0f)
    };

    public Pawn beatee;
    public List<Pawn> beaters = new List<Pawn>();

    public bool fightingBack;

    public float startingPainLevel;

    public bool HasValidBeatee => beatee is { Spawned: true, IsPrisonerOfColony: true } &&
                                  beatee.IsPrisonerInPrisonCell();

    public void ExposeData()
    {
        Scribe_References.Look(ref beatee, "beatee");
        Scribe_Collections.Look(ref beaters, "beaters", LookMode.Reference);

        Scribe_Values.Look(ref fightingBack, "fightingBack");
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
        if (beaters.Contains(newBeater))
        {
            return;
        }

        beaters.Add(newBeater);
        Logger.MessageFormat(this, "{0} joined in on {1}'s beating", newBeater, beatee);
    }

    public void RemoveBeater(Pawn oldBeater)
    {
        if (!beaters.Contains(oldBeater))
        {
            return;
        }

        beaters.Remove(oldBeater);
        Logger.MessageFormat(this, "{0} no longer beating {1}", oldBeater, beatee);
    }

    public bool IsBeating(Pawn beater)
    {
        return beaters.Any(bter => bter == beater);
    }

    public void TryFightBack()
    {
        if (!HasValidBeatee)
        {
            fightingBack = false;
            return;
        }

        if (fightingBack)
        {
            return;
        }

        var fightBackChance = baseFightBackChance;

        if (beatee.story?.traits != null && beatee.skills != null)
        {
            fightBackChance += fightBackChanceMeleeFactor * beatee.skills.GetSkill(SkillDefOf.Melee).Level;
            fightBackChance = FactorInFightingBackTraits(beatee, fightBackChance);
        }

        fightingBack = Rand.Chance(fightBackChance);

        Logger.MessageFormat(this, "{0} fighting back: {1}", beatee, fightingBack);
    }

    public void TryPrisonBreak()
    {
        if (!HasValidBeatee || beatee.Downed)
        {
            return;
        }

        if (!fightingBack)
        {
            return;
        }

        Logger.MessageFormat(this, "{0} prison break chance: {1}", beatee, basePrisonBreakChance);

        if (Rand.Chance(basePrisonBreakChance))
        {
            PrisonBreakUtility.StartPrisonBreak(beatee);
        }
    }

    private float FactorInFightingBackTraits(Pawn pawn, float initialValue)
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