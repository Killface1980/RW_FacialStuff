namespace FacialStuff.Harmony.Optional.PrepC
{
    using System.Linq;

    using EdB.PrepareCarefully;

    using FacialStuff.newStuff;

    using global::Harmony;

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
            if (!pawn.HasCompFace())
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