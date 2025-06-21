using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CM_Beat_Prisoners;

public class InteractionWorker_BreakAttempt : InteractionWorker
{
    private static readonly List<Pair<string, float>> initiatorTraitFactors =
    [
        new("Wimp", 0.5f),
        new("Kind", 0.5f),
        new("Brawler", 1.5f),
        new("Bloodlust", 1.5f),
        new("Psychopath", 1.5f)
    ];

    private static readonly List<Pair<string, float>> recipientTraitFactors =
    [
        new("Wimp", 3.0f),
        new("Masochist", 2.0f),
        new("Tough", 0.5f),
        new("Brawler", 0.5f),
        new("Bloodlust", 0.5f),
        new("Psychopath", 0.5f)
    ];

    private static readonly SimpleCurve ResistanceImpactFactorCurve_Pain =
    [
        new CurvePoint(0f, 0.1f),
        new CurvePoint(0.5f, 2f),
        new CurvePoint(1f, 4f)
    ];

    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks,
        out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
    {
        letterText = null;
        letterLabel = null;
        letterDef = null;
        lookTargets = null;

        var beating = Current.Game.World.GetComponent<BeatingTracker>()?.GetBeatingInProgress(recipient);
        if (beating == null)
        {
            return;
        }

        var currentPainLevel = recipient.health.hediffSet.PainTotal;
        var painInflicted = beating.GetAndResetPainInflicted(currentPainLevel);
        var painFactor = ResistanceImpactFactorCurve_Pain.Evaluate(painInflicted);


        var resistanceReduction = 1f;

        Logger.StartMessage(this, "{0} beat {1}, base resistance reduction  = {2}", initiator, recipient,
            resistanceReduction);
        Logger.AddToMessage("    pain inflicted: {0}, pain factor: {1}", painInflicted, painFactor);

        resistanceReduction *= painFactor;

        resistanceReduction = factorInInitiatorTraits(initiator, resistanceReduction);
        resistanceReduction = factorInRecipientTraits(recipient, resistanceReduction);

        Logger.AddToMessage("Final resistance reduction: {0}", resistanceReduction);
        Logger.DisplayMessage();

        resistanceReduction = Mathf.Min(resistanceReduction, recipient.guest.resistance);

        var resistance = recipient.guest.resistance;
        recipient.guest.resistance = Mathf.Max(0f, recipient.guest.resistance - resistanceReduction);

        string text =
            "TextMote_ResistanceReduced".Translate(resistance.ToString("F1"),
                recipient.guest.resistance.ToString("F1"));
        MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, text, 8f);

        if (recipient.guest.resistance != 0f)
        {
            return;
        }

        var taggedString = "MessagePrisonerResistanceBroken".Translate(recipient.LabelShort, initiator.LabelShort,
            initiator.Named("WARDEN"), recipient.Named("PRISONER"));
        // I'm not sure if the statement below is needed because the beater doesn't directly initiate recruitment/enslavement after the beating, but maybe it's better to leave it as it is. - Virstag
        if (recipient.guest.ExclusiveInteractionMode == PrisonerInteractionModeDefOf.AttemptRecruit)
        {
            taggedString += " " + "MessagePrisonerResistanceBroken_RecruitAttempsWillBegin".Translate();
        }

        Messages.Message(taggedString, recipient, MessageTypeDefOf.PositiveEvent);
    }

    private static float factorInInitiatorTraits(Pawn initiator, float initialValue)
    {
        // Factor initiator traits
        var initiatorTraits = initiator.story.traits;

        foreach (var traitFactor in initiatorTraitFactors)
        {
            if (!initiatorTraits.allTraits.Any(trait => trait.def.defName == traitFactor.First))
            {
                continue;
            }

            initialValue *= traitFactor.Second;

            Logger.AddToMessage("    initiator - {0}: {1}", traitFactor.First, traitFactor.Second);
        }

        return initialValue;
    }

    private static float factorInRecipientTraits(Pawn recipient, float initialValue)
    {
        // Factor recipient traits
        var recipientTraits = recipient.story.traits;

        foreach (var traitFactor in recipientTraitFactors)
        {
            if (!recipientTraits.allTraits.Any(trait => trait.def.defName == traitFactor.First))
            {
                continue;
            }

            initialValue *= traitFactor.Second;

            Logger.AddToMessage("    recipient - {0}: {1}", traitFactor.First, traitFactor.Second);
        }

        return initialValue;
    }
}