using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.Harmony
{
    // ReSharper disable once InconsistentNaming
    [HarmonyPatch(
        typeof(PawnRenderer),
        "RenderPawnInternal",
        new[]
        {
            typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode),
            typeof(bool), typeof(bool)
        })]
    [HarmonyBefore("com.showhair.rimworld.mod")]
    public static class HarmonyPatch_PawnRenderer
    {
        private static FieldInfo pawnHeadOverlaysFieldInfo;

        private static Type _pawnRendererType;

        // Verse.Altitudes
        public const float LayerSpacing = 0.46875f;

        // private static FieldInfo PawnFieldInfo;
        private static FieldInfo _woundOverlayFieldInfo;
        private static MethodInfo DrawEquipmentMethodInfo;

        public static bool Prefix(PawnRenderer __instance,
                                  Vector3 rootLoc,
                                  Quaternion quat,
                                  bool renderBody,
                                  Rot4 bodyFacing,
                                  Rot4 headFacing,
                                  RotDrawMode bodyDrawType,
                                  bool portrait,
                                  bool headStump, ref Vector3 __state)
        {
            GetReflections();

            // Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);
            PawnGraphicSet graphics = __instance.graphics;

            Pawn pawn = graphics.pawn;

            if (!graphics.AllResolved)
            {
                graphics.ResolveAllGraphics();
            }

            ;
            bool footy = pawn.GetCompAnim(out CompBodyAnimator compAnim) && Controller.settings.UseFeet;

            PawnWoundDrawer woundDrawer = (PawnWoundDrawer)_woundOverlayFieldInfo?.GetValue(__instance);

            // if (Patches2.Plants)
            // {
            //     if (pawn.Spawned)
            //     {
            //         Plant plant = (pawn.Position + IntVec3.South).GetPlant(pawn.Map);
            //         if (plant != null && Patches2.plantMoved.Contains(plant))
            //         {
            //             rootLoc.y = plant.DrawPos.y - (Patches2.steps / 2);
            //         }
            //     }
            // }

            // Try to move the y position behind while another pawn is standing near
            // if (false)
            if (!portrait && pawn.Spawned)
            {
                RecalcRootLocY(ref rootLoc, pawn);
            }

            Vector3 footPos = rootLoc;

#if develop
            if (faceComp.IgnoreRenderer)
            {
                switch (faceComp.rotation)
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

            // No face => must be animal, simplify it
            Quaternion bodyQuat = quat;

            if (!pawn.GetCompFace(out CompFace compFace))
            {
                if (!footy)
                {
                    return true;
                }

                if (compAnim.AnimatorOpen)
                {
                    bodyFacing = MainTabWindow_Animator.BodyRot;
                    headFacing = MainTabWindow_Animator.HeadRot;
                }

                compAnim.TickDrawers(bodyFacing, graphics);
                compAnim.ApplyBodyWobble(ref rootLoc, ref footPos, ref bodyQuat);

                RenderAnimalPawn(
                                   pawn,
                                   graphics,
                                   rootLoc,
                                   bodyQuat,
                                   renderBody,
                                   bodyFacing,
                                   bodyDrawType,
                                   portrait,
                                   woundDrawer,
                                   compAnim,
                                   footPos);

                return false;
            }

            // Let vanilla do the job if no FacePawn or pawn not a teenager or any other known mod accessing the renderer
            if (compFace.IsChild || compFace.Deactivated)
            {
                return true;
            }

            if (compAnim.AnimatorOpen)
            {
                bodyFacing = MainTabWindow_Animator.BodyRot;
                headFacing = MainTabWindow_Animator.HeadRot;
            }

            compFace.TickDrawers(bodyFacing, headFacing, graphics);

            compAnim.TickDrawers(bodyFacing, graphics);

            // Use the basic quat
            Quaternion headQuat = bodyQuat;

            // Rotate head if possble and wobble around
            if (!portrait || compAnim.AnimatorOpen)
            {
                if (footy)
                {
                    compAnim.ApplyBodyWobble(ref rootLoc, ref footPos, ref bodyQuat);
                }

                // Reset the quat as it has been changed
                headQuat = bodyQuat;
                compFace.ApplyHeadRotation(renderBody, ref headQuat);
            }

            // Regular FacePawn rendering 14+ years

            // Render body
            compAnim.DrawBody(rootLoc, bodyQuat, bodyDrawType, woundDrawer, renderBody, portrait);

            Vector3 drawPos = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                a.y += Offsets.YOffset_Head;
                drawPos.y += Offsets.YOffset_Shell;
            }
            else
            {
                a.y += Offsets.YOffset_Shell;
                drawPos.y += Offsets.YOffset_Head;
            }

            if (graphics.headGraphic != null)
            {
                // Rendererd pawn faces
                Vector3 b = headQuat * compFace.BaseHeadOffsetAt(portrait);

                Vector3 locFacialY = a + b;
                Vector3 currentLoc = rootLoc + b;
                currentLoc.y += Offsets.YOffset_OnHead;

                compFace.DrawBasicHead(out bool headDrawn, bodyDrawType, portrait, headStump, ref locFacialY, headQuat);

                if (headDrawn)
                {
                    if (bodyDrawType != RotDrawMode.Dessicated && !headStump)
                    {
                        if (compFace.Props.hasWrinkles)
                        {
                            compFace.DrawWrinkles(bodyDrawType, ref locFacialY, headQuat, portrait);
                        }

                        if (compFace.Props.hasEyes)
                        {
                            compFace.DrawNaturalEyes(ref locFacialY, portrait, headQuat);

                            // the brow above
                            compFace.DrawBrows(ref locFacialY, headQuat, portrait);

                            // and now the added eye parts
                            compFace.DrawUnnaturalEyeParts(ref locFacialY, headQuat, portrait);
                        }

                        if (compFace.Props.hasMouth)
                        {
                            compFace.DrawNaturalMouth(ref locFacialY, portrait, headQuat);
                        }

                        // Portrait obviously ignores the y offset, thus render the beard after the body apparel (again)
                        if (compFace.Props.hasBeard)
                        {
                            // if (!portrait)
                            compFace.DrawBeardAndTache(ref locFacialY, portrait, headQuat);
                        }

                        // Deactivated, looks kinda crappy ATM
                        // if (pawn.Dead)
                        // {
                        // Material deadEyeMat = faceComp.DeadEyeMatAt(headFacing, bodyDrawType);
                        // if (deadEyeMat != null)
                        // {
                        // GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, headQuat, deadEyeMat, portrait);
                        // locFacialY.y += YOffsetInterval_OnFace;
                        // }

                        // }
                        // else
                    }
                }

                if (!headStump)
                {
                    compFace.DrawHairAndHeadGear(
                                                 rootLoc,
                                                 bodyDrawType,
                                                 ref currentLoc,
                                                 b,
                                                 portrait,
                                                 renderBody,
                                                 headQuat);
                }

                compFace.DrawAlienHeadAddons(portrait, headQuat, currentLoc);
            }

            compAnim.DrawApparel(bodyQuat, drawPos, portrait, renderBody);

            compFace.DrawAlienBodyAddons(bodyQuat, drawPos, portrait, renderBody);

            compAnim.DrawEquipment(drawPos, portrait);

            // if (!portrait)
            // {
            //      Traverse.Create(__instance).Method("DrawEquipment", new object[] { rootLoc }).GetValue();
            //
            //      DrawEquipmentMethodInfo?.Invoke(__instance, new object[] { drawPos });
            //
            // }

            bool showHands = compAnim.Props.bipedWithHands && Controller.settings.UseHands;
            if (showHands)
            {
                Vector3 handPos = drawPos;
                handPos.y = rootLoc.y;
                compAnim.DrawHands(bodyQuat, handPos, portrait, false);
            }

            if (footy)
            {
                compAnim?.DrawFeet(bodyQuat, footPos, portrait);
            }

            if (!portrait)
            {
                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    foreach (Apparel ap in wornApparel)
                    {
                        ap.DrawWornExtras();
                    }
                }

                Vector3 bodyLoc = rootLoc;
                bodyLoc.y += Offsets.YOffset_Status;

                PawnHeadOverlays headOverlays = (PawnHeadOverlays)pawnHeadOverlaysFieldInfo?.GetValue(__instance);
                if (headOverlays != null)
                {
                    compFace.DrawHeadOverlays(headOverlays, bodyLoc, headQuat);
                }
            }

            __state = drawPos;
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

        private static void RecalcRootLocY(ref Vector3 rootLoc, Pawn pawn)
        {
            Vector3 loc = rootLoc;
            List<Pawn> pawns = pawn.Map.mapPawns.AllPawnsSpawned
               .Where(
                      otherPawn => otherPawn != pawn &&
                                   otherPawn.DrawPos.x >= loc.x - 1 &&
                                   otherPawn.DrawPos.x <= loc.x + 1 &&
                                   otherPawn.DrawPos.z < loc.z).ToList();
            List<Pawn> leftOfPawn = pawns.Where(other => other.DrawPos.x < loc.x).ToList();

            if (!pawns.NullOrEmpty())
            {
                loc.y -= 0.075f * pawns.Count;
                loc.y += 0.005f * leftOfPawn.Count;
            }

            rootLoc = loc;

            return;
            //List<Pawn> pawns = pawn.Map.mapPawns.AllPawnsSpawned
            //    .Where(
            //        otherPawn => otherPawn != pawn && otherPawn.DrawPos.x >= loc.x - 1
            //                     && otherPawn.DrawPos.x <= loc.x + 1 && otherPawn.DrawPos.z < loc.z).ToList();
            //List<Pawn> leftOf = pawns.Where(other => other.DrawPos.x < loc.x).ToList();

            List<IntVec3> allCells = new List<IntVec3> { pawn.Position };
            List<IntVec3> allCheckedCells = new List<IntVec3>();

            ExtendSearchGrid(pawn.DrawPos.ToIntVec3(), ref allCells);

            List<Pawn> pawnList = new List<Pawn> { pawn };
            while (true)
            {
                List<IntVec3> checkedCells = new List<IntVec3>();
                List<IntVec3> cells = new List<IntVec3>(allCells);

                bool newFound = false;
                foreach (IntVec3 c in allCells.Where(x => !allCheckedCells.Contains(x)))
                {
                    List<Thing> thingList = c.GetThingList(pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if (thingList[i] is Pawn p)
                        {
                            if (pawnList.Contains(p))
                            {
                                continue;
                            }

                            pawnList.Add(p);
                            ExtendSearchGrid(p.DrawPos.ToIntVec3(), ref cells);
                            newFound = true;
                        }
                    }

                    checkedCells.Add(c);
                }

                if (!newFound)
                {
                    break;
                }

                allCells.AddRange(cells);
                allCheckedCells.AddRange(checkedCells);
            }

            if (pawnList.Count > 1)
            {
                List<Pawn> below = pawnList.Where(x => x != pawn && x.DrawPos.z <= pawn.DrawPos.z).ToList();
                float steps = 0.3f / pawnList.Count;

                List<Pawn> leftOf = below.Where(other => other.DrawPos.x < pawn.DrawPos.x).ToList();

                rootLoc.y -= steps * below.Count;
                rootLoc.y += steps * 0.2f * leftOf.Count;
            }
        }

        private static void ExtendSearchGrid(IntVec3 searchPos, ref List<IntVec3> cells)
        {
            List<IntVec3> allcells = GenAdj.CellsAdjacent8Way(searchPos, Rot4.North, new IntVec2(1, 1)).ToList();
            foreach (IntVec3 c in allcells)
            {
                if (!cells.Contains(c))
                {
                    cells.Add(c);
                }
            }
        }

        private static void RenderAnimalPawn(
            Pawn pawn,
            PawnGraphicSet graphics,
            Vector3 rootLoc,
            Quaternion quat,
            bool renderBody,
            Rot4 bodyFacing,
            RotDrawMode bodyDrawType,
            bool portrait,
            PawnWoundDrawer woundDrawer,
            CompBodyAnimator compAnim,
            Vector3 footPos)
        {
            Mesh mesh = null;
            Vector3 loc = rootLoc;
            if (renderBody)
            {
                loc.x += compAnim.BodyAnim?.offCenterX ?? 0f;
                loc.y += Offsets.YOffset_Body;
                if (bodyDrawType == RotDrawMode.Dessicated &&
                    !pawn.RaceProps.Humanlike
                 && graphics.dessicatedGraphic != null && !portrait)
                {
                    graphics.dessicatedGraphic.Draw(loc, bodyFacing, pawn);
                }
                else
                {
                    if (pawn.RaceProps.Humanlike)
                    {
                        mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
                    }
                    else
                    {
                        mesh = graphics.nakedGraphic.MeshAt(bodyFacing);
                    }

                    List<Material> list = graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Material damagedMat = graphics.flasher.GetDamagedMat(list[i]);
                        GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
                        loc.y += Offsets.YOffset_Behind;
                    }

                    if (bodyDrawType == RotDrawMode.Fresh)
                    {
                        Vector3 drawLoc = rootLoc;
                        drawLoc.y += Offsets.YOffset_Wounds;
                        woundDrawer.RenderOverBody(drawLoc, mesh, quat, portrait);
                    }
                }
            }

            Vector3 vector = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                vector.y += Offsets.YOffset_Shell;
            }
            else
            {
                vector.y += Offsets.YOffset_Head;
            }

            if (!portrait && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0
             && graphics.packGraphic != null)
            {
                UnityEngine.Graphics.DrawMesh(mesh, vector, quat, graphics.packGraphic.MatAt(bodyFacing), 0);
            }

            footPos.y = loc.y;
            compAnim.DrawHands(quat, footPos, portrait, false);
            compAnim.DrawFeet(quat, footPos, portrait);
        }

        private static void GetReflections()
        {
            if (_pawnRendererType != null)
            {
                return;
            }

            _pawnRendererType = typeof(PawnRenderer);
            DrawEquipmentMethodInfo = _pawnRendererType.GetMethod(
                                                                 "DrawEquipment",
                                                                 BindingFlags.NonPublic | BindingFlags.Instance);
            // PawnFieldInfo = PawnRendererType.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            _woundOverlayFieldInfo = _pawnRendererType.GetField(
                                                              "woundOverlays",
                                                              BindingFlags.NonPublic | BindingFlags.Instance);
            pawnHeadOverlaysFieldInfo = _pawnRendererType.GetField(
                                                                  "statusOverlays",
                                                                  BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }

    /*
    [HarmonyPatch(
        typeof(PawnRenderer),
        "RenderPawnAt",
        new[]
            {
                typeof(Vector3), typeof( RotDrawMode), typeof(bool)
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
        private static FieldInfo WoundOverlayFieldInfo;

        public static bool Prefix(
            PawnRenderer __instance, Vector3 drawLoc, RotDrawMode bodyDrawType, bool headStump)
        {
            GetReflections();
            Pawn pawn = __instance.graphics.pawn;
            if (!__instance.graphics.AllResolved)
            {
                __instance.graphics.ResolveAllGraphics();
            }
            if (pawn.GetPosture() == PawnPosture.Standing)
            {
                __instance.RenderPawnInternal(drawLoc, Quaternion.identity, true, bodyDrawType, headStump);
                if (pawn.carryTracker != null)
                {
                    Thing carriedThing = pawn.carryTracker.CarriedThing;
                    if (carriedThing != null)
                    {
                        Vector3 vector = drawLoc;
                        bool flag = false;
                        bool flip = false;
                        if (pawn.CurJob == null || !pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref vector, ref flag, ref flip))
                        {
                            if (carriedThing is Pawn || carriedThing is Corpse)
                            {
                                vector += new Vector3(0.44f, 0f, 0f);
                            }
                            else
                            {
                                vector += new Vector3(0.18f, 0f, 0.05f);
                            }
                        }
                        if (flag)
                        {
                            vector.y -= 0.0390625f;
                        }
                        else
                        {
                            vector.y += 0.0390625f;
                        }
                        carriedThing.DrawAt(vector, flip);
                    }
                }
                if (pawn.def.race.specialShadowData != null)
                {
                    if (__instance.shadowGraphic == null)
                    {
                        __instance.shadowGraphic = new Graphic_Shadow(pawn.def.race.specialShadowData);
                    }
                    __instance.shadowGraphic.Draw(drawLoc, Rot4.North, pawn, 0f);
                }
                if (__instance.graphics.nakedGraphic != null && __instance.graphics.nakedGraphic.ShadowGraphic != null)
                {
                    __instance.graphics.nakedGraphic.ShadowGraphic.Draw(drawLoc, Rot4.North, pawn, 0f);
                }
            }
            else
            {
                Rot4 rot = __instance.LayingFacing();
                Building_Bed building_Bed = pawn.CurrentBed();
                bool renderBody;
                Quaternion quat;
                Vector3 rootLoc;
                if (building_Bed != null && pawn.RaceProps.Humanlike)
                {
                    renderBody = building_Bed.def.building.bed_showSleeperBody;
                    Rot4 rotation = building_Bed.Rotation;
                    rotation.AsInt += 2;
                    quat = rotation.AsQuat;
                    AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)building_Bed.def.altitudeLayer, 15);
                    Vector3 vector2 = pawn.Position.ToVector3ShiftedWithAltitude(altLayer);
                    Vector3 vector3 = vector2;
                    vector3.y += 0.02734375f;
                    float d = -__instance.BaseHeadOffsetAt(Rot4.South).z;
                    Vector3 a = rotation.FacingCell.ToVector3();
                    rootLoc = vector2 + a * d;
                    rootLoc.y += 0.0078125f;
                }
                else
                {
                    renderBody = true;
                    rootLoc = drawLoc;
                    if (!pawn.Dead && pawn.CarriedBy == null)
                    {
                        rootLoc.y = Altitudes.AltitudeFor(AltitudeLayer.LayingPawn) + 0.0078125f;
                    }
                    if (pawn.Downed || pawn.Dead)
                    {
                        quat = Quaternion.AngleAxis(__instance.wiggler.downedAngle, Vector3.up);
                    }
                    else if (pawn.RaceProps.Humanlike)
                    {
                        quat = rot.AsQuat;
                    }
                    else
                    {
                        Rot4 rot2 = Rot4.West;
                        int num = pawn.thingIDNumber % 2;
                        if (num != 0)
                        {
                            if (num == 1)
                            {
                                rot2 = Rot4.East;
                            }
                        }
                        else
                        {
                            rot2 = Rot4.West;
                        }
                        quat = rot2.AsQuat;
                    }
                }
                __instance.RenderPawnInternal(rootLoc, quat, renderBody, rot, rot, bodyDrawType, false, headStump);
            }
            if (pawn.Spawned && !pawn.Dead)
            {
                pawn.stances.StanceTrackerDraw();
                pawn.pather.PatherDraw();
            }
            __instance.DrawDebug();
        }

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
            PawnHeadOverlaysFieldInfo = PawnRendererType.GetField(
                "statusOverlays",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
    */
}