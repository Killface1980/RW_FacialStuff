using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PawnPlus
{
    public class Job_EditAppearance : JobDriver
    {
        private const TargetIndex CellInd = TargetIndex.B;

        private const TargetIndex Dresser = TargetIndex.A;

        private static readonly string ErrorMessage = "FaceStyling job called on building with a troubled youth and serious issues.";

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(this.job.targetA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(Dresser);
            this.FailOnDespawnedNullOrForbidden(Dresser);
            yield return Toils_Reserve.Reserve(Dresser);
            yield return Toils_Goto.GotoCell(CellInd, PathEndMode.OnCell);
            yield return this.Toils_WaitWithSoundAndEffect();
        }

        private Toil Toils_WaitWithSoundAndEffect()
        {
            Toil toil = new Toil
            {
                initAction = delegate
                {
                    CompFaceEditor faceStylerNew = this.TargetA.Thing?.TryGetComp<CompFaceEditor>();
                    if(faceStylerNew != null)
                    {
                        Pawn actor = this.GetActor();
                        if(actor != null && 
                            actor.Position == this.TargetA.Thing.InteractionCell)
                        {
                            Find.WindowStack.Add(new UI.DialogEditAppearance(actor));
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
