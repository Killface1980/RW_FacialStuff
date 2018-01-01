namespace FacialStuff
{
    using System.Collections.Generic;
    using System.Linq;

    using FacialStuff.Graphics;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class HumanHeadDrawer : PawnHeadDrawer
    {
        #region Protected Fields

        protected float BodyWobble;

        #endregion Protected Fields

        #region Public Methods

        public override void ApplyHeadRotation(bool renderBody, ref Quaternion headQuat)
        {
            if (this.CompFace.Props.canRotateHead && Controller.settings.UseHeadRotator)
            {
                this.headFacing = this.CompFace.HeadRotator.Rotation(this.headFacing, renderBody);
                headQuat *= this.QuatHead(this.headFacing);

                // * Quaternion.AngleAxis(faceComp.headWiggler.downedAngle, Vector3.up);
            }
        }

        public override void BaseHeadOffsetAt(ref Vector3 offset, bool portrait)
        {
            Pawn pawn = this.Pawn;
            float horHeadOffset = HorHeadOffsets[(int)pawn.story.bodyType];
            float verHeadOffset = VerHeadOffsets[(int)pawn.story.bodyType];

            if (this.CompAnimator.AnimatorOpen)
            {
                horHeadOffset += MainTabWindow_Animator.HorHeadOffset;
                verHeadOffset += MainTabWindow_Animator.VerHeadOffset;
            }

            switch (this.bodyFacing.AsInt)
            {
                case 0:
                    offset = new Vector3(0f, 0f, verHeadOffset);
                    break;

                case 1:
                    offset = new Vector3(horHeadOffset, 0f, verHeadOffset);
                    break;

                case 2:
                    offset = new Vector3(0f, 0f, verHeadOffset);
                    break;

                case 3:
                    offset = new Vector3(-horHeadOffset, 0f, verHeadOffset);
                    break;

                default:
                    Log.Error("BaseHeadOffsetAt error in " + pawn);
                    offset = Vector3.zero;
                    return;
            }

            if (!portrait)
            {
                if (this.IsMoving)
                {
                    float bam = this.BodyWobble;

                    // Let's try a slightly stiffy head
                    offset.z -= 0.25f * bam;
                }
            }
        }

        public override void DrawApparel(Quaternion quat, Vector3 vector, bool renderBody, bool portrait)
        {
            if (portrait || renderBody && !this.CompAnimator.HideShellLayer || !renderBody
                && !Controller.settings.HideShellWhileRoofed && Controller.settings.IgnoreRenderBody)
            {
                for (int index = 0; index < this.Graphics.apparelGraphics.Count; index++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = this.Graphics.apparelGraphics[index];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        Mesh bodyMesh = this.GetPawnMesh(true, portrait);
                        Material material3 = apparelGraphicRecord.graphic.MatAt(this.bodyFacing);
                        material3 = this.Graphics.flasher.GetDamagedMat(material3);
                        GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, material3, portrait);

                        // possible fix for phasing apparel
                        vector.y += Offsets.YOffsetInterval_OnFace;
                    }
                }
            }
        }

        public override void DrawBasicHead(
            Quaternion headQuat,
            RotDrawMode bodyDrawType,
            bool headStump,
            bool portrait,
            ref Vector3 locFacialY,
            out bool headDrawn)
        {
            Material headMaterial = this.Graphics.HeadMatAt(this.headFacing, bodyDrawType, headStump);
            if (headMaterial != null)
            {
                GenDraw.DrawMeshNowOrLater(
                    this.GetPawnMesh(false, portrait),
                    locFacialY,
                    headQuat,
                    headMaterial,
                    portrait);
                locFacialY.y += Offsets.YOffsetInterval_OnFace;
                headDrawn = true;
            }
            else
            {
                headDrawn = false;
            }
        }

        public override void DrawBeardAndTache(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Mesh headMesh = this.GetPawnMesh(false, portrait);

            Material beardMat = this.CompFace.FaceMaterial.BeardMatAt(this.headFacing);
            Material moustacheMatAt = this.CompFace.FaceMaterial.MoustacheMatAt(this.headFacing);

            if (beardMat != null)
            {
                GenDraw.DrawMeshNowOrLater(headMesh, locFacialY, headQuat, beardMat, portrait);
                locFacialY.y += Offsets.YOffsetInterval_OnFace;
            }

            if (moustacheMatAt != null)
            {
                GenDraw.DrawMeshNowOrLater(headMesh, locFacialY, headQuat, moustacheMatAt, portrait);
                locFacialY.y += Offsets.YOffsetInterval_OnFace;
            }
        }

        public override void DrawBrows(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Material browMat = this.CompFace.FaceMaterial.BrowMatAt(this.headFacing);
            if (browMat != null)
            {
                Mesh eyeMesh = this.CompFace.EyeMeshSet.mesh.MeshAt(this.headFacing);
                GenDraw.DrawMeshNowOrLater(
                    eyeMesh,
                    locFacialY + this.EyeOffset(this.headFacing),
                    headQuat,
                    browMat,
                    portrait);
                locFacialY.y += Offsets.YOffsetInterval_OnFace;
            }
        }

        public override void DrawHairAndHeadGear(
            Vector3 rootLoc,
            Quaternion headQuat,
            RotDrawMode bodyDrawType,
            bool renderBody,
            bool portrait,
            Vector3 b,
            ref Vector3 currentLoc)
        {
            Mesh hairMesh = this.GetPawnHairMesh(portrait);
            List<ApparelGraphicRecord> apparelGraphics = this.Graphics.apparelGraphics;
            List<ApparelGraphicRecord> headgearGraphics = null;
            if (!apparelGraphics.NullOrEmpty())
            {
                headgearGraphics = apparelGraphics
                    .Where(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead).ToList();
            }

            bool noRenderRoofed = this.CompAnimator.HideHat;
            bool noRenderBed = Controller.settings.HideHatInBed && !renderBody;
            bool noRenderGoggles = Controller.settings.FilterHats;

            if (!headgearGraphics.NullOrEmpty())
            {
                bool filterHeadgear = portrait && Prefs.HatsOnlyOnMap || !portrait && noRenderRoofed;

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

                    if (this.CompFace.Props.hasOrganicHair || noRenderBed || filterHeadgear
                        || !apCoversFullHead && !apCoversUpperHead && noRenderGoggles)
                    {
                        Material mat = this.Graphics.HairMatAt(this.headFacing);
                        GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, mat, portrait);
                        currentLoc.y += Offsets.YOffsetInterval_OnFace;
                    }
                    else if (Controller.settings.MergeHair && !apCoversFullHead)
                    {
                        // If not, display the hair cut
                        HairCutPawn hairPawn = CutHairDB.GetHairCache(this.Pawn);
                        Material hairCutMat = hairPawn.HairCutMatAt(this.headFacing);
                        if (hairCutMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, hairCutMat, portrait);
                            currentLoc.y += Offsets.YOffsetInterval_OnFace;
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
                    headgearGraphics?.Clear();
                }

                // headgearGraphics = headgearGraphics
                // .OrderBy(x => x.sourceApparel.def.apparel.bodyPartGroups.Max(y => y.listOrder)).ToList();
                if (!headgearGraphics.NullOrEmpty())
                {
                    for (int index = 0; index < headgearGraphics?.Count; index++)
                    {
                        ApparelGraphicRecord headgearGraphic = headgearGraphics[index];
                        Material headGearMat = headgearGraphic.graphic.MatAt(this.headFacing);
                        headGearMat = this.Graphics.flasher.GetDamagedMat(headGearMat);

                        Vector3 thisLoc = currentLoc;
                        if (headgearGraphic.sourceApparel.def.apparel.hatRenderedFrontOfFace)
                        {
                            thisLoc = rootLoc + b;
                            thisLoc.y += !(this.bodyFacing == Rot4.North)
                                             ? Offsets.YOffset_PostHead
                                             : Offsets.YOffset_Behind;
                        }

                        GenDraw.DrawMeshNowOrLater(hairMesh, thisLoc, headQuat, headGearMat, portrait);
                        currentLoc.y += Offsets.YOffset_Head;
                    }
                }
            }
            else
            {
                // Draw regular hair if no hat worn
                if (bodyDrawType != RotDrawMode.Dessicated)
                {
                    Material hairMat = this.Graphics.HairMatAt(this.headFacing);
                    GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, hairMat, portrait);
                }
            }
        }

        public override void DrawNaturalEyes(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Mesh eyeMesh = this.CompFace.EyeMeshSet.mesh.MeshAt(this.headFacing);

            // natural eyes
            if (this.CompFace.bodyStat.EyeLeft != PartStatus.Artificial)
            {
                Material leftEyeMat = this.CompFace.FaceMaterial.EyeLeftMatAt(this.headFacing, portrait);
                if (leftEyeMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        eyeMesh,
                        locFacialY + this.EyeOffset(this.headFacing) + this.CompFace.EyeWiggler.EyeMoveL,
                        headQuat,
                        leftEyeMat,
                        portrait);
                    locFacialY.y += Offsets.YOffsetInterval_OnFace;
                }
            }

            if (this.CompFace.bodyStat.EyeRight != PartStatus.Artificial)
            {
                Material rightEyeMat = this.CompFace.FaceMaterial.EyeRightMatAt(this.headFacing, portrait);

                if (rightEyeMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        eyeMesh,
                        locFacialY + this.EyeOffset(this.headFacing) + this.CompFace.EyeWiggler.EyeMoveR,
                        headQuat,
                        rightEyeMat,
                        portrait);
                    locFacialY.y += Offsets.YOffsetInterval_OnFace;
                }
            }
        }

        public override void DrawNaturalMouth(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Material mouthMat = this.CompFace.FaceMaterial.MouthMatAt(this.headFacing, portrait);
            if (mouthMat != null)
            {
                // Mesh meshMouth = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                Mesh meshMouth = this.CompFace.MouthMeshSet.mesh.MeshAt(this.headFacing);
#if develop
                            Vector3 mouthOffset = compFace.BaseMouthOffsetAt(headFacing);
#else
                Vector3 mouthOffset = this.CompFace.MouthMeshSet.OffsetAt(this.headFacing);
#endif

                Vector3 drawLoc = locFacialY + headQuat * mouthOffset;
                GenDraw.DrawMeshNowOrLater(meshMouth, drawLoc, headQuat, mouthMat, portrait);
                locFacialY.y += Offsets.YOffsetInterval_OnFace;
            }
        }

        public override void DrawUnnaturalEyeParts(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Mesh headMesh = this.GetPawnMesh(false, portrait);
            if (this.CompFace.bodyStat.EyeLeft == PartStatus.Artificial)
            {
                Material leftBionicMat = this.CompFace.FaceMaterial.EyeLeftPatchMatAt(this.headFacing);
                if (leftBionicMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        headMesh,
                        locFacialY + this.EyeOffset(this.headFacing),
                        headQuat,
                        leftBionicMat,
                        portrait);
                    locFacialY.y += Offsets.YOffsetInterval_OnFace;
                }
            }

            if (this.CompFace.bodyStat.EyeRight == PartStatus.Artificial)
            {
                Material rightBionicMat = this.CompFace.FaceMaterial.EyeRightPatchMatAt(this.headFacing);

                if (rightBionicMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        headMesh,
                        locFacialY + this.EyeOffset(this.headFacing),
                        headQuat,
                        rightBionicMat,
                        portrait);
                    locFacialY.y += Offsets.YOffsetInterval_OnFace;
                }
            }
        }

        public override void DrawWrinkles(
            Quaternion headQuat,
            RotDrawMode bodyDrawType,
            bool portrait,
            ref Vector3 locFacialY)
        {
            if (!Controller.settings.UseWrinkles)
            {
                return;
            }

            Material wrinkleMat = this.CompFace.FaceMaterial.WrinkleMatAt(this.headFacing, bodyDrawType);

            if (wrinkleMat == null)
            {
                return;
            }

            Mesh headMesh = this.GetPawnMesh(false, portrait);
            GenDraw.DrawMeshNowOrLater(headMesh, locFacialY, headQuat, wrinkleMat, portrait);
            locFacialY.y += Offsets.YOffsetInterval_OnFace;
        }

        public override Vector3 EyeOffset(Rot4 headFacing)
        {
#if develop
                    faceComp.BaseEyeOffsetAt(headFacing);
#else
            return this.CompFace.EyeMeshSet.OffsetAt(headFacing);
#endif
        }

        public override void Initialize()
        {
            base.Initialize();
            this.CompAnimator = this.Pawn.GetComp<CompBodyAnimator>();
        }

        public override Quaternion QuatHead(Rot4 rotation)
        {
            float num = 1f;
            Quaternion asQuat = rotation.AsQuat;
            float x = 1f * Mathf.Sin(num * (this.CompFace.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            float z = 1f * Mathf.Cos(num * (this.CompFace.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            asQuat.SetLookRotation(new Vector3(x, 0f, z), Vector3.up);

            // remove the body rotation
            if (this.IsMoving && Controller.settings.UseFeet)
            {
                if (this.bodyFacing.IsHorizontal)
                {
                    asQuat *= Quaternion.AngleAxis(
                        (this.bodyFacing == Rot4.West ? 1 : -1)
                        * this.CompAnimator.WalkCycle.BodyAngle.Evaluate(this.MovedPercent),
                        Vector3.up);
                }
                else
                {
                    asQuat *= Quaternion.AngleAxis(
                        (this.bodyFacing == Rot4.South ? 1 : -1)
                        * this.CompAnimator.WalkCycle.BodyAngleVertical.Evaluate(this.MovedPercent),
                        Vector3.up);
                }
            }

            return asQuat;
        }

        public override void Tick(Rot4 bodyFacing, Rot4 headFacing, PawnGraphicSet graphics)
        {
            base.Tick(bodyFacing, headFacing, graphics);

            CompBodyAnimator animator = this.CompAnimator;
            if (animator == null)
            {
                return;
            }

            if (animator.BodyAnimator != null)
            {
                this.IsMoving = animator.BodyAnimator.IsMoving(out this.MovedPercent);
            }

            // var curve = bodyFacing.IsHorizontal ? this.walkCycle.BodyOffsetZ : this.walkCycle.BodyOffsetVerticalZ;
            if (Controller.settings.UseFeet)
            {
                if (animator.WalkCycle != null)
                {
                    SimpleCurve curve = animator.WalkCycle.BodyOffsetZ;
                    this.BodyWobble = curve.Evaluate(this.MovedPercent);
                }
            }
            else
            {
                this.BodyWobble = 0f;
            }
        }

        #endregion Public Methods
    }
}