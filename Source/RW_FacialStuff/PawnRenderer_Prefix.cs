using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = HarmonyInstance.Create("com.facialstuff.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message("FacialStuff: Adding Harmony Prefix to PawnRenderer.RenderPawnInternal.");
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) })]
    public static class PawnRenderer_RenderPawnInternal
    {
        private static Type PawnRendererType = null;
        private static FieldInfo PawnFieldInfo;
        private static FieldInfo WoundOverlayFieldInfo;
        private static MethodInfo DrawEquipmentMethodInfo;
        private static FieldInfo PawnHeadOverlaysFieldInfo;

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

        public static bool Prefix(PawnRenderer __instance, Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {
            GetReflections();

            if (!__instance.graphics.AllResolved)
            {
                __instance.graphics.ResolveAllGraphics();
            }

            Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);

            // Skip to default renderer if pawn has no CompFace
            CompFace faceComp = pawn.TryGetComp<CompFace>();
            if (faceComp == null)
            {
                return true;
            }



            Mesh mesh = null;

            if (pawn != null && renderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += 0.0046875f;
                if (bodyDrawType == RotDrawMode.Dessicated && !pawn.RaceProps.Humanlike && __instance.graphics.dessicatedGraphic != null && !portrait)
                {
                    __instance.graphics.dessicatedGraphic.Draw(loc, bodyFacing, pawn);
                }
                else
                {
                    if (pawn.RaceProps.Humanlike)
                    {
                        mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
                    }
                    else
                    {
                        mesh = __instance.graphics.nakedGraphic.MeshAt(bodyFacing);
                    }
                    List<Material> list = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Material damagedMat = __instance.graphics.flasher.GetDamagedMat(list[i]);
                        GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
                        loc.y += 0.0046875f;
                    }
                    if (bodyDrawType == RotDrawMode.Fresh)
                    {
                        Vector3 drawLoc = rootLoc;
                        drawLoc.y += 0.01875f;

                        PawnWoundDrawer woundDrawer = (PawnWoundDrawer)WoundOverlayFieldInfo?.GetValue(__instance);
                        woundDrawer?.RenderOverBody(drawLoc, mesh, quat, portrait);
                    }
                }
            }
            Vector3 vector = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                a.y += 0.0281250011f;
                vector.y += 0.0234375f;
            }
            else
            {
                a.y += 0.0234375f;
                vector.y += 0.0281250011f;
            }
            if (__instance.graphics.headGraphic != null)
            {
                // Rendererd pawn faces
                Vector3 b = quat * __instance.BaseHeadOffsetAt(headFacing);
                Material material = __instance.graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
                Vector3 locFacialY = a + b;
                if (material != null)
                {
           //         locFacialY += faceComp.eyemove;
                    Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                    GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, material, portrait);
                    locFacialY.y += 0.001f;
                    if (!headStump)
                    {
                        Material beardMat = faceComp.BeardMatAt(headFacing);
                        Material browMat = faceComp.BrowMatAt(headFacing);
                        Material mouthMat = faceComp.MouthMatAt(headFacing, bodyDrawType);
                        Material wrinkleMat = faceComp.WrinkleMatAt(headFacing);

                        if (wrinkleMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, wrinkleMat, portrait);
                            locFacialY.y += 0.00001f;
                        }

                        if (mouthMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, mouthMat, portrait);
                            locFacialY.y += 0.00001f;
                        }

                        if (beardMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, beardMat, portrait);
                            locFacialY.y += 0.00001f;
                        }


                        if (pawn.Dead)
                        {
                            Material deadEyeMat = faceComp.DeadEyeMatAt(headFacing, bodyDrawType);
                            if (deadEyeMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, deadEyeMat, portrait);
                                locFacialY.y += 0.00001f;
                            }
                        }
                        else
                        {
                            if (!faceComp.hasLeftEyePatch || faceComp.hasLeftEyePatch && !faceComp.LeftIsSolid)
                            {
                                Material leftEyeMat = faceComp.LeftEyeMatAt(headFacing, portrait);
                                if (leftEyeMat != null)
                                {
                                    GenDraw.DrawMeshNowOrLater(mesh2, locFacialY + faceComp.eyemove, quat, leftEyeMat, portrait);
                                    locFacialY.y += 0.00001f;
                                }
                            }

                            if (!faceComp.hasRightEyePatch || faceComp.hasRightEyePatch && !faceComp.RightIsSolid)
                            {
                                Material rightEyeMat = faceComp.RightEyeMatAt(headFacing, portrait);
                                if (rightEyeMat != null)
                                {
                                    GenDraw.DrawMeshNowOrLater(mesh2, locFacialY + faceComp.eyemove, quat, rightEyeMat, portrait);
                                    locFacialY.y += 0.00001f;
                                }
                            }
                        }

                        if (browMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, browMat, portrait);
                            locFacialY.y += 0.00001f;
                        }

                        if (faceComp.hasLeftEyePatch)
                        {
                            Material leftBionicMat = faceComp.LeftEyePatchMatAt(headFacing);
                            if (leftBionicMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, leftBionicMat, portrait);
                                locFacialY.y += 0.00001f;
                            }
                        }

                        if (faceComp.hasRightEyePatch)
                        {
                            Material rightBionicMat = faceComp.RightEyePatchMatAt(headFacing);

                            if (rightBionicMat != null)
                            {
                                GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, quat, rightBionicMat, portrait);
                                locFacialY.y += 0.00001f;
                            }
                        }
                    }
                }

                Vector3 loc2 = rootLoc + b;
                loc2.y += 0.0328125022f;

       //         loc2 += faceComp.eyemove;

                bool hatVisible = false;

                if (!portrait || !Prefs.HatsOnlyOnMap)
                {
                    Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                    List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                    for (int j = 0; j < apparelGraphics.Count; j++)
                    {
                        if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                        {
                            bool flag = true;
                            if (FS_Settings.HideHatInBed)
                            {
                                flag = renderBody;
                            }

                            if (flag)
                            {
                                hatVisible = true;

                                Material hairCutMat = faceComp.HairCutMatAt(headFacing);
                                if (hairCutMat != null)
                                {
                                    GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, hairCutMat, portrait);
                                    loc2.y += 0.0001f;
                               //     loc2.y += 0.0328125022f;
                                }

                                Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
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
                        break;
                    }
                }
            }

            if (!portrait && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0)
            {
                Graphics.DrawMesh(mesh, vector, quat, __instance.graphics.packGraphic.MatAt(pawn.Rotation, null), 0);
            }

            if (!portrait)
            {
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
                bodyLoc.y += 0.0421875f;

                ((PawnHeadOverlays)PawnHeadOverlaysFieldInfo?.GetValue(__instance))?.
                    RenderStatusOverlays(bodyLoc, quat, MeshPool.humanlikeHeadSet.MeshAt(headFacing));
            }
            return false;
        }
    }


}