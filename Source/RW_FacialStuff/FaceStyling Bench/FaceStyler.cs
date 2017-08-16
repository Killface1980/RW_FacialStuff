namespace FacialStuff.FaceStyling_Bench
{
    using System;
    using System.Collections.Generic;

    using Verse;
    using Verse.AI;

    internal class FaceStyler : Building
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public void FaceStyling(Pawn pawn)
        {
            Find.WindowStack.Add(new Dialog_FaceStyling(pawn));
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            {
                if (!myPawn.CanReserve(this))
                {
                    FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null);
                    return new List<FloatMenuOption> { item };
                }

                if (!myPawn.CanReach(this, PathEndMode.Touch, Danger.Some))
                {
                    FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    return new List<FloatMenuOption> { item2 };
                }

                Action action2 = delegate
                    {
                        // IntVec3 InteractionSquare = (this.Position + new IntVec3(0, 0, 1)).RotatedBy(this.Rotation);
                        Job FaceStyleChanger = new Job(
                            DefDatabase<JobDef>.GetNamed("FaceStyleChanger"),
                            this,
                            this.InteractionCell);
                        if (!myPawn.jobs.TryTakeOrderedJob(FaceStyleChanger))
                        {
                            // This is used to force go job, it will work even when drafted
                            myPawn.jobs.jobQueue.EnqueueFirst(FaceStyleChanger);
                            myPawn.jobs.StopAll();
                        }
                    };

                list.Add(new FloatMenuOption("FSStylizeFace".Translate(), action2));
            }

            return list;
        }
    }
}