namespace FacialStuff.Harmony.optional.PrepC
{
    using System.Linq;

    using FacialStuff;

    using global::Harmony;

    using Verse;

    public static class PresetLoaderPatch
    {
        [HarmonyPostfix]
        public static void LoadFace(ref EdB.PrepareCarefully.CustomPawn __result, EdB.PrepareCarefully.SaveRecordPawnV3 record)
        {
            if (SaveRecordPawnV3Patch.savedPawns.Keys.Contains(record))
            {
                Pawn pawn = __result.Pawn;
                if (pawn?.TryGetComp<CompFace>() != null)
                {
                    PawnFace pawnFace = SaveRecordPawnV3Patch.savedPawns[record];
                    CompFace compFace = pawn.TryGetComp<CompFace>();
                    compFace.pawnFace = pawnFace;

                    pawn.story.hairColor = compFace.pawnFace.HairColor;
                    
                }
            }
        }
    }
}
