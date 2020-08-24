// ReSharper disable StyleCop.SA1401

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
    [StaticConstructorOnStartup]
    public static class MeshPoolFS
    {
        #region Public Fields

        public static readonly GraphicMeshSet[] HumanEyeSet       = new GraphicMeshSet[12];
        public static readonly GraphicMeshSet[] HumanlikeMouthSet = new GraphicMeshSet[12];
        
        public static List<Vector2> mouthOffsetsHeadType =
            new List<Vector2>
            {
                new Vector2(0.18491f, 0.15724f), // MaleAverageNormalOffset
                new Vector2(0.19874f, 0.15346f), // MaleAveragePointyOffset
                new Vector2(0.21636f, 0.14843f), // MaleAverageWideOffset
                new Vector2(0.12810f, 0.17610f), // MaleNarrowNormalOffset
                new Vector2(0.11824f, 0.17358f), // MaleNarrowPointyOffset
                new Vector2(0.11825f, 0.17623f), // MaleNarrowWideOffset
                new Vector2(0.14331f, 0.13585f), // FemaleAverageNormalOffset
                new Vector2(0.16100f, 0.13836f), // FemaleAveragePointyOffset
                new Vector2(0.16604f, 0.13962f), // FemaleAverageWideOffset
                new Vector2(0.12956f, 0.15346f), // FemaleNarrowNormalOffset
                new Vector2(0.12328f, 0.16604f), // FemaleNarrowPointyOffset
                new Vector2(0.12075f, 0.16101f) // FemaleNarrowWideOffset
            };


        #endregion Public Fields

        #region Private Fields

        private const float HumanlikeHeadAverageWidth = 0.75f;
        private const float HumanlikeHeadNarrowWidth  = 0.65f;

        #endregion Private Fields

        #region Public Constructors

        static MeshPoolFS()
        {
            HumanlikeMouthSet[(int) FullHead.MaleAverageNormal] = 
                new GraphicMeshSet(HumanlikeHeadAverageWidth);

            HumanlikeMouthSet[(int) FullHead.MaleAveragePointy] = 
                new GraphicMeshSet(0.7f, HumanlikeHeadAverageWidth);

            HumanlikeMouthSet[(int) FullHead.MaleAverageWide] = 
                new GraphicMeshSet(HumanlikeHeadAverageWidth);

            HumanlikeMouthSet[(int) FullHead.MaleNarrowNormal] = 
                new GraphicMeshSet(0.6f, HumanlikeHeadAverageWidth);

            HumanlikeMouthSet[(int)FullHead.MaleNarrowPointy] = 
                new GraphicMeshSet(
                    0.55f,
                    HumanlikeHeadAverageWidth);

			HumanlikeMouthSet[(int)FullHead.MaleNarrowWide] = 
                new GraphicMeshSet(
					HumanlikeHeadNarrowWidth,
					HumanlikeHeadAverageWidth);

			HumanlikeMouthSet[(int)FullHead.FemaleAverageNormal] = 
                new GraphicMeshSet(
				    0.7f,
				    HumanlikeHeadAverageWidth);

			HumanlikeMouthSet[(int)FullHead.FemaleAveragePointy] = 
                new GraphicMeshSet(
					HumanlikeHeadNarrowWidth,
					HumanlikeHeadAverageWidth);

			HumanlikeMouthSet[(int)FullHead.FemaleAverageWide] = 
                new GraphicMeshSet(
					0.7f,
					HumanlikeHeadAverageWidth);

			HumanlikeMouthSet[(int)FullHead.FemaleNarrowNormal] = 
                new GraphicMeshSet(
					0.5f,
					HumanlikeHeadAverageWidth);

			HumanlikeMouthSet[(int)FullHead.FemaleNarrowPointy] = 
                new GraphicMeshSet(
					0.5f,
					HumanlikeHeadAverageWidth);

			HumanlikeMouthSet[(int)FullHead.FemaleNarrowWide] = 
                new GraphicMeshSet(
					0.6f,
					HumanlikeHeadAverageWidth);

			HumanEyeSet[(int)FullHead.MaleAverageNormal] = 
                new GraphicMeshSet(
					HumanlikeHeadAverageWidth);

			HumanEyeSet[(int)FullHead.MaleAveragePointy] = 
                new GraphicMeshSet(
					HumanlikeHeadAverageWidth);

			HumanEyeSet[(int)FullHead.MaleAverageWide] = 
                new GraphicMeshSet(
					HumanlikeHeadAverageWidth);

			HumanEyeSet[(int)FullHead.MaleNarrowNormal] = 
                new GraphicMeshSet(
					HumanlikeHeadNarrowWidth,
					HumanlikeHeadAverageWidth);
			HumanEyeSet[(int)FullHead.MaleNarrowPointy] = 
                new GraphicMeshSet(
					HumanlikeHeadNarrowWidth,
					HumanlikeHeadAverageWidth);
			HumanEyeSet[(int)FullHead.MaleNarrowWide] = 
                new GraphicMeshSet(
					HumanlikeHeadNarrowWidth,
					HumanlikeHeadAverageWidth);

			HumanEyeSet[(int)FullHead.FemaleAverageNormal] = 
                new GraphicMeshSet(
					HumanlikeHeadAverageWidth);
			HumanEyeSet[(int)FullHead.FemaleAveragePointy] = 
                new GraphicMeshSet(
					HumanlikeHeadAverageWidth);
			HumanEyeSet[(int)FullHead.FemaleAverageWide] = 
                new GraphicMeshSet(HumanlikeHeadAverageWidth);

			HumanEyeSet[(int)FullHead.FemaleNarrowNormal] = 
                new GraphicMeshSet(
					HumanlikeHeadNarrowWidth,
					HumanlikeHeadAverageWidth);
			HumanEyeSet[(int)FullHead.FemaleNarrowPointy] = 
                new GraphicMeshSet(
					HumanlikeHeadNarrowWidth);
			HumanEyeSet[(int)FullHead.FemaleNarrowWide] = 
                new GraphicMeshSet(
					HumanlikeHeadNarrowWidth,
					HumanlikeHeadAverageWidth);
		}

        #endregion Public Constructors

        public static FullHead GetFullHeadType(Gender gender, CrownType crownType, HeadType headType)
		{
            int genderVal = ((int)gender - 1) * 6;
            int crownVal = ((int)crownType - 1) * 3;
            int headTypeVal = (int)headType;
            return (FullHead)(genderVal + crownVal + headTypeVal);
		}
    }
}