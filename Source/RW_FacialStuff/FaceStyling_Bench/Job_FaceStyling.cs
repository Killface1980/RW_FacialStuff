namespace FacialStuff.FaceStyling_Bench
{
    using System.Collections.Generic;

    using Verse;
    using Verse.AI;

    // ReSharper disable once UnusedMember.Global
    public class Job_FaceStyling : JobDriver
    {
        private const TargetIndex CellInd = TargetIndex.B;

        private const TargetIndex FSBench = TargetIndex.A;

        private static readonly string ErrorMessage = "FaceStyling job called on building with a troubled youth and serious issues.";

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
            yield return this.Toils_WaitWithSoundAndEffect();
        }

        private Toil Toils_WaitWithSoundAndEffect()
        {
            Toil toil = new Toil
                            {
                                initAction = delegate
                                    {
                                        CompFaceEditor faceStylerNew =
                                            this.TargetA.Thing.TryGetComp<CompFaceEditor>();
                                        if (faceStylerNew != null)
                                        {
                                            Pawn actor = this.GetActor();
                                            if (actor != null
                                                && actor.Position == this.TargetA.Thing.InteractionCell)
                                            {
                                                faceStylerNew.OpenFSDialog(actor);
                                            }
                                        }
                                        else
                                        {
                                            Log.Error(ErrorMessage);
                                        }
                                    },
                                defaultCompleteMode = ToilCompleteMode.Instant
                            };
            return toil;
        }
    }
}