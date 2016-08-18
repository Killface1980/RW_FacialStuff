using UnityEngine;

namespace Verse
{
    [StaticConstructorOnStartup]
    public static class MeshPoolFS
    {
        private const int MaxGridMeshSize = 15;

        private const float HumanlikeBodyWidth = 1.5f;

        private const float HumanlikeHeadAverageWidth = 1.5f;

        private const float HumanlikeHeadNarrowWidth = 1.3f;

        public static readonly GraphicMeshSet humanlikeEyeSetAverage;

        public static readonly GraphicMeshSet humanlikeEyeSetNarrow;




        public static readonly Mesh circle;

        public static readonly Mesh[] pies;

        static MeshPoolFS()
        {
            humanlikeEyeSetAverage = new GraphicMeshSet(1.5f);
            humanlikeEyeSetNarrow = new GraphicMeshSet(1.3f, 1.5f);
        }

    }
}
