namespace FacialStuff.Harmony
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using FacialStuff.Components;

    using global::Harmony;

    using RimWorld;

    using UnityEngine;

    using Verse;

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


        public const float YOffset_Head = 0.02734375f;

        private const float YOffset_OnHead = 0.03125f;


        private const float YOffset_Shell = 0.0234375f;

        private const float YOffset_Status = 0.04296875f;

        public const float YOffset_Wounds = 0.01953125f;

        public const float YOffsetInterval_Clothes = 0.004f;


        private static FieldInfo PawnHeadOverlaysFieldInfo;

        private static Type PawnRendererType;

        // private static FieldInfo PawnFieldInfo;
        private static FieldInfo WoundOverlayFieldInfo;

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
            PawnGraphicSet graphics = __instance.graphics;

            Pawn pawn = graphics.pawn;

            if (!graphics.AllResolved)
            {
                graphics.ResolveAllGraphics();
            }

           bool wantsAnimation = pawn.GetCompAnim(out CompBodyAnimator compAnim);

            PawnWoundDrawer woundDrawer = (PawnWoundDrawer)WoundOverlayFieldInfo?.GetValue(__instance);
            // Let vanilla do the job if no FacePawn or pawn not a teenager or any other known mod accessing the renderer
            if (!pawn.GetCompFace(out CompFace compFace) || compFace.IsChild || compFace.Deactivated)
            {
                if (wantsAnimation)
                {
                compAnim.ApplyBodyWobble(ref rootLoc, ref quat);
                    if (compAnim.AnimatorOpen)
                    {
                        bodyFacing = headFacing = compAnim.rotation;
                    }
                    compAnim.TickDrawers(bodyFacing, graphics);
                    RenderAnimatedPawn(
                        pawn,
                        graphics,
                        rootLoc,
                        quat,
                        renderBody,
                        bodyFacing,
                        bodyDrawType,
                        portrait,
                        woundDrawer, compAnim);
                    return false;
                }
                return true;
            }
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
            if (compAnim.AnimatorOpen)
            {
                bodyFacing = headFacing = compAnim.rotation;
            }

            compFace.TickDrawers(bodyFacing, headFacing, graphics);
            compAnim.TickDrawers(bodyFacing, graphics);

            // Use the basic quat
            Quaternion headQuat = quat;

            var originZ = rootLoc.z;

            // Rotate head if possble and wobble around
            if (!portrait || compAnim.AnimatorOpen)
            {
                compAnim.ApplyBodyWobble(ref rootLoc, ref quat);
                // Reset the quat as it has been changed
                headQuat = quat;
                compFace.ApplyHeadRotation(renderBody, ref headQuat);
            }
            // Regular FacePawn rendering 14+ years

            // Render body
            compFace.DrawBody(rootLoc, quat, bodyDrawType, woundDrawer, renderBody, portrait);




            Vector3 drawPos = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                a.y += YOffset_Head;
                drawPos.y += YOffset_Shell;
            }
            else
            {
                a.y += YOffset_Shell;
                drawPos.y += YOffset_Head;
            }



            if (graphics.headGraphic != null)
            {
                // Rendererd pawn faces

                Vector3 b = headQuat * compFace.BaseHeadOffsetAt(portrait);

                Vector3 locFacialY = a + b;
                Vector3 currentLoc = rootLoc + b;
                currentLoc.y += YOffset_OnHead;

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
                        // locFacialY.y += YOffsetOnFace;
                        // }

                        // }
                        // else
                    }
                }

                if (!headStump)
                {
                    compFace.DrawHairAndHeadGear(rootLoc, bodyDrawType, ref currentLoc, b, portrait, renderBody, headQuat);
                }

                compFace.DrawAlienHeadAddons(portrait, headQuat, currentLoc);
            }

            compFace.DrawApparel(quat, drawPos, portrait, renderBody);

            compFace.DrawAlienBodyAddons(quat, drawPos, portrait, renderBody);

            // Draw the beard, for the RenderPortrait
            // if (portrait && !headStump)
            // {
            // Vector3 b = headQuat * __instance.BaseHeadOffsetAt(headFacing);
            // Vector3 locFacialY = a + b;
            // // no rotation wanted
            // Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
            // DrawBeardAndTache(headFacing, portrait, faceComp, mesh2, locFacialY, headQuat);
            // }

            if (portrait && compAnim.AnimatorOpen)
            {
                compFace.DrawEquipment(drawPos, portrait);
            }

            var footPos = drawPos;
            footPos.z = originZ;

            compAnim.DrawFeet(footPos, portrait);

            if (!portrait)
            {

                compFace.DrawEquipment(drawPos, false);

                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    foreach (Apparel ap in wornApparel)
                    {
                        ap.DrawWornExtras();
                    }
                }


                Vector3 bodyLoc = rootLoc;
                bodyLoc.y += YOffset_Status;

                PawnHeadOverlays headOverlays = (PawnHeadOverlays)PawnHeadOverlaysFieldInfo?.GetValue(__instance);
                if (headOverlays != null)
                {
                    compFace.DrawHeadOverlays(headOverlays, bodyLoc, headQuat);
                }
            }

            return false;
        }

        private static void RenderAnimatedPawn(Pawn pawn, PawnGraphicSet graphics, Vector3 rootLoc,
                                               Quaternion quat,
                                               bool renderBody,
                                               Rot4 bodyFacing,
                                               RotDrawMode bodyDrawType,
                                               bool portrait,
                                               PawnWoundDrawer woundDrawer, CompBodyAnimator compAnim)
        {
            Mesh mesh = null;
            if (renderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += 0.0078125f;
                if (bodyDrawType == RotDrawMode.Dessicated && !pawn.RaceProps.Humanlike && graphics.dessicatedGraphic != null && !portrait)
                {
                    graphics.dessicatedGraphic.Draw(loc, bodyFacing, pawn, 0f);
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
                        loc.y += 0.00390625f;
                    }
                    if (bodyDrawType == RotDrawMode.Fresh)
                    {
                        Vector3 drawLoc = rootLoc;
                        drawLoc.y += 0.01953125f;
                        woundDrawer.RenderOverBody(drawLoc, mesh, quat, portrait);
                    }
                }
            }
            Vector3 vector = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                a.y += 0.02734375f;
                vector.y += 0.0234375f;
            }
            else
            {
                a.y += 0.0234375f;
                vector.y += 0.02734375f;
            }

            if (!portrait && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0 && graphics.packGraphic != null)
            {
                Graphics.DrawMesh(mesh, vector, quat, graphics.packGraphic.MatAt(bodyFacing, null), 0);
            }

            compAnim.DrawFeet(rootLoc, portrait);
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