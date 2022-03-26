using FacialStuff.AnimatorWindows;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using static FacialStuff.Offsets;

namespace FacialStuff.Harmony
{
    // Not working, no idea what it is for.
    //[HarmonyPatch(typeof(Pawn_DrawTracker), nameof(Pawn_DrawTracker.DrawPos), MethodType.Getter)]
    public static class DrawPos_Patch
    {
        public static Vector3 offset = Vector3.zero;
        public static bool offsetEnabled = false;

        public static void Postfix(ref Vector3 __result)
        {
            if (offsetEnabled)
            {
                __result = offset;
            }
        }
    }

    // ReSharper disable once InconsistentNaming

    // private void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
    [HarmonyPatch(
        typeof(PawnRenderer),
        "RenderPawnInternal",
        new[]
        {
            typeof(Vector3),
            typeof(float),
            typeof(bool),
            typeof(Rot4),
            typeof(RotDrawMode),
            typeof(PawnRenderFlags)
        })]
    [HarmonyBefore("com.showhair.rimworld.mod")]
    public static class HarmonyPatch_PawnRenderer
    {
        // Verse.Altitudes
        public const float LayerSpacing = 0.46875f;

        private const float YOffset_Head = 0.0231660213f;
        private const float YOffset_Utility = 0.0289575271f;
        private const float YOffset_Utility_South = 0.00579150533f;
        private static readonly Type _pawnRendererType = typeof(PawnRenderer);

        private static readonly MethodInfo DrawBodyApparelMethodInfo = _pawnRendererType.GetMethod(
        "DrawBodyApparel",
        BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo DrawBodyMethodInfo = _pawnRendererType.GetMethod(
        "DrawBody",
        BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo DrawDynamicPartsMethodInfo = _pawnRendererType.GetMethod(
                "DrawDynamicParts",
        BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo DrawHeadHairMethodInfo = _pawnRendererType.GetMethod("DrawHeadHair", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo DrawBodyMethodInfoMethodInfo = _pawnRendererType.GetMethod("DrawBody", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo PawnHeadOverlaysFieldInfo = _pawnRendererType.GetField(
                    "statusOverlays",
                    BindingFlags.NonPublic | BindingFlags.Instance);

        // private static FieldInfo PawnFieldInfo;
        private static readonly FieldInfo WoundOverlayFieldInfo = _pawnRendererType.GetField(
            "woundOverlays",
            BindingFlags.NonPublic | BindingFlags.Instance);

        /*
                private static bool _logY = false;
        */

        /// <summary>
        ///     Simple Circle to Circle intersection test.
        /// </summary>
        /// <param name="x0">Circle 0 X-position</param>
        /// <param name="y0">Circle 0 Y-position</param>
        /// <param name="radius0">Circle 0 Radius</param>
        /// <param name="x1">Circle 1 X-position</param>
        /// <param name="y1">Circle 1 Y-position</param>
        /// <param name="radius1">Circle 1 Radius</param>
        /// <returns>True if a intersection occured. False if not.</returns>
        public static bool CircleIntersectionTest(float x0, float y0, float radius0, float x1, float y1, float radius1)
        {
            float radiusSum = radius0 * radius0 + radius1 * radius1;
            float distance = (x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0);

            // Intersection occured.
            if (distance <= radiusSum)
            {
                return true;
            }

            // No intersection.
            return false;
        }

        /// <summary>
        ///     Gets all Pawns inside the supplied radius. If any.
        /// </summary>
        /// <param name="center">Radius center.</param>
        /// <param name="map">Map to look in.</param>
        /// <param name="radius">The radius from the center.</param>
        /// <param name="targetPredicate">Optional predicate on each candidate.</param>
        /// <returns>Matching Pawns inside the Radius.</returns>
        public static IEnumerable<Pawn> GetPawnsInsideRadius(LocalTargetInfo center, Map map, float radius,
                                                             Predicate<Pawn> targetPredicate)
        {
            //With no predicate, just grab everything.
            if (targetPredicate == null)
            {
                targetPredicate = thing => true;
            }

            foreach (Pawn pawn in map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn))
            {
                if (CircleIntersectionTest(pawn.Position.x, pawn.Position.y, 1f, center.Cell.x, center.Cell.y, radius)
                 && targetPredicate(pawn))
                {
                    yield return pawn;
                }
            }
        }

        public static bool Prefix(PawnRenderer __instance,
                                  Vector3 rootLoc,
                                  float angle,
                                  bool renderBody,
                                  Rot4 bodyFacing,
                                  RotDrawMode bodyDrawType,
                                  PawnRenderFlags flags)
        {
            // Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);
            PawnGraphicSet graphics = __instance.graphics;
            bool portrait = flags.FlagSet(PawnRenderFlags.Portrait);
            bool drawNow = flags.FlagSet(PawnRenderFlags.DrawNow);
            bool headStump = flags.FlagSet(PawnRenderFlags.HeadStump);
            Pawn pawn = graphics.pawn;

            if (!pawn.RaceProps.Humanlike && !Controller.settings.UsePaws)
            {
                return true;
            }

            if (!graphics.AllResolved)
            {
                graphics.ResolveAllGraphics();
            }

            CompFace compFace = pawn.GetCompFace();
            bool hasFace = compFace != null;

            // Let vanilla do the job if no FacePawn or pawn not a teenager or any other known mod accessing the renderer
            if (hasFace)
            {
                if (pawn.IsChild() || pawn.GetCompAnim().Deactivated)
                {
                    return true;
                }
            }

            CompBodyAnimator compAnim = pawn.GetCompAnim();
            bool showFeet = compAnim != null && Controller.settings.UseFeet;

            // No face, no animator, return
            if (!hasFace && compAnim == null)
            {
                return true;
            }

            Rot4 headFacing = bodyFacing;

            PawnWoundDrawer woundDrawer = (PawnWoundDrawer)WoundOverlayFieldInfo?.GetValue(__instance);

            // Try to move the y position behind while another pawn is standing near
            if (compAnim != null && (!portrait && pawn.Spawned && !compAnim.IsRider))
            {
                RecalcRootLocY(ref rootLoc, (Pawn)pawn, (CompBodyAnimator)compAnim);
            }

            // Let's save the basic location for later
            Vector3 footPos = rootLoc;

            // No face => must be animal, simplify it
            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Quaternion bodyQuat = quat;
            Quaternion footQuat = bodyQuat;



            // Use the basic quat
            Quaternion headQuat = bodyQuat;

            // Rotate head if possible and wobble around
            if (!portrait || HarmonyPatchesFS.AnimatorIsOpen())
            {

                // Reset the quat as it has been changed
                compFace?.ApplyHeadRotation(renderBody, ref headQuat);
            }

            // Regular FacePawn rendering 14+ years

            // Render body
            // if (renderBody)
            // compAnim?.DrawBody(rootLoc, angle, bodyFacing, bodyDrawType, flags, out bodyMesh);

            Vector3 bodyPos = rootLoc;
            Vector3 headPos = rootLoc;
            if (bodyFacing == Rot4.North)
            {
                headPos.y += YOffset_Shell;
                bodyPos.y += YOffset_Head;
            }
            else
            {
                headPos.y += YOffset_Head;
                bodyPos.y += YOffset_Shell;
            }

            if (!portrait)
            {
                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    foreach (Apparel ap in wornApparel)
                    {
                        DrawPos_Patch.offset = rootLoc;
                        DrawPos_Patch.offsetEnabled = true;
                        ap.DrawWornExtras();
                        DrawPos_Patch.offsetEnabled = false;
                    }
                }

                Vector3 bodyLoc = rootLoc;
                bodyLoc.y += YOffset_Status;

                PawnHeadOverlays headOverlays = (PawnHeadOverlays)PawnHeadOverlaysFieldInfo?.GetValue(__instance);
                if (headOverlays != null)
                {
                    compFace?.DrawHeadOverlays(headOverlays, bodyLoc, headQuat);
                }
            }

            compAnim?.DrawApparel(bodyQuat, bodyPos, flags, renderBody);

            // TODO: What happened to invisible?
            compAnim?.DrawAlienBodyAddons(bodyQuat, bodyPos, flags, renderBody, bodyFacing, pawn.IsInvisible());
            // compAnim?.DrawAlienBodyAddons(bodyQuat, bodyPos, portrait, renderBody, bodyFacing, invisible);

            if (!portrait && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0
                && graphics.packGraphic != null)
            {
                Mesh mesh = graphics.nakedGraphic.MeshAt(bodyFacing);
                Graphics.DrawMesh(mesh, bodyPos, quat, graphics.packGraphic.MatAt(bodyFacing), 0);
            }

            // No wobble for equipment, looks funnier - nah!
            // Vector3 equipPos = rootLoc;
            // equipPos.y = drawPos.y;

            //compAnim.DrawEquipment(drawPos, portrait);

            // Original for rewrite

            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 vector = rootLoc;
            Vector3 vector2 = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                vector2.y += YOffset_Head;
                vector.y += 3f / 148f;
            }
            else
            {
                vector2.y += 3f / 148f;
                vector.y += YOffset_Head;
            }
            Vector3 utilityLoc = rootLoc;
            utilityLoc.y += ((bodyFacing == Rot4.South) ? YOffset_Utility_South : YOffset_Utility);
            Vector3 drawLoc;

            Mesh bodyMesh = null;

            if (renderBody || Controller.settings.IgnoreRenderBody)
            {
                // DrawPawnBody(rootLoc, angle, bodyFacing, bodyDrawType, flags, out bodyMesh);
                  compAnim?.DrawBody(rootLoc, angle, bodyFacing, bodyDrawType, flags, out bodyMesh);
                 //DrawBodyMethodInfo?.Invoke(__instance, new object[] { rootLoc, angle, bodyFacing, bodyDrawType, flags, bodyMesh });
                if (pawn.RaceProps.Humanlike)
                {
                    bodyMesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
                }
                else
                {
                    bodyMesh = graphics.nakedGraphic.MeshAt(bodyFacing);
                }

                //
                drawLoc = rootLoc;
                drawLoc.y += 0.009687258f;
                if (bodyDrawType == RotDrawMode.Fresh)
                {
                    woundDrawer.RenderOverBody(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), BodyTypeDef.WoundLayer.Body, bodyFacing, false);
                }
                if (renderBody && flags.FlagSet(PawnRenderFlags.Clothes))
                {
                    // DrawBodyApparel(vector, utilityLoc, bodyMesh, angle, bodyFacing, flags);
                    DrawBodyApparelMethodInfo?.Invoke(__instance, new object[] { vector, utilityLoc, bodyMesh, angle, bodyFacing, flags });
                }
                drawLoc = rootLoc;
                drawLoc.y += 0.0221660212f;
                if (bodyDrawType == RotDrawMode.Fresh)
                {
                    woundDrawer.RenderOverBody(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), BodyTypeDef.WoundLayer.Body, bodyFacing, true);
                }

                bool showHands = Controller.settings.UseHands;
                Vector3 handPos = bodyPos;

            }
            Vector3 vector3 = Vector3.zero;
            drawLoc = rootLoc;
            drawLoc.y += YOffset_Utility;
            Vector3 b = Vector3.zero;
            Vector3 headDrawLoc = Vector3.zero;
            if (graphics.headGraphic != null)
            {
                Vector3 offsetAt = !hasFace
                   ? __instance.BaseHeadOffsetAt(bodyFacing)
                   : compFace.BaseHeadOffsetAt(portrait, pawn);

                b = bodyQuat * offsetAt;
                headDrawLoc = headPos + b;

                // vector3 = quaternion * __instance.BaseHeadOffsetAt(bodyFacing);
                Material material = graphics.HeadMatAt(bodyFacing, bodyDrawType, flags.FlagSet(PawnRenderFlags.HeadStump), flags.FlagSet(PawnRenderFlags.Portrait), !flags.FlagSet(PawnRenderFlags.Cache));
                if (material != null)
                {
                    // var headDrawLoc = vector2 + vector3;
                    if (!hasFace)
                    {
                        {
                            Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                            GenDraw.DrawMeshNowOrLater(mesh2, headDrawLoc, quat, material, drawNow);
                        }
                    }
                    else
                    {
                        // GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(bodyFacing), vector2 + vector3, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
                        compFace.DrawBasicHead(out bool headDrawn, bodyDrawType, flags, headStump, headDrawLoc, headQuat);
                        if (headDrawn)
                        {
                            if (bodyDrawType != RotDrawMode.Dessicated && !headStump)
                            {
                                if (compFace.Props.hasWrinkles)
                                {
                                    Vector3 wrinkleLoc = headDrawLoc;
                                    wrinkleLoc.y += YOffset_Wrinkles;
                                    compFace.DrawWrinkles(bodyDrawType, wrinkleLoc, headQuat, flags);
                                }

                                if (compFace.Props.hasEyes)
                                {
                                    Vector3 eyeLoc = headDrawLoc;
                                    eyeLoc.y += YOffset_Eyes;

                                    compFace.DrawNaturalEyes(eyeLoc, flags, headQuat);

                                    Vector3 browLoc = headDrawLoc;
                                    browLoc.y += YOffset_Brows;
                                    // the brow above
                                    compFace.DrawBrows(browLoc, headQuat, flags);

                                    // and now the added eye parts
                                    Vector3 unnaturalEyeLoc = headDrawLoc;
                                    unnaturalEyeLoc.y += YOffset_UnnaturalEyes;
                                    compFace.DrawUnnaturalEyeParts(unnaturalEyeLoc, headQuat, flags);
                                }

                                // Portrait obviously ignores the y offset, thus render the beard after the body apparel (again)
                                if (compFace.Props.hasBeard)
                                {
                                    Vector3 beardLoc = headDrawLoc;
                                    Vector3 tacheLoc = headDrawLoc;

                                    beardLoc.y += headFacing == Rot4.North ? -YOffset_Head - YOffset_Beard : YOffset_Beard;
                                    tacheLoc.y += headFacing == Rot4.North ? -YOffset_Head - YOffset_Tache : YOffset_Tache;

                                    compFace.DrawBeardAndTache(beardLoc, tacheLoc, flags, headQuat);
                                }
                            }
                        }
                    }
                }
            }
            if (bodyDrawType == RotDrawMode.Fresh)
            {
                woundDrawer.RenderOverBody(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), BodyTypeDef.WoundLayer.Head, bodyFacing);
            }
            if (graphics.headGraphic != null)
            {
                // DrawHeadHair(rootLoc, vector3, angle, bodyFacing, bodyFacing, bodyDrawType, flags);
                // DrawHeadHairMethodInfo?.Invoke(__instance, new object[] { rootLoc, vector3, angle, bodyFacing, bodyFacing, bodyDrawType, flags });

                if (!headStump)
                {
                    if (compFace.Props.hasMouth)
                    {
                        Vector3 mouthLoc = headDrawLoc;
                        mouthLoc.y += YOffset_Mouth;
                        compFace.DrawNaturalMouth(mouthLoc, flags, headQuat);
                    }

                    Vector3 overHead = rootLoc + b;
                    overHead.y += YOffset_OnHead;

                    Vector3 hairLoc = overHead;
                    Vector3 headgearLoc = overHead;
                    Vector3 hatInFrontOfFace = rootLoc + b;

                    hairLoc.y += YOffset_HairOnHead;
                    headgearLoc.y += YOffset_GearOnHead;
                    hatInFrontOfFace.y += ((!(headFacing == Rot4.North)) ? YOffset_PostHead : YOffset_Behind);

                    compFace?.DrawHairAndHeadGear(hairLoc, headgearLoc,
                                                 bodyDrawType,
                                                 portrait,
                                                 renderBody,
                                                 headQuat, hatInFrontOfFace);

                    compFace?.DrawAlienHeadAddons(headPos, flags, headQuat, overHead);
                }
            }
            if (!flags.FlagSet(PawnRenderFlags.Portrait) && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0 && graphics.packGraphic != null)
            {
                GenDraw.DrawMeshNowOrLater(bodyMesh, Matrix4x4.TRS(vector, quaternion, Vector3.one), graphics.packGraphic.MatAt(bodyFacing), flags.FlagSet(PawnRenderFlags.DrawNow));
            }
            if (!flags.FlagSet(PawnRenderFlags.Portrait) && !flags.FlagSet(PawnRenderFlags.Cache))
            {
                DrawDynamicPartsMethodInfo?.Invoke(__instance, new object[] { rootLoc, angle, bodyFacing, flags });
            }
            return false;
        }

        private static float GetBodysizeScaling(float bodySizeFactor, Pawn pawn)
        {
            float num = bodySizeFactor;
            float num2 = 1f;
            try
            {
                int curLifeStageIndex = pawn.ageTracker.CurLifeStageIndex;
                int num3 = pawn.RaceProps.lifeStageAges.Count - 1;
                LifeStageAge val = pawn.RaceProps.lifeStageAges[curLifeStageIndex];
                if (num3 == curLifeStageIndex && curLifeStageIndex != 0 && bodySizeFactor != 1f)
                {
                    LifeStageAge val2 = pawn.RaceProps.lifeStageAges[curLifeStageIndex - 1];
                    num = val2.def.bodySizeFactor + (float)Math.Round((val.def.bodySizeFactor - val2.def.bodySizeFactor) / (val.minAge - val2.minAge) * (pawn.ageTracker.AgeBiologicalYearsFloat - val2.minAge), 2);
                }
                else if (num3 == curLifeStageIndex)
                {
                    num = bodySizeFactor;
                }
                else if (curLifeStageIndex == 0)
                {
                    LifeStageAge val3 = pawn.RaceProps.lifeStageAges[curLifeStageIndex + 1];
                    num = val.def.bodySizeFactor + (float)Math.Round((val3.def.bodySizeFactor - val.def.bodySizeFactor) / (val3.minAge - val.minAge) * (pawn.ageTracker.AgeBiologicalYearsFloat - val.minAge), 2);
                }
                else
                {
                    LifeStageAge val3 = pawn.RaceProps.lifeStageAges[curLifeStageIndex + 1];
                    num = val.def.bodySizeFactor + (float)Math.Round((val3.def.bodySizeFactor - val.def.bodySizeFactor) / (val3.minAge - val.minAge) * (pawn.ageTracker.AgeBiologicalYearsFloat - val.minAge), 2);
                }
                if (pawn.RaceProps.baseBodySize > 0f)
                {
                    num2 = pawn.RaceProps.baseBodySize;
                }
            }
            catch
            {
            }
            return num * num2;
        }

        private static void RecalcRootLocY(ref Vector3 rootLoc, Pawn pawn, CompBodyAnimator compAnimator)
        {
            Vector3 loc = rootLoc;
            CellRect viewRect = Find.CameraDriver.CurrentViewRect;
            viewRect = viewRect.ExpandedBy(1);

            List<Pawn> pawns = new();
            foreach (Pawn otherPawn in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (!viewRect.Contains(otherPawn.Position)) { continue; }
                if (otherPawn == pawn) { continue; }
                if (otherPawn.DrawPos.x < loc.x - 0.5f)
                { continue; }

                if (otherPawn.DrawPos.x > loc.x + 0.5f)
                { continue; }

                if (otherPawn.DrawPos.z >= loc.z) { continue; } // ignore above

                pawns.Add(otherPawn);
            }
            // pawns = pawn.Map.mapPawns.AllPawnsSpawned
            //                .Where(
            //                       otherPawn => _viewRect.Contains(otherPawn.Position) &&
            //                       otherPawn != pawn &&
            //                                    otherPawn.DrawPos.x >= loc.x - 1 &&
            //                                    otherPawn.DrawPos.x <= loc.x + 1 &&
            //                                    otherPawn.DrawPos.z <= loc.z).ToList();
            //  List<Pawn> leftOfPawn = pawns.Where(other => other.DrawPos.x <= loc.x).ToList();
            bool flag = compAnimator != null;
            if (!pawns.NullOrEmpty())
            {
                float pawnOffset = YOffsetPawns * pawns.Count;
                loc.y -= pawnOffset;
                if (flag)
                {
                    compAnimator.DrawOffsetY = pawnOffset;
                }
                //   loc.y -= 0.1f * leftOfPawn.Count;
            }
            else
            {
                if (flag)
                {
                    compAnimator.DrawOffsetY = 0f;
                }
            }
            rootLoc = loc;
        }
    }

    [HarmonyPatch(
        typeof(PawnRenderer),
        "RenderPawnAt",
        new[]
            {
            typeof(Vector3), typeof( Rot4), typeof(bool)
            })]
    [HarmonyBefore("com.showhair.rimworld.mod")]
    public static class HarmonyPatch_RenderPawnAt
    {
        public const float YOffset_Head = 0.02734375f;

        private const float YOffset_OnHead = 0.03125f;

        private const float YOffset_Shell = 0.0234375f;

        private const float YOffset_Status = 0.04296875f;

        public const float YOffset_Wounds = 0.01953125f;

        public const float YOffsetInterval_Clothes = 0.00390625f;

        private static FieldInfo PawnHeadOverlaysFieldInfo;

        private static Type PawnRendererType;

        // private static FieldInfo PawnFieldInfo;
        public static bool Prefix(PawnRenderer __instance, ref Vector3 drawLoc, Rot4? rotOverride, bool neverAimWeapon)
        {
            Pawn pawn = __instance.graphics.pawn;
            if (!pawn.RaceProps.Humanlike && !Controller.settings.UsePaws)
            {
                return true;
            }
            CompFace compFace = pawn.GetCompFace();
            bool hasFace = compFace != null;
            CompBodyAnimator compAnim = pawn.GetCompAnim();
            if (!hasFace && compAnim == null)
            {
                return true;
            }
            if (hasFace)
            {
                if (pawn.IsChild() || pawn.GetCompAnim().Deactivated)
                {
                    return true;
                }
            }
            // No face, no animator, return
            if (!hasFace && compAnim == null)
            {
                return true;
            }
            Rot4 bodyFacing = rotOverride ?? pawn.Rotation;
            Rot4 headFacing = bodyFacing;
            if (HarmonyPatchesFS.AnimatorIsOpen())
            {
                bodyFacing = MainTabWindow_BaseAnimator.BodyRot;
                headFacing = MainTabWindow_BaseAnimator.HeadRot;
            }
            PawnGraphicSet graphics = __instance.graphics;
            compFace?.TickDrawers(bodyFacing, headFacing, graphics);
            compAnim?.TickDrawers(bodyFacing, graphics);






            return true;
        }
    }
}