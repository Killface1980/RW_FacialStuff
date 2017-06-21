namespace FacialStuff
{
    using Verse;

    [StaticConstructorOnStartup]
    public static class MeshPoolFs
    {
        public static readonly GraphicMeshSet humanlikeMouthSetAverageMale;

        public static readonly GraphicMeshSet humanlikeMouthSetNarrowMale;

        public static readonly GraphicMeshSet humanlikeMouthSetAverageFemale;

        public static readonly GraphicMeshSet humanlikeMouthSetNarrowFemale;

        private const float HumanlikeMouthHeight = 0.75f;

        private const float HumanlikeMouthHeightFemale = 0.6f;

        private const float HumanlikeMouthAverageWidthMale = 0.7f;

        private const float HumanlikeMouthNarrowWidthMale = 0.6f;

        private const float HumanlikeMouthAverageWidthFemale = 0.65f;

        private const float HumanlikeMouthNarrowWidthFemale = 0.55f;

        static MeshPoolFs()
        {
            humanlikeMouthSetAverageMale = new GraphicMeshSet(HumanlikeMouthAverageWidthMale, HumanlikeMouthHeight);
            humanlikeMouthSetNarrowMale = new GraphicMeshSet(HumanlikeMouthNarrowWidthMale, HumanlikeMouthHeight);

            humanlikeMouthSetAverageFemale = new GraphicMeshSet(HumanlikeMouthAverageWidthFemale, HumanlikeMouthHeightFemale);
            humanlikeMouthSetNarrowFemale = new GraphicMeshSet(HumanlikeMouthNarrowWidthFemale, HumanlikeMouthHeightFemale);
        }
    }
}
