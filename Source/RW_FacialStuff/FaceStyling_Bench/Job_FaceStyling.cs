namespace FacialStuff.FaceStyling_Bench
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using Verse;
    using Verse.AI;

    // ReSharper disable once UnusedMember.Global
    public class Job_FaceStyling : JobDriver
    {
        #region Private Fields

        private const TargetIndex CellInd = TargetIndex.B;
        private const TargetIndex ColorChanger = TargetIndex.A;

        [NotNull]
        private static readonly string ErrorMessage = "FaceStyling job called on building that is not Cabinet";

        #endregion Private Fields

        #region Protected Methods

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
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
                                   FaceStyler faceStyler = this.TargetA.Thing as FaceStyler;
                                   if (faceStyler != null)
                                   {
                                       FaceStyler thing = (FaceStyler)this.TargetA.Thing;
                                       if (this.GetActor().Position == this.TargetA.Thing.InteractionCell)
                                       {
                                           thing.FaceStyling(this.GetActor());
                                       }
                                   }
                                   else
                                   {
                                       Log.Error(ErrorMessage.Translate());
                                   }
                               },
                           defaultCompleteMode = ToilCompleteMode.Instant
                       };
        }

        #endregion Private Methods
    }
}