// ReSharper disable StyleCop.SA1401

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PawnPlus.Graphics
{
    [StaticConstructorOnStartup]
    public static class MeshPoolFS
    {
        private class MeshSet
        {
            private Mesh _mesh;
            private Mesh _flippedMesh;

            public MeshSet(float width, float height)
			{
                _mesh = MeshPool.GridPlane(new Vector2(width, height));
                _flippedMesh = MeshPool.GridPlaneFlip(new Vector2(width, height));
            }

            public Mesh MeshAt(Rot4 rot, bool flipped)
			{
                if(!flipped)
				{
					switch(rot.AsInt)
					{ 
                        case 0:
                            return _mesh;

                        case 1:
                            return _mesh;

                        case 2:
                            return _mesh;

                        case 3:
                            return _flippedMesh;
                    }
				}
                else
				{
					switch(rot.AsInt)
					{
                        case 0:
                            return _flippedMesh;

                        case 1:
                            return _mesh;

                        case 2:
                            return _flippedMesh;

                        case 3:
                            return _flippedMesh;
                    }
                }
                return null;
			}
        }
        
        #region Public Fields

        private static MeshSet[] HumanEyeSet = new MeshSet[3];
                
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
            float width, height;
            GetFaceMeshDimension(CrownType.Average, out width, out height);
            HumanEyeSet[1] = new MeshSet(width, height);
            GetFaceMeshDimension(CrownType.Narrow, out width, out height);
            HumanEyeSet[2] = new MeshSet(width, height);
            
            // For Undefined CrownType
            HumanEyeSet[0] = HumanEyeSet[1];
        }

        #endregion Public Constructors

        public static Mesh GetFaceMesh(CrownType crownType, Rot4 headFacing, bool mirrored)
		{
            return HumanEyeSet[(int)crownType].MeshAt(headFacing, mirrored);
        }

        public static FullHead GetFullHeadType(Gender gender, CrownType crownType, HeadType headType)
		{
            int genderVal = ((int)gender - 1) * 6;
            int crownVal = ((int)crownType - 1) * 3;
            int headTypeVal = (int)headType;
            return (FullHead)(genderVal + crownVal + headTypeVal);
		}

        public static void GetFaceMeshDimension(CrownType crownType, out float width, out float height)
		{
            if(crownType == CrownType.Average)
			{
                width = HumanlikeHeadAverageWidth;
                height = HumanlikeHeadAverageWidth;
            } else
			{
                width = HumanlikeHeadNarrowWidth;
                height = HumanlikeHeadAverageWidth;
            }
        }
    }
}