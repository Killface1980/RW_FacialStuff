using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_FacialStuff.Detouring
{
    public static class LovePartnerRelationUtility
    {
        [Detour(typeof(RimWorld.LovePartnerRelationUtility), bindingFlags = (BindingFlags.Static | BindingFlags.Public))]  // RimWorld.LovePartnerRelationUtility
        public static float LovePartnerRelationGenerationChance(Pawn pawn, Pawn otherPawn, PawnGenerationRequest request, bool ex)
        {
            if (pawn.ageTracker.AgeBiologicalYearsFloat < 14f)
            {
                return 0f;
            }
            if (otherPawn.ageTracker.AgeBiologicalYearsFloat < 14f)
            {
                return 0f;
            }
            if ((pawn.gender == otherPawn.gender && otherPawn.story.traits.HasTrait(TraitDefOf.Gay) && otherPawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) == 0) || !request.AllowGay)
            {
                return 0f;
            }

            if (pawn.gender != otherPawn.gender && otherPawn.story.traits.HasTrait(TraitDefOf.Gay) && otherPawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) == 0)
            {
                return 0f;
            }
            float num = 1f;
            if (ex)
            {
                int num2 = 0;
                List<DirectPawnRelation> directRelations = otherPawn.relations.DirectRelations;
                for (int i = 0; i < directRelations.Count; i++)
                {
                    if (LovePartnerRelationUtility.IsExLovePartnerRelation(directRelations[i].def))
                    {
                        num2++;
                    }
                }
                num = Mathf.Pow(0.2f, num2);
            }
            else if (HasAnyLovePartner(otherPawn))
            {
                return 0f;
            }
            float num3 = (pawn.gender != otherPawn.gender) ? 1f : 0.01f;
            float generationChanceAgeFactor = GetGenerationChanceAgeFactor(pawn);
            float generationChanceAgeFactor2 = GetGenerationChanceAgeFactor(otherPawn);
            float generationChanceAgeGapFactor = GetGenerationChanceAgeGapFactor(pawn, otherPawn, ex);
            float num4 = 1f;
            if (pawn.GetRelations(otherPawn).Any(x => x.familyByBloodRelation))
            {
                num4 = 0.01f;
            }
            float num5;
            if (request.FixedSkinWhiteness.HasValue)
            {
                num5 = ChildRelationUtility.GetSkinSimilarityFactor(request.FixedSkinWhiteness.Value, otherPawn.story.skinWhiteness);
            }
            else
            {
                num5 = PawnSkinColors.GetWhitenessCommonalityFactor(otherPawn.story.skinWhiteness);
            }
            return num * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * num3 * num5 * num4;
        }

        private static float GetGenerationChanceAgeFactor(Pawn p)
        {
            float value = GenMath.LerpDouble(14f, 27f, 0f, 1f, p.ageTracker.AgeBiologicalYearsFloat);
            return Mathf.Clamp(value, 0f, 1f);
        }

        public static bool IsExLovePartnerRelation(PawnRelationDef relation)
        {
            return relation == PawnRelationDefOf.ExLover || relation == PawnRelationDefOf.ExSpouse;
        }

        public static bool HasAnyLovePartner(Pawn pawn)
        {
            return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null) != null || pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null) != null || pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null) != null;
        }

        private static float GetGenerationChanceAgeGapFactor(Pawn p1, Pawn p2, bool ex)
        {
            float num = Mathf.Abs(p1.ageTracker.AgeBiologicalYearsFloat - p2.ageTracker.AgeBiologicalYearsFloat);
            if (ex)
            {
                float num2 = MinPossibleAgeGapAtMinAgeToGenerateAsLovers(p1, p2);
                if (num2 >= 0f)
                {
                    num = Mathf.Min(num, num2);
                }
                float num3 = MinPossibleAgeGapAtMinAgeToGenerateAsLovers(p2, p1);
                if (num3 >= 0f)
                {
                    num = Mathf.Min(num, num3);
                }
            }
            if (num > 40f)
            {
                return 0f;
            }
            float value = GenMath.LerpDouble(0f, 20f, 1f, 0.001f, num);
            return Mathf.Clamp(value, 0.001f, 1f);
        }

        private static float MinPossibleAgeGapAtMinAgeToGenerateAsLovers(Pawn p1, Pawn p2)
        {
            float num = p1.ageTracker.AgeChronologicalYearsFloat - 14f;
            if (num < 0f)
            {
                Log.Warning("at < 0");
                return 0f;
            }
            float num2 = PawnRelationUtility.MaxPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat, p2.ageTracker.AgeChronologicalYearsFloat, num);
            float num3 = PawnRelationUtility.MinPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat, num);
            if (num2 < 0f)
            {
                return -1f;
            }
            if (num2 < 14f)
            {
                return -1f;
            }
            if (num3 <= 14f)
            {
                return 0f;
            }
            return num3 - 14f;
        }


    }
}
