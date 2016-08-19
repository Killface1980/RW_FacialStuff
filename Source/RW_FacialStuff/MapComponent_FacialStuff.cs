using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RW_FacialStuff
{

    public class MapComponent_FacialStuff : MapComponent
    {
        public static List<SaveablePawn> PawnCache = new List<SaveablePawn>();

    //    public static List<FacePreset> FacePresets = new List<FacePreset>();

        public static MapComponent_FacialStuff Get
        {
            get
            {
                MapComponent_FacialStuff getComponent = Find.Map.components.OfType<MapComponent_FacialStuff>().FirstOrDefault();
                if (getComponent == null)
                {
                    getComponent = new MapComponent_FacialStuff();
                    Find.Map.components.Add(getComponent);
                }

                return getComponent;
            }
        }

        public static SaveablePawn GetCache(Pawn pawn)
        {
            foreach (SaveablePawn c in PawnCache)
                if (c.Pawn == pawn)
                    return c;
            SaveablePawn n = new SaveablePawn { Pawn = pawn };
            PawnCache.Add(n);
            return n;
        }

   //   public static FacePreset GetPreset(Pawn pawn)
   //   {
   //       foreach (FacePreset c in FacePresets)
   //           if (c.Pawn == pawn)
   //               return c;
   //       SaveablePawn n = new SaveablePawn { Pawn = pawn };
   //       PawnCache.Add(n);
   //       return n;
   //   }

        public override void ExposeData()
        {
            Scribe_Collections.LookList(ref PawnCache, "Pawns", LookMode.Deep);
        //    Scribe_Collections.LookList(ref FacePresets, "FacePresets", LookMode.Deep);

            if (PawnCache == null)
                PawnCache = new List<SaveablePawn>();
        //  if (FacePresets == null)
        //      FacePresets = new List<FacePreset>();
        }
    }
}
