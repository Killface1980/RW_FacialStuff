using UnityEngine;

using Verse;

namespace FacialStuff
{
    using System.Collections.Generic;
    using System.Linq;

    using FacialStuff.Enums;
    using FacialStuff.Graphics;
    using FacialStuff.Harmony;

    using JetBrains.Annotations;

    using RimWorld;

    public abstract class PawnDrawer
    {

        #region Public Fields

        public const float YOffset_Behind = 0.00390625f;
        public const float YOffset_Body = 0.0078125f;
        public const float YOffset_PostHead = 0.03515625f;
        public CompFace CompFace;

        public const float YOffsetOnFace = 0.0001f;

        #endregion Public Fields

        #region Protected Constructors

        #endregion Protected Constructors

        #region Public Methods

        public virtual void ApplyHeadRotation(bool renderBody, ref Rot4 headFacing, ref Quaternion headQuat)
        {
            if (this.CompFace.Props.canRotateHead && Controller.settings.UseHeadRotator)
            {
                headFacing = this.CompFace.HeadRotator.Rotation(headFacing, renderBody);
                headQuat *= this.HeadQuat(headFacing);

                // * Quaternion.AngleAxis(faceComp.headWiggler.downedAngle, Vector3.up);
            }

        }

        public virtual List<Material> BodyBaseAt(
            PawnGraphicSet graphics,
            Rot4 bodyFacing,
            RotDrawMode bodyDrawType,
            CompFace compFace,
            MaxLayerToShow layer)
        {
            switch (layer)
            {
                case MaxLayerToShow.Naked:
                    return compFace.NakedMatsBodyBaseAt(bodyFacing, bodyDrawType);
                case MaxLayerToShow.OnSkin:
                    return compFace.UnderwearMatsBodyBaseAt(bodyFacing, bodyDrawType);
                default:
                    return graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
            }
        }

        public virtual void DrawAlienBodyAddons(Quaternion quat, Rot4 bodyFacing, Vector3 rootLoc, bool portrait, bool renderBody, PawnGraphicSet graphics)
        {
            // Just for the Aliens
        }

        public virtual void DrawAlienHeadAddons(bool portrait, Quaternion headQuat, Rot4 headFacing, Vector3 currentLoc)
        {
            // Just for the Aliens
        }
        public virtual void DrawApparel(PawnGraphicSet graphics, Quaternion quat, Rot4 bodyFacing, Vector3 vector, bool renderBody, bool portrait)
        {
            if (portrait || renderBody && !this.CompFace.HideShellLayer || !renderBody && !Controller.settings.HideShellWhileRoofed
                && Controller.settings.IgnoreRenderBody)
            {
                for (int index = 0; index < graphics.apparelGraphics.Count; index++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = graphics.apparelGraphics[index];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        var bodyMesh = this.GetPawnMesh(bodyFacing, true);
                        Material material3 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
                        material3 = graphics.flasher.GetDamagedMat(material3);
                        GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, material3, portrait);

                        // possible fix for phasing apparel
                        vector.y += YOffsetOnFace;
                    }
                }
            }
        }

        public virtual void DrawBasicHead(
                    out bool headDrawn,
            PawnGraphicSet graphics,
            Rot4 headFacing,
            RotDrawMode bodyDrawType,
            bool portrait,
            bool headStump,
            ref Vector3 locFacialY,
            Quaternion headQuat)
        {
            var headMaterial = graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
            if (headMaterial != null)
            {
                GenDraw.DrawMeshNowOrLater(
                    this.GetPawnMesh(headFacing, false),
                    locFacialY,
                    headQuat,
                    headMaterial,
                    portrait);
                    locFacialY.y += YOffsetOnFace;
                headDrawn = true;
            }
            else
            {
                headDrawn = false;
            }
        }

        public virtual void DrawBeardAndTache(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            Mesh headMesh = this.GetPawnMesh(headFacing, false);

            Material beardMat = this.CompFace.FaceMaterial.BeardMatAt(headFacing);
            Material moustacheMatAt = this.CompFace.FaceMaterial.MoustacheMatAt(headFacing);

            if (beardMat != null)
            {
                GenDraw.DrawMeshNowOrLater(
                    headMesh,
                    locFacialY,
                    headQuat,
                    beardMat,
                    portrait);
                locFacialY.y += YOffsetOnFace;
            }

            if (moustacheMatAt != null)
            {
                GenDraw.DrawMeshNowOrLater(
                    headMesh,
                    locFacialY,
                   headQuat,
                    moustacheMatAt,
                 portrait);
                locFacialY.y += YOffsetOnFace;
            }
        }

        public virtual void DrawBody(
                    PawnGraphicSet graphics,
            [CanBeNull] PawnWoundDrawer woundDrawer,
            Vector3 rootLoc,
            Quaternion quat,
            Rot4 bodyFacing,
            RotDrawMode bodyDrawType,
            bool renderBody,
            bool portrait)
        {
            if (renderBody || Controller.settings.IgnoreRenderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += YOffset_Body;

                var bodyMesh = this.GetPawnMesh(bodyFacing, true);

                List<Material> bodyBaseAt = null;
                bool flag = true;
                if (!portrait && Controller.settings.HideShellWhileRoofed)
                {
                    if (this.CompFace.InRoom)
                    {
                        MaxLayerToShow layer;
                        if (this.CompFace.InPrivateRoom)
                        {
                            layer = renderBody
                                        ? Controller.settings.LayerInPrivateRoom
                                        : Controller.settings.LayerInOwnedBed;
                        }
                        else
                        {
                            layer = renderBody ? Controller.settings.LayerInRoom : Controller.settings.LayerInBed;
                        }

                        bodyBaseAt = this.BodyBaseAt(graphics, bodyFacing, bodyDrawType, this.CompFace, layer);
                        flag = false;
                    }
                }

                if (flag)
                {
                    bodyBaseAt = graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                }

                for (int i = 0; i < bodyBaseAt.Count; i++)
                {
                    Material damagedMat = graphics.flasher.GetDamagedMat(bodyBaseAt[i]);
                    GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quat, damagedMat, portrait);
                    loc.y += HarmonyPatch_PawnRenderer.YOffsetInterval_Clothes;
                }

                if (bodyDrawType == RotDrawMode.Fresh)
                {
                    Vector3 drawLoc = rootLoc;
                    drawLoc.y += HarmonyPatch_PawnRenderer.YOffset_Wounds;

                    woundDrawer?.RenderOverBody(drawLoc, bodyMesh, quat, portrait);
                }
            }
        }
        public virtual void DrawBrows(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            Material browMat = this.CompFace.FaceMaterial.BrowMatAt(headFacing);
            if (browMat != null)
            {
                Mesh eyeMesh = this.CompFace.EyeMeshSet.mesh.MeshAt(headFacing);
                GenDraw.DrawMeshNowOrLater(
                    eyeMesh,
                    locFacialY + this.EyeOffset(headFacing),
                    headQuat,
                    browMat,
                    portrait);
                locFacialY.y += YOffsetOnFace;
            }
        }

        public virtual void DrawHairAndHeadGear(PawnGraphicSet graphics, Vector3 rootLoc, Quaternion headQuat, Rot4 bodyFacing, RotDrawMode bodyDrawType, Rot4 headFacing, bool renderBody, bool portrait, Vector3 b, ref Vector3 currentLoc)
        {
            Mesh hairMesh = this.GetPawnHairMesh(headFacing, graphics);
            List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
            List<ApparelGraphicRecord> headgearGraphics = null;
            if (!apparelGraphics.NullOrEmpty())
            {
                headgearGraphics = apparelGraphics
                    .Where(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead).ToList();
            }

            bool noRenderRoofed = this.CompFace.HideHat;
            bool noRenderBed = Controller.settings.HideHatInBed && (!renderBody);
            bool noRenderGoggles = Controller.settings.FilterHats;

            if (!headgearGraphics.NullOrEmpty())
            {
                bool filterHeadgear = (portrait && Prefs.HatsOnlyOnMap) || (!portrait && noRenderRoofed);

                // Draw regular hair if appparel or environment allows it (FS feature)
                if (bodyDrawType != RotDrawMode.Dessicated)
                {
                    // draw full or partial hair
                    bool apCoversFullHead =
                        headgearGraphics.Any(
                            x => x.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)
                                 && !x.sourceApparel.def.apparel.hatRenderedFrontOfFace);

                    bool apCoversUpperHead =
                        headgearGraphics.Any(
                            x => x.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead)
                                 && !x.sourceApparel.def.apparel.hatRenderedFrontOfFace);

                    if (noRenderBed || filterHeadgear || !apCoversFullHead && !apCoversUpperHead && noRenderGoggles)
                    {
                        Material mat = graphics.HairMatAt(headFacing);
                        GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, mat, portrait);
                        currentLoc.y += YOffsetOnFace;
                    }
                    else if (Controller.settings.MergeHair && !apCoversFullHead)
                    {
                        // If not, display the hair cut
                        HairCutPawn hairPawn = CutHairDB.GetHairCache(this.CompFace.pawn);
                        Material hairCutMat = hairPawn.HairCutMatAt(headFacing);
                        if (hairCutMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, hairCutMat, portrait);
                            currentLoc.y += YOffsetOnFace;
                        }
                    }
                }
                else
                {
                    filterHeadgear = false;
                }

                if (filterHeadgear)
                {
                    // Filter the head gear to only show non-hats, show nothing while in bed
                    if (noRenderGoggles)
                    {
                        headgearGraphics = headgearGraphics
                            .Where(
                                x => !x.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)
                                     && !x.sourceApparel.def.apparel.bodyPartGroups.Contains(
                                         BodyPartGroupDefOf.UpperHead)).ToList();
                    }
                    else
                    {
                        // Clear if nothing to show
                        headgearGraphics.Clear();
                    }
                }

                if (noRenderBed)
                {
                    headgearGraphics.Clear();
                }

                // headgearGraphics = headgearGraphics
                // .OrderBy(x => x.sourceApparel.def.apparel.bodyPartGroups.Max(y => y.listOrder)).ToList();
                if (!headgearGraphics.NullOrEmpty())
                {
                    for (int index = 0; index < headgearGraphics.Count; index++)
                    {
                        ApparelGraphicRecord headgearGraphic = headgearGraphics[index];
                        Material headGearMat = headgearGraphic.graphic.MatAt(headFacing);
                        headGearMat = graphics.flasher.GetDamagedMat(headGearMat);

                        Vector3 thisLoc = currentLoc;
                        if (headgearGraphic.sourceApparel.def.apparel.hatRenderedFrontOfFace)
                        {
                            thisLoc = rootLoc + b;
                            thisLoc.y += !(bodyFacing == Rot4.North)
                                             ? YOffset_PostHead
                                             : YOffset_Behind;
                        }

                        GenDraw.DrawMeshNowOrLater(hairMesh, thisLoc, headQuat, headGearMat, portrait);
                        currentLoc.y += HarmonyPatch_PawnRenderer.YOffset_Head;
                    }
                }
            }
            else
            {
                // Draw regular hair if no hat worn
                if (bodyDrawType != RotDrawMode.Dessicated)
                {
                    Material hairMat = graphics.HairMatAt(headFacing);
                    GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, hairMat, portrait);
                }
            }
        }

        public virtual void DrawHeadOverlays(Rot4 headFacing, PawnHeadOverlays headOverlays, Vector3 bodyLoc, Quaternion headQuat)
        {
            headOverlays?.RenderStatusOverlays(bodyLoc, headQuat, this.GetPawnMesh(headFacing, false));
        }

        public virtual void DrawNaturalEyes(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            Mesh eyeMesh = this.CompFace.EyeMeshSet.mesh.MeshAt(headFacing);

            // natural eyes
            if (!this.CompFace.HasEyePatchLeft)
            {
                Material leftEyeMat =
                    this.CompFace.FaceMaterial.EyeLeftMatAt(headFacing, portrait);
                if (leftEyeMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        eyeMesh,
                        locFacialY + this.EyeOffset(headFacing) + this.CompFace.EyeWiggler.EyeMoveL,
                       headQuat,
                        leftEyeMat,
                        portrait);
                    locFacialY.y += YOffsetOnFace;
                }
            }

            if (!this.CompFace.HasEyePatchRight)
            {
                Material rightEyeMat =
                    this.CompFace.FaceMaterial.EyeRightMatAt(headFacing, portrait);
                if (rightEyeMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        eyeMesh,
                        locFacialY + this.EyeOffset(headFacing) + this.CompFace.EyeWiggler.EyeMoveR,
                       headQuat,
                        rightEyeMat,
                        portrait);
                    locFacialY.y += YOffsetOnFace;
                }
            }
        }

        public virtual void DrawNaturalMouth(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            Material mouthMat = this.CompFace.FaceMaterial.MouthMatAt(headFacing, portrait);
            if (mouthMat != null)
            {
                // Mesh meshMouth = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                Mesh meshMouth = this.CompFace.MouthMeshSet.mesh.MeshAt(headFacing);
#if develop
                            Vector3 mouthOffset = compFace.BaseMouthOffsetAt(headFacing);
#else
                Vector3 mouthOffset = this.CompFace.MouthMeshSet.OffsetAt(headFacing);
#endif

                Vector3 drawLoc = locFacialY + headQuat * mouthOffset;
                GenDraw.DrawMeshNowOrLater(meshMouth, drawLoc, headQuat, mouthMat, portrait);
                locFacialY.y += YOffsetOnFace;
            }
        }

        public virtual void DrawUnnaturalEyeParts(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            Mesh headMesh = this.GetPawnMesh(headFacing, false);
            if (this.CompFace.HasEyePatchLeft)
            {
                Material leftBionicMat = this.CompFace.FaceMaterial.EyeLeftPatchMatAt(headFacing);
                if (leftBionicMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        headMesh,
                        locFacialY + this.EyeOffset(headFacing),
                       headQuat,
                        leftBionicMat,
                        portrait);
                    locFacialY.y += YOffsetOnFace;
                }
            }

            if (this.CompFace.HasEyePatchRight)
            {
                Material rightBionicMat = this.CompFace.FaceMaterial.EyeRightPatchMatAt(headFacing);

                if (rightBionicMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(headMesh, locFacialY + this.EyeOffset(headFacing), headQuat, rightBionicMat, portrait);
                    locFacialY.y += YOffsetOnFace;
                }
            }
        }

        public virtual void DrawWrinkles(Quaternion headQuat, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, ref Vector3 locFacialY)
        {
            Material wrinkleMat = this.CompFace.FaceMaterial.WrinkleMatAt(headFacing, bodyDrawType);
            if (wrinkleMat != null)
            {
                Mesh headMesh = this.GetPawnMesh(headFacing, false);
                GenDraw.DrawMeshNowOrLater(headMesh, locFacialY, headQuat, wrinkleMat, portrait);
                locFacialY.y += YOffsetOnFace;
            }
        }

        public virtual Vector3 EyeOffset(Rot4 headFacing)
        {

#if develop
                    faceComp.BaseEyeOffsetAt(headFacing);
#else
            return this.CompFace.EyeMeshSet.OffsetAt(headFacing);
#endif 
        }

        public virtual Mesh GetPawnHairMesh(Rot4 headFacing, PawnGraphicSet graphics)
        {
            return graphics.HairMeshSet.MeshAt(headFacing);
        }

        public virtual Mesh GetPawnMesh(Rot4 facing, bool wantsBody)
        {
            return wantsBody ? MeshPool.humanlikeBodySet.MeshAt(facing) : MeshPool.humanlikeHeadSet.MeshAt(facing);
        }

        public virtual Quaternion HeadQuat(Rot4 rotation)
        {
            float num = 1f;
            Quaternion asQuat = rotation.AsQuat;
            float x = 1f * Mathf.Sin(num * (this.CompFace.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            float z = 1f * Mathf.Cos(num * (this.CompFace.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            asQuat.SetLookRotation(new Vector3(x, 0f, z), Vector3.up);
            return asQuat;
        }
        public virtual void Initialize()
        {
        }

        #endregion Public Methods
    }
}