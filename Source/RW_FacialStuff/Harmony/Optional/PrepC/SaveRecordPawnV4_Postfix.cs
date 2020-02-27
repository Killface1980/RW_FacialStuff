using System.Collections.Generic;
using System.Linq;
using EdB.PrepareCarefully;
using HarmonyLib;
using Verse;

namespace FacialStuff.Harmony.Optional.PrepC
{
    public static class SaveRecordPawnV4_Postfix
    {
        // Don't use CustomPawn here, must be object!!!
        public static readonly Dictionary<object, SaveRecordFaceV4> LoadedPawns =
            new Dictionary<object, SaveRecordFaceV4>();

        // Don't use CustomPawn here, must be object!!!
        public static readonly Dictionary<string, object> SavedPawns = new Dictionary<string, object>();

        [HarmonyPostfix]
        public static void ExposeFaceData(SaveRecordPawnV4 __instance)
        {
            if (SavedPawns.Keys.Contains(__instance.id) && Scribe.mode == LoadSaveMode.Saving)
            {
                CustomPawn customPawn = SavedPawns[__instance.id] as CustomPawn;
                if (!customPawn?.Pawn.HasCompFace() == true)
                {
                    return;
                }

                if (customPawn != null)
                {
                    SaveRecordFaceV4 face = new SaveRecordFaceV4(customPawn.Pawn);
                    face.ExposeData();
                }
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                SaveRecordFaceV4 face = new SaveRecordFaceV4();
                face.ExposeData();
                LoadedPawns.Add(__instance, face);
            }
        }
    }
}