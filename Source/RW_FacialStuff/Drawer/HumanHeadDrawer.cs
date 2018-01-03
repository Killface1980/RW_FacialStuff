using System.Collections.Generic;
using System.Linq;
using FacialStuff.GraphicsFS;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff
{
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
                this.HeadFacing = this.CompFace.HeadRotator.Rotation(this.HeadFacing, renderBody);
                headQuat   *= this.QuatHead(this.HeadFacing);

                // * Quaternion.AngleAxis(faceComp.headWiggler.downedAngle, Vector3.up);
            }
        }

        public override void BaseHeadOffsetAt(ref Vector3 offset, bool portrait)
        {
            Pawn  pawn          = this.Pawn;
            float horHeadOffset = HorHeadOffsets[(int) pawn.story.bodyType];
            float verHeadOffset = VerHeadOffsets[(int) pawn.story.bodyType];

            CompBodyAnimator animator = this.CompAnimator;
            if (animator != null && animator.AnimatorOpen)
            {
                horHeadOffset += MainTabWindow_Animator.HorHeadOffset;
                verHeadOffset += MainTabWindow_Animator.VerHeadOffset;
            }

            switch (this.BodyFacing.AsInt)
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


        public override void DrawBasicHead(
        Quaternion  headQuat,
        RotDrawMode bodyDrawType,
        bool        headStump,
        bool        portrait,
        ref Vector3 locFacialY,
        out bool    headDrawn)
        {
            Material headMaterial = this.Graphics.HeadMatAt(this.HeadFacing, bodyDrawType, headStump);
            if (headMaterial != null)
            {
                GenDraw.DrawMeshNowOrLater(this.GetPawnMesh(false, portrait),
                                           locFacialY,
                                           headQuat,
                                           headMaterial,
                                           portrait);
                locFacialY.y += Offsets.YOffsetInterval_OnFace;
                headDrawn    =  true;
            }
            else
            {
                headDrawn = false;
            }
        }

        public override void DrawBeardAndTache(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Mesh headMesh = this.GetPawnMesh(false, portrait);

            Material beardMat       = this.CompFace.FaceMaterial.BeardMatAt(this.HeadFacing);
            Material moustacheMatAt = this.CompFace.FaceMaterial.MoustacheMatAt(this.HeadFacing);

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
            Material browMat = this.CompFace.FaceMaterial.BrowMatAt(this.HeadFacing);
            if (browMat != null)
            {
                Mesh eyeMesh = this.CompFace.EyeMeshSet.Mesh.MeshAt(this.HeadFacing);
                GenDraw.DrawMeshNowOrLater(
                                           eyeMesh,
                                           locFacialY + this.EyeOffset(this.HeadFacing),
                                           headQuat,
                                           browMat,
                                           portrait);
                locFacialY.y += Offsets.YOffsetInterval_OnFace;
            }
        }

        public override void DrawHairAndHeadGear(
        Vector3     rootLoc,
        Quaternion  headQuat,
        RotDrawMode bodyDrawType,
        bool        renderBody,
        bool        portrait,
        Vector3     b,
        ref Vector3 currentLoc)
        {
            Mesh                       hairMesh         = this.GetPawnHairMesh(portrait);
            List<ApparelGraphicRecord> apparelGraphics  = this.Graphics.apparelGraphics;
            List<ApparelGraphicRecord> headgearGraphics = null;
            if (!apparelGraphics.NullOrEmpty())
            {
                headgearGraphics = apparelGraphics
                                  .Where(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead).ToList();
            }

            CompBodyAnimator animator        = this.CompAnimator;
            bool             noRenderRoofed  = animator != null                 && animator.HideHat;
            bool             noRenderBed     = Controller.settings.HideHatInBed && !renderBody;
            bool             noRenderGoggles = Controller.settings.FilterHats;

            if (!headgearGraphics.NullOrEmpty())
            {
                bool filterHeadgear = portrait && Prefs.HatsOnlyOnMap || !portrait && noRenderRoofed;

                // Draw regular hair if appparel or environment allows it (FS feature)
                if (bodyDrawType != RotDrawMode.Dessicated)
                {
                    // draw full or partial hair
                    bool apCoversFullHead =
                    headgearGraphics.Any(
                                         x => x.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf
                                                                                                 .FullHead)
                                           && !x.sourceApparel.def.apparel.hatRenderedFrontOfFace);

                    bool apCoversUpperHead =
                    headgearGraphics.Any(
                                         x => x.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf
                                                                                                 .UpperHead)
                                           && !x.sourceApparel.def.apparel.hatRenderedFrontOfFace);

                    if (this.CompFace.Props.hasOrganicHair || noRenderBed || filterHeadgear
                     || !apCoversFullHead && !apCoversUpperHead && noRenderGoggles)
                    {
                        Material mat = this.Graphics.HairMatAt(this.HeadFacing);
                        GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, mat, portrait);
                        currentLoc.y += Offsets.YOffsetInterval_OnFace;
                    }
                    else if (Controller.settings.MergeHair && !apCoversFullHead)
                    {
                        // If not, display the hair cut
                        HairCutPawn hairPawn   = CutHairDB.GetHairCache(this.Pawn);
                        Material    hairCutMat = hairPawn.HairCutMatAt(this.HeadFacing);
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
                                                 x =>
                                                     !x.sourceApparel.def.apparel.bodyPartGroups
                                                       .Contains(BodyPartGroupDefOf.FullHead)
                                                  && !x.sourceApparel.def.apparel.bodyPartGroups.Contains(
                                                                                                          BodyPartGroupDefOf
                                                                                                         .UpperHead))
                                          .ToList();
                    }
                    else
                    {
                        // Clear if nothing to show
                        headgearGraphics?.Clear();
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
                        Material             headGearMat     = headgearGraphic.graphic.MatAt(this.HeadFacing);
                        headGearMat                          = this.Graphics.flasher.GetDamagedMat(headGearMat);

                        Vector3 thisLoc = currentLoc;
                        if (headgearGraphic.sourceApparel.def.apparel.hatRenderedFrontOfFace)
                        {
                            thisLoc   =  rootLoc + b;
                            thisLoc.y += !(this.BodyFacing == Rot4.North)
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
                    Material hairMat = this.Graphics.HairMatAt(this.HeadFacing);
                    GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, hairMat, portrait);
                }
            }
        }

        public override void DrawNaturalEyes(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Mesh eyeMesh = this.CompFace.EyeMeshSet.Mesh.MeshAt(this.HeadFacing);

            // natural eyes
            if (this.CompFace.BodyStat.EyeLeft != PartStatus.Artificial)
            {
                Material leftEyeMat = this.CompFace.FaceMaterial.EyeLeftMatAt(this.HeadFacing, portrait);
                if (leftEyeMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                                               eyeMesh,
                                               locFacialY + this.EyeOffset(this.HeadFacing) + this.CompFace.EyeWiggler.EyeMoveL,
                                               headQuat,
                                               leftEyeMat,
                                               portrait);
                    locFacialY.y += Offsets.YOffsetInterval_OnFace;
                }
            }

            if (this.CompFace.BodyStat.EyeRight != PartStatus.Artificial)
            {
                Material rightEyeMat = this.CompFace.FaceMaterial.EyeRightMatAt(this.HeadFacing, portrait);

                if (rightEyeMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                                               eyeMesh,
                                               locFacialY + this.EyeOffset(this.HeadFacing) + this.CompFace.EyeWiggler.EyeMoveR,
                                               headQuat,
                                               rightEyeMat,
                                               portrait);
                    locFacialY.y += Offsets.YOffsetInterval_OnFace;
                }
            }
        }

        public override void DrawNaturalMouth(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Material mouthMat = this.CompFace.FaceMaterial.MouthMatAt(this.HeadFacing, portrait);
            if (mouthMat != null)
            {
                // Mesh meshMouth = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                Mesh meshMouth = this.CompFace.MouthMeshSet.Mesh.MeshAt(this.HeadFacing);
#if develop
                            Vector3 mouthOffset = compFace.BaseMouthOffsetAt(headFacing);
#else
                Vector3 mouthOffset = this.CompFace.MouthMeshSet.OffsetAt(this.HeadFacing);
#endif

                Vector3 drawLoc = locFacialY + headQuat * mouthOffset;
                GenDraw.DrawMeshNowOrLater(meshMouth, drawLoc, headQuat, mouthMat, portrait);
                locFacialY.y += Offsets.YOffsetInterval_OnFace;
            }
        }

        public override void DrawUnnaturalEyeParts(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Mesh headMesh = this.GetPawnMesh(false, portrait);
            if (this.CompFace.BodyStat.EyeLeft == PartStatus.Artificial)
            {
                Material leftBionicMat = this.CompFace.FaceMaterial.EyeLeftPatchMatAt(this.HeadFacing);
                if (leftBionicMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                                               headMesh,
                                               locFacialY + this.EyeOffset(this.HeadFacing),
                                               headQuat,
                                               leftBionicMat,
                                               portrait);
                    locFacialY.y += Offsets.YOffsetInterval_OnFace;
                }
            }

            if (this.CompFace.BodyStat.EyeRight == PartStatus.Artificial)
            {
                Material rightBionicMat = this.CompFace.FaceMaterial.EyeRightPatchMatAt(this.HeadFacing);

                if (rightBionicMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                                               headMesh,
                                               locFacialY + this.EyeOffset(this.HeadFacing),
                                               headQuat,
                                               rightBionicMat,
                                               portrait);
                    locFacialY.y += Offsets.YOffsetInterval_OnFace;
                }
            }
        }

        public override void DrawWrinkles(
        Quaternion  headQuat,
        RotDrawMode bodyDrawType,
        bool        portrait,
        ref Vector3 locFacialY)
        {
            if (!Controller.settings.UseWrinkles)
            {
                return;
            }

            Material wrinkleMat = this.CompFace.FaceMaterial.WrinkleMatAt(this.HeadFacing, bodyDrawType);

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
            float      num    = 1f;
            Quaternion asQuat = rotation.AsQuat;
            float      x      = 1f * Mathf.Sin(num * (this.CompFace.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            float      z      = 1f * Mathf.Cos(num * (this.CompFace.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            asQuat.SetLookRotation(new Vector3(x, 0f, z), Vector3.up);

            // remove the body rotation
            if (this.IsMoving && Controller.settings.UseFeet)
            {
                if (this.BodyFacing.IsHorizontal)
                {
                    asQuat *= Quaternion.AngleAxis(
                                                   (this.BodyFacing == Rot4.West ? 1 : -1)
                                                 * this.CompAnimator?.WalkCycle?.BodyAngle.Evaluate(this.MovedPercent) ?? 0f,
                                                   Vector3.up);
                }
                else
                {
                    asQuat *= Quaternion.AngleAxis(
                                                   (this.BodyFacing == Rot4.South ? 1 : -1)
                                                 * this.CompAnimator?.WalkCycle?.BodyAngleVertical.Evaluate(this.MovedPercent) ?? 0f,
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
                    this.BodyWobble        = curve.Evaluate(this.MovedPercent);
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