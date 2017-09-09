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

            HumanlikeMouthSet[(int)MaleAverageNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)MaleAverageNormal]);

            HumanlikeMouthSet[(int)MaleAveragePointy] = new GraphicVectorMeshSet(
                0.7f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)MaleAveragePointy]);

            HumanlikeMouthSet[(int)MaleAverageWide] =
                new GraphicVectorMeshSet(HumanlikeHeadAverageWidth,
                    Settings.MouthVector[(int)MaleAverageWide]);

            HumanlikeMouthSet[(int)MaleNarrowNormal] = new GraphicVectorMeshSet(
                0.6f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)MaleNarrowNormal]);

            HumanlikeMouthSet[(int)MaleNarrowPointy] = new GraphicVectorMeshSet(
                0.55f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)MaleNarrowPointy]);

            HumanlikeMouthSet[(int)MaleNarrowWide] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)MaleNarrowWide]);

            HumanlikeMouthSet[(int)FemaleAverageNormal] = new GraphicVectorMeshSet(
                0.7f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FemaleAverageNormal]);

            HumanlikeMouthSet[(int)FemaleAveragePointy] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FemaleAveragePointy]);

            HumanlikeMouthSet[(int)FemaleAverageWide] = new GraphicVectorMeshSet(
                0.7f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FemaleAverageWide]);

            HumanlikeMouthSet[(int)FemaleNarrowNormal] = new GraphicVectorMeshSet(
                0.5f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FemaleNarrowNormal]);

            HumanlikeMouthSet[(int)FemaleNarrowPointy] = new GraphicVectorMeshSet(
                0.5f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FemaleNarrowPointy]);

            HumanlikeMouthSet[(int)FemaleNarrowWide] = new GraphicVectorMeshSet(
                0.6f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FemaleNarrowWide]);

            HumanEyeSet[(int)MaleAverageNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)MaleAverageNormal]);

            HumanEyeSet[(int)MaleAveragePointy] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)MaleAveragePointy]);

            HumanEyeSet[(int)MaleAverageWide] =
                new GraphicVectorMeshSet(HumanlikeHeadAverageWidth,
                    Settings.EyeVector[(int)MaleAverageWide]);

            HumanEyeSet[(int)MaleNarrowNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)MaleNarrowNormal]);
            HumanEyeSet[(int)MaleNarrowPointy] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)MaleNarrowPointy]);
            HumanEyeSet[(int)MaleNarrowWide] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)MaleNarrowWide]);

            HumanEyeSet[(int)FemaleAverageNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FemaleAverageNormal]);
            HumanEyeSet[(int)FemaleAveragePointy] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FemaleAveragePointy]);
            HumanEyeSet[(int)FemaleAverageWide] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FemaleAverageWide]);

            HumanEyeSet[(int)FemaleNarrowNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FemaleNarrowNormal]);
            HumanEyeSet[(int)FemaleNarrowPointy] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FemaleNarrowPointy]);
            HumanEyeSet[(int)FemaleNarrowWide] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FemaleNarrowWide]);
        }

        #endregion Public Constructors
    }
}