using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
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



        // Verse.PawnRenderer
  

        //   private static readonly float[] HorMouthOffsetSex = new float[] { 0f, FS_Settings.MaleOffsetX, FS_Settings.FemaleOffsetX };
        //
        //   private static readonly float[] VerMouthOffsetSex = new float[] { 0f, FS_Settings.MaleOffsetY, FS_Settings.FemaleOffsetY };


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

        [HarmonyAfter(new string[] { "rimworld.erdelf.alien_race.main" })]
        public static bool Prefix(PawnRenderer __instance, Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {
            GetReflections();
            Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);
            //    Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            //ThingDef_AlienRace alienProps = pawn.def as ThingDef_AlienRace;
            CompFace faceComp = pawn.TryGetComp<CompFace>();

            //   if (alienProps != null)
            //       Log.Message(alienProps.alienRace.ToString());

            Mesh mesh = null;

            #region Facial Stuff

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
                    loc.y += 0.0046875f;

                    mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);

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
                                //          Mesh meshMouth = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                                Mesh meshMouth = faceComp.MouthMeshSet.MeshAt(headFacing);

                                Vector3 drawLoc = locFacialY + quat * faceComp.BaseMouthOffsetAt(bodyFacing);
                                GenDraw.DrawMeshNowOrLater(meshMouth, drawLoc, quat, mouthMat, portrait);
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
                                        GenDraw.DrawMeshNowOrLater(
                                            mesh2,
                                            locFacialY + faceComp.eyemoveL,
                                            quat,
                                            leftEyeMat,
                                            portrait);
                                        locFacialY.y += 0.00001f;
                                    }
                                }

                                if (!faceComp.hasRightEyePatch || faceComp.hasRightEyePatch && !faceComp.RightIsSolid)
                                {
                                    Material rightEyeMat = faceComp.RightEyeMatAt(headFacing, portrait);
                                    if (rightEyeMat != null)
                                    {
                                        GenDraw.DrawMeshNowOrLater(
                                            mesh2,
                                            locFacialY + faceComp.eyemoveR,
                                            quat,
                                            rightEyeMat,
                                            portrait);
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
                    loc2.y = Mathf.Max(loc2.y, locFacialY.y);

                    //         loc2 += faceComp.eyemove;

                    bool hatVisible = false;

                    if (!portrait || !Prefs.HatsOnlyOnMap)
                    {
                        Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                        List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                        apparelGraphics.Where(apr => apr.sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                            .ToList().ForEach(
                                apr =>
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
                                                    //     loc2.y += 0.0328125022f;
                                                }
                                            }

                                            Material material2 = apr.graphic.MatAt(bodyFacing, null);
                                            material2 = __instance.graphics.flasher.GetDamagedMat(material2);
                                            GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material2, portrait);
                                        }
                                    });
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
                    __instance.graphics.apparelGraphics
                        .Where(apr => apr.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell).ToList().ForEach(
                            apr =>
                                {
                                    Material material3 = apr.graphic.MatAt(bodyFacing, null);
                                    material3 = __instance.graphics.flasher.GetDamagedMat(material3);
                                    GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material3, portrait);
                                });
                }
            }

            #endregion

            //   else if (alienProps != null)
            //   {
            //       return false;
            //       if (!__instance.graphics.AllResolved)
            //       {
            //           __instance.graphics.ResolveAllGraphics();
            //       }
            //       if (renderBody)
            //       {
            //           Vector3 loc = rootLoc;
            //           loc.y += 0.00483870972f;
            //
            //           if (bodyDrawType == RotDrawMode.Rotting)
            //           {
            //               Mesh mesh2;
            //               if (__instance.graphics.dessicatedGraphic.ShouldDrawRotated)
            //               {
            //                   mesh2 = MeshPool.GridPlane(
            //                       portrait
            //                           ? alienProps.alienRace.generalSettings.alienPartGenerator.CustomPortraitDrawSize
            //                           : alienProps.alienRace.generalSettings.alienPartGenerator.CustomDrawSize);
            //               }
            //               else
            //               {
            //                   Vector2 size = portrait
            //                                      ? alienProps.alienRace.generalSettings.alienPartGenerator
            //                                          .CustomPortraitDrawSize
            //                                      : alienProps.alienRace.generalSettings.alienPartGenerator.CustomDrawSize;
            //                   if (bodyFacing.IsHorizontal)
            //                   {
            //                       size = size.Rotated();
            //                   }
            //                   if (bodyFacing == Rot4.West
            //                       && (__instance.graphics.dessicatedGraphic.data == null
            //                           || __instance.graphics.dessicatedGraphic.data.allowFlip))
            //                   {
            //                       return MeshPool.GridPlaneFlip(size);
            //                   }
            //                   return MeshPool.GridPlane(size);
            //               }
            //
            //               Quaternion rotation = (Quaternion)Traverse.Create(__instance.graphics.dessicatedGraphic)
            //                   .Method("QuatFromRot").GetValue<Quaternion>(bodyFacing);
            //               Material material = __instance.graphics.dessicatedGraphic.MatAt(bodyFacing, pawn);
            //               Graphics.DrawMesh(mesh2, loc, rotation, material, 0);
            //               if (__instance.graphics.dessicatedGraphic.ShadowGraphic != null)
            //               {
            //                   __instance.graphics.dessicatedGraphic.ShadowGraphic.DrawWorker(
            //                       loc,
            //                       bodyFacing,
            //                       pawn.def,
            //                       pawn);
            //               }
            //
            //               //__instance.graphics.dessicatedGraphic.Draw(loc, bodyFacing, pawn);
            //           }
            //           else
            //           {
            //               mesh = (portrait
            //                           ? alienProps.alienRace.generalSettings.alienPartGenerator.bodyPortraitSet.MeshAt(
            //                               bodyFacing)
            //                           : alienProps.alienRace.generalSettings.alienPartGenerator.bodySet
            //                               .MeshAt(bodyFacing));
            //           }
            //
            //           List<Material> list = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
            //           list.ForEach(
            //               m =>
            //                   {
            //                       Material damagedMat = __instance.graphics.flasher.GetDamagedMat(m);
            //                       GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
            //                       loc.y += 0.00483870972f;
            //                   });
            //           if (bodyDrawType == RotDrawMode.Fresh)
            //           {
            //               Vector3 drawLoc = rootLoc;
            //               drawLoc.y += 0.0193548389f;
            //               Traverse.Create(__instance).Field("woundOverlays").GetValue<PawnWoundDrawer>()
            //                   .RenderOverBody(drawLoc, mesh, quat, portrait);
            //           }
            //       }
            //
            //       Vector3 vector = rootLoc;
            //       Vector3 a = rootLoc;
            //       if (bodyFacing != Rot4.North)
            //       {
            //           a.y += 0.0290322583f;
            //           vector.y += 0.0241935477f;
            //       }
            //       else
            //       {
            //           a.y += 0.0241935477f;
            //           vector.y += 0.0290322583f;
            //       }
            //       if (__instance.graphics.headGraphic != null)
            //       {
            //           Vector3 b = quat * __instance.BaseHeadOffsetAt(headFacing);
            //           Material material = __instance.graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
            //           if (material != null)
            //           {
            //               Mesh mesh2 = (portrait
            //                                 ? alienProps.alienRace.generalSettings.alienPartGenerator.headPortraitSet
            //                                     .MeshAt(headFacing)
            //                                 : alienProps.alienRace.generalSettings.alienPartGenerator.headSet.MeshAt(
            //                                     headFacing));
            //               GenDraw.DrawMeshNowOrLater(mesh2, a + b, quat, material, portrait);
            //           }
            //           Vector3 loc2 = rootLoc + b;
            //           loc2.y += 0.03387097f;
            //           bool hatVisible = false;
            //           if (!portrait || !Prefs.HatsOnlyOnMap)
            //           {
            //               Mesh mesh3 = (pawn.story.crownType == CrownType.Narrow
            //                                 ? (portrait
            //                                        ? alienProps.alienRace.generalSettings.alienPartGenerator
            //                                            .hairPortraitSetNarrow
            //                                        : alienProps.alienRace.generalSettings.alienPartGenerator.hairSetNarrow
            //                                   )
            //                                 : (portrait
            //                                        ? alienProps.alienRace.generalSettings.alienPartGenerator
            //                                            .hairPortraitSetAverage
            //                                        : alienProps.alienRace.generalSettings.alienPartGenerator
            //                                            .hairSetAverage)).MeshAt(headFacing);
            //               List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
            //               apparelGraphics.Where(apr => apr.sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
            //                   .ToList().ForEach(
            //                       apr =>
            //                           {
            //                               bool flagBody = true;
            //                               if (FS_Settings.HideHatInBed)
            //                               {
            //                                   flagBody = renderBody;
            //                               }
            //                               if (flagBody)
            //                               {
            //                                   hatVisible = true;
            //                                   if (FS_Settings.MergeHair)
            //                                   {
            //                                       HaircutPawn hairPawn = SaveableCache.GetHairCache(pawn);
            //                                       Material hairCutMat = hairPawn.HairCutMatAt(headFacing);
            //                                       if (hairCutMat != null)
            //                                       {
            //                                           GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, hairCutMat, portrait);
            //                                           loc2.y += 0.0001f;
            //                                           //     loc2.y += 0.0328125022f;
            //                                       }
            //                                   }
            //                                   Material material2 = apr.graphic.MatAt(bodyFacing, null);
            //                                   material2 = __instance.graphics.flasher.GetDamagedMat(material2);
            //                                   GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material2, portrait);
            //                               }
            //                           });
            //           }
            //           if (!hatVisible && bodyDrawType != RotDrawMode.Dessicated && !headStump)
            //           {
            //               Mesh mesh4 = (pawn.story.crownType == CrownType.Narrow
            //                                 ? (portrait
            //                                        ? alienProps.alienRace.generalSettings.alienPartGenerator
            //                                            .hairPortraitSetNarrow
            //                                        : alienProps.alienRace.generalSettings.alienPartGenerator.hairSetNarrow
            //                                   )
            //                                 : (portrait
            //                                        ? alienProps.alienRace.generalSettings.alienPartGenerator
            //                                            .hairPortraitSetAverage
            //                                        : alienProps.alienRace.generalSettings.alienPartGenerator
            //                                            .hairSetAverage)).MeshAt(headFacing);
            //               Material mat = __instance.graphics.HairMatAt(headFacing);
            //               GenDraw.DrawMeshNowOrLater(mesh4, loc2, quat, mat, portrait);
            //           }
            //       }
            //       if (renderBody)
            //       {
            //           __instance.graphics.apparelGraphics
            //               .Where(apr => apr.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell).ToList().ForEach(
            //                   apr =>
            //                       {
            //                           Material material3 = apr.graphic.MatAt(bodyFacing, null);
            //                           material3 = __instance.graphics.flasher.GetDamagedMat(material3);
            //                           GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material3, portrait);
            //                       });
            //
            //           if (pawn.GetComp<AlienPartGenerator.AlienComp>().Tail != null
            //               && alienProps.alienRace.generalSettings.alienPartGenerator.CanDrawTail(pawn))
            //           {
            //               //mesh = MeshPool.plane10;
            //
            //               mesh = portrait
            //                          ? alienProps.alienRace.generalSettings.alienPartGenerator.tailPortraitMesh
            //                          : alienProps.alienRace.generalSettings.alienPartGenerator.tailMesh;
            //
            //               float MoffsetX = 0.42f;
            //               float MoffsetZ = -0.22f;
            //               float MoffsetY = -0.3f;
            //               float num = -40;
            //
            //               if (pawn.Rotation == Rot4.North)
            //               {
            //                   MoffsetX = 0f;
            //                   MoffsetY = 0.3f;
            //                   MoffsetZ = -0.55f;
            //                   mesh = portrait
            //                              ? alienProps.alienRace.generalSettings.alienPartGenerator.tailPortraitMesh
            //                              : alienProps.alienRace.generalSettings.alienPartGenerator.tailMesh;
            //                   num = 0;
            //               }
            //               else if (pawn.Rotation == Rot4.East)
            //               {
            //                   MoffsetX = -MoffsetX;
            //                   num = -num + 0; //TailAngle
            //                   mesh = portrait
            //                              ? alienProps.alienRace.generalSettings.alienPartGenerator.tailPortraitMeshFlipped
            //                              : alienProps.alienRace.generalSettings.alienPartGenerator.tailMeshFlipped;
            //               }
            //
            //               Vector3 scaleVector = new Vector3(MoffsetX, MoffsetY, MoffsetZ);
            //               scaleVector.x *= 1f + (1f - (portrait
            //                                                ? alienProps.alienRace.generalSettings.alienPartGenerator
            //                                                    .CustomPortraitDrawSize
            //                                                : alienProps.alienRace.generalSettings.alienPartGenerator
            //                                                    .CustomDrawSize).x);
            //               scaleVector.y *= 1f + (1f - (portrait
            //                                                ? alienProps.alienRace.generalSettings.alienPartGenerator
            //                                                    .CustomPortraitDrawSize
            //                                                : alienProps.alienRace.generalSettings.alienPartGenerator
            //                                                    .CustomDrawSize).y);
            //
            //               Graphics.DrawMesh(
            //                   mesh,
            //                   vector + scaleVector,
            //                   Quaternion.AngleAxis(num, Vector3.up),
            //                   pawn.GetComp<AlienPartGenerator.AlienComp>().Tail.MatAt(pawn.Rotation),
            //                   0);
            //           }
            //       }
            //   }
            else
            {
                return true;
            }

            if (!portrait)
            {
                //  Traverse.Create(__instance).Method("DrawEquipment", new object[] { rootLoc }).GetValue();
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

                ((PawnHeadOverlays)PawnHeadOverlaysFieldInfo?.GetValue(__instance))?.RenderStatusOverlays(
                    bodyLoc,
                    quat,
                    MeshPool.humanlikeHeadSet.MeshAt(headFacing));
                //    Traverse.Create(__instance).Field("statusOverlays").GetValue<PawnHeadOverlays>().RenderStatusOverlays(bodyLoc, quat, portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.headPortraitSet.MeshAt(headFacing) : alienProps.alienRace.generalSettings.alienPartGenerator.headSet.MeshAt(headFacing));
            }
            return false;
        }
    }


}