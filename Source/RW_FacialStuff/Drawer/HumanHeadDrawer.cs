using FacialStuff.AnimatorWindows;
using FacialStuff.GraphicsFS;
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

        public override void DrawBasicHead(
        Vector3 drawLoc,
            Quaternion headQuat,
            RotDrawMode bodyDrawType,
            bool headStump,
            bool portrait,
            out bool headDrawn)
        {
            Material headMaterial = this.Graphics.HeadMatAt_NewTemp(this.HeadFacing, bodyDrawType, headStump);
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
            if (this.CompFace.PawnFace.BeardDef.IsBeardNotHair())
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

            Mesh eyeMesh = this.CompFace.EyeMeshSet.MeshAt(this.HeadFacing);
            GenDraw.DrawMeshNowOrLater(
                                       eyeMesh,
                                       drawLoc + this.EyeOffset(this.HeadFacing),
                                       headQuat,
                                       browMat,
                                       portrait);
        }

        public override void DrawNaturalEyes(Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            Mesh eyeMesh = this.CompFace.EyeMeshSet.MeshAt(this.HeadFacing);

            FaceGraphic faceGraphic = this.CompFace.PawnFaceGraphic;
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
                    left + this.EyeOffset(this.HeadFacing),
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
                    right + this.EyeOffset(this.HeadFacing),
                    headQuat,
                    rightEyeMat,
                    portrait);
            }
        }
        public override void DrawNaturalEars(Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            Mesh earMesh = this.CompFace.EyeMeshSet.MeshAt(this.HeadFacing);

            FaceGraphic faceGraphic = this.CompFace.PawnFaceGraphic;
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
                    left,
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
                    right,
                    headQuat,
                    earRightMatAt,
                    portrait);
            }
        }

        public override void DrawNaturalMouth(Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            Material mouthMat = CompFace.FaceMaterial.MouthMatAt(HeadFacing, portrait);
            if (mouthMat == null)
            {
                return;
            }
            Mesh meshMouth = MeshPoolFS.HumanlikeMouthSet[(int)CompFace.FullHeadType].MeshAt(HeadFacing);
            Vector3 mouthOffset = MeshPoolFS.mouthOffsetsHeadType[(int)CompFace.FullHeadType];
            switch(HeadFacing.AsInt)
            {
                case 1: 
                    mouthOffset = new Vector3(mouthOffset.x, 0f, -mouthOffset.y);
                    break;
                case 2: 
                    mouthOffset = new Vector3(0, 0f, -mouthOffset.y);
                    break;
                case 3: 
                    mouthOffset = new Vector3(-mouthOffset.x, 0f, -mouthOffset.y);
                    break;
                default: 
                    mouthOffset = Vector3.zero;
                    break;
            }
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
                                               left,
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
                                               right,
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
                                               left,
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
                                               right,
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
        
        public override void Initialize()
        {
            base.Initialize();
            this.CompAnimator = this.Pawn.GetComp<CompBodyAnimator>();
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