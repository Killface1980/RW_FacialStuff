using System;
using System.Collections.Generic;
using System.Reflection;

using Harmony;

using RimWorld;

using UnityEngine;

using Verse;

namespace FacialStuff
{
    using System.Linq;

    using FacialStuff.Detouring;

    using RW_FacialStuff;

    [StaticConstructorOnStartup]
    class HarmonyPatches
    {

        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("com.facialstuff.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message("FacialStuff: Adding Harmony Prefix to PawnRenderer.RenderPawnInternal.");

        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) })]
    public static class PawnRenderer_RenderPawnInternal
    {
        private static Type PawnRendererType;
        private static FieldInfo PawnFieldInfo;
        private static FieldInfo WoundOverlayFieldInfo;
        private static MethodInfo DrawEquipmentMethodInfo;
        private static FieldInfo PawnHeadOverlaysFieldInfo;



        private const float YOffset_PrimaryEquipmentUnder = 0f;

        private const float YOffset_Body = 0.0046875f;

        private const float YOffsetInterval_Clothes = 0.0046875f;

        private const float YOffset_Wounds = 0.01875f;

        private const float YOffset_Shell = 0.0234375f;

        private const float YOffset_Head = 0.0281250011f;

        private const float YOffset_OnHead = 0.0328125022f;


        private const float YOffset_Status = 0.0421875f;

        private const float YOffsetOnFace = 0.00001f;

        // Verse.PawnRenderer


        // private static readonly float[] HorMouthOffsetSex = new float[] { 0f, FS_Settings.MaleOffsetX, FS_Settings.FemaleOffsetX };
        // private static readonly float[] VerMouthOffsetSex = new float[] { 0f, FS_Settings.MaleOffsetY, FS_Settings.FemaleOffsetY };
        private static void GetReflections()
        {
            if (PawnRendererType == null)
            {
                PawnRendererType = typeof(PawnRenderer);
                PawnFieldInfo = PawnRendererType.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
                WoundOverlayFieldInfo = PawnRendererType.GetField("woundOverlays", BindingFlags.NonPublic | BindingFlags.Instance);
                DrawEquipmentMethodInfo = PawnRendererType.GetMethod("DrawEquipment", BindingFlags.NonPublic | BindingFlags.Instance);
                PawnHeadOverlaysFieldInfo = PawnRendererType.GetField("statusOverlays", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        [HarmonyAfter("rimworld.erdelf.alien_race.main")]
        public static bool Prefix(PawnRenderer __instance, Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {
            GetReflections();
            Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);

            // Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            CompFace faceComp = pawn.TryGetComp<CompFace>();


            Mesh mesh = null;

            if (faceComp != null)
            {
#if develop
                if (faceComp.ignoreRenderer)
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
                if (!__instance.graphics.AllResolved)
                {
                    __instance.graphics.ResolveAllGraphics();
                }

                if (renderBody)
                {
                    Vector3 loc = rootLoc;
                    loc.y += YOffset_Body;

                    mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);

                    List<Material> list = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Material damagedMat = __instance.graphics.flasher.GetDamagedMat(list[i]);
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

                        Mesh mesh2eyes = MeshPoolFs.HumanEyeSet[(int)faceComp.fullHeadType].MeshAt(headFacing);

                        // Experiments with head motion
                        // locFacialY += faceComp.eyemove;
                        //// quat *= Quaternion.AngleAxis(faceComp.headWiggler.downedAngle, Vector3.up);
                        GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, material, portrait);
                        locFacialY.y += 0.001f;
                        if (!headStump)
                        {
                            Material beardMat = faceComp.BeardMatAt(headFacing);
                            Material browMat = faceComp.BrowMatAt(headFacing);
                            Material mouthMat = faceComp.MouthMatAt(headFacing, bodyDrawType);
                            Material wrinkleMat = faceComp.WrinkleMatAt(headFacing, bodyDrawType);

                            if (wrinkleMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, wrinkleMat, portrait);
                                locFacialY.y += YOffsetOnFace;
                            }

                            if (mouthMat != null)
                            {
                                // Mesh meshMouth = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                                Mesh meshMouth = faceComp.MouthMeshSet.MeshAt(headFacing);

                                Vector3 drawLoc = locFacialY + quat * faceComp.BaseMouthOffsetAt(bodyFacing);
                                GenDraw.DrawMeshNowOrLater(meshMouth, drawLoc, quat, mouthMat, portrait);
                                locFacialY.y += YOffsetOnFace;
                            }

                            if (beardMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, beardMat, portrait);
                                locFacialY.y += YOffsetOnFace;
                            }

                            if (pawn.Dead)
                            {
                                Material deadEyeMat = faceComp.DeadEyeMatAt(headFacing, bodyDrawType);
                                if (deadEyeMat != null)
                                {
                                    GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, deadEyeMat, portrait);
                                    locFacialY.y += YOffsetOnFace;
                                }
                            }
                            else
                            {
                                Material leftEyeMat = faceComp.LeftEyeMatAt(headFacing, portrait);
                                if (leftEyeMat != null)
                                {
                                    GenDraw.DrawMeshNowOrLater(
                                        mesh2eyes,
                                        locFacialY + faceComp.eyeWiggler.EyemoveL,
                                        quat,
                                        leftEyeMat,
                                        portrait);
                                    locFacialY.y += YOffsetOnFace;
                                }

                                Material rightEyeMat = faceComp.RightEyeMatAt(headFacing, portrait);
                                if (rightEyeMat != null)
                                {
                                    GenDraw.DrawMeshNowOrLater(
                                        mesh2eyes,
                                        locFacialY + faceComp.eyeWiggler.EyemoveR,
                                        quat,
                                        rightEyeMat,
                                        portrait);
                                    locFacialY.y += YOffsetOnFace;
                                }
                            }

                            if (browMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(mesh2eyes, locFacialY, quat, browMat, portrait);
                                locFacialY.y += YOffsetOnFace;
                            }

                            if (faceComp.HasLeftEyePatch)
                            {
                                Material leftBionicMat = faceComp.LeftEyePatchMatAt(headFacing);
                                if (leftBionicMat != null)
                                {
                                    GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, leftBionicMat, portrait);
                                    locFacialY.y += YOffsetOnFace;
                                }
                            }

                            if (faceComp.HasRightEyePatch)
                            {
                                Material rightBionicMat = faceComp.RightEyePatchMatAt(headFacing);

                                if (rightBionicMat != null)
                                {
                                    GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, rightBionicMat, portrait);
                                    locFacialY.y += YOffsetOnFace;
                                }
                            }
                        }
                    }

                    Vector3 loc2 = rootLoc + b;
                    loc2.y += YOffset_OnHead;
                    loc2.y = Mathf.Max(loc2.y, locFacialY.y);

                    // loc2 += faceComp.eyemove;
                    bool hatVisible = false;

                    if (!portrait || !Prefs.HatsOnlyOnMap)
                    {
                        Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                        List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                        for (int j = 0; j < apparelGraphics.Count; j++)
                        {
                            if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                            {
                                bool flagBody = true;
                                if (FS_Settings.HideHatInBed)
                                {
                                    flagBody = renderBody;
                                }

                                if (flagBody)
                                {
                                    hatVisible = true;
                                    if (FS_Settings.MergeHair)
                                    {
                                        HaircutPawn hairPawn = SaveableCache.GetHairCache(pawn);
                                        Material hairCutMat = hairPawn.HairCutMatAt(headFacing);
                                        if (hairCutMat != null)
                                        {
                                            GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, hairCutMat, portrait);
                                            loc2.y += 0.0001f;

                                            // loc2.y += 0.0328125022f;
                                        }
                                    }

                                    Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing);
                                    material2 = __instance.graphics.flasher.GetDamagedMat(material2);
                                    GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material2, portrait);
                                }
                            }
                        }
                    }

                    if (!hatVisible && bodyDrawType != RotDrawMode.Dessicated && !headStump)
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
                        if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                        {
                            Material material3 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                            material3 = __instance.graphics.flasher.GetDamagedMat(material3);
                            GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material3, portrait);
                        }
                    }
                }
            }
            else
            {
                return true;
            }

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