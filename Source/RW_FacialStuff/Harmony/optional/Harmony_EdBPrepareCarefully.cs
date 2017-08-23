namespace FacialStuff.Harmony.optional
{
    using System;

    using FacialStuff.Harmony.optional.PrepC;

    using global::Harmony;

    using Verse;

    [StaticConstructorOnStartup]
    class Harmony_EdBPrepareCarefully
    {
        static Harmony_EdBPrepareCarefully()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.facialstuff.prepare_carefully_patch");
            try
            {
                ((Action)(() =>
                    {
                        if (AccessTools.Method(
                                typeof(EdB.PrepareCarefully.PrepareCarefully),
                                nameof(EdB.PrepareCarefully.PrepareCarefully.Initialize)) != null)
                        {
                            harmony.Patch(
                                AccessTools.Method(typeof(EdB.PrepareCarefully.PanelBackstory), "DrawPanelContent"),
                                null,
                                new HarmonyMethod(typeof(PanelBackstoryPatch), nameof(PanelBackstoryPatch.AddFaceEditButton)));

                            harmony.Patch(
                                AccessTools.Method(
                                    typeof(EdB.PrepareCarefully.PresetLoaderVersion3),
                                    "LoadPawn",
                                    new[] { typeof(EdB.PrepareCarefully.SaveRecordPawnV3) }),
                                null,
                                new HarmonyMethod(typeof(PresetLoaderPatch), nameof(PresetLoaderPatch.LoadFace)));

                            harmony.Patch(
                                AccessTools.Method(
                                    typeof(EdB.PrepareCarefully.PresetSaver),
                                    "SaveToFile",
                                    new[] { typeof(EdB.PrepareCarefully.PrepareCarefully), typeof(string) }),
                                null,
                                null,
                                new HarmonyMethod(typeof(PresetSaverPatch), nameof(PresetSaverPatch.SavePawnRef)));

                            harmony.Patch(
                                AccessTools.Method(
                                    typeof(EdB.PrepareCarefully.ColonistSaver),
                                    "SaveToFile",
                                    new[] { typeof(EdB.PrepareCarefully.CustomPawn), typeof(string) }),
                                null,
                                null,
                                new HarmonyMethod(typeof(PresetSaverPatch), nameof(PresetSaverPatch.SavePawnRef)));

                            harmony.Patch(
                                AccessTools.Method(typeof(EdB.PrepareCarefully.SaveRecordPawnV3), "ExposeData"),
                                null,
                                new HarmonyMethod(typeof(SaveRecordPawnV3Patch), nameof(SaveRecordPawnV3Patch.ExposeFaceData)));
                        }
                    }))();
            }
            catch (TypeLoadException)
            {
            }
        }
    }
}
