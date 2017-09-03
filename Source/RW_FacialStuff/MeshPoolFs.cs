namespace FacialStuff
{
    using FacialStuff.Enums;

    using Verse;

    [StaticConstructorOnStartup]
    public static class MeshPoolFS
    {
        #region Public Fields

        public static readonly GraphicMeshSet[] HumanEyeSet = new GraphicMeshSet[12];

        public static readonly GraphicMeshSet[] HumanlikeMouthSet = new GraphicMeshSet[12];

        #endregion Public Fields

        #region Private Fields

        private const float HumanlikeHeadAverageWidth = 0.75f;

        private const float HumanlikeHeadNarrowWidth = 0.65f;

        #endregion Private Fields

        #region Public Constructors

        static MeshPoolFS()
        {
            HumanlikeMouthSet[(int)FullHead.MaleAverageNormal] = new GraphicMeshSet(0.75f, 0.75f);
            HumanlikeMouthSet[(int)FullHead.MaleAveragePointy] = new GraphicMeshSet(0.7f, 0.75f);
            HumanlikeMouthSet[(int)FullHead.MaleAverageWide] = new GraphicMeshSet(0.75f, 0.75f);

            HumanlikeMouthSet[(int)FullHead.MaleNarrowNormal] = new GraphicMeshSet(0.6f, 0.75f);
            HumanlikeMouthSet[(int)FullHead.MaleNarrowPointy] = new GraphicMeshSet(0.55f, 0.75f);
            HumanlikeMouthSet[(int)FullHead.MaleNarrowWide] = new GraphicMeshSet(0.65f, 0.75f);

            HumanlikeMouthSet[(int)FullHead.FemaleAverageNormal] = new GraphicMeshSet(0.7f, 0.75f);
            HumanlikeMouthSet[(int)FullHead.FemaleAveragePointy] = new GraphicMeshSet(0.65f, 0.75f);
            HumanlikeMouthSet[(int)FullHead.FemaleAverageWide] = new GraphicMeshSet(0.7f, 0.75f);

            HumanlikeMouthSet[(int)FullHead.FemaleNarrowNormal] = new GraphicMeshSet(0.5f, 0.75f);
            HumanlikeMouthSet[(int)FullHead.FemaleNarrowPointy] = new GraphicMeshSet(0.5f, 0.75f);
            HumanlikeMouthSet[(int)FullHead.FemaleNarrowWide] = new GraphicMeshSet(0.6f, 0.75f);

            HumanEyeSet[(int)FullHead.MaleAverageNormal] = new GraphicMeshSet(0.75f, 0.75f);
            HumanEyeSet[(int)FullHead.MaleAveragePointy] = new GraphicMeshSet(0.75f, 0.75f);
            HumanEyeSet[(int)FullHead.MaleAverageWide] = new GraphicMeshSet(0.75f, 0.75f);

            HumanEyeSet[(int)FullHead.MaleNarrowNormal] = new GraphicMeshSet(0.65f, 0.75f);
            HumanEyeSet[(int)FullHead.MaleNarrowPointy] = new GraphicMeshSet(0.65f, 0.75f);
            HumanEyeSet[(int)FullHead.MaleNarrowWide] = new GraphicMeshSet(0.65f, 0.75f);

            HumanEyeSet[(int)FullHead.FemaleAverageNormal] = new GraphicMeshSet(0.75f, 0.75f);
            HumanEyeSet[(int)FullHead.FemaleAveragePointy] = new GraphicMeshSet(0.75f, 0.75f);
            HumanEyeSet[(int)FullHead.FemaleAverageWide] = new GraphicMeshSet(0.75f, 0.75f);

            HumanEyeSet[(int)FullHead.FemaleNarrowNormal] = new GraphicMeshSet(0.65f, 0.75f);
            HumanEyeSet[(int)FullHead.FemaleNarrowPointy] = new GraphicMeshSet(0.65f, 0.75f);
            HumanEyeSet[(int)FullHead.FemaleNarrowWide] = new GraphicMeshSet(0.65f, 0.75f);
        }

        #endregion Public Constructors
    }
}