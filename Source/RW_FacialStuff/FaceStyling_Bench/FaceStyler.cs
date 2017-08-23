// ReSharper disable All
namespace FacialStuff.FaceStyling_Bench
{
    using System;
    using System.Collections.Generic;
    using Verse;
    using Verse.AI;

    public class FaceStyler : Building
    {

        #region Public Methods

        public void FaceStyling( Pawn pawn)
        {
            Find.WindowStack.Add(new DialogFaceStyling(pawn));
        }


      
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions( Pawn pawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            {
                if (!pawn.CanReserve(this))
                {
                    FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null);
                    return new List<FloatMenuOption> { item };
                }

                if (!pawn.CanReach(this, PathEndMode.Touch, Danger.Some))
                {
                    FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    return new List<FloatMenuOption> { item2 };
                }

                if (pawn.TryGetComp<CompFace>() == null)
                {
                    FloatMenuOption item3 = new FloatMenuOption("FacialStuffEditor.CannotUseNoFacePawn".Translate(pawn), null);
                    return new List<FloatMenuOption> { item3 };
                }

                Action action2 = delegate
                    {
                        // IntVec3 InteractionSquare = (this.Position + new IntVec3(0, 0, 1)).RotatedBy(this.Rotation);
                        Job FaceStyleChanger = new Job(
                            DefDatabase<JobDef>.GetNamed("FaceStyleChanger"),
                            this,
                            this.InteractionCell);
                        FaceStyleChanger.locomotionUrgency = LocomotionUrgency.Walk;
                        if (!pawn.jobs.TryTakeOrderedJob(FaceStyleChanger))
                        {
                            // This is used to force go job, it will work even when drafted
                            pawn.jobs.jobQueue.EnqueueFirst(FaceStyleChanger);
                            pawn.jobs.StopAll();
                        }
                    };

                list.Add(new FloatMenuOption("FacialStuffEditor.EditFace".Translate(), action2));
            }

            return list;
        }

        #endregion Public Methods
    }
}