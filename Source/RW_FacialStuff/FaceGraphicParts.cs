namespace FacialStuff
{
    using FacialStuff.Graphics_FS;

    using JetBrains.Annotations;

    using Verse;

    class FaceGraphicParts
    {
        #region Public Fields

        [CanBeNull]
        public Graphic BrowGraphic;

        [CanBeNull]
        public Graphic DeadEyeGraphic;

        [CanBeNull]
        public Graphic MainBeardGraphic;

        [CanBeNull]
        public Graphic MoustacheGraphic;

        [CanBeNull]
        public Graphic RottingWrinkleGraphic;

        [CanBeNull]
        public Graphic WrinkleGraphic;

        [CanBeNull]
        public Graphic_Multi_AddedHeadParts EyeLeftPatchGraphic;

        [CanBeNull]
        public Graphic_Multi_AddedHeadParts EyeRightPatchGraphic;

        [CanBeNull]
        public Graphic_Multi_NaturalEyes EyeLeftClosedGraphic;

        [CanBeNull]
        public Graphic_Multi_NaturalEyes EyeLeftGraphic;

        [CanBeNull]
        public Graphic_Multi_NaturalEyes EyeRightClosedGraphic;

        [CanBeNull]
        public Graphic_Multi_NaturalEyes EyeRightGraphic;

        [CanBeNull]
        public Graphic_Multi_NaturalHeadParts MouthGraphic;

        #endregion Public Fields
    }

}