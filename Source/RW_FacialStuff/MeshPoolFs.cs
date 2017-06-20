namespace FacialStuff
{
    using Verse;

    [StaticConstructorOnStartup]
    public static class MeshPoolFs
    {
        public static readonly GraphicMeshSet humanlikeHeadSetAverage;

        public static readonly GraphicMeshSet humanlikeHeadSetNarrow;

        public static readonly GraphicMeshSet humanlikeHeadSetAverageFemale;

        public static readonly GraphicMeshSet humanlikeHeadSetNarrowFemale;

        private const float HumanlikeHeadAverageWidth = 1.5f;

        private const float HumanlikeHeadNarrowWidth = 1.3f;

        private const float HumanlikeHeadAverageWidthFemale = 1.4f;
        private const float HumanlikeHeadNarrowWidthFemale = 1.2f;

        static MeshPoolFs()
        {
            humanlikeHeadSetAverage = new GraphicMeshSet(HumanlikeHeadAverageWidth);
            humanlikeHeadSetNarrow = new GraphicMeshSet(HumanlikeHeadNarrowWidth, HumanlikeHeadAverageWidth);

            humanlikeHeadSetAverageFemale = new GraphicMeshSet(HumanlikeHeadAverageWidthFemale);
            humanlikeHeadSetNarrowFemale = new GraphicMeshSet(HumanlikeHeadNarrowWidthFemale, HumanlikeHeadAverageWidthFemale);
        }
    }
}
