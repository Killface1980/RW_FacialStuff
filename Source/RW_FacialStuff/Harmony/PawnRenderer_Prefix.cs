namespace FacialStuff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using FacialStuff.Graphics;

    using global::Harmony;

    using RimWorld;

    using UnityEngine;

    using Verse;

    // ReSharper disable once InconsistentNaming
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) })]
    [HarmonyBefore("com.showhair.rimworld.mod")]
    public static class HarmonyPatch_PawnRenderer
    {
        private const string DrawFullhair = "DrawFullHair";

        private const float YOffset_PrimaryEquipmentUnder = 0f;

        private const float YOffset_Body = 0.0046875f;

        private const float YOffsetInterval_Clothes = 0.0046875f;

        private const float YOffset_Wounds = 0.01875f;

        private const float YOffset_Shell = 0.0234375f;

        private const float YOffset_Head = 0.0281250011f;

        private const float YOffset_OnHead = 0.0328125022f;

        private const float YOffset_Status = 0.0421875f;

        private const float YOffsetOnFace = 0.0001f;


        private static Type PawnRendererType;

        // private static FieldInfo PawnFieldInfo;
        private static FieldInfo WoundOverlayFieldInfo;


        private static MethodInfo DrawEquipmentMethodInfo;


        private static FieldInfo PawnHeadOverlaysFieldInfo;

        // Verse.PawnRenderer

        // private static readonly float[] HorMouthOffsetSex = new float[] { 0f, FS_Settings.MaleOffsetX, FS_Settings.FemaleOffsetX };
        // private static readonly float[] VerMouthOffsetSex = new float[] { 0f, FS_Settings.MaleOffsetY, FS_Settings.FemaleOffsetY };
        private static void GetReflections()
        {
            if (PawnRendererType != null)
            {
                return;
            }

            PawnRendererType = typeof(PawnRenderer);

            // PawnFieldInfo = PawnRendererType.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            WoundOverlayFieldInfo = PawnRendererType.GetField(
                "woundOverlays",
                BindingFlags.NonPublic | BindingFlags.Instance);
            DrawEquipmentMethodInfo = PawnRendererType.GetMethod(
                "DrawEquipment",
                BindingFlags.NonPublic | BindingFlags.Instance);
            PawnHeadOverlaysFieldInfo = PawnRendererType.GetField(
                "statusOverlays",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static bool Prefix(
            PawnRenderer __instance,
            Vector3 rootLoc,
            Quaternion quat,
            bool renderBody,
            Rot4 bodyFacing,
            Rot4 headFacing,
            RotDrawMode bodyDrawType,
            bool portrait,
            bool headStump)
        {
            GetReflections();

            // Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);
            Pawn pawn = __instance.graphics.pawn;

            if (!__instance.graphics.AllResolved)
            {
                __instance.graphics.ResolveAllGraphics();
            }

            CompFace faceComp = pawn.TryGetComp<CompFace>();

            // Let vanilla do the job if no FacePawn or pawn not a teenager
            if (faceComp == null || faceComp.IsChild)
            {
                return true;
            }

            if (faceComp.DontRender)
            {
                return true;
            }

            Mesh bodyMesh = null;
#if develop
            if (faceComp.IgnoreRenderer)
            {
                switch (faceComp.rotationInt)
                {
                    case 0:
                        bodyFacing = Rot4.North;
                        break;

                    case 1:
                        bodyFacing = Rot4.East;
                        break;

                    case 2:
                        bodyFacing = Rot4.South;
                        break;

                    case 3:
                        bodyFacing = Rot4.West;
                        break;
                }
                headFacing = bodyFacing;
            }

#endif

            // Regular FacePawn rendering 14+ years
            if (renderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += YOffset_Body;

                bodyMesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);

                List<Material> bodyBaseAt = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                for (int i = 0; i < bodyBaseAt.Count; i++)
                {
                    Material damagedMat = __instance.graphics.flasher.GetDamagedMat(bodyBaseAt[i]);
                    GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quat, damagedMat, portrait);
                    loc.y += YOffsetInterval_Clothes;
                }

                if (bodyDrawType == RotDrawMode.Fresh)
                {
                    Vector3 drawLoc = rootLoc;
                    drawLoc.y += YOffset_Wounds;

                    PawnWoundDrawer woundDrawer = (PawnWoundDrawer)WoundOverlayFieldInfo?.GetValue(__instance);
                    woundDrawer?.RenderOverBody(drawLoc, bodyMesh, quat, portrait);
                }
            }

            Quaternion headQuat = quat;

            if (!portrait && Controller.settings.UseHeadRotator)
            {
                headFacing = faceComp.HeadRotator.Rotation(headFacing);
                headQuat *= faceComp.HeadQuat(headFacing);

                // * Quaternion.AngleAxis(faceComp.headWiggler.downedAngle, Vector3.up);
            }

            Vector3 vector = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                a.y += YOffset_Head;
                vector.y += YOffset_Shell;
            }
            else
            {
                a.y += YOffset_Shell;
                vector.y += YOffset_Head;
            }

            if (__instance.graphics.headGraphic != null)
            {
                // Rendererd pawn faces
                Vector3 b = headQuat * __instance.BaseHeadOffsetAt(headFacing);
                Material headMaterial = __instance.graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
                Vector3 locFacialY = a + b;
                if (headMaterial != null)
                {
                    Mesh headMesh = MeshPool.humanlikeHeadSet.MeshAt(headFacing);

                    Mesh eyeMesh = faceComp.EyeMeshSet.mesh.MeshAt(headFacing);
#if develop
                    Vector3 offsetEyes = faceComp.BaseEyeOffsetAt(headFacing);
#else
                    Vector3 offsetEyes = faceComp.EyeMeshSet.OffsetAt(headFacing);
#endif
                    GenDraw.DrawMeshNowOrLater(headMesh, locFacialY, headQuat, headMaterial, portrait);
                    locFacialY.y += YOffsetOnFace;
                    if (bodyDrawType != RotDrawMode.Dessicated && !headStump)
                    {
                        Material browMat = faceComp.BrowMatAt(headFacing);
                        Material mouthMat = faceComp.MouthMatAt(headFacing, portrait);
                        Material wrinkleMat = faceComp.WrinkleMatAt(headFacing, bodyDrawType);

                        if (wrinkleMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(headMesh, locFacialY, headQuat, wrinkleMat, portrait);
                            locFacialY.y += YOffsetOnFace;
                        }

                        // natural eyes
                        if (!faceComp.HasEyePatchLeft)
                        {
                            Material leftEyeMat = faceComp.EyeLeftMatAt(headFacing, portrait);
                            if (leftEyeMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(
                                    eyeMesh,
                                    locFacialY + offsetEyes + faceComp.EyeWiggler.EyeMoveL,
                                    headQuat,
                                    leftEyeMat,
                                    portrait);
                                locFacialY.y += YOffsetOnFace;
                            }
                        }

                        if (!faceComp.HasEyePatchRight)
                        {
                            Material rightEyeMat = faceComp.EyeRightMatAt(headFacing, portrait);
                            if (rightEyeMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(
                                    eyeMesh,
                                    locFacialY + offsetEyes + faceComp.EyeWiggler.EyeMoveR,
                                    headQuat,
                                    rightEyeMat,
                                    portrait);
                                locFacialY.y += YOffsetOnFace;
                            }
                        }

                        // the brow above
                        if (browMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(eyeMesh, locFacialY + offsetEyes, headQuat, browMat, portrait);
                            locFacialY.y += YOffsetOnFace;
                        }

                        // and now the added eye parts
                        if (faceComp.HasEyePatchLeft)
                        {
                            Material leftBionicMat = faceComp.EyeLeftPatchMatAt(headFacing);
                            if (leftBionicMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(
                                    headMesh,
                                    locFacialY + offsetEyes,
                                    headQuat,
                                    leftBionicMat,
                                    portrait);
                                locFacialY.y += YOffsetOnFace;
                            }
                        }

                        if (faceComp.HasEyePatchRight)
                        {
                            Material rightBionicMat = faceComp.EyeRightPatchMatAt(headFacing);

                            if (rightBionicMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(
                                    headMesh,
                                    locFacialY + offsetEyes,
                                    headQuat,
                                    rightBionicMat,
                                    portrait);
                                locFacialY.y += YOffsetOnFace;
                            }
                        }

                        if (mouthMat != null)
                        {
                            // Mesh meshMouth = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            Mesh meshMouth = faceComp.MouthMeshSet.mesh.MeshAt(headFacing);
#if develop
                            Vector3 mouthOffset = faceComp.BaseMouthOffsetAt(headFacing);
#else
                            Vector3 mouthOffset = faceComp.MouthMeshSet.OffsetAt(headFacing);
#endif

                            Vector3 drawLoc = locFacialY + (headQuat * mouthOffset);
                            GenDraw.DrawMeshNowOrLater(meshMouth, drawLoc, headQuat, mouthMat, portrait);
                            locFacialY.y += YOffsetOnFace;
                        }

                        // Portrait obviously ignores the y offset, thus render the beard after the body apparel (again)
                        if (!portrait)
                        {
                            DrawBeardAndTache(headFacing, portrait, faceComp, headMesh, locFacialY, headQuat);
                        }

                        // Deactivated, looks kinda crappy ATM
                        // if (pawn.Dead)
                        // {
                        // Material deadEyeMat = faceComp.DeadEyeMatAt(headFacing, bodyDrawType);
                        // if (deadEyeMat != null)
                        // {
                        // GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, headQuat, deadEyeMat, portrait);
                        // locFacialY.y += YOffsetOnFace;
                        // }

                        // }
                        // else
                    }
                }

                Vector3 loc2 = rootLoc + b;
                loc2.y += YOffset_OnHead;

                bool showRegularHair = true;
                Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);

                if (!headStump)
                {
                    if (!portrait || !Prefs.HatsOnlyOnMap)
                    {
                        List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                        List<ApparelGraphicRecord> headgearGraphics =
                            apparelGraphics.Where(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead).ToList();

                        bool noRenderRoofed = Controller.settings.HideHatWhileRoofed && faceComp.Roofed;
                        bool noRenderBed = Controller.settings.HideHatInBed && !renderBody;

                        // Bool - only ONE hair drawn. Checks if it needs the hair cut texture.
                        if (!headgearGraphics.NullOrEmpty())
                        {
                            showRegularHair = false;

                            // Draw regular hair if appparel or environment allows it (FS feature)
                            if (bodyDrawType != RotDrawMode.Dessicated && !headStump)
                            {
                                if (headgearGraphics.Any(x => x.sourceApparel.def.apparel.tags.Contains(DrawFullhair)) || noRenderRoofed || noRenderBed)
                                {
                                    Material mat = __instance.graphics.HairMatAt(headFacing);
                                    GenDraw.DrawMeshNowOrLater(mesh3, loc2, headQuat, mat, portrait);
                                    loc2.y += YOffsetOnFace;
                                }
                                else if (Controller.settings.MergeHair)
                                {
                                    // If not, display the hair cut
                                    HairCutPawn hairPawn = CutHairDB.GetHairCache(pawn);
                                    Material hairCutMat = hairPawn.HairCutMatAt(headFacing);
                                    if (hairCutMat != null)
                                    {
                                        GenDraw.DrawMeshNowOrLater(mesh3, loc2, headQuat, hairCutMat, portrait);
                                        loc2.y += YOffsetOnFace;
                                    }
                                }
                            }

                            if (!noRenderRoofed && !noRenderBed)
                            {
                                for (int j = 0; j < headgearGraphics.Count; j++)
                                {
                                    // Now draw the actual head gear
                                    Material material2 = headgearGraphics[j].graphic.MatAt(headFacing);
                                    material2 = __instance.graphics.flasher.GetDamagedMat(material2);
                                    GenDraw.DrawMeshNowOrLater(mesh3, loc2, headQuat, material2, portrait);
                                    loc2.y += YOffsetOnFace;
                                }
                            }
                        }
                    }

                    // Draw regular hair if no hat worn
                    if (showRegularHair && bodyDrawType != RotDrawMode.Dessicated)
                    {
                        Material mat = __instance.graphics.HairMatAt(headFacing);
                        GenDraw.DrawMeshNowOrLater(mesh3, loc2, headQuat, mat, portrait);
                    }
                }

                if (renderBody)
                {
                    for (int k = 0; k < __instance.graphics.apparelGraphics.Count; k++)
                    {
                        ApparelGraphicRecord apparelGraphicRecord = __instance.graphics.apparelGraphics[k];
                        if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer != ApparelLayer.Shell)
                        {
                            continue;
                        }

                        Material material3 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
                        material3 = __instance.graphics.flasher.GetDamagedMat(material3);
                        GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, material3, portrait);

                        // possible fix for phasing apparel
                        vector.y += YOffsetOnFace;
                    }
                }
            }

            // Draw the beard, for the RenderPortrait
            if (portrait && !headStump)
            {
                Vector3 b = headQuat * __instance.BaseHeadOffsetAt(headFacing);
                Vector3 locFacialY = a + b;

                // no rotation wanted
                Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);

                DrawBeardAndTache(headFacing, portrait, faceComp, mesh2, locFacialY, headQuat);
            }

            // ReSharper disable once InvertIf
            if (!portrait)
            {
                // Traverse.Create(__instance).Method("DrawEquipment", new object[] { rootLoc }).GetValue();
                DrawEquipmentMethodInfo?.Invoke(__instance, new object[] { rootLoc });

                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    for (int l = 0; l < wornApparel.Count; l++)
                    {
                        wornApparel[l].DrawWornExtras();
                    }
                }

                Vector3 bodyLoc = rootLoc;
                bodyLoc.y += YOffset_Status;

                ((PawnHeadOverlays)PawnHeadOverlaysFieldInfo?.GetValue(__instance))?.RenderStatusOverlays(
                    bodyLoc,
                    headQuat,
                    MeshPool.humanlikeHeadSet.MeshAt(headFacing));

                // Traverse.Create(__instance).Field("statusOverlays").GetValue<PawnHeadOverlays>().RenderStatusOverlays(bodyLoc, quat, portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.headPortraitSet.MeshAt(headFacing) : alienProps.alienRace.generalSettings.alienPartGenerator.headSet.MeshAt(headFacing));
            }

            return false;
        }

        private static void DrawBeardAndTache(
            Rot4 headFacing,
            bool portrait,
            CompFace faceComp,
            Mesh mesh2,
            Vector3 locFacialY,
            Quaternion headQuat)
        {
            Material beardMat = faceComp.BeardMatAt(headFacing);
            Material moustacheMatAt = faceComp.MoustacheMatAt(headFacing);

            if (beardMat != null)
            {
                GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, headQuat, beardMat, portrait);
                locFacialY.y += YOffsetOnFace;
            }

            if (moustacheMatAt != null)
            {
                GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, headQuat, moustacheMatAt, portrait);
                locFacialY.y += YOffsetOnFace;
            }
        }
    }
}