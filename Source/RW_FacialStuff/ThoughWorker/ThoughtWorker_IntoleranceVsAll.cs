using RimWorld;
using Verse;

namespace RW_FacialStuff
{
    public class ThoughtWorker_IntoleranceVsAll : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
        {
            if (!pawn.RaceProps.Humanlike)
            {
                return false;
            }
            if (pawn.story.traits.DegreeOfTrait(TraitDef.Named("Intolerance")) <= 0)
            {
                return false;
            }
                if (!otherPawn.RaceProps.Humanlike)
            {
                return false;
            }
            if (!RelationsUtility.PawnsKnowEachOther(pawn, otherPawn))
            {
                return false;
            }

            int num = pawn.story.traits.DegreeOfTrait(TraitDef.Named("Intolerance"));
            if (num == 1)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            if (num == 2)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            if (num == 3)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            return true;
        }
    }
}
