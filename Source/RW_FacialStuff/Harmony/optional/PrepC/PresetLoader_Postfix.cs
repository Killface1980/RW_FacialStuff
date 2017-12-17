namespace FacialStuff.Harmony.Optional.PrepC
{
    using System.Linq;

    using EdB.PrepareCarefully;

    using global::Harmony;

    using RimWorld;

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
            if (pawn.GetCompFace(out CompFace compFace))
            {
                if (pawnFace == null)
                {
                    pawnFace = new PawnFace(compFace, Faction.OfPlayer.def, false);
                }
                compFace.SetPawnFace(pawnFace);
                pawn.story.hairColor = compFace.PawnFace.HairColor;
            }

            // ReSharper disable once PossibleNullReferenceException
        }
    }
}