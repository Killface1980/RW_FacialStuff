namespace FacialStuff.FaceStyling_Bench
{
    using System.Collections.Generic;

    using Verse;
    using Verse.AI;

    // ReSharper disable once UnusedMember.Global
    public class Job_FaceStyling : JobDriver
    {
        #region Private Fields

        private const TargetIndex CellInd = TargetIndex.B;
        private const TargetIndex ColorChanger = TargetIndex.A;

        private static readonly string ErrorMessage = "FaceStyling job called on building that is not Cabinet";

        #endregion Private Fields

        #region Protected Methods


        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(ColorChanger);
            this.FailOnDespawnedNullOrForbidden(ColorChanger);
            yield return Toils_Reserve.Reserve(ColorChanger);
            yield return Toils_Goto.GotoCell(CellInd, PathEndMode.OnCell);
            yield return this.Toils_WaitWithSoundAndEffect();
        }

        #endregion Protected Methods

        #region Private Methods

        private Toil Toils_WaitWithSoundAndEffect()
        {
            return new Toil
            {
                initAction = delegate
                    {
                        FaceStyler faceStylerNew = this.TargetA.Thing as FaceStyler;
                        if (faceStylerNew != null)
                        {
                            FaceStyler thing = (FaceStyler)this.TargetA.Thing;
                            Pawn actor = this.GetActor();
                            if (actor != null && actor.Position == this.TargetA.Thing.InteractionCell)
                            {
                                thing.FaceStyling(actor);
                            }
                        }
                        else
                        {
                            Log.Error(ErrorMessage);
                        }
                    },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        #endregion Private Methods
    }
}