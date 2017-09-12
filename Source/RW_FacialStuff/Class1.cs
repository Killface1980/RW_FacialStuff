using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse
{
    [StaticConstructorOnStartup]
    public static class MeshPool
    {
        private const int MaxGridMeshSize = 15;

        private const float HumanlikeBodyWidth = 1.5f;

        private const float HumanlikeHeadAverageWidth = 1.5f;

        private const float HumanlikeHeadNarrowWidth = 1.3f;

        public static readonly GraphicMeshSet humanlikeBodySet;

        public static readonly GraphicMeshSet humanlikeHeadSet;

        public static readonly GraphicMeshSet humanlikeHairSetAverage;

        public static readonly GraphicMeshSet humanlikeHairSetNarrow;

        public static readonly Mesh plane025;

        public static readonly Mesh plane03;

        public static readonly Mesh plane05;

        public static readonly Mesh plane08;

        public static readonly Mesh plane10;

        public static readonly Mesh plane10Back;

        public static readonly Mesh plane10Flip;

        public static readonly Mesh plane14;

        public static readonly Mesh plane20;

        public static readonly Mesh wholeMapPlane;

        private static Dictionary<Vector2, Mesh> planes;

        private static Dictionary<Vector2, Mesh> planesFlip;

        public static readonly Mesh circle;

        public static readonly Mesh[] pies;

        static MeshPool()
        {
            MeshPool.humanlikeBodySet = new GraphicMeshSet(1.5f);
            MeshPool.humanlikeHeadSet = new GraphicMeshSet(1.5f);
            MeshPool.humanlikeHairSetAverage = new GraphicMeshSet(1.5f);
            MeshPool.humanlikeHairSetNarrow = new GraphicMeshSet(1.3f, 1.5f);
        }

    }
}
