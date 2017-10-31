namespace FacialStuff.Harmony.optional.PrepC
{
    using EdB.PrepareCarefully;
    using global::Harmony;
    using System.Linq;
    using Verse;

    public static class PresetLoader_Postfix
    {
        [HarmonyPostfix]
        public static void LoadFace(ref CustomPawn __result, SaveRecordPawnV3 record)
        {
            if (!SaveRecordPawnV3_Postfix.LoadedPawns.Keys.Contains(record))
            {
                return;
            }

            Pawn pawn = __result.Pawn;
            if (pawn?.TryGetComp<CompFace>() == null)
            {
                return;
            }

            PawnFace pawnFace = SaveRecordPawnV3_Postfix.LoadedPawns[record].Face;
            CompFace compFace = pawn.TryGetComp<CompFace>();
            compFace.SetPawnFace(pawnFace);

            // ReSharper disable once PossibleNullReferenceException
            pawn.story.hairColor = compFace.PawnFace.HairColor;
        }
    }
}