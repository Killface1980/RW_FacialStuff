namespace FacialStuff
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using RimWorld;

    using UnityEngine;

    using Verse;

    // [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) })]
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

        private const float YOffsetOnFace = 0.001f;

        
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

        // [HarmonyBefore("rimworld.erdelf.alien_race.main", "rimworld.jecrell.cthulhu.cults", "net.pardeike.zombieland")]
        public static bool RenderPawnInternal_Prefix(
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
            if (faceComp == null || !faceComp.OldEnough)
            {
                return true;
            }

            if (faceComp.Dontrender)
            {
                return true;
            }

            Mesh mesh = null;
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

                mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);

                List<Material> bodyBaseAt = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                for (int i = 0; i < bodyBaseAt.Count; i++)
                {
                    Material damagedMat = __instance.graphics.flasher.GetDamagedMat(bodyBaseAt[i]);
                    GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
                    loc.y += YOffsetInterval_Clothes;
                }

                if (bodyDrawType == RotDrawMode.Fresh)
                {
                    Vector3 drawLoc = rootLoc;
                    drawLoc.y += YOffset_Wounds;

                    PawnWoundDrawer woundDrawer = (PawnWoundDrawer)WoundOverlayFieldInfo?.GetValue(__instance);
                    woundDrawer?.RenderOverBody(drawLoc, mesh, quat, portrait);
                }
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
                Vector3 b = quat * __instance.BaseHeadOffsetAt(headFacing);
                Material material = __instance.graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
                Vector3 locFacialY = a + b;
                if (material != null)
                {
                    Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);

                    Mesh mesh2eyes = MeshPoolFS.HumanEyeSet[(int)faceComp.FullHeadType].MeshAt(headFacing);

                    // Experiments with head motion
                    // locFacialY += faceComp.eyemove;
                    Quaternion headQuat = quat; // * Quaternion.AngleAxis(faceComp.headWiggler.downedAngle, Vector3.up);
                    GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, headQuat, material, portrait);
                    locFacialY.y += YOffsetOnFace;
                    if (!headStump)
                    {
                        Material browMat = faceComp.BrowMatAt(headFacing);
                        Material mouthMat = faceComp.MouthMatAt(headFacing, portrait);
                        Material wrinkleMat = faceComp.WrinkleMatAt(headFacing, bodyDrawType);

                        if (wrinkleMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, headQuat, wrinkleMat, portrait);
                            locFacialY.y += YOffsetOnFace;
                        }

                        Material leftEyeMat = faceComp.EyeLeftMatAt(headFacing, portrait);
                        if (leftEyeMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(
                                mesh2eyes,
                                locFacialY + faceComp.EyeWiggler.EyeMoveL,
                                headQuat,
                                leftEyeMat,
                                portrait);
                            locFacialY.y += YOffsetOnFace;
                        }

                        Material rightEyeMat = faceComp.EyeRightMatAt(headFacing, portrait);
                        if (rightEyeMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(
                                mesh2eyes,
                                locFacialY + faceComp.EyeWiggler.EyeMoveR,
                                headQuat,
                                rightEyeMat,
                                portrait);
                            locFacialY.y += YOffsetOnFace;
                        }

                        if (browMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(mesh2eyes, locFacialY, headQuat, browMat, portrait);
                            locFacialY.y += YOffsetOnFace;
                        }

                        if (faceComp.HasEyePatchLeft)
                        {
                            Material leftBionicMat = faceComp.EyeLeftPatchMatAt(headFacing);
                            if (leftBionicMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, headQuat, leftBionicMat, portrait);
                                locFacialY.y += YOffsetOnFace;
                            }
                        }

                        if (faceComp.HasEyePatchRight)
                        {
                            Material rightBionicMat = faceComp.EyeRightPatchMatAt(headFacing);

                            if (rightBionicMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, headQuat, rightBionicMat, portrait);
                                locFacialY.y += YOffsetOnFace;
                            }
                        }

                        if (mouthMat != null)
                        {
                            // Mesh meshMouth = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            Mesh meshMouth = faceComp.MouthMeshSet.MeshAt(headFacing);

                            Vector3 drawLoc = locFacialY + (headQuat * faceComp.BaseMouthOffsetAt(bodyFacing));
                            GenDraw.DrawMeshNowOrLater(meshMouth, drawLoc, headQuat, mouthMat, portrait);
                            locFacialY.y += YOffsetOnFace;
                        }

                        // Portrait obviously ignores the y offset, thus render the beard after the body apparel
                        if (!portrait)
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

                bool showFullHair = true;

                if (!portrait || !Prefs.HatsOnlyOnMap)
                {
                    Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                    List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                    for (int j = 0; j < apparelGraphics.Count; j++)
                    {
                        if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer != ApparelLayer.Overhead)
                        {
                            continue;
                        }
                        bool showHat = true;

                        // removes the hat if the body is not shown
                        if (Controller.settings.HideHatInBed)
                        {
                            showHat = renderBody;
                        }

                        // Don't show hats indoors
                        if (!portrait && Controller.settings.HideHatWhileRoofed && pawn.Map != null
                            && faceComp.Roofed)
                        {
                            showHat = false;
                        }

                        if (!showHat)
                        {
                            continue;
                        }
                        showFullHair = false;

                        // Draw regular hair if appparel allows it
                        if (apparelGraphics[j].sourceApparel.def.apparel.tags.Contains(DrawFullhair))
                        {
                            if (bodyDrawType != RotDrawMode.Dessicated && !headStump)
                            {
                                Mesh mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                                Material mat = __instance.graphics.HairMatAt(headFacing);
                                GenDraw.DrawMeshNowOrLater(mesh4, loc2, quat, mat, portrait);
                                loc2.y += YOffsetOnFace;
                            }
                        }
                        else if (Controller.settings.MergeHair)
                        {
                            // Display the hair cut
                            HairCutPawn hairPawn = CutHairDB.GetHairCache(pawn);
                            Material hairCutMat = hairPawn.HairCutMatAt(headFacing);
                            if (hairCutMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, hairCutMat, portrait);
                                loc2.y += YOffsetOnFace;

                                // loc2.y += 0.0328125022f;
                            }
                        }

                        // Now draw the actual hat
                        Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing);
                        material2 = __instance.graphics.flasher.GetDamagedMat(material2);
                        GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material2, portrait);
                    }
                }

                // Draw regular hair if no hat worn
                if (showFullHair && bodyDrawType != RotDrawMode.Dessicated && !headStump)
                {
                    Mesh mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                    Material mat = __instance.graphics.HairMatAt(headFacing);
                    GenDraw.DrawMeshNowOrLater(mesh4, loc2, quat, mat, portrait);
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
                    GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material3, portrait);
                }
            }

            // Draw the beard, for the RenderPortrait
            if (portrait && !headStump)
            {
                Material beardMat = faceComp.BeardMatAt(headFacing);
                Material moustacheMatAt = faceComp.MoustacheMatAt(headFacing);
                Quaternion headQuat = quat; // * Quaternion.AngleAxis(faceComp.headWiggler.downedAngle, Vector3.up);
                Vector3 b = quat * __instance.BaseHeadOffsetAt(headFacing);
                Vector3 locFacialY = a + b;

                Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);

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
                    quat,
                    MeshPool.humanlikeHeadSet.MeshAt(headFacing));

                // Traverse.Create(__instance).Field("statusOverlays").GetValue<PawnHeadOverlays>().RenderStatusOverlays(bodyLoc, quat, portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.headPortraitSet.MeshAt(headFacing) : alienProps.alienRace.generalSettings.alienPartGenerator.headSet.MeshAt(headFacing));
            }

            return false;
        }
    }
}