namespace FacialStuff.Harmony.optional
{
    using System;

    using FacialStuff.Harmony.optional.PrepC;

    using global::Harmony;

    using Verse;

    // Blatantly stolen from "Psychology"
    [StaticConstructorOnStartup]
    internal static class Harmony_EdBPrepareCarefully
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
                                new HarmonyMethod(
                                    typeof(PanelBackstory_Postfix),
                                    nameof(PanelBackstory_Postfix.AddFaceEditButton)));

                            harmony.Patch(
                                AccessTools.Method(
                                    typeof(EdB.PrepareCarefully.PresetLoaderVersion3),
                                    "LoadPawn",
                                    new[] { typeof(EdB.PrepareCarefully.SaveRecordPawnV3) }),
                                null,
                                new HarmonyMethod(typeof(PresetLoader_Postfix), nameof(PresetLoader_Postfix.LoadFace)));

                            harmony.Patch(
                                AccessTools.Method(
                                    typeof(EdB.PrepareCarefully.PresetSaver),
                                    "SaveToFile",
                                    new[] { typeof(EdB.PrepareCarefully.PrepareCarefully), typeof(string) }),
                                null,
                                null,
                                new HarmonyMethod(
                                    typeof(PresetSaver_Postfix),
                                    nameof(PresetSaver_Postfix.SavePawnRef)));

                            harmony.Patch(
                                AccessTools.Method(
                                    typeof(EdB.PrepareCarefully.ColonistSaver),
                                    "SaveToFile",
                                    new[] { typeof(EdB.PrepareCarefully.CustomPawn), typeof(string) }),
                                null,
                                null,
                                new HarmonyMethod(
                                    typeof(PresetSaver_Postfix),
                                    nameof(PresetSaver_Postfix.SavePawnRef)));

                            harmony.Patch(
                                AccessTools.Method(
                                    typeof(EdB.PrepareCarefully.SaveRecordPawnV3),
                                    nameof(EdB.PrepareCarefully.SaveRecordPawnV3.ExposeData)),
                                null,
                                new HarmonyMethod(
                                    typeof(SaveRecordPawnV3_Postfix),
                                    nameof(SaveRecordPawnV3_Postfix.ExposeFaceData)));
                        }

                        // harmony.Patch(
                        // AccessTools.Method(typeof(EdB.PrepareCarefully.DialogManageImplants), "InitializeWithPawn"),
                        // null,
                        // new HarmonyMethod(
                        // typeof(DialogManageImplantsPatch),
                        // nameof(DialogManageImplantsPatch.InitializeWithPawn)));
                        // harmony.Patch(
                        // AccessTools.Method(typeof(EdB.PrepareCarefully.DialogManageImplants), "AddImplant"),
                        // new HarmonyMethod(
                        // typeof(DialogManageImplantsPatch),
                        // nameof(DialogManageImplantsPatch.UpdatePawnForImpplants)),
                        // null);
                        // harmony.Patch(
                        // AccessTools.Method(typeof(EdB.PrepareCarefully.DialogManageImplants), "RemoveImplant"),
                        // new HarmonyMethod(
                        // typeof(DialogManageImplantsPatch),
                        // nameof(DialogManageImplantsPatch.UpdatePawnForImpplants)),
                        // null);
                    }))();
            }
            catch (TypeLoadException)
            {
            }
        }
    }
}