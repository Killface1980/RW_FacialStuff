﻿// ReSharper disable All

namespace FacialStuff
{
    using System;
    using System.Collections.Generic;

    using FacialStuff.FaceEditor;

    using Verse;
    using Verse.AI;

    public class CompFaceEditor : ThingComp
    {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            Building styler = this.parent as Building;

            List<FloatMenuOption> list = new List<FloatMenuOption>();
            {
                if (!selPawn.CanReserve(styler))
                {
                    FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null);
                    return new List<FloatMenuOption> { item };
                }

                if (!selPawn.CanReach(styler, PathEndMode.Touch, Danger.Some))
                {
                    FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    return new List<FloatMenuOption> { item2 };
                }

                if (!selPawn.HasCompFace())
                {
                    FloatMenuOption item3 = new FloatMenuOption(
                        "FacialStuffEditor.CannotUseNoFacePawn".Translate(selPawn),
                        null);
                    return new List<FloatMenuOption> { item3 };
                }

                Action action2 = delegate
                    {
                        // IntVec3 InteractionSquare = (this.Position + new IntVec3(0, 0, 1)).RotatedBy(this.Rotation);
                        Job FaceStyleChanger = new Job(
                                                   DefDatabase<JobDef>.GetNamed("FaceStyleChanger"),
                                                   styler,
                                                   styler.InteractionCell)
                                                   {
                                                       locomotionUrgency = LocomotionUrgency
                                                           .Sprint
                                                   };
                        if (!selPawn.jobs.TryTakeOrderedJob(FaceStyleChanger))
                        {
                            // This is used to force go job, it will work even when drafted
                            selPawn.jobs.jobQueue.EnqueueFirst(FaceStyleChanger);
                            selPawn.jobs.StopAll();
                        }
                    };

                list.Add(new FloatMenuOption("FacialStuffEditor.EditFace".Translate(), action2));
            }

            return list;
        }

        public void OpenFSDialog(Pawn pawn)
        {
            Find.WindowStack.Add(new DialogFaceStyling(pawn));
        }
    }
}