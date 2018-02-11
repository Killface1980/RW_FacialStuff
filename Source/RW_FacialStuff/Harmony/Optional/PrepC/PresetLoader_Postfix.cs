using System.Linq;
using EdB.PrepareCarefully;
using Harmony;
using RimWorld;
using Verse;

namespace FacialStuff.Harmony.Optional.PrepC
{
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
                // ReSharper disable once PossibleNullReferenceException
                pawn.story.hairColor = compFace.PawnFace.HairColor;
            }
        }
    }
}