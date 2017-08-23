namespace FacialStuff.Harmony.optional.PrepC
{
    using System.Collections.Generic;
    using System.Linq;

    using EdB.PrepareCarefully;

    using FacialStuff;

    using global::Harmony;

    using Verse;

    public static class SaveRecordPawnV3Patch
    {
        [HarmonyPostfix]
        public static void ExposeFaceData(SaveRecordPawnV3 __instance)
        {
            if (SavedPawns.Keys.Contains(__instance.id) && Scribe.mode == LoadSaveMode.Saving)
            {
                CustomPawn customPawn = SavedPawns[__instance.id];
                if (customPawn?.Pawn.TryGetComp<CompFace>() != null)
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

        public static Dictionary<SaveRecordPawnV3, SaveRecordFaceV3> LoadedPawns = new Dictionary<SaveRecordPawnV3, SaveRecordFaceV3>();

        public static Dictionary<string, CustomPawn> SavedPawns = new Dictionary<string, CustomPawn>();
    }
}
