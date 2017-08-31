namespace FacialStuff.Harmony.optional.PrepC
{
    using System.Collections.Generic;
    using System.Linq;

    using EdB.PrepareCarefully;

    using FacialStuff;

    using global::Harmony;

    using Verse;

    public static class SaveRecordPawnV3_Postfix
    {
        [HarmonyPostfix]
        public static void ExposeFaceData(SaveRecordPawnV3 __instance)
        {
            if (SavedPawns.Keys.Contains(__instance.id) && Scribe.mode == LoadSaveMode.Saving)
            {
                CustomPawn customPawn = SavedPawns[__instance.id] as CustomPawn;
                if (customPawn?.Pawn.TryGetComp<CompFace>() == null)
                {
                    return;
                }

                SaveRecordFaceV3 face = new SaveRecordFaceV3(customPawn.Pawn);
                face.ExposeData();
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                SaveRecordFaceV3 face = new SaveRecordFaceV3();
                face.ExposeData();
                LoadedPawns.Add(__instance, face);
            }
        }

        // Don't use CustomPawn here, must be object!!!
        public static readonly Dictionary<object, SaveRecordFaceV3> LoadedPawns = new Dictionary<object, SaveRecordFaceV3>();

        // Don't use CustomPawn here, must be object!!!
        public static readonly Dictionary<string, object> SavedPawns = new Dictionary<string, object>();
    }
}
