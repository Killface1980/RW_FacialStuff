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
            if (this.CompFace.FaceData.BeardDef.IsBeardNotHair())
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

            Mesh eyeMesh = MeshPoolFS.GetFaceMesh(CompFace.PawnCrownType, HeadFacing, false);
            GenDraw.DrawMeshNowOrLater(
                eyeMesh,
                drawLoc,
                headQuat,
                browMat,
                portrait);
        }

        public override void DrawNaturalEyes(Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            FaceGraphic faceGraphic = CompFace.PawnFaceGraphic;
            if(faceGraphic == null)
            {
                return;
            }
            if(HeadFacing == Rot4.North)
			{
                return;
			}
            if(HeadFacing != Rot4.East)
            {
                // Draw left eye
                DrawEye(faceGraphic, drawLoc, headQuat, portrait, true, 0, CompFace.BodyStat.EyeLeft, Offsets.YOffset_LeftPart);
            }
            if(HeadFacing != Rot4.West)
			{
                // Draw right eye
                DrawEye(faceGraphic, drawLoc, headQuat, portrait, false, 1, CompFace.BodyStat.EyeRight, Offsets.YOffset_RightPart);
            }
        }

        private void DrawEye(FaceGraphic faceGraphic, Vector3 drawLoc, Quaternion headQuat, bool portrait, bool mirrored, int partIdx, PartStatus eyeStatus, float offset)
		{
            Mesh eyeMesh = MeshPoolFS.GetFaceMesh(CompFace.PawnCrownType, HeadFacing, mirrored);
            Material eyeMat = null;
            if(eyeStatus == PartStatus.Natural || eyeStatus == PartStatus.Artificial && !faceGraphic.EyePatchRightTexExists())
            {
                eyeMat = this.CompFace.FaceMaterial.EyeMatAt(HeadFacing, partIdx, eyeStatus, portrait);
            } else if(eyeStatus == PartStatus.Missing)
            {
                eyeMat = this.CompFace.FaceMaterial.EyeMissingMatAt(HeadFacing, portrait);
            }
            if(eyeMat != null)
            {
                drawLoc.y += offset;
                GenDraw.DrawMeshNowOrLater(
                    eyeMesh,
                    drawLoc,
                    headQuat,
                    eyeMat,
                    portrait);
            }
        }

        public override void DrawNaturalEars(Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            Mesh earMesh = MeshPoolFS.GetFaceMesh(CompFace.PawnCrownType, HeadFacing, false);
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