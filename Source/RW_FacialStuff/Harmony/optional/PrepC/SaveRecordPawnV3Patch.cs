namespace FacialStuff.Harmony.optional.PrepC
{
    using System.Collections.Generic;
    using System.Linq;

    using FacialStuff;

    using global::Harmony;

    using Verse;

    public static class SaveRecordPawnV3Patch
    {
        [HarmonyPostfix]
        public static void ExposeFaceData(EdB.PrepareCarefully.SaveRecordPawnV3 __instance)
        {
            EdB.PrepareCarefully.CustomPawn customPawn = customPawns[__instance.id] as EdB.PrepareCarefully.CustomPawn;
            if (customPawns.Keys.Contains(__instance.id) && Scribe.mode == LoadSaveMode.Saving)
            {
                if (customPawn?.Pawn.TryGetComp<CompFace>() != null)
                {
                    CompFace face = customPawn.Pawn.TryGetComp<CompFace>();
                    face.ExposeFaceData();
                }
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                CompFace face = customPawn.Pawn.TryGetComp<CompFace>();
                face.ExposeFaceData();
                savedPawns.Add(__instance, face.pawnFace);
            }
        }

        public static Dictionary<object, PawnFace> savedPawns = new Dictionary<object, PawnFace>();

        public static Dictionary<string, object> customPawns = new Dictionary<string, object>();
    }
}
