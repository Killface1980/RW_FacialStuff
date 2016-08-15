using RimWorld;
using UnityEngine;
using Verse;

namespace RW_FacialStuff.Sexuality
{
    public class Pawn_RelationsTrackerModded
    {
        private Pawn pawn;

        // RimWorld.Pawn_RelationsTracker
        public float AttractionToModded(Pawn otherPawn)
        {

            if (pawn.def != otherPawn.def || pawn == otherPawn)
            {
                return 0f;
            }
            float num = 1f;
            float num2 = 1f;
            float pawnAgeBiological = pawn.ageTracker.AgeBiologicalYearsFloat;
            float otherPawnAgeBiological = otherPawn.ageTracker.AgeBiologicalYearsFloat;
            if (pawn.gender == Gender.Male)
            {
                // if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
                if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
                {
                    if (pawn.RaceProps.Humanlike && pawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) == 0)
                    {
                        if (otherPawn.gender == Gender.Female)
                        {
                            return 0f;
                        }
                    }
                }
                else
                {
                    if (otherPawn.gender == Gender.Male)
                    {
                        return 0.05f;
                    }
                }
                num2 = GenMath.FlatHill(16f, 20f, pawnAgeBiological, pawnAgeBiological + 15f, otherPawnAgeBiological);
            }
            else if (pawn.gender == Gender.Female)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
                {
                    if (pawn.RaceProps.Humanlike && pawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) == 0)
                    {
                        if (otherPawn.gender == Gender.Male)
                        {
                            return 0f;
                        }
                    }
                }
                else 
                {
                    if (otherPawn.gender == Gender.Female)
                    {
                        num = 0.15f;
                    }
                }

                if (otherPawnAgeBiological < pawnAgeBiological - 10f)
                {
                    return 0f;
                }
                if (otherPawnAgeBiological < pawnAgeBiological - 3f)
                {
                    num2 = Mathf.InverseLerp(pawnAgeBiological - 10f, pawnAgeBiological - 3f, otherPawnAgeBiological) * 0.2f;
                }
                else
                {
                    num2 = GenMath.FlatHill(0.2f, pawnAgeBiological - 3f, pawnAgeBiological, pawnAgeBiological + 10f, pawnAgeBiological + 40f, 0.1f, otherPawnAgeBiological);
                }
            }
            float num3 = 1f;
            num3 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Talking));
            num3 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Manipulation));
            num3 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Moving));
            float num4 = 1f;
            foreach (PawnRelationDef current in pawn.GetRelations(otherPawn))
            {
                num4 *= current.attractionFactor;
            }
            int degreeOfTraitBeauty = 0;
            if (otherPawn.RaceProps.Humanlike)
            {
                degreeOfTraitBeauty = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
            }
            float num6 = 1f;
            if (degreeOfTraitBeauty < 0)
            {
                num6 = 0.3f;
            }
            else if (degreeOfTraitBeauty > 0)
            {
                num6 = 2.3f;
            }
            float num7 = Mathf.InverseLerp(15f, 18f, pawnAgeBiological);
            float num8 = Mathf.InverseLerp(15f, 18f, otherPawnAgeBiological);
            return num * num2 * num3 * num4 * num7 * num8 * num6;
        }

    }
}
