namespace FacialStuff
{
    using FacialStuff.Defs;
    using FacialStuff.Drawer;

    using RimWorld;

    using UnityEngine;

    using Verse;
    using Verse.AI;

    public class QuadrupedDrawer : HumanBipedDrawer
    {

        public override void DrawFeet(Vector3 rootLoc, bool portrait)
        {
            if (portrait && !this.CompAnimator.AnimatorOpen)
            {
                return;
            }
            if (this.Pawn.RaceProps.Animal)
            {
                rootLoc.y = -Offsets.YOffset_Behind;
            }
            this.DrawFrontPaws(rootLoc, portrait);
            base.DrawFeet(rootLoc, portrait);
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
            BodyAnimDef body = this.CompAnimator.bodyAnim;

            Rot4 rot = this.bodyFacing;
            if (body == null)
            {
                return;
            }
            JointLister jointPositions = this.GetJointPositions(
                body.shoulderOffsets[rot.AsInt],
                body.shoulderOffsets[Rot4.North.AsInt].x,
                0f);

            Vector3 rightFootAnim = Vector3.zero;
            Vector3 leftFootAnim = Vector3.zero;

            float offsetJoint = this.CompAnimator.walkCycle.ShoulderOffsetHorizontalX.Evaluate(this.movedPercent);
            WalkCycleDef cycle = this.CompAnimator.walkCycle;





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
                cycle.FrontPawAngle);


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
                matRight = this.flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.FrontPawGraphicRight?.MatAt(rot));
                matLeft = this.flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.FrontPawGraphicLeft?.MatAt(rot));
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
