namespace RW_FacialStuff
{
    using HugsLib.Utils;
    using System.Collections.Generic;

    using Verse;

    public class MapComponent_FacialStuff : MapComponent
    {
        public static List<SaveablePawn> PawnCache = new List<SaveablePawn>();

        public MapComponent_FacialStuff(Map map)
            : base(map)
        {
            MapComponentUtility.EnsureIsActive(this);
            this.map = map;
        }

        public static SaveablePawn GetCache(Pawn pawn)
        {
            List<SaveablePawn> cache = PawnCache;

            for (int index = 0; index < cache.Count; index++)
            {
                SaveablePawn c = cache[index];
                if (c.Pawn == pawn)
                {
                    PawnCache.RemoveAt(index);
                    return c;
                }
            }
            return null;
        }

        // public static FacePreset GetPreset(Pawn pawn)
        // {
        // foreach (FacePreset c in FacePresets)
        // if (c.Pawn == pawn)
        // return c;
        // SaveablePawn n = new SaveablePawn { Pawn = pawn };
        // PawnCache.Add(n);
        // return n;
        // }
        public override void ExposeData()
        {
            Scribe_Collections.LookList(ref PawnCache, "Pawns", LookMode.Deep);

            // Scribe_Collections.LookList(ref FacePresets, "FacePresets", LookMode.Deep);

            // if (FacePresets == null)
            // FacePresets = new List<FacePreset>();
        }
    }
}