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
                    if (pawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) >= 0)
                    {
                        if (otherPawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) < 0)
                        {
                            return -15f;
                        }
                    }
                    else
                    {
                        return 15f;
                    }
                }
                else
                {
                    if (otherPawn.story.traits.HasTrait(TraitDef.Named("Gay")))
                    {
                        if (otherPawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) == 0)
                        {
                            return -15f;
                        }
                        if (otherPawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) == 1)
                        {
                            return -10f;
                        }
                    }
                    else
                    {
                        return 15f;
                    }
                }
            }
            if (num == 2)
            {
                if (Math.Abs(pawn.story.skinWhiteness - otherPawn.story.skinWhiteness) > 0.4f)
                {
                    return -30f;
                }
                if (Math.Abs(pawn.story.skinWhiteness - otherPawn.story.skinWhiteness) > 0.3f)
                {
                    return -20f;
                }
                if (Math.Abs(pawn.story.skinWhiteness - otherPawn.story.skinWhiteness) > 0.2f)
                {
                    return -15f;
                }
                return +15f;
            }
            if (num == 3)
            {
                float hate = 0f;
                if (pawn.story.traits.HasTrait(TraitDef.Named("Gay")))
                {
                    if (pawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) >= 0)
                    {
                        if (otherPawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) < 0)
                        {
                            hate -= 15f;
                        }
                    }
                    else
                    {
                        hate += 15f;
                    }
                }
                else
                {
                    if (otherPawn.story.traits.HasTrait(TraitDef.Named("Gay")))
                    {
                        if (otherPawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) == 0)
                        {
                            hate -= 15f;
                        }
                        if (otherPawn.story.traits.DegreeOfTrait(TraitDef.Named("Gay")) == 1)
                        {
                            hate -= 10f;
                        }
                    }
                    else
                    {
                        hate += 10f;
                    }
                }

                if (Math.Abs(pawn.story.skinWhiteness - otherPawn.story.skinWhiteness) > 0.4f)
                {
                    hate -= 30f;
                }
                if (Math.Abs(pawn.story.skinWhiteness - otherPawn.story.skinWhiteness) > 0.3f)
                {
                    hate -= 20f;
                }
                if (Math.Abs(pawn.story.skinWhiteness - otherPawn.story.skinWhiteness) > 0.2f)
                {
                    hate -= 15f;
                }
                else
                {
                    hate += 25f;
                }

                return hate;
            }
            return 0f;
        }
    }
}
