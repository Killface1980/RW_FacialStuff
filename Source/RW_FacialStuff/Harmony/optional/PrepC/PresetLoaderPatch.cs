namespace FacialStuff.Harmony.optional.PrepC
{
    using System.Linq;

    using EdB.PrepareCarefully;

    using FacialStuff;

    using global::Harmony;

    using Verse;

    public static class PresetLoaderPatch
    {
        [HarmonyPostfix]
        public static void LoadFace( ref CustomPawn __result, SaveRecordPawnV3 record)
        {
            if (!SaveRecordPawnV3Patch.LoadedPawns.Keys.Contains(record))
            {
                return;
            }
            Pawn pawn = __result.Pawn;
            if (pawn?.TryGetComp<CompFace>() == null)
            {
                return;
            }
            PawnFace pawnFace = SaveRecordPawnV3Patch.LoadedPawns[record].Face;
            CompFace compFace = pawn.TryGetComp<CompFace>();
            compFace.SetPawnFace(pawnFace);

            // ReSharper disable once PossibleNullReferenceException
            pawn.story.hairColor = compFace.PawnFace.HairColor;
        }
    }
}
