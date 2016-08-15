using System;
using RimWorld;

namespace RW_FacialStuff
{
    public class Thought_IntoleranceVsAll : Thought_SituationalSocial
    {
        public override float OpinionOffset()
        {
            int num = pawn.story.traits.DegreeOfTrait(TraitDef.Named("Intolerance"));
            if (num <= 0)
            {
                return 0f;
            }
            if (num == 1)
            {
                if (pawn.story.traits.HasTrait(TraitDef.Named("Gay")))
                {
                    if (pawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) != otherPawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")))
                    {
                        return -10f;
                    }

                }
                else
                {
                    if (otherPawn.story.traits.HasTrait(TraitDef.Named("Gay")))
                    {
                        if (otherPawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) == 0)
                        {
                            return -10f;
                        }
                        if (otherPawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) == 1)
                        {
                            return -5f;
                        }
                    }
                }
            }
            if (num == 2)
            {
                if (Math.Abs(pawn.story.skinWhiteness - otherPawn.story.skinWhiteness) > 0.5f)
                {
                    return -15f;
                }
                if (Math.Abs(pawn.story.skinWhiteness - otherPawn.story.skinWhiteness) > 0.35f)
                {
                    return -10f;
                }
                if (Math.Abs(pawn.story.skinWhiteness - otherPawn.story.skinWhiteness) > 0.2f)
                {
                    return -5f;
                }
            }
            if (num == 3)
            {
                float hate = 0f;
                if (pawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) !=
                    otherPawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")))
                    hate -= 10f;

                if (Math.Abs(pawn.story.skinWhiteness - otherPawn.story.skinWhiteness) > 0.5f)
                {
                    hate -= 15f;
                }
                if (Math.Abs(pawn.story.skinWhiteness - otherPawn.story.skinWhiteness) > 0.35f)
                {
                    hate -= 10f;
                }
                if (Math.Abs(pawn.story.skinWhiteness - otherPawn.story.skinWhiteness) > 0.2f)
                {
                    hate -= 5f;
                }

                return hate;
            }
            return 0f;
        }
    }
}
