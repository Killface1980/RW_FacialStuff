using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_FacialStuff.Detouring
{
    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    static class ResolveAllGraphics_Postfix
    {
        public static Dictionary<string, Pawn> HeadIndex = new Dictionary<string, Pawn>();

        [HarmonyPostfix]
        public static void ResolveAllGraphics(PawnGraphicSet __instance)
        {
            if (false)
            {
                ExportHeadBackToPNG();
            }

            CompFace faceComp = __instance.pawn.TryGetComp<CompFace>();
            if (faceComp == null)
                return;

            GraphicDatabaseHeadRecordsModded.BuildDatabaseIfNecessary();


            if (!faceComp.optimized)
            {
                faceComp.DefineFace(__instance.pawn);
            }

            //if (pawnSave.optimized)
            //    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, pawnSave.headGraphicIndex);

            __instance.nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(__instance.pawn.story.bodyType,
                ShaderDatabase.CutoutSkin, __instance.pawn.story.SkinColor);
            __instance.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                __instance.pawn.story.bodyType, ShaderDatabase.CutoutSkin,
                PawnGraphicSet.RottingColor*__instance.pawn.story.SkinColor);
            __instance.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(
                "Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);
            __instance.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
            __instance.headStumpGraphic = GraphicDatabaseHeadRecords.GetStump(__instance.pawn.story.SkinColor);
            __instance.desiccatedHeadStumpGraphic = GraphicDatabaseHeadRecords.GetStump(PawnGraphicSet.RottingColor);
            __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.hairDef.texPath,
                ShaderDatabase.Cutout, Vector2.one, __instance.pawn.story.hairColor);
            __instance.desiccatedHeadGraphic =
                GraphicDatabaseHeadRecords.GetHeadNamed(__instance.pawn.story.HeadGraphicPath,
                    __instance.pawn.story.SkinColor*PawnGraphicSet.RottingColor);
            __instance.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
            __instance.ResolveApparelGraphics();
            PortraitsCache.Clear();

            if (!faceComp.sessionOptimized)
            {
                // Build the empty head index once to be used for the blank heads
                {
                    if (HeadIndex.Count == 0)
                        for (int i = 0; i < 1024; i++)
                        {
                            HeadIndex.Add(i.ToString("0000"), null);
                        }
                }
                // Get the first free index and go on
                foreach (KeyValuePair<string, Pawn> pair in HeadIndex)
                {
                    if (pair.Value == null)
                    {
                        string index = pair.Key;
                        HeadIndex.Remove(pair.Key);
                        HeadIndex.Add(index, __instance.pawn);

                        faceComp.headGraphicIndex = "Heads/Blank/" + pair.Key;
                        GraphicDatabaseHeadRecordsModded.headsModded.Add(
                            new GraphicDatabaseHeadRecordsModded.HeadGraphicRecordModded(__instance.pawn));
                        break;
                    }
                }

                //pawnSave.headGraphicIndex = "Heads/Blank/" + GraphicDatabaseHeadRecordsModded.headIndex.ToString("0000");
                //GraphicDatabaseHeadRecordsModded.headsModded.Add(new GraphicDatabaseHeadRecordsModded.HeadGraphicRecordModded(pawn));
                //GraphicDatabaseHeadRecordsModded.headIndex += 1;
            }

            __instance.headGraphic = __instance.pawn.RaceProps.hasGenders
                ? GraphicDatabaseHeadRecordsModded.ModifiedVanillaHead(__instance.pawn, __instance.pawn.story.SkinColor,
                    __instance.hairGraphic)
                : GraphicDatabaseHeadRecords.GetHeadNamed(__instance.pawn.story.HeadGraphicPath,
                    __instance.pawn.story.SkinColor);

            //           typeof(Pawn_StoryTracker).GetField("skinColor", BindingFlags.Instance | BindingFlags.Public).SetValue(pawn.story, Color.cyan);


/*
                                                    for (int j = 0; j < apparelGraphics.Count; j++)
                                                    {
                                                        if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                                                        {
                                                            // INTERESTING                
                                                            pawn.Drawer.renderer.graphics.headGraphic = hairGraphic;
                                                        }
                                                    }
                                                    */
        }

        public static void ExportHeadBackToPNG()
        {

            Texture2D finalTexture = new Texture2D(128, 128);

            int startX = 0;
            int startY = 0;

            for (int x = startX; x < finalTexture.width; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {
                    finalTexture.SetPixel(x, y, Color.clear);
                }
            }

            finalTexture.Apply();

            for (int i = 0; i < 1024; i++)
            {
                byte[] bytes = finalTexture.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/Textures/Heads/Blank/" + i.ToString("0000") + "_front.png", bytes);
                File.WriteAllBytes("Mods/RW_FacialStuff/Textures/Heads/Blank/" + i.ToString("0000") + "_side.png", bytes);
                File.WriteAllBytes("Mods/RW_FacialStuff/Textures/Heads/Blank/" + i.ToString("0000") + "_back.png", bytes);

            }

            Object.DestroyImmediate(finalTexture);
        }
    }
}
