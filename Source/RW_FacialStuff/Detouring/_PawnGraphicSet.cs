using System.Collections.Generic;
using System.IO;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_FacialStuff.Detouring
{
    using static GraphicDatabaseHeadRecordsModded;

    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    static class ResolveAllGraphics_Postfix
    {
        private static readonly Dictionary<string, Pawn> HeadIndex = new Dictionary<string, Pawn>();

        [HarmonyPostfix]
        public static void ResolveAllGraphics(PawnGraphicSet __instance)
        {
            if (false)
            {
                ExportHeadBackToPNG();
            }

            CompFace faceComp = __instance.pawn.TryGetComp<CompFace>();

            if (faceComp == null)
            {
                return;
            }

            BuildDatabaseIfNecessary();


            if (!faceComp.optimized)
            {
                faceComp.DefineFace();
            }

            if (__instance.pawn.RaceProps.hasGenders)
            {
                Color rotColor = __instance.pawn.story.SkinColor * Headhelper.skinRottingMultiplyColor;

                if (faceComp.SetHeadType())
                {
                    if (faceComp.InitializeGraphics())
                    {
                        faceComp.HairCutGraphic = CutHairDb.Get<Graphic_Multi>(__instance.pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, __instance.pawn.story.hairColor);
                        __instance.headGraphic = GetModdedHeadNamed(__instance.pawn, true, __instance.pawn.story.SkinColor);
                        __instance.desiccatedHeadGraphic = GetModdedHeadNamed(__instance.pawn, true, rotColor);
                        __instance.desiccatedHeadStumpGraphic = GetStump(rotColor);
                        __instance.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                            __instance.pawn.story.bodyType, ShaderDatabase.CutoutSkin,
                            rotColor);
                        PortraitsCache.Clear();
                    }

                }

            }
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

            Object.Destroy(finalTexture);
        }
    }
}
