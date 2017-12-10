namespace FacialStuff.Harmony
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using FacialStuff.Enums;
    using FacialStuff.Graphics;
    using FacialStuff.Harmony.Optional;

    using global::Harmony;

    using JetBrains.Annotations;

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

        public const float YOffsetInterval_Clothes = 0.00390625f;


        private static MethodInfo DrawEquipmentMethodInfo;

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

            // Let vanilla do the job if no FacePawn or pawn not a teenager or any other known mod accessing the renderer
            if (!pawn.GetCompFace(out CompFace compFace) || compFace.IsChild || compFace.Deactivated)
            {
                return true;
            }

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
            PawnWoundDrawer woundDrawer = (PawnWoundDrawer)WoundOverlayFieldInfo?.GetValue(__instance);
            compFace.DrawBody(graphics, rootLoc, quat, bodyFacing, bodyDrawType, woundDrawer, renderBody, portrait);

            Quaternion headQuat = quat;

            if (!portrait)
            {
                compFace.ApplyHeadRotation(renderBody, ref headFacing, ref headQuat);
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



            if (graphics.headGraphic != null)
            {
                // Rendererd pawn faces
                Vector3 b = headQuat * __instance.BaseHeadOffsetAt(headFacing);

                Vector3 locFacialY = a + b;
                Vector3 currentLoc = rootLoc + b;
                currentLoc.y += YOffset_OnHead;

                compFace.DrawBasicHead(out bool headDrawn, graphics, headFacing, bodyDrawType, portrait, headStump, ref locFacialY, headQuat);

                if (headDrawn)
                {

                    if (bodyDrawType != RotDrawMode.Dessicated && !headStump)
                    {

                        if (compFace.Props.hasWrinkles)
                        {
                            compFace.DrawWrinkles(bodyDrawType, ref locFacialY, headFacing, headQuat, portrait);
                        }

                        if (compFace.Props.hasEyes)
                        {
                            compFace.DrawNaturalEyes(ref locFacialY, portrait, headFacing, headQuat);

                            // the brow above
                            compFace.DrawBrows(ref locFacialY, headFacing, headQuat, portrait);

                            // and now the added eye parts

                            compFace.DrawUnnaturalEyeParts(ref locFacialY, headQuat, headFacing, portrait);
                        }

                        if (compFace.Props.hasMouth)
                        {
                            compFace.DrawNaturalMouth(ref locFacialY, portrait, headFacing, headQuat);
                        }

                        // Portrait obviously ignores the y offset, thus render the beard after the body apparel (again)
                        if (compFace.Props.hasBeard)
                        {
                            // if (!portrait)
                            compFace.DrawBeardAndTache(ref locFacialY, portrait, headFacing, headQuat);
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
                    compFace.DrawHairAndHeadGear(rootLoc, bodyFacing, bodyDrawType, ref currentLoc, b, headFacing, graphics, portrait, renderBody, headQuat);
                }

                compFace.DrawAlienHeadAddons(portrait, headQuat, headFacing, currentLoc);
            }

            compFace.DrawApparel(quat, bodyFacing, vector, portrait, renderBody, graphics);

            compFace.DrawAlienBodyAddons(quat, bodyFacing, vector, portrait, renderBody, graphics);

            // Draw the beard, for the RenderPortrait
            // if (portrait && !headStump)
            // {
            // Vector3 b = headQuat * __instance.BaseHeadOffsetAt(headFacing);
            // Vector3 locFacialY = a + b;
            // // no rotation wanted
            // Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
            // DrawBeardAndTache(headFacing, portrait, faceComp, mesh2, locFacialY, headQuat);
            // }

            // ReSharper disable once InvertIf
            if (!portrait)
            {
                // Traverse.Create(__instance).Method("DrawEquipment", new object[] { rootLoc }).GetValue();
                DrawEquipmentMethodInfo?.Invoke(__instance, new object[] { rootLoc });

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
                    compFace.DrawHeadOverlays(headFacing, headOverlays, bodyLoc, headQuat);
                }
            }

            return false;
        }



        public static Mesh GetPawnMesh(Rot4 facing, bool wantsBody)
        {
            return wantsBody ? MeshPool.humanlikeBodySet.MeshAt(facing) : MeshPool.humanlikeHeadSet.MeshAt(facing);
        }



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
    }
}