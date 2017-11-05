using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
    using FacialStuff;
    using FacialStuff.newStuff;

    public class JoyGiver_ChangeAppearance : JoyGiver_InteractBuilding
    {
        protected override bool CanDoDuringParty
        {
            get
            {
                return true;
            }
        }

        protected override Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            Job result;
            if (t.InteractionCell.Standable(t.Map) && !t.IsForbidden(pawn) && !t.InteractionCell.IsForbidden(pawn) && !pawn.Map.pawnDestinationReservationManager.IsReserved(t.InteractionCell))
            {
                result = new Job(this.def.jobDef, t, t.InteractionCell);
            }
            else
            {
                result = null;
            }

            if (pawn.GetFace(out CompFace face))
            {
                if (!face.NeedsStyling)
                {
                    result = null;
                }
            }
            return result;
        }

    }
}
