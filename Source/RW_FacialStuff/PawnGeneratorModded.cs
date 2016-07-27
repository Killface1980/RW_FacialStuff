using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{



    public static class PawnGeneratorModded
    {

        private static List<PawnGeneratorModded.PawnGenerationStatus> pawnsBeingGenerated = new List<PawnGeneratorModded.PawnGenerationStatus>();

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct PawnGenerationStatus
        {
            public Pawn Pawn { get; private set; }

            public List<Pawn> PawnsGeneratedInTheMeantime { get; private set; }

            public PawnGenerationStatus(Pawn pawn, List<Pawn> pawnsGeneratedInTheMeantime)
            {
                this.Pawn = pawn;
                this.PawnsGeneratedInTheMeantime = pawnsGeneratedInTheMeantime;
            }
        }

        // Verse.PawnGenerator
        private static void GiveRandomTraits(Pawn pawn, bool allowGay)
        {
            if (pawn.story == null)
            {
                return;
            }
            if (pawn.story.childhood.forcedTraits != null)
            {
                List<TraitEntry> forcedTraits = pawn.story.childhood.forcedTraits;
                for (int i = 0; i < forcedTraits.Count; i++)
                {
                    TraitEntry traitEntry = forcedTraits[i];
                    if (traitEntry.def == null)
                    {
                        Log.Error("Null forced trait def on " + pawn.story.childhood);
                    }
                    else if (!pawn.story.traits.HasTrait(traitEntry.def))
                    {
                        pawn.story.traits.GainTrait(new Trait(traitEntry.def, traitEntry.degree));
                    }
                }
            }
            if (pawn.story.adulthood.forcedTraits != null)
            {
                List<TraitEntry> forcedTraits2 = pawn.story.adulthood.forcedTraits;
                for (int j = 0; j < forcedTraits2.Count; j++)
                {
                    TraitEntry traitEntry2 = forcedTraits2[j];
                    if (traitEntry2.def == null)
                    {
                        Log.Error("Null forced trait def on " + pawn.story.adulthood);
                    }
                    else if (!pawn.story.traits.HasTrait(traitEntry2.def))
                    {
                        pawn.story.traits.GainTrait(new Trait(traitEntry2.def, traitEntry2.degree));
                    }
                }
            }
            int num = Rand.RangeInclusive(2, 3);
            if (allowGay && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn)))
            {
                Trait trait = new Trait(TraitDefOf.Gay, PawnGenerator.RandomTraitDegree(TraitDefOf.Gay));
                pawn.story.traits.GainTrait(trait);
            }
            while (pawn.story.traits.allTraits.Count < num)
            {
                TraitDef newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn));
                if (!pawn.story.traits.HasTrait(newTraitDef))
                {
                    if (newTraitDef == TraitDefOf.Gay)
                    {
                        if (!allowGay)
                        {
                            continue;
                        }
                        if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))
                        {
                            continue;
                        }
                    }
                    if (!pawn.story.traits.allTraits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) && (newTraitDef.conflictingTraits == null || !newTraitDef.conflictingTraits.Any((TraitDef tr) => pawn.story.traits.HasTrait(tr))))
                    {
                        if (newTraitDef.requiredWorkTypes == null || !pawn.story.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes))
                        {
                            if (!pawn.story.WorkTagIsDisabled(newTraitDef.requiredWorkTags))
                            {
                                int degree = PawnGenerator.RandomTraitDegree(newTraitDef);
                                if (!pawn.story.childhood.DisallowsTrait(newTraitDef, degree) && !pawn.story.adulthood.DisallowsTrait(newTraitDef, degree))
                                {
                                    Trait trait2 = new Trait(newTraitDef, degree);
                                    if (pawn.mindState == null || pawn.mindState.mentalBreaker == null || pawn.mindState.mentalBreaker.BreakThresholdExtreme + trait2.OffsetOfStat(StatDefOf.MentalBreakThreshold) <= 40f)
                                    {
                                        pawn.story.traits.GainTrait(trait2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        // Verse.PawnGenerator
        private static SimpleCurve DefaultAgeGenerationCurve = new SimpleCurve
{
    new CurvePoint(0.05f, 0f),
    new CurvePoint(0.1f, 100f),
    new CurvePoint(0.675f, 100f),
    new CurvePoint(0.75f, 30f),
    new CurvePoint(0.875f, 18f),
    new CurvePoint(1f, 10f),
    new CurvePoint(1.125f, 3f),
    new CurvePoint(1.25f, 0f)
};

        // Verse.PawnGenerator
        private static void GenerateRandomAge(Pawn pawn, PawnGenerationRequest request)
        {
            if (request.FixedBiologicalAge.HasValue && request.FixedChronologicalAge.HasValue)
            {
                float? fixedBiologicalAge = request.FixedBiologicalAge;
                bool arg_67_0;
                if (fixedBiologicalAge.HasValue)
                {
                    float? fixedChronologicalAge = request.FixedChronologicalAge;
                    if (fixedChronologicalAge.HasValue)
                    {
                        arg_67_0 = (fixedBiologicalAge.Value > fixedChronologicalAge.Value);
                        goto IL_67;
                    }
                }
                arg_67_0 = false;
                IL_67:
                if (arg_67_0)
                {
                    Log.Warning(string.Concat(new object[]
                    {
                "Tried to generate age for pawn ",
                pawn,
                ", but pawn generation request demands biological age (",
                request.FixedBiologicalAge,
                ") to be greater than chronological age (",
                request.FixedChronologicalAge,
                ")."
                    }));
                }
            }
            if (request.Newborn)
            {
                pawn.ageTracker.AgeBiologicalTicks = 0L;
            }
            else if (request.FixedBiologicalAge.HasValue)
            {
                pawn.ageTracker.AgeBiologicalTicks = (long)(request.FixedBiologicalAge.Value * 3600000f);
            }
            else
            {
                int num = 0;
                float num2;
                while (true)
                {
                    if (pawn.RaceProps.ageGenerationCurve != null)
                    {
                        num2 = (float)Mathf.RoundToInt(Rand.ByCurve(pawn.RaceProps.ageGenerationCurve, 200));
                    }
                    else if (pawn.RaceProps.IsMechanoid)
                    {
                        num2 = (float)Rand.Range(0, 2500);
                    }
                    else
                    {
                        num2 = Rand.ByCurve(PawnGeneratorModded.DefaultAgeGenerationCurve, 200) * pawn.RaceProps.lifeExpectancy;
                    }
                    num++;
                    if (num > 300)
                    {
                        break;
                    }
                    if (num2 <= (float)pawn.kindDef.maxGenerationAge && num2 >= (float)pawn.kindDef.minGenerationAge)
                    {
                        goto IL_1D7;
                    }
                }
                Log.Error("Tried 300 times to generate age for " + pawn);
                IL_1D7:
                pawn.ageTracker.AgeBiologicalTicks = (long)(num2 * 3600000f) + (long)Rand.Range(0, 3600000);
            }
            if (request.Newborn)
            {
                pawn.ageTracker.AgeChronologicalTicks = 0L;
            }
            else if (request.FixedChronologicalAge.HasValue)
            {
                pawn.ageTracker.AgeChronologicalTicks = (long)(request.FixedChronologicalAge.Value * 3600000f);
            }
            else
            {
                int num3;
                if (Rand.Value < pawn.kindDef.backstoryCryptosleepCommonality)
                {
                    float value = Rand.Value;
                    if (value < 0.7f)
                    {
                        num3 = Rand.Range(0, 100);
                    }
                    else if (value < 0.95f)
                    {
                        num3 = Rand.Range(100, 1000);
                    }
                    else
                    {
                        int max = GenDate.CurrentYear - 2026 - pawn.ageTracker.AgeBiologicalYears;
                        num3 = Rand.Range(1000, max);
                    }
                }
                else
                {
                    num3 = 0;
                }
                int ticksAbs = GenTicks.TicksAbs;
                long num4 = (long)ticksAbs - pawn.ageTracker.AgeBiologicalTicks;
                num4 -= (long)num3 * 3600000L;
                pawn.ageTracker.BirthAbsTicks = num4;
            }
            if (pawn.ageTracker.AgeBiologicalTicks > pawn.ageTracker.AgeChronologicalTicks)
            {
                pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
            }
        }

        // Verse.PawnGenerator

        // Verse.PawnGenerator
        private static void DiscardGeneratedPawn(Pawn pawn)
        {
            if (Find.WorldPawns.Contains(pawn))
            {
                Find.WorldPawns.RemovePawn(pawn);
            }
            Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
            List<Pawn> pawnsGeneratedInTheMeantime = PawnGeneratorModded.pawnsBeingGenerated.Last<PawnGeneratorModded.PawnGenerationStatus>().PawnsGeneratedInTheMeantime;
            if (pawnsGeneratedInTheMeantime != null)
            {
                for (int i = 0; i < pawnsGeneratedInTheMeantime.Count; i++)
                {
                    Pawn pawn2 = pawnsGeneratedInTheMeantime[i];
                    if (Find.WorldPawns.Contains(pawn2))
                    {
                        Find.WorldPawns.RemovePawn(pawn2);
                    }
                    Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.Discard);
                    for (int j = 0; j < PawnGeneratorModded.pawnsBeingGenerated.Count; j++)
                    {
                        PawnGeneratorModded.pawnsBeingGenerated[j].PawnsGeneratedInTheMeantime.Remove(pawn2);
                    }
                }
            }
        }
        // Verse.PawnGenerator
        private static void GeneratePawnRelations(Pawn pawn, ref PawnGenerationRequest request)
        {
            if (!pawn.RaceProps.Humanlike)
            {
                return;
            }
            List<KeyValuePair<Pawn, PawnRelationDef>> list = new List<KeyValuePair<Pawn, PawnRelationDef>>();
            List<PawnRelationDef> allDefsListForReading = DefDatabase<PawnRelationDef>.AllDefsListForReading;
            IEnumerable<Pawn> enumerable = from x in PawnUtility.AllPawnsMapOrWorldAliveOrDead
                                           where x.def == pawn.def
                                           select x;
            foreach (Pawn current in enumerable)
            {
                if (current.ThingState == ThingState.Discarded)
                {
                    Log.Warning(string.Concat(new object[]
                    {
                "Warning during generating pawn relations for ",
                pawn,
                ": Pawn ",
                current,
                " is discarded, yet he was yielded by PawnUtility. Discarding a pawn means that he is no longer managed by anything."
                    }));
                }
                else
                {
                    for (int i = 0; i < allDefsListForReading.Count; i++)
                    {
                        if (allDefsListForReading[i].generationChanceFactor > 0f)
                        {
                            list.Add(new KeyValuePair<Pawn, PawnRelationDef>(current, allDefsListForReading[i]));
                        }
                    }
                }
            }
            PawnGenerationRequest localReq = request;
            KeyValuePair<Pawn, PawnRelationDef> keyValuePair = list.RandomElementByWeightWithDefault(delegate (KeyValuePair<Pawn, PawnRelationDef> x)
            {
                if (!x.Value.familyByBloodRelation)
                {
                    return 0f;
                }
                return x.Value.generationChanceFactor * x.Value.Worker.GenerationChance(pawn, x.Key, localReq);
            }, 82f);
            if (keyValuePair.Key != null)
            {
                keyValuePair.Value.Worker.CreateRelation(pawn, keyValuePair.Key, ref request);
            }
            KeyValuePair<Pawn, PawnRelationDef> keyValuePair2 = list.RandomElementByWeightWithDefault(delegate (KeyValuePair<Pawn, PawnRelationDef> x)
            {
                if (x.Value.familyByBloodRelation)
                {
                    return 0f;
                }
                return x.Value.generationChanceFactor * x.Value.Worker.GenerationChance(pawn, x.Key, localReq);
            }, 82f);
            if (keyValuePair2.Key != null)
            {
                keyValuePair2.Value.Worker.CreateRelation(pawn, keyValuePair2.Key, ref request);
            }
        }

        // Verse.PawnGenerator
        private static Pawn DoGenerateNewNakedPawn(ref PawnGenerationRequest request, out string error, bool ignoreScenarioRequirements)
        {
            error = null;
            Pawn pawn = (Pawn)ThingMaker.MakeThing(request.KindDef.race, null);
            pawnsBeingGenerated.Add(new PawnGenerationStatus(pawn, null));
            Pawn result;
            try
            {
                pawn.kindDef = request.KindDef;
                pawn.SetFactionDirect(request.Faction);
                PawnComponentsUtility.CreateInitialComponents(pawn);
                if (request.FixedGender.HasValue)
                {
                    pawn.gender = request.FixedGender.Value;
                }
                else if (pawn.RaceProps.hasGenders)
                {
                    if (Rand.Value < 0.5f)
                    {
                        pawn.gender = Gender.Male;
                    }
                    else
                    {
                        pawn.gender = Gender.Female;
                    }
                }
                else
                {
                    pawn.gender = Gender.None;
                }
                PawnGeneratorModded.GenerateRandomAge(pawn, request);
                pawn.needs.SetInitialLevels();
                typeof(PawnGenerator)..GetMethod("GenerateInitialHediffs", BindingFlags.Static | BindingFlags.NonPublic).;
                PawnGeneratorModded.GenerateInitialHediffs(pawn, request);
                if (!request.Newborn && request.CanGeneratePawnRelations)
                {
                    PawnGeneratorModded.GeneratePawnRelations(pawn, ref request);
                }
                if (pawn.RaceProps.Humanlike)
                {
                    pawn.story.skinWhiteness = ((!request.FixedSkinWhiteness.HasValue) ? PawnSkinColors.RandomSkinWhiteness() : request.FixedSkinWhiteness.Value);
                    pawn.story.crownType = ((Rand.Value >= 0.5f) ? CrownType.Narrow : CrownType.Average);
                    pawn.story.hairColor = PawnHairColors.RandomHairColor(pawn.story.SkinColor, pawn.ageTracker.AgeBiologicalYears);
                    PawnBioGenerator.GiveAppropriateBioTo(pawn, request.FixedLastName);
                    PawnGeneratorModded.GiveRandomTraits(pawn, request.AllowGay);
                    pawn.story.hairDef = PawnHairChooser.RandomHairDefFor(pawn, request.Faction.def);
                    pawn.story.GenerateSkillsFromBackstory();
                }
                if (pawn.workSettings != null && request.Faction.IsPlayer)
                {
                    pawn.workSettings.EnableAndInitialize();
                }
                if (request.Faction != null && pawn.RaceProps.Animal)
                {
                    pawn.GenerateNecessaryName();
                }
                if (!request.AllowDead && (pawn.Dead || pawn.Destroyed))
                {
                    PawnGeneratorModded.DiscardGeneratedPawn(pawn);
                    error = "Generated dead pawn.";
                    result = null;
                }
                else if (!request.AllowDowned && pawn.Downed)
                {
                    PawnGeneratorModded.DiscardGeneratedPawn(pawn);
                    error = "Generated downed pawn.";
                    result = null;
                }
                else if (request.MustBeCapableOfViolence && pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    PawnGeneratorModded.DiscardGeneratedPawn(pawn);
                    error = "Generated pawn incapable of violence.";
                    result = null;
                }
                else if (!ignoreScenarioRequirements && request.Context == PawnGenerationContext.PlayerStarter && !Find.Scenario.AllowPlayerStartingPawn(pawn))
                {
                    PawnGeneratorModded.DiscardGeneratedPawn(pawn);
                    error = "Generated pawn doesn't meet scenario requirements.";
                    result = null;
                }
                else if (request.Validator != null && !request.Validator(pawn))
                {
                    PawnGeneratorModded.DiscardGeneratedPawn(pawn);
                    error = "Generated pawn didn't pass validator check.";
                    result = null;
                }
                else
                {
                    for (int i = 0; i < PawnGeneratorModded.pawnsBeingGenerated.Count - 1; i++)
                    {
                        if (PawnGeneratorModded.pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime == null)
                        {
                            PawnGeneratorModded.pawnsBeingGenerated[i] = new PawnGeneratorModded.PawnGenerationStatus(PawnGeneratorModded.pawnsBeingGenerated[i].Pawn, new List<Pawn>());
                        }
                        PawnGeneratorModded.pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime.Add(pawn);
                    }
                    result = pawn;
                }
            }
            finally
            {
                PawnGeneratorModded.pawnsBeingGenerated.RemoveLast<PawnGeneratorModded.PawnGenerationStatus>();
            }
            return result;
        }

    }
    }

