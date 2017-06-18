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
            Pawn pawn = __instance.pawn;
            CompFace faceComp = pawn.TryGetComp<CompFace>();

            if (faceComp == null)
            {
                return;
            }

            BuildDatabaseIfNecessary();


            if (!faceComp.optimized)
            {
                faceComp.DefineFace();
            }

            if (pawn.RaceProps.hasGenders)
            {
                Color rotColor = pawn.story.SkinColor * Headhelper.skinRottingMultiplyColor;

                if (faceComp.SetHeadType())
                {
                    if (faceComp.InitializeGraphics())
                    {
                        faceComp.HairCutGraphic = CutHairDb.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                        __instance.headGraphic = GetModdedHeadNamed(pawn, pawn.story.SkinColor);
                        __instance.desiccatedHeadGraphic = GetModdedHeadNamed(pawn, rotColor);
                        __instance.desiccatedHeadStumpGraphic = GetStump(rotColor);
                        __instance.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                            pawn.story.bodyType, ShaderDatabase.CutoutSkin,
                            rotColor);
                        PortraitsCache.Clear();
                    }

                }

            }
        }

    }
}
