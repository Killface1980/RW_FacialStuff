﻿using FacialStuff.AnimatorWindows;
using FacialStuff.Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class QuadrupedDrawer : HumanBipedDrawer
    {
        public override void DrawFeet(Quaternion bodyQuat, Quaternion footQuat, Vector3 rootLoc, bool portrait)
        {
            if (portrait && !HarmonyPatchesFS.AnimatorIsOpen())
            {
                return;
            }

            // Fix the position, maybe needs new code in GetJointPositions()?
            if (!this.BodyFacing.IsHorizontal)
            {
         //       rootLoc.y -=  Offsets.YOffset_Behind;
            }
            rootLoc.y += this.BodyFacing == Rot4.South ? -Offsets.YOffset_HandsFeetOver : 0;

            this.DrawFrontPaws(bodyQuat, footQuat, rootLoc, portrait);

         //   rootLoc.y += this.BodyFacing == Rot4.North ? Offsets.YOffset_Behind : -Offsets.YOffset_Behind;

            base.DrawFeet(bodyQuat, footQuat, rootLoc, portrait);
        }

        public override void DrawHands(Quaternion bodyQuat, Vector3 drawPos, bool portrait, Thing carriedThing = null,
                                       bool       flip = false)
        {
           // base.DrawHands(bodyQuat, drawPos, portrait, carrying, drawSide);
        }

        protected virtual void DrawFrontPaws(Quaternion bodyQuat, Quaternion footQuat, Vector3 rootLoc, bool portrait)
        {
            if (!this.CompAnimator.Props.quadruped)
            {
                return;
            }


            // Basic values
            BodyAnimDef body = this.CompAnimator.BodyAnim;

            Rot4 rot = this.BodyFacing;
            if (body == null)
            {
                return;
            }

            JointLister jointPositions = this.GetJointPositions(JointType.Shoulder,
                                                                body.shoulderOffsets[rot.AsInt],
                                                                body.shoulderOffsets[Rot4.North.AsInt].x);

            Vector3 rightFootAnim = Vector3.zero;
            Vector3 leftFootAnim = Vector3.zero;
            float footAngleRight = 0f;

            float footAngleLeft = 0f;
            float offsetJoint = 0;

            WalkCycleDef cycle = this.CompAnimator.WalkCycle;
            if (cycle != null)
            {
                offsetJoint = cycle.ShoulderOffsetHorizontalX.Evaluate(this.CompAnimator.MovedPercent);

                // Center = drawpos of carryThing
                this.DoWalkCycleOffsets(
                                        ref rightFootAnim,
                                        ref leftFootAnim,
                                        ref footAngleRight,
                                        ref footAngleLeft,
                                        ref offsetJoint,
                                        cycle.FrontPawPositionX,
                                        cycle.FrontPawPositionZ,
                                        cycle.FrontPawAngle);
            }

            this.GetBipedMesh(out Mesh footMeshRight, out Mesh footMeshLeft);

            Material matLeft;

            Material matRight;
            if (MainTabWindow_BaseAnimator.Colored)
            {
                matRight = this.CompAnimator.PawnBodyGraphic?.FrontPawGraphicRightCol?.MatAt(rot);
                matLeft = this.CompAnimator.PawnBodyGraphic?.FrontPawGraphicLeftCol?.MatAt(rot);
            }
            else
            {
                switch (rot.AsInt)
                {
                    default:
                        matRight = this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.FrontPawGraphicRight
                                                                 ?.MatAt(rot));
                        matLeft = this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.FrontPawGraphicLeft
                                                                ?.MatAt(rot));
                        break;

                    case 1:
                        matRight = this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.FrontPawGraphicRight
                                                                 ?.MatAt(rot));
                        matLeft = this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic
                                                                ?.FrontPawGraphicLeftShadow?.MatAt(rot));
                        break;

                    case 3:
                        matRight = this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic
                                                                 ?.FrontPawGraphicRightShadow?.MatAt(rot));
                        matLeft = this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.FrontPawGraphicLeft
                                                                ?.MatAt(rot));
                        break;
                }
            }


            Quaternion drawQuat = this.CompAnimator.IsMoving ? footQuat : bodyQuat;
            Vector3 ground = rootLoc + drawQuat * new Vector3(0, 0, OffsetGroundZ);

            if (matLeft != null)
            {
                if (this.CompAnimator.BodyStat.FootLeft != PartStatus.Missing)
                {
                    GenDraw.DrawMeshNowOrLater(
                                               footMeshLeft,
                                               ground + jointPositions.LeftJoint + leftFootAnim,
                                               drawQuat * Quaternion.AngleAxis(footAngleLeft, Vector3.up),
                                               matLeft,
                                               portrait);
                }
            }

            if (matRight != null)
            {
                if (this.CompAnimator.BodyStat.FootRight != PartStatus.Missing)
                {
                    GenDraw.DrawMeshNowOrLater(
                                               footMeshRight,
                                               ground + jointPositions.RightJoint +
            rightFootAnim,
                                               drawQuat * Quaternion.AngleAxis(footAngleRight, Vector3.up),
                                               matRight,
                                               portrait);
                }
            }

            if (MainTabWindow_BaseAnimator.Develop)
            {
                // for debug
                Material centerMat = GraphicDatabase
                                    .Get<Graphic_Single>("Hands/Ground", ShaderDatabase.Transparent, Vector2.one,
                                                         Color.cyan).MatSingle;

                GenDraw.DrawMeshNowOrLater(
                                           footMeshLeft,
                                           ground + jointPositions.LeftJoint +
                                           new Vector3(offsetJoint, 0.301f, 0),
                                           drawQuat * Quaternion.AngleAxis(0, Vector3.up),
                                           centerMat,
                                           portrait);

                GenDraw.DrawMeshNowOrLater(
                                           footMeshRight,
                                           ground + jointPositions.RightJoint +
                                           new Vector3(offsetJoint, 0.301f, 0),
                                           drawQuat * Quaternion.AngleAxis(0, Vector3.up),
                                           centerMat,
                                           portrait);

                // UnityEngine.Graphics.DrawMesh(handsMesh, center + new Vector3(0, 0.301f, z),
                // Quaternion.AngleAxis(0, Vector3.up), centerMat, 0);
                // UnityEngine.Graphics.DrawMesh(handsMesh, center + new Vector3(0, 0.301f, z2),
                // Quaternion.AngleAxis(0, Vector3.up), centerMat, 0);
            }
        }
    }
}