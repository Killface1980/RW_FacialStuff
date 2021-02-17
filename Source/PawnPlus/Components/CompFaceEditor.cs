
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PawnPlus
{

    public class CompFaceEditor : ThingComp
    {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            Building styler = this.parent as Building;

            /*
            if(selPawn.IsChild())
            {
                FloatMenuOption item = new FloatMenuOption("Pawn must be at least 18 with CSL activated.", null);
                return new List<FloatMenuOption> { item };
            }
            */

            if(!pawn.CanReserve(styler))
            {
                FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null);
                yield return item;
                yield break;
            }

            if(!pawn.CanReach(styler, PathEndMode.Touch, Danger.Some))
            {
                FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                yield return item2;
                yield break;
            }

            if(!pawn.HasCompFace())
            {
                FloatMenuOption item3 = new FloatMenuOption(
                    "FacialStuffEditor.CannotUseNoFacePawn".Translate(pawn), 
                    null);
                yield return item3;
                yield break;
            }

            if(pawn.GetCompFace(out CompFace compFace) && pawn.GetCompAnim().Deactivated)
            {
                FloatMenuOption item4 = new FloatMenuOption(
                    "FacialStuffEditor.CannotUseShouldNotRender".Translate(pawn), 
                    null);
                yield return item4;
                yield break;
            }

            Action action = delegate
            {
                Job editAppearanceJob = new Job(
                    DefDatabase<JobDef>.GetNamed("PawnPlus_EditAppearance"),
                    styler,
                    styler.InteractionCell)
                {
                    locomotionUrgency = LocomotionUrgency.Sprint
                };
                if(!pawn.jobs.TryTakeOrderedJob(editAppearanceJob))
                {
                    // This is used to force go job, it will work even when drafted
                    pawn.jobs.jobQueue.EnqueueFirst(editAppearanceJob);
                    pawn.jobs.StopAll();
                }
            };
            yield return new FloatMenuOption("FacialStuffEditor.EditFace".Translate(), action);
            yield break;
        }
    }
}
