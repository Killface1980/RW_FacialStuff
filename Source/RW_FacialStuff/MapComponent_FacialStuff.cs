namespace RW_FacialStuff
{
    using System.Collections.Generic;

    using Verse;

    public class MapComponent_FacialStuff : MapComponent
    {
        public static List<SaveablePawn> PawnCache = new List<SaveablePawn>();

        public MapComponent_FacialStuff(Map map)
            : base(map)
        {
        }

        public static SaveablePawn GetCache(Pawn pawn)
        {
            foreach (SaveablePawn c in PawnCache)
            {
                if (c.Pawn == pawn) return c;
            }

            SaveablePawn n = new SaveablePawn { Pawn = pawn };
            PawnCache.Add(n);
            return n;
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
            if (PawnCache == null)
            {
                PawnCache = new List<SaveablePawn>();
            }

            // if (FacePresets == null)
            // FacePresets = new List<FacePreset>();
        }
    }
}