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

        public static readonly GraphicVectorMeshSet[] HumanEyeSet       = new GraphicVectorMeshSet[12];
        public static readonly GraphicVectorMeshSet[] HumanlikeMouthSet = new GraphicVectorMeshSet[12];

        public static Vector2 EyeFemaleAverageNormalOffset = new Vector2(-0.01006f, 0f);
        public static Vector2 EyeFemaleAveragePointyOffset = new Vector2(-0.01258f, 0f);
        public static Vector2 EyeFemaleAverageWideOffset   = new Vector2(-0.01835f, 0f);
        public static Vector2 EyeFemaleNarrowNormalOffset  = new Vector2(-0.02264f, 0f);
        public static Vector2 EyeFemaleNarrowPointyOffset  = new Vector2(-0.01256f, 0f);
        public static Vector2 EyeFemaleNarrowWideOffset    = new Vector2(-0.01509f, 0f);
        public static Vector2 EyeMaleAverageNormalOffset   = new Vector2(0f,        0f);
        public static Vector2 EyeMaleAveragePointyOffset   = new Vector2(-0.01256f, 0f);
        public static Vector2 EyeMaleAverageWideOffset     = new Vector2(0f,        0f);
        public static Vector2 EyeMaleNarrowNormalOffset    = new Vector2(-0.02516f, 0f);
        public static Vector2 EyeMaleNarrowPointyOffset    = new Vector2(-0.02516f, 0f);
        public static Vector2 EyeMaleNarrowWideOffset      = new Vector2(-0.02516f, 0f);

        public static Vector2 MouthFemaleAverageNormalOffset = new Vector2(0.14331f, 0.13585f);
        public static Vector2 MouthFemaleAveragePointyOffset = new Vector2(0.16100f, 0.13836f);
        public static Vector2 MouthFemaleAverageWideOffset   = new Vector2(0.16604f, 0.13962f);
        public static Vector2 MouthFemaleNarrowNormalOffset  = new Vector2(0.12956f, 0.15346f);
        public static Vector2 MouthFemaleNarrowPointyOffset  = new Vector2(0.12328f, 0.16604f);
        public static Vector2 MouthFemaleNarrowWideOffset    = new Vector2(0.12075f, 0.16101f);
        public static Vector2 MouthMaleAverageNormalOffset   = new Vector2(0.18491f, 0.15724f);
        public static Vector2 MouthMaleAveragePointyOffset   = new Vector2(0.19874f, 0.15346f);
        public static Vector2 MouthMaleAverageWideOffset     = new Vector2(0.21636f, 0.14843f);
        public static Vector2 MouthMaleNarrowNormalOffset    = new Vector2(0.12810f, 0.17610f);
        public static Vector2 MouthMaleNarrowPointyOffset    = new Vector2(0.11824f, 0.17358f);
        public static Vector2 MouthMaleNarrowWideOffset      = new Vector2(0.11825f, 0.17623f);

        #endregion Public Fields

        #region Private Fields

        private const float HumanlikeHeadAverageWidth = 0.75f;
        private const float HumanlikeHeadNarrowWidth  = 0.65f;

        #endregion Private Fields

        #region Public Constructors

        static MeshPoolFS()
        {
            List<Vector2> eyeVector = new List<Vector2>
                                      {
                                      EyeMaleAverageNormalOffset,   // 0
                                      EyeMaleAveragePointyOffset,   // 1
                                      EyeMaleAverageWideOffset,     // 2
                                      EyeMaleNarrowNormalOffset,    // 3
                                      EyeMaleNarrowPointyOffset,    // 4
                                      EyeMaleNarrowWideOffset,      // 5
                                      EyeFemaleAverageNormalOffset, // 6
                                      EyeFemaleAveragePointyOffset, // 7
                                      EyeFemaleAverageWideOffset,   // 8
                                      EyeFemaleNarrowNormalOffset,  // 9
                                      EyeFemaleNarrowPointyOffset,  // 10
                                      EyeFemaleNarrowWideOffset     // 11
                                      };

            List<Vector2> mouthVector = new List<Vector2>
                                        {
                                        MouthMaleAverageNormalOffset,
                                        MouthMaleAveragePointyOffset,
                                        MouthMaleAverageWideOffset,
                                        MouthMaleNarrowNormalOffset,
                                        MouthMaleNarrowPointyOffset,
                                        MouthMaleNarrowWideOffset,
                                        MouthFemaleAverageNormalOffset,
                                        MouthFemaleAveragePointyOffset,
                                        MouthFemaleAverageWideOffset,
                                        MouthFemaleNarrowNormalOffset,
                                        MouthFemaleNarrowPointyOffset,
                                        MouthFemaleNarrowWideOffset
                                        };

            HumanlikeMouthSet[(int) FullHead.MaleAverageNormal] = new GraphicVectorMeshSet(
                                                                                           HumanlikeHeadAverageWidth,
                                                                                           mouthVector
                                                                                           [(int) FullHead.MaleAverageNormal]);

            HumanlikeMouthSet[(int) FullHead.MaleAveragePointy] = new GraphicVectorMeshSet(
                                                                                           0.7f,
                                                                                           HumanlikeHeadAverageWidth,
                                                                                           mouthVector
                                                                                           [(int) FullHead.MaleAveragePointy]);

            HumanlikeMouthSet[(int) FullHead.MaleAverageWide] = new GraphicVectorMeshSet(
                                                                                         HumanlikeHeadAverageWidth,
                                                                                         mouthVector
                                                                                         [(int) FullHead.MaleAverageWide]);

            HumanlikeMouthSet[(int) FullHead.MaleNarrowNormal] = new GraphicVectorMeshSet(
                                                                                          0.6f,
                                                                                          HumanlikeHeadAverageWidth,
                                                                                          mouthVector
                                                                                          [(int) FullHead.MaleNarrowNormal]);

            HumanlikeMouthSet[(int) FullHead.MaleNarrowPointy] = new GraphicVectorMeshSet(
                                                                                          0.55f,
                                                                                          HumanlikeHeadAverageWidth,
                                                                                          mouthVector
                                                                                          [(int) FullHead.MaleNarrowPointy]);

            HumanlikeMouthSet[(int) FullHead.MaleNarrowWide] = new GraphicVectorMeshSet(
                                                                                        HumanlikeHeadNarrowWidth,
                                                                                        HumanlikeHeadAverageWidth,
                                                                                        mouthVector
                                                                                        [(int) FullHead.MaleNarrowWide]);

            HumanlikeMouthSet[(int) FullHead.FemaleAverageNormal] = new GraphicVectorMeshSet(
                                                                                             0.7f,
                                                                                             HumanlikeHeadAverageWidth,
                                                                                             mouthVector
                                                                                             [(int) FullHead.FemaleAverageNormal]);

            HumanlikeMouthSet[(int) FullHead.FemaleAveragePointy] = new GraphicVectorMeshSet(
                                                                                             HumanlikeHeadNarrowWidth,
                                                                                             HumanlikeHeadAverageWidth,
                                                                                             mouthVector
                                                                                             [(int) FullHead.FemaleAveragePointy]);

            HumanlikeMouthSet[(int) FullHead.FemaleAverageWide] = new GraphicVectorMeshSet(
                                                                                           0.7f,
                                                                                           HumanlikeHeadAverageWidth,
                                                                                           mouthVector
                                                                                           [(int) FullHead.FemaleAverageWide]);

            HumanlikeMouthSet[(int) FullHead.FemaleNarrowNormal] = new GraphicVectorMeshSet(
                                                                                            0.5f,
                                                                                            HumanlikeHeadAverageWidth,
                                                                                            mouthVector
                                                                                            [(int) FullHead.FemaleNarrowNormal]);

            HumanlikeMouthSet[(int) FullHead.FemaleNarrowPointy] = new GraphicVectorMeshSet(
                                                                                            0.5f,
                                                                                            HumanlikeHeadAverageWidth,
                                                                                            mouthVector
                                                                                            [(int) FullHead.FemaleNarrowPointy]);

            HumanlikeMouthSet[(int) FullHead.FemaleNarrowWide] = new GraphicVectorMeshSet(
                                                                                          0.6f,
                                                                                          HumanlikeHeadAverageWidth,
                                                                                          mouthVector
                                                                                          [(int) FullHead.FemaleNarrowWide]);

            HumanEyeSet[(int) FullHead.MaleAverageNormal] = new GraphicVectorMeshSet(
                                                                                     HumanlikeHeadAverageWidth,
                                                                                     eyeVector
                                                                                     [(int) FullHead.MaleAverageNormal]);

            HumanEyeSet[(int) FullHead.MaleAveragePointy] = new GraphicVectorMeshSet(
                                                                                     HumanlikeHeadAverageWidth,
                                                                                     eyeVector
                                                                                     [(int) FullHead.MaleAveragePointy]);

            HumanEyeSet[(int) FullHead.MaleAverageWide] = new GraphicVectorMeshSet(
                                                                                   HumanlikeHeadAverageWidth,
                                                                                   eyeVector
                                                                                   [(int) FullHead.MaleAverageWide]);

            HumanEyeSet[(int) FullHead.MaleNarrowNormal] = new GraphicVectorMeshSet(
                                                                                    HumanlikeHeadNarrowWidth,
                                                                                    HumanlikeHeadAverageWidth,
                                                                                    eyeVector
                                                                                    [(int) FullHead.MaleNarrowNormal]);
            HumanEyeSet[(int) FullHead.MaleNarrowPointy] = new GraphicVectorMeshSet(
                                                                                    HumanlikeHeadNarrowWidth,
                                                                                    HumanlikeHeadAverageWidth,
                                                                                    eyeVector
                                                                                    [(int) FullHead.MaleNarrowPointy]);
            HumanEyeSet[(int) FullHead.MaleNarrowWide] = new GraphicVectorMeshSet(
                                                                                  HumanlikeHeadNarrowWidth,
                                                                                  HumanlikeHeadAverageWidth,
                                                                                  eyeVector
                                                                                  [(int) FullHead.MaleNarrowWide]);

            HumanEyeSet[(int) FullHead.FemaleAverageNormal] = new GraphicVectorMeshSet(
                                                                                       HumanlikeHeadAverageWidth,
                                                                                       eyeVector
                                                                                       [(int) FullHead.FemaleAverageNormal]);
            HumanEyeSet[(int) FullHead.FemaleAveragePointy] = new GraphicVectorMeshSet(
                                                                                       HumanlikeHeadAverageWidth,
                                                                                       eyeVector
                                                                                       [(int) FullHead.FemaleAveragePointy]);
            HumanEyeSet[(int) FullHead.FemaleAverageWide] = new GraphicVectorMeshSet(
                                                                                     HumanlikeHeadAverageWidth,
                                                                                     eyeVector
                                                                                     [(int) FullHead.FemaleAverageWide]);

            HumanEyeSet[(int) FullHead.FemaleNarrowNormal] = new GraphicVectorMeshSet(
                                                                                      HumanlikeHeadNarrowWidth,
                                                                                      HumanlikeHeadAverageWidth,
                                                                                      eyeVector
                                                                                      [(int) FullHead.FemaleNarrowNormal]);
            HumanEyeSet[(int) FullHead.FemaleNarrowPointy] = new GraphicVectorMeshSet(
                                                                                      HumanlikeHeadNarrowWidth,
                                                                                      HumanlikeHeadAverageWidth,
                                                                                      eyeVector
                                                                                      [(int) FullHead.FemaleNarrowPointy]);
            HumanEyeSet[(int) FullHead.FemaleNarrowWide] = new GraphicVectorMeshSet(
                                                                                    HumanlikeHeadNarrowWidth,
                                                                                    HumanlikeHeadAverageWidth,
                                                                                    eyeVector
                                                                                    [(int) FullHead.FemaleNarrowWide]);
        }

        #endregion Public Constructors
    }
}