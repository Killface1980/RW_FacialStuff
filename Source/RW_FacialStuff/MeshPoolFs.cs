namespace FacialStuff
{
    using Verse;

    using static FacialStuff.Enums.FullHead;

    [StaticConstructorOnStartup]
    public static class MeshPoolFS
    {
        #region Public Fields

        public static readonly GraphicVectorMeshSet[] HumanEyeSet = new GraphicVectorMeshSet[12];

        public static readonly GraphicVectorMeshSet[] HumanlikeMouthSet = new GraphicVectorMeshSet[12];

        #endregion Public Fields

        #region Private Fields

        private const float HumanlikeHeadAverageWidth = 0.75f;

        private const float HumanlikeHeadNarrowWidth = 0.65f;

        #endregion Private Fields

        #region Public Constructors

        static MeshPoolFS()
        {
            Settings settings = Controller.settings;

            HumanlikeMouthSet[(int)MaleAverageNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                settings.MaleAverageNormalOffset);

            HumanlikeMouthSet[(int)MaleAveragePointy] = new GraphicVectorMeshSet(
                0.7f,
                HumanlikeHeadAverageWidth,
                settings.MaleAveragePointyOffset);

            HumanlikeMouthSet[(int)MaleAverageWide] =
                new GraphicVectorMeshSet(HumanlikeHeadAverageWidth, settings.MaleAverageWideOffset);

            HumanlikeMouthSet[(int)MaleNarrowNormal] = new GraphicVectorMeshSet(
                0.6f,
                HumanlikeHeadAverageWidth,
                settings.MaleNarrowNormalOffset);

            HumanlikeMouthSet[(int)MaleNarrowPointy] = new GraphicVectorMeshSet(
                0.55f,
                HumanlikeHeadAverageWidth,
                settings.MaleNarrowPointyOffset);

            HumanlikeMouthSet[(int)MaleNarrowWide] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                settings.MaleNarrowWideOffset);

            HumanlikeMouthSet[(int)FemaleAverageNormal] = new GraphicVectorMeshSet(
                0.7f,
                HumanlikeHeadAverageWidth,
                settings.FemaleAverageNormalOffset);

            HumanlikeMouthSet[(int)FemaleAveragePointy] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                settings.FemaleAveragePointyOffset);

            HumanlikeMouthSet[(int)FemaleAverageWide] = new GraphicVectorMeshSet(
                0.7f,
                HumanlikeHeadAverageWidth,
                settings.FemaleAverageWideOffset);

            HumanlikeMouthSet[(int)FemaleNarrowNormal] = new GraphicVectorMeshSet(
                0.5f,
                HumanlikeHeadAverageWidth,
                settings.FemaleNarrowNormalOffset);

            HumanlikeMouthSet[(int)FemaleNarrowPointy] = new GraphicVectorMeshSet(
                0.5f,
                HumanlikeHeadAverageWidth,
                settings.FemaleNarrowPointyOffset);

            HumanlikeMouthSet[(int)FemaleNarrowWide] = new GraphicVectorMeshSet(
                0.6f,
                HumanlikeHeadAverageWidth,
                settings.FemaleNarrowWideOffset);

            HumanEyeSet[(int)MaleAverageNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                settings.EyeMaleAverageNormalOffset);
            HumanEyeSet[(int)MaleAveragePointy] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                settings.EyeMaleAveragePointyOffset);
            HumanEyeSet[(int)MaleAverageWide] =
                new GraphicVectorMeshSet(HumanlikeHeadAverageWidth, settings.EyeMaleAverageWideOffset);

            HumanEyeSet[(int)MaleNarrowNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                settings.EyeMaleNarrowNormalOffset);
            HumanEyeSet[(int)MaleNarrowPointy] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                settings.EyeMaleNarrowPointyOffset);
            HumanEyeSet[(int)MaleNarrowWide] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                settings.EyeMaleNarrowWideOffset);

            HumanEyeSet[(int)FemaleAverageNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                settings.EyeFemaleAverageNormalOffset);
            HumanEyeSet[(int)FemaleAveragePointy] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                settings.EyeFemaleAveragePointyOffset);
            HumanEyeSet[(int)FemaleAverageWide] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                settings.EyeFemaleAverageWideOffset);

            HumanEyeSet[(int)FemaleNarrowNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                settings.EyeFemaleNarrowNormalOffset);
            HumanEyeSet[(int)FemaleNarrowPointy] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                settings.EyeFemaleNarrowPointyOffset);
            HumanEyeSet[(int)FemaleNarrowWide] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                settings.EyeFemaleNarrowWideOffset);
        }

        #endregion Public Constructors
    }
}