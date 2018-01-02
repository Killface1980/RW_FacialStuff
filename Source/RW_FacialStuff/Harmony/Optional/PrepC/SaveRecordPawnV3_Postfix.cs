using System.Collections.Generic;
using System.Linq;
using EdB.PrepareCarefully;
using Harmony;
using Verse;

namespace FacialStuff.Harmony.Optional.PrepC
{
    public static class SaveRecordPawnV3_Postfix
    {
        // Don't use CustomPawn here, must be object!!!
        public static readonly Dictionary<object, SaveRecordFaceV3> LoadedPawns =
            new Dictionary<object, SaveRecordFaceV3>();

        // Don't use CustomPawn here, must be object!!!
        public static readonly Dictionary<string, object> SavedPawns = new Dictionary<string, object>();

        [HarmonyPostfix]
        public static void ExposeFaceData(SaveRecordPawnV3 __instance)
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
                    SaveRecordFaceV3 face = new SaveRecordFaceV3(customPawn.Pawn);
                    face.ExposeData();
                }
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                SaveRecordFaceV3 face = new SaveRecordFaceV3();
                face.ExposeData();
                LoadedPawns.Add(__instance, face);
            }
        }
    }
}