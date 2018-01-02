using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class CentipedeDrawer : HumanBipedDrawer
    {
        public CentipedeDrawer()
        {
        }

        public override void DrawFeet(Vector3 rootLoc, bool portrait)
        {
            if (portrait && !this.CompAnimator.AnimatorOpen)
            {
                return;
            }

            int legsCount = 4;
            float legSpan = 1f;


            Vector3 ground = rootLoc;
            ground.z += OffsetGround;

            Rot4 rot = this.BodyFacing;

            // Basic values
            BodyAnimDef body = this.CompAnimator.BodyAnim;
            JointLister groundPos = this.GetJointPositions(
                                                           body.hipOffsets[rot.AsInt],
                                                           body.hipOffsets[Rot4.North.AsInt].x);

            WalkCycleDef cycle = this.CompAnimator.WalkCycle;
            Vector3 rightFootAnim=Vector3.zero;
            Vector3 leftFootAnim= Vector3.zero;
            float footAngleRight = 0;
            float footAngleLeft = 0;
            float offsetJoint = 0;
            if (cycle != null)
            {
                 offsetJoint = cycle.HipOffsetHorizontalX.Evaluate(this.MovedPercent);

                this.DoWalkCycleOffsets(
                                        ref  rightFootAnim,
                                        ref  leftFootAnim,
                                        ref  footAngleRight,
                                        ref  footAngleLeft,
                                        ref offsetJoint,
                                        cycle.FootPositionX,
                                        cycle.FootPositionZ,
                                        cycle.FootAngle);

            }
            this.GetMeshesFoot(out Mesh footMeshRight, out Mesh footMeshLeft);

            float bodyAngle = 0f;

            Pawn pawn = this.Pawn;
            if (pawn.Downed || pawn.Dead)
            {
                bodyAngle = pawn.Drawer.renderer.wiggler.downedAngle;
            }

            Material matRight;

            Material matLeft;
            if (!MainTabWindow_Animator.Colored)
            {
                switch (rot.AsInt)
                {
                    default:
                        matRight = this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.FootGraphicRight
                                                                 ?.MatAt(rot));
                        matLeft = this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.FootGraphicLeft
                                                                ?.MatAt(rot));
                        break;

                    case 1:
                        matRight = this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.FootGraphicRight
                                                                 ?.MatAt(rot));
                        matLeft = this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic
                                                                ?.FootGraphicLeftShadow
                                                                ?.MatAt(rot));
                        break;

                    case 3:
                        matRight = this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic
                                                                 ?.FootGraphicRightShadow
                                                                 ?.MatAt(rot));
                        matLeft = this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.FootGraphicLeft
                                                                ?.MatAt(rot));
                        break;
                }
            }
            else
            {
                matRight = this.CompAnimator.PawnBodyGraphic?.FootGraphicRightCol?.MatAt(rot);
                matLeft = this.CompAnimator.PawnBodyGraphic?.FootGraphicLeftCol?.MatAt(rot);
            }

            bool drawRight = matRight != null && this.CompAnimator.BodyStat.FootRight != PartStatus.Missing;

            bool drawLeft = matLeft != null && this.CompAnimator.BodyStat.FootLeft != PartStatus.Missing;

            if (drawLeft)
            {
                GenDraw.DrawMeshNowOrLater(
                                           footMeshLeft,
                                           (ground + groundPos.LeftJoint + leftFootAnim)
                                          .RotatedBy(bodyAngle),
                                           Quaternion.AngleAxis(bodyAngle + footAngleLeft, Vector3.up),
                                           matLeft,
                                           portrait);
            }

            if (drawRight)
            {
                GenDraw.DrawMeshNowOrLater(
                                           footMeshRight,
                                           (ground + groundPos.RightJoint + rightFootAnim)
                                          .RotatedBy(bodyAngle),
                                           Quaternion.AngleAxis(bodyAngle + footAngleRight, Vector3.up),
                                           matRight,
                                           portrait);
            }

            if (MainTabWindow_Animator.Develop)
            {
                // for debug
                Material centerMat = GraphicDatabase
                                    .Get<Graphic_Single>("Hands/Ground", ShaderDatabase.Transparent, Vector2.one,
                                                         Color.red).MatSingle;

                GenDraw.DrawMeshNowOrLater(
                                           footMeshLeft,
                                           ground.RotatedBy(bodyAngle) + groundPos.LeftJoint +
                                           new Vector3(offsetJoint, -0.301f, 0),
                                           Quaternion.AngleAxis(0, Vector3.up),
                                           centerMat,
                                           portrait);

                GenDraw.DrawMeshNowOrLater(
                                           footMeshRight,
                                           ground.RotatedBy(bodyAngle) + groundPos.RightJoint +
                                           new Vector3(offsetJoint, 0.301f, 0),
                                           Quaternion.AngleAxis(0, Vector3.up),
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
