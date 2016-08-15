using System;
using RimWorld;
using Verse;

namespace RW_FacialStuff
{
    public class ThoughtWorker_Ugly : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                return false;
            }
            int otherDegreeOfBeauty = other.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
            int pawnDegreeOfBeauty = pawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
            if (otherDegreeOfBeauty == -1)
            {
                if (pawnDegreeOfBeauty == -1)
                {
                    return ThoughtState.ActiveAtStage(2);
                }
                if (pawnDegreeOfBeauty == -2)
                {
                    return ThoughtState.ActiveAtStage(4);
                }
                return ThoughtState.ActiveAtStage(0);
            }
            if (otherDegreeOfBeauty == -2)
            {
                if (pawnDegreeOfBeauty == -1)
                {
                    return ThoughtState.ActiveAtStage(3);
                }
                if (pawnDegreeOfBeauty == -2)
                {
                    return ThoughtState.ActiveAtStage(5);
                }
                return ThoughtState.ActiveAtStage(1);
            }
            return false;
        }
    }
}
