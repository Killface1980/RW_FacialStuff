namespace FacialStuff
{
    using Verse;

    [StaticConstructorOnStartup]
    public static partial class MeshPoolFs
    {
        public static readonly GraphicMeshSet[] HumanlikeMouthSet = new GraphicMeshSet[12];

        private const float HumanlikeMouthHeight = 0.75f;

        private const float HumanlikeMouthHeightFemale = 0.6f;

        private const float HumanlikeMouthAverageWidthMale = 0.7f;

        private const float HumanlikeMouthNarrowWidthMale = 0.6f;

        private const float HumanlikeMouthAverageWidthFemale = 0.6f;

        private const float HumanlikeMouthNarrowWidthFemale = 0.5f;

        public static readonly GraphicMeshSet HumanlikeMouthSetAverageMale;

        public static readonly GraphicMeshSet HumanlikeMouthSetNarrowMale;

        public static readonly GraphicMeshSet HumanlikeMouthSetAverageFemale;

        public static readonly GraphicMeshSet HumanlikeMouthSetNarrowFemale;

        public static readonly GraphicMeshSet[] HumanEyeSet = new GraphicMeshSet[12];

        static MeshPoolFs()
        {
            HumanlikeMouthSet[(int)FullHead.MaleAverageNormal] = new GraphicMeshSet(0.7f, 0.75f);
            HumanlikeMouthSet[(int)FullHead.MaleAveragePointy] = new GraphicMeshSet(0.65f, 0.75f);
            HumanlikeMouthSet[(int)FullHead.MaleAverageWide] = new GraphicMeshSet(0.75f, 0.75f);

            HumanlikeMouthSet[(int)FullHead.MaleNarrowNormal] = new GraphicMeshSet(0.6f, 0.7f);
            HumanlikeMouthSet[(int)FullHead.MaleNarrowPointy] = new GraphicMeshSet(0.55f, 0.7f);
            HumanlikeMouthSet[(int)FullHead.MaleNarrowWide] = new GraphicMeshSet(0.65f, 0.7f);

            HumanlikeMouthSet[(int)FullHead.FemaleAverageNormal] = new GraphicMeshSet(0.6f, 0.7f);
            HumanlikeMouthSet[(int)FullHead.FemaleAveragePointy] = new GraphicMeshSet(0.55f, 0.7f);
            HumanlikeMouthSet[(int)FullHead.FemaleAverageWide] = new GraphicMeshSet(0.65f, 0.7f);

            HumanlikeMouthSet[(int)FullHead.FemaleNarrowNormal] = new GraphicMeshSet(0.5f, 0.65f);
            HumanlikeMouthSet[(int)FullHead.FemaleNarrowPointy] = new GraphicMeshSet(0.45f, 0.65f);
            HumanlikeMouthSet[(int)FullHead.FemaleNarrowWide] = new GraphicMeshSet(0.55f, 0.65f);


            HumanEyeSet[(int)FullHead.MaleAverageNormal] = new GraphicMeshSet(0.75f, 0.75f);
            HumanEyeSet[(int)FullHead.MaleAveragePointy] = new GraphicMeshSet(0.75f, 0.75f);
            HumanEyeSet[(int)FullHead.MaleAverageWide] = new GraphicMeshSet(0.75f, 0.75f);

            HumanEyeSet[(int)FullHead.MaleNarrowNormal] = new GraphicMeshSet(0.6f, 0.75f);
            HumanEyeSet[(int)FullHead.MaleNarrowPointy] = new GraphicMeshSet(0.6f, 0.75f);
            HumanEyeSet[(int)FullHead.MaleNarrowWide] = new GraphicMeshSet(0.6f, 0.75f);

            HumanEyeSet[(int)FullHead.FemaleAverageNormal] = new GraphicMeshSet(0.75f, 0.75f);
            HumanEyeSet[(int)FullHead.FemaleAveragePointy] = new GraphicMeshSet(0.75f, 0.75f);
            HumanEyeSet[(int)FullHead.FemaleAverageWide] = new GraphicMeshSet(0.75f, 0.75f);

            HumanEyeSet[(int)FullHead.FemaleNarrowNormal] = new GraphicMeshSet(0.6f, 0.75f);
            HumanEyeSet[(int)FullHead.FemaleNarrowPointy] = new GraphicMeshSet(0.6f, 0.75f);
            HumanEyeSet[(int)FullHead.FemaleNarrowWide] = new GraphicMeshSet(0.6f, 0.75f);


            // HumanlikeMouthSetAverageMale = new GraphicMeshSet(HumanlikeMouthAverageWidthMale, HumanlikeMouthHeight);
            // HumanlikeMouthSetNarrowMale = new GraphicMeshSet(HumanlikeMouthNarrowWidthMale, HumanlikeMouthHeight);
            // HumanlikeMouthSetAverageFemale = new GraphicMeshSet(HumanlikeMouthAverageWidthFemale, HumanlikeMouthHeightFemale);
            // HumanlikeMouthSetNarrowFemale = new GraphicMeshSet(HumanlikeMouthNarrowWidthFemale, HumanlikeMouthHeightFemale);
        }
    }
}
