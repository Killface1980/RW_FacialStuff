namespace FacialStuff.FaceStyling_Bench
{
    using System;
    using System.Collections.Generic;

    using FacialStuff.newStuff;

    using RimWorld;

    using Verse;
    using Verse.AI;

    // ReSharper disable once UnusedMember.Global
    public class Job_FaceStylingJoy : JobDriver_VisitJoyThing
    {
        private const TargetIndex FSBench = TargetIndex.A;

        private const TargetIndex CellInd = TargetIndex.B;

        private static readonly string ErrorMessage = "FaceStyling job called on building that is not Cabinet";
        private Thing ArtThing
        {
            get
            {
                return this.job.targetA.Thing;
            }
        }
        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(FSBench);
            this.FailOnDespawnedNullOrForbidden(FSBench);
            yield return Toils_Reserve.Reserve(FSBench);
            yield return Toils_Goto.GotoCell(CellInd, PathEndMode.OnCell);
            yield return this.Toil_Wait();
            yield return this.Toils_WaitWithSoundAndEffect();
        }

        private Toil Toil_Wait()
        {
            Toil toil = Toils_General.Wait(this.job.def.joyDuration);
            toil.FailOnCannotTouch(CellInd, PathEndMode.OnCell);
            toil.tickAction = this.GetWaitTickAction();
            return toil;
        }

        private Toil Toils_WaitWithSoundAndEffect()
        {
            Toil toil = new Toil();

            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.initAction = delegate
                {
                    FaceStyler faceStylerNew = this.ArtThing as FaceStyler;
                    if (faceStylerNew != null)
                    {
                        Pawn actor = this.GetActor();
                        if (actor != null)
                        {
                            if (actor.GetFace(out CompFace face))
                            {
                                face.FaceRandomizer();
                            }

                            ;
                        }
                    }
                };
            return toil;
        }

        // RimWorld.JobDriver_ViewArt
        protected override Action GetWaitTickAction()
        {
            return delegate
                {
                    // float num = this.ArtThing.GetStatValue(StatDefOf.EntertainmentStrengthFactor, true);
                    // float num2 = this.ArtThing.GetStatValue(StatDefOf.Beauty, true) / this.ArtThing.def.GetStatValueAbstract(StatDefOf.Beauty, null);
                    // num *= ((num2 <= 0f) ? 0f : num2);
                    this.pawn.rotationTracker.FaceCell(base.TargetA.Cell);
                    this.pawn.GainComfortFromCellIfPossible();

                    // float extraJoyGainFactor = num;
                    float extraJoyGainFactor = 0.5f;
                    JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.GoToNextToil, extraJoyGainFactor);
                };
        }

    }
}