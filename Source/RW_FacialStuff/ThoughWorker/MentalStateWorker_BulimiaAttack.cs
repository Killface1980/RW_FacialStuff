using RimWorld;
using Verse;

namespace RW_FacialStuff
{
    public class MentalStateWorker_BulimiaAttack : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            return base.StateCanOccur(pawn) && pawn.GetPosture() == PawnPosture.Standing && Find.ListerThings.ThingsOfDef(ThingDefOf.Beer).Count > 0;
        }
    }
}
