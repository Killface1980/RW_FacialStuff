using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff
{
    using FacialStuff.Defs;

    using UnityEngine;

    using Verse;
    using Verse.AI;

    public class QuadrupedDrawer : HumanBipedDrawer
    {
        public override void SelectWalkcycle()
        {
            if (this.CompAnimator.AnimatorOpen)
            {
                this.walkCycle = MainTabWindow_Animator.EditorWalkcycle;
            }
            else if (this.Pawn.CurJob != null)
            {
                // Todo: create cycles
                switch (this.Pawn.CurJob.locomotionUrgency)
                {
                    case LocomotionUrgency.None:
                    case LocomotionUrgency.Amble:
                        this.walkCycle = WalkCycleDefOf.Quadruped_Walk;
                        break;
                    case LocomotionUrgency.Walk:
                        this.walkCycle = WalkCycleDefOf.Quadruped_Walk;
                        break;
                    case LocomotionUrgency.Jog:
                        this.walkCycle = WalkCycleDefOf.Quadruped_Walk;
                        break;
                    case LocomotionUrgency.Sprint:
                        this.walkCycle = WalkCycleDefOf.Quadruped_Walk;
                        break;
                }
            }

        }

        public override void DrawFeet(Vector3 rootLoc, bool portrait)
        {
            base.DrawFeet(rootLoc, portrait);
            this.DrawFrontPaws(rootLoc, portrait);
        }

        public virtual void DrawFrontPaws(Vector3 rootLoc, bool portrait)
        {
            if (!this.CompAnimator.Props.quadruped)
            {
                return;
            }

            Material matLeft;
            Material matRight;

            // Basic values
            var body = this.CompAnimator.bodySizeDefinition;
            float rightFootHorizontal = -body.hipWidth;
            float leftFootHorizontal = -rightFootHorizontal;
            float rightFootDepth = 0.035f;
            float leftFootDepth = rightFootDepth;
            float rightFootVertical = 0f;//-body.legLength;
            float leftFootVertical = rightFootVertical;

            // Center = drawpos of carryThing
            Vector3 center = rootLoc;

            float footAngleRight = 0f;
            float footAngleLeft = 0f;
            Rot4 rot = bodyFacing;

            // Offsets for hands from the pawn center
            center.z += body.hipOffsetVerticalFromCenter;

            if (rot.IsHorizontal)
            {
                rightFootHorizontal /= 3;
                leftFootHorizontal /= 3;
                if (rot == Rot4.East)
                {
                    leftFootDepth *= -1;
                }
                else
                {
                    // x *= -1;
                    // x2 *= -1;
                    rightFootDepth *= -1;
                }
            }
            else if (rot == Rot4.North)
            {
                rightFootDepth = leftFootDepth = -0.02f;
                rightFootHorizontal *= -1;
                leftFootHorizontal *= -1;
            }


            // Swing the hands, try complete the cycle
            if (this.isMoving)
            {
                float flot = this.movedPercent;
                if (flot <= 0.5f)
                {
                    flot += 0.5f;
                }
                else
                {
                    flot -= 0.5f;
                }

                WalkCycleDef cycle = this.walkCycle;
                if (rot.IsHorizontal)
                {
                    rightFootHorizontal = cycle.FrontPawPositionX.Evaluate(this.movedPercent);
                    leftFootHorizontal = cycle.FrontPawPositionX.Evaluate(flot);

                    footAngleRight = cycle.FrontPawAngle.Evaluate(this.movedPercent);
                    footAngleLeft = cycle.FrontPawAngle.Evaluate(flot);
                    rightFootVertical += cycle.FrontPawPositionZ.Evaluate(this.movedPercent);
                    leftFootVertical += cycle.FrontPawPositionZ.Evaluate(flot);
                    if (rot == Rot4.West)
                    {
                        rightFootHorizontal *= -1f;
                        leftFootHorizontal *= -1f;
                        footAngleLeft *= -1f;
                        footAngleRight *= -1f;
                    }
                }
                else
                {
                    rightFootVertical += cycle.FrontPawPositionVerticalZ.Evaluate(this.movedPercent);
                    leftFootVertical += cycle.FrontPawPositionVerticalZ.Evaluate(flot);
                }
            }

            Mesh footMeshRight = MeshPool.plane10;
            Mesh footMeshLeft = MeshPool.plane10Flip;
            if (rot.IsHorizontal)
            {
                float multi = rot == Rot4.West ? -1f : 1f;

                // Align the center to the hip
                center.x += multi * body.hipOffsetHorWhenFacingHorizontal;

                if (rot == Rot4.East)
                {
                    footMeshRight = footMeshLeft = MeshPool.plane10;
                }
                else
                {
                    footMeshRight = footMeshLeft = MeshPool.plane10Flip;
                }
            }
#if develop
            {
 }
#else
            {
            }
#endif
            if (MainTabWindow_Animator.Colored)
            {
                matRight = this.CompAnimator.PawnBodyGraphic?.FrontPawGraphicRightCol?.MatAt(rot);
                matLeft = this.CompAnimator.PawnBodyGraphic?.FrontPawGraphicLeftCol?.MatAt(rot);
            }
            else
            {
                matRight = this.CompAnimator.PawnBodyGraphic?.FrontPawGraphicRight?.MatAt(rot);
                matLeft = this.CompAnimator.PawnBodyGraphic?.FrontPawGraphicLeft?.MatAt(rot);
            }




            float bodyAngle = 0f;

            Pawn pawn = this.Pawn;
            if (pawn.Downed || pawn.Dead)
            {
                bodyAngle = pawn.Drawer.renderer.wiggler.downedAngle;
            }

            if (matRight != null)
            {
                if (this.CompAnimator.bodyStat.footRight != PartStatus.Missing)
                {
                    GenDraw.DrawMeshNowOrLater(
                        footMeshRight,
                        center.RotatedBy(bodyAngle) + new Vector3(rightFootHorizontal, rightFootDepth, rightFootVertical),
                        Quaternion.AngleAxis(bodyAngle + footAngleRight, Vector3.up),
                        matRight,
                        portrait);
                }
            }

            if (matLeft != null)
            {
                if (this.CompAnimator.bodyStat.footLeft != PartStatus.Missing)
                {
                    GenDraw.DrawMeshNowOrLater(
                        footMeshLeft,
                        center.RotatedBy(bodyAngle) + new Vector3(leftFootHorizontal, leftFootDepth, leftFootVertical),
                        Quaternion.AngleAxis(bodyAngle + footAngleLeft, Vector3.up),
                        matLeft,
                        portrait);
                }
            }

            if (MainTabWindow_Animator.develop)
            {
                // for debug
                Material centerMat = GraphicDatabase.Get<Graphic_Multi>(
                    "Hands/Human_Foot",
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    Color.yellow).MatFront;

                GenDraw.DrawMeshNowOrLater(
                    footMeshRight,
                    center.RotatedBy(bodyAngle) + new Vector3(0, 0.301f, 0),
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
