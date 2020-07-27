using FacialStuff.AnimatorWindows;
using FacialStuff.GraphicsFS;
using FacialStuff.HairCut;
using FacialStuff.Harmony;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using FacialStuff.Animator;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class HumanHeadDrawer : PawnHeadDrawer
    {
        #region Public Methods

        public override void ApplyHeadRotation(bool renderBody, ref Quaternion headQuat)
        {
            CompProperties_Face compFaceProps = this.CompFace.Props;
            if (compFaceProps == null) return;
            
            PawnHeadRotator compFaceHeadRotator = this.CompFace.HeadRotator;
            if (compFaceHeadRotator == null) return;
            
            if (!Controller.settings.UseHeadRotator || !compFaceProps.canRotateHead) return;

            this.HeadFacing = compFaceHeadRotator.Rotation(this.HeadFacing, renderBody);

            // Todo: make it more natural
            if (Pawn.CurJob != null)
            {
                if (Pawn.CurJobDef != JobDefOf.Wait) return;
            }
            headQuat *= this.QuatHead(this.HeadFacing);


            // * Quaternion.AngleAxis(faceComp.headWiggler.downedAngle, Vector3.up);
        }

        public override void BaseHeadOffsetAt(ref Vector3 offset, bool portrait, Pawn pawn1)
        {
            Pawn pawn = this.Pawn;
            Vector2 headOffset = pawn.story.bodyType.headOffset;
            float horHeadOffset = headOffset.x;
            float verHeadOffset = headOffset.y;

            CompBodyAnimator animator = this.CompAnimator;
            if (animator.BodyAnim != null)
            {
                horHeadOffset += animator.BodyAnim.headOffset.x;
                verHeadOffset += animator.BodyAnim.headOffset.y;
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

            if (!portrait || HarmonyPatchesFS.AnimatorIsOpen())
            {
                CompBodyAnimator bodyAnimator = this.CompAnimator;
                if (bodyAnimator != null && (bodyAnimator.IsMoving || HarmonyPatchesFS.AnimatorIsOpen()))
                {
                    // Let's try a slightly stiffy head
                    {
                        offset.z -= 0.25f * bodyAnimator.BodyOffsetZ;
                        offset.z += bodyAnimator.HeadffsetZ;
                    }
                }
            }
        }

        public override void DrawBasicHead(
        Vector3 drawLoc,
            Quaternion headQuat,
            RotDrawMode bodyDrawType,
            bool headStump,
            bool portrait,
            out bool headDrawn)
        {
            Material headMaterial = this.Graphics.HeadMatAt(this.HeadFacing, bodyDrawType, headStump);
            if (headMaterial != null)
            {
                GenDraw.DrawMeshNowOrLater(this.GetPawnMesh(false, portrait),
                                           drawLoc,
                                           headQuat,
                                           headMaterial,
                                           portrait);
                headDrawn = true;
            }
            else
            {
                headDrawn = false;
            }
        }

        public override void DrawBeardAndTache(Vector3 beardLoc, Vector3 tacheLoc, Quaternion headQuat, bool portrait)
        {
            Mesh headMesh = this.GetPawnMesh(false, portrait);
            if (CompFace.PawnFace.BeardDef.IsBeardNotHair())
            {
                headMesh = this.GetPawnHairMesh(portrait);
            }
            Material beardMat = this.CompFace.FaceMaterial.BeardMatAt(this.HeadFacing);
            Material moustacheMatAt = this.CompFace.FaceMaterial.MoustacheMatAt(this.HeadFacing);

            if (beardMat != null)
            {
                GenDraw.DrawMeshNowOrLater(headMesh, beardLoc, headQuat, beardMat, portrait);
            }

            if (moustacheMatAt != null)
            {
                GenDraw.DrawMeshNowOrLater(headMesh, tacheLoc, headQuat, moustacheMatAt, portrait);
            }
        }

        public override void DrawBrows(Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            Material browMat = this.CompFace.FaceMaterial.BrowMatAt(this.HeadFacing);
            if (browMat == null)
            {
                return;
            }

            Mesh eyeMesh = this.CompFace.EyeMeshSet.Mesh.MeshAt(this.HeadFacing);
            GenDraw.DrawMeshNowOrLater(
                                       eyeMesh,
                                       drawLoc + this.EyeOffset(this.HeadFacing),
                                       headQuat,
                                       browMat,
                                       portrait);
        }

        public override void DrawHairAndHeadGear(Vector3 hairLoc, Vector3 headgearLoc,
                                                 RotDrawMode bodyDrawType,
                                                 Quaternion headQuat,
                                                 bool renderBody,
                                                 bool portrait, Vector3 hatInFrontOfFace)
        {
            Mesh hairMesh = this.GetPawnHairMesh(portrait);
            List<ApparelGraphicRecord> apparelGraphics = this.Graphics.apparelGraphics;
            List<ApparelGraphicRecord> headgearGraphics = null;
            if (!apparelGraphics.NullOrEmpty())
            {
                headgearGraphics = apparelGraphics
                                  .Where(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead ||
                                              x.sourceApparel.def.apparel.LastLayer == DefDatabase<ApparelLayerDef>.GetNamedSilentFail("OnHead") ||
                                              x.sourceApparel.def.apparel.LastLayer == DefDatabase<ApparelLayerDef>.GetNamedSilentFail("StrappedHead") ||
                                              x.sourceApparel.def.apparel.LastLayer == DefDatabase<ApparelLayerDef>.GetNamedSilentFail("MiddleHead")).ToList();
            }

            CompBodyAnimator animator = this.CompAnimator;

            bool noRenderGoggles = Controller.settings.FilterHats;

            bool showRoyalHeadgear = Pawn.royalty?.MostSeniorTitle != null && Controller.settings.ShowRoyalHeadgear;
            bool noRenderRoofed = animator != null && animator.HideHat && !showRoyalHeadgear;
            bool noRenderBed = Controller.settings.HideHatInBed && !renderBody && !showRoyalHeadgear;

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
                     || (!apCoversFullHead && !apCoversUpperHead && noRenderGoggles))
                    {
                        Material mat = this.Graphics.HairMatAt(this.HeadFacing);
                        GenDraw.DrawMeshNowOrLater(hairMesh, hairLoc, headQuat, mat, portrait);
                    }
                    else if (Controller.settings.MergeHair) // && !apCoversFullHead)
                    {
                        // If not, display the hair cut
                        HairCutPawn hairPawn = CutHairDB.GetHairCache(this.Pawn);
                        Material hairCutMat = hairPawn.HairCutMatAt(this.HeadFacing);
                        if (hairCutMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(hairMesh, hairLoc, headQuat, hairCutMat, portrait);
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

                if (noRenderBed && !showRoyalHeadgear)
                {
                    headgearGraphics?.Clear();
                }

                // headgearGraphics = headgearGraphics
                // .OrderBy(x => x.sourceApparel.def.apparel.bodyPartGroups.Max(y => y.listOrder)).ToList();
                if (headgearGraphics.NullOrEmpty())
                {
                    return;
                }

                for (int index = 0; index < headgearGraphics?.Count; index++)
                {
                    ApparelGraphicRecord headgearGraphic = headgearGraphics[index];
                    Material headGearMat = headgearGraphic.graphic.MatAt(this.HeadFacing);
                    headGearMat = this.Graphics.flasher.GetDamagedMat(headGearMat);

                    if (headgearGraphic.sourceApparel.def.apparel.hatRenderedFrontOfFace)
                    {
                        headgearLoc = hatInFrontOfFace;
                    }

                    GenDraw.DrawMeshNowOrLater(hairMesh, headgearLoc, headQuat, headGearMat, portrait);
                    headgearLoc.y += Offsets.YOffsetInterval_Clothes;
                }
            }
            else
            {
                // Draw regular hair if no hat worn
                if (bodyDrawType != RotDrawMode.Dessicated)
                {
                    Material hairMat = this.Graphics.HairMatAt(this.HeadFacing);
                    GenDraw.DrawMeshNowOrLater(hairMesh, hairLoc, headQuat, hairMat, portrait);
                }
            }
        }

        public override void DrawNaturalEyes(Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            Mesh eyeMesh = this.CompFace.EyeMeshSet.Mesh.MeshAt(this.HeadFacing);

            PawnFaceGraphic faceGraphic = this.CompFace.PawnFaceGraphic;
            // natural eyes
            PartStatus eyeLeft = this.CompFace.BodyStat.EyeLeft;
            if (faceGraphic == null)
            {
                return;
            }
            Material leftEyeMat = null;

            if (eyeLeft == PartStatus.Natural ||
                eyeLeft == PartStatus.Artificial &&
                !faceGraphic.EyePatchLeftTexExists())
            {
                leftEyeMat = this.CompFace.FaceMaterial.EyeLeftMatAt(this.HeadFacing, portrait);
            }
            else if (eyeLeft == PartStatus.Missing)
            {
                leftEyeMat = this.CompFace.FaceMaterial.EyeLeftMissingMatAt(this.HeadFacing, portrait);
            }
            if (leftEyeMat != null)
            {
                Vector3 left = drawLoc;
                drawLoc.y += Offsets.YOffset_LeftPart;

                GenDraw.DrawMeshNowOrLater(
                    eyeMesh,
                    left + this.EyeOffset(this.HeadFacing) +
                    this.CompFace.EyeWiggler.EyeMoveL,
                    headQuat,
                    leftEyeMat,
                    portrait);
            }
            Material rightEyeMat = null;

            PartStatus eyeRight = this.CompFace.BodyStat.EyeRight;
            if (eyeRight == PartStatus.Natural ||
            eyeRight == PartStatus.Artificial &&
            !faceGraphic.EyePatchRightTexExists())
            {
                rightEyeMat = this.CompFace.FaceMaterial.EyeRightMatAt(this.HeadFacing, portrait);
            }
            else if (eyeRight == PartStatus.Missing)
            {
                rightEyeMat = this.CompFace.FaceMaterial.EyeRightMissingMatAt(this.HeadFacing, portrait);
            }
            if (rightEyeMat != null)
            {
                Vector3 right = drawLoc;
                right.y += Offsets.YOffset_RightPart;

                GenDraw.DrawMeshNowOrLater(
                    eyeMesh,
                    right + this.EyeOffset(this.HeadFacing) +
                    this.CompFace.EyeWiggler.EyeMoveR,
                    headQuat,
                    rightEyeMat,
                    portrait);
            }
        }
        public override void DrawNaturalEars(Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            Mesh earMesh = this.CompFace.EyeMeshSet.Mesh.MeshAt(this.HeadFacing);

            PawnFaceGraphic faceGraphic = this.CompFace.PawnFaceGraphic;
            // natural eyes
            PartStatus earLeft = this.CompFace.BodyStat.EarLeft;
            if (faceGraphic == null)
            {
                return;
            }
            Material earLeftMatAt = null;

            if (earLeft == PartStatus.Natural ||
                earLeft == PartStatus.Artificial &&
                !faceGraphic.EarPatchLeftTexExists())
            {
                earLeftMatAt = this.CompFace.FaceMaterial.EarLeftMatAt(this.HeadFacing, portrait);
            }
            else if (earLeft == PartStatus.Missing)
            {
                earLeftMatAt = this.CompFace.FaceMaterial.EarLeftMissingMatAt(this.HeadFacing, portrait);
            }
            if (earLeftMatAt != null)
            {
                Vector3 left = drawLoc;
                drawLoc.y += Offsets.YOffset_LeftPart;

                GenDraw.DrawMeshNowOrLater(
                    earMesh,
                    left + this.EarOffset(this.HeadFacing),
                    headQuat,
                    earLeftMatAt,
                    portrait);
            }
            Material earRightMatAt = null;

            PartStatus earRight = this.CompFace.BodyStat.EarRight;
            if (earRight == PartStatus.Natural ||
            earRight == PartStatus.Artificial &&
            !faceGraphic.EarPatchRightTexExists())
            {
                earRightMatAt = this.CompFace.FaceMaterial.EarRightMatAt(this.HeadFacing, portrait);
            }
            else if (earRight == PartStatus.Missing)
            {
                earRightMatAt = this.CompFace.FaceMaterial.EarRightMissingMatAt(this.HeadFacing, portrait);
            }
            if (earRightMatAt != null)
            {
                Vector3 right = drawLoc;
                right.y += Offsets.YOffset_RightPart;

                GenDraw.DrawMeshNowOrLater(
                    earMesh,
                    right + this.EarOffset(this.HeadFacing),
                    headQuat,
                    earRightMatAt,
                    portrait);
            }
        }

        public override void DrawNaturalMouth(Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            Material mouthMat = this.CompFace.FaceMaterial.MouthMatAt(this.HeadFacing, portrait);
            if (mouthMat == null)
            {
                return;
            }

            // Mesh meshMouth = __instance.graphics.HairMeshSet.MeshAt(headFacing);
            Mesh meshMouth = this.CompFace.MouthMeshSet.Mesh.MeshAt(this.HeadFacing);

            Vector3 mouthOffset = Controller.settings.Develop
                                  ? this.CompFace.BaseMouthOffsetAtDevelop(this.HeadFacing)
                                  : this.CompFace.MouthMeshSet.OffsetAt(this.HeadFacing);

            Vector3 mouthLoc = drawLoc + headQuat * mouthOffset;
            GenDraw.DrawMeshNowOrLater(meshMouth, mouthLoc, headQuat, mouthMat, portrait);
        }

        public override void DrawUnnaturalEyeParts(Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            Mesh headMesh = this.GetPawnMesh(false, portrait);
            if (this.CompFace.BodyStat.EyeLeft == PartStatus.Artificial)
            {
                Material leftBionicMat = this.CompFace.FaceMaterial.EyeLeftPatchMatAt(this.HeadFacing);
                if (leftBionicMat != null)
                {
                    Vector3 left = drawLoc;
                    left.y += Offsets.YOffset_LeftPart;
                    GenDraw.DrawMeshNowOrLater(
                                               headMesh,
                                               left + this.EyeOffset(this.HeadFacing),
                                               headQuat,
                                               leftBionicMat,
                                               portrait);
                }
            }

            if (this.CompFace.BodyStat.EyeRight == PartStatus.Artificial)
            {
                Material rightBionicMat = this.CompFace.FaceMaterial.EyeRightPatchMatAt(this.HeadFacing);

                if (rightBionicMat != null)
                {
                    Vector3 right = drawLoc;
                    right.y += Offsets.YOffset_RightPart;
                    GenDraw.DrawMeshNowOrLater(
                                               headMesh,
                                               right + this.EyeOffset(this.HeadFacing),
                                               headQuat,
                                               rightBionicMat,
                                               portrait);
                }
            }
        }
        public override void DrawUnnaturalEarParts(Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            Mesh headMesh = this.GetPawnMesh(false, portrait);
            if (this.CompFace.BodyStat.EarLeft == PartStatus.Artificial)
            {
                Material leftBionicMat = this.CompFace.FaceMaterial.EarLeftPatchMatAt(this.HeadFacing);
                if (leftBionicMat != null)
                {
                    Vector3 left = drawLoc;
                    left.y += Offsets.YOffset_LeftPart;
                    GenDraw.DrawMeshNowOrLater(
                                               headMesh,
                                               left + this.EarOffset(this.HeadFacing),
                                               headQuat,
                                               leftBionicMat,
                                               portrait);
                }
            }

            if (this.CompFace.BodyStat.EarRight == PartStatus.Artificial)
            {
                Material rightBionicMat = this.CompFace.FaceMaterial.EarRightPatchMatAt(this.HeadFacing);

                if (rightBionicMat != null)
                {
                    Vector3 right = drawLoc;
                    right.y += Offsets.YOffset_RightPart;
                    GenDraw.DrawMeshNowOrLater(
                                               headMesh,
                                               right + this.EarOffset(this.HeadFacing),
                                               headQuat,
                                               rightBionicMat,
                                               portrait);
                }
            }
        }

        public override void DrawWrinkles(
         Vector3 drawLoc,
            RotDrawMode bodyDrawType,
            Quaternion headQuat,
            bool portrait)
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
            GenDraw.DrawMeshNowOrLater(headMesh, drawLoc, headQuat, wrinkleMat, portrait);
        }

        public override Vector3 EyeOffset(Rot4 headFacing)
        {
            return Controller.settings.Develop
                   ? this.CompFace.BaseEyeOffsetAt(headFacing)
                   : this.CompFace.EyeMeshSet.OffsetAt(headFacing);
        }

        public override Vector3 EarOffset(Rot4 headFacing)
        {
            return Controller.settings.Develop
                   ? this.CompFace.BaseEyeOffsetAt(headFacing)
                   : this.CompFace.EyeMeshSet.OffsetAt(headFacing);
        }

        public override void Initialize()
        {
            base.Initialize();
            this.CompAnimator = this.Pawn.GetComp<CompBodyAnimator>();
        }

        public override Quaternion QuatHead(Rot4 rotation)
        {
            CompBodyAnimator animator = this.CompAnimator;

            Quaternion asQuat = rotation.AsQuat;

            if (animator != null)
            {
                float headRotatorCurrentMovement;
                if ((animator.IsMoving || HarmonyPatchesFS.AnimatorIsOpen()) && rotation.IsHorizontal)
                {
                    headRotatorCurrentMovement = animator.HeadAngleX;
                }
                else
                {
                    headRotatorCurrentMovement = this.CompFace.HeadRotator.CurrentMovement;
                }
                float num = 1f;

                float x =
                    1f * Mathf.Sin(num * (headRotatorCurrentMovement * 0.1f) % (2 * Mathf.PI));
                float z =
                    1f * Mathf.Cos(num * (headRotatorCurrentMovement * 0.1f) % (2 * Mathf.PI));
                asQuat.SetLookRotation(new Vector3(x, 0f, z), Vector3.up);

            }


            return asQuat;

            // remove the body rotation
            if (animator != null && (animator.IsMoving && Controller.settings.UseFeet))
            {
                WalkCycleDef walkCycle = this.CompAnimator?.WalkCycle;
                if (this.BodyFacing.IsHorizontal)
                {
                    asQuat *= Quaternion.AngleAxis(
                                                   (this.BodyFacing == Rot4.West ? 1 : -1)
                                                 * walkCycle?.BodyAngle.Evaluate(this.CompAnimator.MovedPercent) ?? 0f,
                                                   Vector3.up);
                }
                else
                {
                    asQuat *= Quaternion.AngleAxis(
                                               (this.BodyFacing == Rot4.South ? 1 : -1)
                                             * walkCycle?.BodyAngleVertical
                                                         .Evaluate(this.CompAnimator.MovedPercent) ?? 0f,
                                               Vector3.up);
                }
            }

            return asQuat;
        }

        public override void Tick(Rot4 bodyFacing, Rot4 headFacing, PawnGraphicSet graphics)
        {
            // Do I need this? Seems pretty emtpy by now

            base.Tick(bodyFacing, headFacing, graphics);

            CompBodyAnimator animator = this.CompAnimator;
            if (animator == null)
            {
            }

            // var curve = bodyFacing.IsHorizontal ? this.walkCycle.BodyOffsetZ : this.walkCycle.BodyOffsetVerticalZ;
        }

        #endregion Public Methods
    }
}