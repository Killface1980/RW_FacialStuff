using System.Linq;
using EdB.PrepareCarefully;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FacialStuff.Harmony.Optional.PrepC
{
    public static class PresetLoaderV4_Postfix
    {
        [HarmonyPostfix]
        public static void LoadFace(ref CustomPawn __result, SaveRecordPawnV4 record)
        {
            if (!SaveRecordPawnV4_Postfix.LoadedPawns.Keys.Contains(record))
            {
                return;
            }

            Pawn pawn = __result.Pawn;
            if (!pawn.HasCompFace())
            {
                return;
            }

            PawnFace pawnFace = SaveRecordPawnV4_Postfix.LoadedPawns[record].Face;
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