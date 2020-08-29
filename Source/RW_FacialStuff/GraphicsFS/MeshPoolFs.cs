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

        private static GraphicMeshSet[,] HumanEyeSet = new GraphicMeshSet[3, 2];
                
        public static List<Vector2> mouthOffsetsHeadType =
            new List<Vector2>
            {
                new Vector2(0.18491f, -0.15724f), // MaleAverageNormalOffset
                new Vector2(0.19874f, -0.15346f), // MaleAveragePointyOffset
                new Vector2(0.21636f, -0.14843f), // MaleAverageWideOffset
                new Vector2(0.12810f, -0.17610f), // MaleNarrowNormalOffset
                new Vector2(0.11824f, -0.17358f), // MaleNarrowPointyOffset
                new Vector2(0.11825f, -0.17623f), // MaleNarrowWideOffset
                new Vector2(0.14331f, -0.13585f), // FemaleAverageNormalOffset
                new Vector2(0.16100f, -0.13836f), // FemaleAveragePointyOffset
                new Vector2(0.16604f, -0.13962f), // FemaleAverageWideOffset
                new Vector2(0.12956f, -0.15346f), // FemaleNarrowNormalOffset
                new Vector2(0.12328f, -0.16604f), // FemaleNarrowPointyOffset
                new Vector2(0.12075f, -0.16101f) // FemaleNarrowWideOffset
            };


        #endregion Public Fields

        #region Private Fields

        private const float HumanlikeHeadAverageWidth = 0.75f;
        private const float HumanlikeHeadNarrowWidth  = 0.65f;

        #endregion Private Fields

        #region Public Constructors

        static MeshPoolFS()
        {
            HumanEyeSet[1, 0] =
                new GraphicMeshSet(
                    HumanlikeHeadAverageWidth);

            HumanEyeSet[2, 0] =
                new GraphicMeshSet(
                    HumanlikeHeadNarrowWidth,
                    HumanlikeHeadAverageWidth);
            for(int i = 1; i < HumanEyeSet.GetLength(0); ++i)
            {
                // This behavor might change in the future, so it may be wise to manually create mirrored mesh instead
                HumanEyeSet[i, 1] = new GraphicMeshSet(
                    HumanEyeSet[i, 0].MeshAt(Rot4.West),
                    HumanEyeSet[i, 0].MeshAt(Rot4.West));
            }

            // For Undefined CrownType
            HumanEyeSet[0, 0] = HumanEyeSet[1, 0];
            HumanEyeSet[0, 1] = HumanEyeSet[1, 1];
        }

        #endregion Public Constructors

        public static Mesh GetFaceMesh(CrownType crownType, Rot4 headFacing, bool mirrored)
		{
            return HumanEyeSet[(int)crownType, mirrored ? 1 : 0].MeshAt(headFacing);
        }

        public static FullHead GetFullHeadType(Gender gender, CrownType crownType, HeadType headType)
		{
            int genderVal = ((int)gender - 1) * 6;
            int crownVal = ((int)crownType - 1) * 3;
            int headTypeVal = (int)headType;
            return (FullHead)(genderVal + crownVal + headTypeVal);
		}
    }
}