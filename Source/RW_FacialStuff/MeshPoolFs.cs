namespace FacialStuff
{
    using FacialStuff.Enums;

    using Verse;

    [StaticConstructorOnStartup]
    public static class MeshPoolFS
    {
        private const float HumanlikeHeadAverageWidth = 0.75f;

        private const float HumanlikeHeadNarrowWidth = 0.65f;

        public static readonly GraphicVectorMeshSet[] HumanEyeSet = new GraphicVectorMeshSet[12];

        public static readonly GraphicVectorMeshSet[] HumanlikeMouthSet = new GraphicVectorMeshSet[12];

        static MeshPoolFS()
        {
            HumanlikeMouthSet[(int)FullHead.MaleAverageNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FullHead.MaleAverageNormal]);

            HumanlikeMouthSet[(int)FullHead.MaleAveragePointy] = new GraphicVectorMeshSet(
                0.7f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FullHead.MaleAveragePointy]);

            HumanlikeMouthSet[(int)FullHead.MaleAverageWide] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FullHead.MaleAverageWide]);

            HumanlikeMouthSet[(int)FullHead.MaleNarrowNormal] = new GraphicVectorMeshSet(
                0.6f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FullHead.MaleNarrowNormal]);

            HumanlikeMouthSet[(int)FullHead.MaleNarrowPointy] = new GraphicVectorMeshSet(
                0.55f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FullHead.MaleNarrowPointy]);

            HumanlikeMouthSet[(int)FullHead.MaleNarrowWide] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FullHead.MaleNarrowWide]);

            HumanlikeMouthSet[(int)FullHead.FemaleAverageNormal] = new GraphicVectorMeshSet(
                0.7f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FullHead.FemaleAverageNormal]);

            HumanlikeMouthSet[(int)FullHead.FemaleAveragePointy] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FullHead.FemaleAveragePointy]);

            HumanlikeMouthSet[(int)FullHead.FemaleAverageWide] = new GraphicVectorMeshSet(
                0.7f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FullHead.FemaleAverageWide]);

            HumanlikeMouthSet[(int)FullHead.FemaleNarrowNormal] = new GraphicVectorMeshSet(
                0.5f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FullHead.FemaleNarrowNormal]);

            HumanlikeMouthSet[(int)FullHead.FemaleNarrowPointy] = new GraphicVectorMeshSet(
                0.5f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FullHead.FemaleNarrowPointy]);

            HumanlikeMouthSet[(int)FullHead.FemaleNarrowWide] = new GraphicVectorMeshSet(
                0.6f,
                HumanlikeHeadAverageWidth,
                Settings.MouthVector[(int)FullHead.FemaleNarrowWide]);

            HumanEyeSet[(int)FullHead.MaleAverageNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FullHead.MaleAverageNormal]);

            HumanEyeSet[(int)FullHead.MaleAveragePointy] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FullHead.MaleAveragePointy]);

            HumanEyeSet[(int)FullHead.MaleAverageWide] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FullHead.MaleAverageWide]);

            HumanEyeSet[(int)FullHead.MaleNarrowNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FullHead.MaleNarrowNormal]);
            HumanEyeSet[(int)FullHead.MaleNarrowPointy] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FullHead.MaleNarrowPointy]);
            HumanEyeSet[(int)FullHead.MaleNarrowWide] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FullHead.MaleNarrowWide]);

            HumanEyeSet[(int)FullHead.FemaleAverageNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FullHead.FemaleAverageNormal]);
            HumanEyeSet[(int)FullHead.FemaleAveragePointy] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FullHead.FemaleAveragePointy]);
            HumanEyeSet[(int)FullHead.FemaleAverageWide] = new GraphicVectorMeshSet(
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FullHead.FemaleAverageWide]);

            HumanEyeSet[(int)FullHead.FemaleNarrowNormal] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FullHead.FemaleNarrowNormal]);
            HumanEyeSet[(int)FullHead.FemaleNarrowPointy] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FullHead.FemaleNarrowPointy]);
            HumanEyeSet[(int)FullHead.FemaleNarrowWide] = new GraphicVectorMeshSet(
                HumanlikeHeadNarrowWidth,
                HumanlikeHeadAverageWidth,
                Settings.EyeVector[(int)FullHead.FemaleNarrowWide]);
        }
    }
}