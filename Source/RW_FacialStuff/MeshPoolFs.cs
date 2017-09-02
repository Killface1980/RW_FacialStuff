namespace FacialStuff
{
    using FacialStuff.Enums;

    using Verse;

    [StaticConstructorOnStartup]
    public static class MeshPoolFS
    {
        #region Public Fields

        public static readonly GraphicMeshSet[] HumanEyeSet = new GraphicMeshSet[6];

        public static readonly GraphicMeshSet[] HumanlikeMouthSet = new GraphicMeshSet[6];

        #endregion Public Fields

        #region Private Fields

        private const float HumanlikeHeadAverageWidth = 0.75f;

        private const float HumanlikeHeadNarrowWidth = 0.65f;

        #endregion Private Fields

        #region Public Constructors

        static MeshPoolFS()
        {
            HumanlikeMouthSet[(int)FullHead.AverageNormal] = new GraphicMeshSet(HumanlikeHeadAverageWidth);
            HumanlikeMouthSet[(int)FullHead.AveragePointy] = new GraphicMeshSet(HumanlikeHeadAverageWidth);
            HumanlikeMouthSet[(int)FullHead.AverageWide] = new GraphicMeshSet(HumanlikeHeadAverageWidth);

            HumanlikeMouthSet[(int)FullHead.NarrowNormal] = new GraphicMeshSet(HumanlikeHeadNarrowWidth, HumanlikeHeadAverageWidth);
            HumanlikeMouthSet[(int)FullHead.NarrowPointy] = new GraphicMeshSet(HumanlikeHeadNarrowWidth, HumanlikeHeadAverageWidth);
            HumanlikeMouthSet[(int)FullHead.NarrowWide] = new GraphicMeshSet(HumanlikeHeadNarrowWidth, HumanlikeHeadAverageWidth);

            HumanEyeSet[(int)FullHead.AverageNormal] = new GraphicMeshSet(HumanlikeHeadAverageWidth);
            HumanEyeSet[(int)FullHead.AveragePointy] = new GraphicMeshSet(HumanlikeHeadAverageWidth);
            HumanEyeSet[(int)FullHead.AverageWide] = new GraphicMeshSet(HumanlikeHeadAverageWidth);

            HumanEyeSet[(int)FullHead.NarrowNormal] = new GraphicMeshSet(HumanlikeHeadNarrowWidth, HumanlikeHeadAverageWidth);
            HumanEyeSet[(int)FullHead.NarrowPointy] = new GraphicMeshSet(HumanlikeHeadNarrowWidth, HumanlikeHeadAverageWidth);
            HumanEyeSet[(int)FullHead.NarrowWide] = new GraphicMeshSet(HumanlikeHeadNarrowWidth, HumanlikeHeadAverageWidth);

        }

        #endregion Public Constructors
    }
}