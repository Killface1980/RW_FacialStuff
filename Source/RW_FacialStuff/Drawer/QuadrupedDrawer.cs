using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff
{
    using FacialStuff.Defs;
    using FacialStuff.Drawer;

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

            Vector3 ground = rootLoc;
            ground.z += OffsetGround;



            // Basic values
            var body = this.CompAnimator.bodySizeDefinition;

            Rot4 rot = bodyFacing;
            JointLister jointPositions = this.GetJointPositions(rot, body.shoulderOffsets[rot.AsInt]);

            Vector3 rightFootAnim = Vector3.zero;
            Vector3 leftFootAnim = Vector3.zero;

            float offsetJoint = this.walkCycle.ShoulderOffsetHorizontalX.Evaluate(this.movedPercent);
            WalkCycleDef cycle = this.walkCycle;





            // Center = drawpos of carryThing

            float footAngleRight = 0f;

            float footAngleLeft = 0f;
            this.DoWalkCycleOffsets(
                ref rightFootAnim,
                ref leftFootAnim,
                ref footAngleRight,
                ref footAngleLeft,
                ref offsetJoint,
                cycle.FrontPawPositionX,
                cycle.FrontPawPositionZ,
                cycle.FrontPawAngle,
                cycle.FrontPawPositionVerticalZ);


            this.GetMeshesFoot(out Mesh footMeshRight, out Mesh footMeshLeft);
#if develop
            {
 }
#else
            {
            }

            Material matLeft;

            Material matRight;
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
                        ground.RotatedBy(bodyAngle) + jointPositions.rightJoint+ rightFootAnim,
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
                        ground.RotatedBy(bodyAngle) + jointPositions.leftJoint+leftFootAnim,
                        Quaternion.AngleAxis(bodyAngle + footAngleLeft, Vector3.up),
                        matLeft,
                        portrait);
                }
            }

            if (MainTabWindow_Animator.develop)
            {
                // for debug
                Material centerMat = GraphicDatabase.Get<Graphic_Single>(
                    "Hands/Ground",
                    ShaderDatabase.Transparent,
                    Vector2.one,
                    Color.cyan).MatSingle;


                GenDraw.DrawMeshNowOrLater(
                    footMeshLeft,
                    ground.RotatedBy(bodyAngle) + jointPositions.leftJoint + new Vector3(offsetJoint, 0.301f, 0),
                    Quaternion.AngleAxis(0, Vector3.up),
                    centerMat,
                    portrait);

                GenDraw.DrawMeshNowOrLater(
                        footMeshRight,
                        ground.RotatedBy(bodyAngle) + jointPositions.rightJoint + new Vector3(offsetJoint, 0.301f, 0),
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
