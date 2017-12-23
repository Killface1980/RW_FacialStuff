using System.Collections.Generic;
using System.Linq;

namespace FacialStuff
{
    using FacialStuff.Components;
    using FacialStuff.Defs;
    using FacialStuff.Graphics;
    using FacialStuff.Harmony;
    using JetBrains.Annotations;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;

    public class HumanBipedDrawer : PawnBodyDrawer
    {
        #region Private Fields

        #endregion Private Fields

        #region Public Constructors

        public HumanBipedDrawer()
        {
            // Needs a constructor
        }

        #endregion Public Constructors

        #region Public Methods

        protected float BodyWobble;


        public override void ApplyBodyWobble(ref Vector3 rootLoc, ref Quaternion quat)
        {
            if (this.isMoving)
            {
                float bam = this.BodyWobble;

                // Log.Message(CompFace.Pawn + " - " + this.movedPercent + " - " + bam.ToString());
                rootLoc.z += bam;
                quat = this.QuatBody(quat, this.movedPercent);
            }

            base.ApplyBodyWobble(ref rootLoc, ref quat);
        }

        public override void DrawFeet(Vector3 rootLoc, bool portrait)
        {
            Material matLeft;
            Material matRight;

            // Basic values
            var body = this.CompAnimator.bodySizeDefinition;
            float rightFootHorizontal = -body.hipWidth;
            float leftFootHorizontal = -rightFootHorizontal;
            float rightFootDepth = 0.035f;
            float leftFootDepth = rightFootDepth;
            float rightFootVertical = 0f;// -body.legLength;
            float leftFootVertical = rightFootVertical;

            float offsetGround = -0.625f;

            Vector3 ground = rootLoc;
            ground.z += offsetGround;

            float footAngleRight = 0f;
            float footAngleLeft = 0f;
            Rot4 rot = bodyFacing;

            List<Vector2> groundPos = this.GetJointPositions(
                rot,
                body.hipOffsetHorWhenFacingHorizontal,
                0,
                body.hipWidth);

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
                    rightFootHorizontal = cycle.FootPositionX.Evaluate(this.movedPercent);
                    leftFootHorizontal = cycle.FootPositionX.Evaluate(flot);

                    footAngleRight = cycle.FootAngle.Evaluate(this.movedPercent);
                    footAngleLeft = cycle.FootAngle.Evaluate(flot);
                    rightFootVertical += cycle.FootPositionZ.Evaluate(this.movedPercent);
                    leftFootVertical += cycle.FootPositionZ.Evaluate(flot);
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
                    rightFootVertical += cycle.FootPositionVerticalZ.Evaluate(this.movedPercent);
                    leftFootVertical += cycle.FootPositionVerticalZ.Evaluate(flot);
                }
            }

            Mesh footMeshRight = MeshPool.plane10;
            Mesh footMeshLeft = MeshPool.plane10Flip;
            if (rot.IsHorizontal)
            {
                float multi = rot == Rot4.West ? -1f : 1f;

                // Align the center to the hip
                ground.x += multi * body.hipOffsetHorWhenFacingHorizontal;

                if (rot == Rot4.East)
                {
                    footMeshRight = footMeshLeft = MeshPool.plane10;
                }
                else
                {
                    footMeshRight = footMeshLeft = MeshPool.plane10Flip;
                }
            }

            if (MainTabWindow_Animator.Colored)
            {
                matRight = this.CompAnimator.PawnBodyGraphic?.FootGraphicRightCol?.MatAt(rot);
                matLeft = this.CompAnimator.PawnBodyGraphic?.FootGraphicLeftCol?.MatAt(rot);
            }
            else
            {
                matRight = this.CompAnimator.PawnBodyGraphic?.FootGraphicRight?.MatAt(rot);
                matLeft = this.CompAnimator.PawnBodyGraphic?.FootGraphicLeft?.MatAt(rot);
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
                        ground.RotatedBy(bodyAngle) + new Vector3(rightFootHorizontal, rightFootDepth, rightFootVertical),
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
                        ground.RotatedBy(bodyAngle) + new Vector3(leftFootHorizontal, leftFootDepth, leftFootVertical),
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
                    Color.yellow).MatAt(rot);

                foreach (Vector2 pos in groundPos)
                {
                    GenDraw.DrawMeshNowOrLater(
                    footMeshRight,
                    ground.RotatedBy(bodyAngle) + new Vector3(pos.x, 0.301f, pos.y),
                    Quaternion.AngleAxis(0, Vector3.up),
                    centerMat,
                    portrait);
                }

                // UnityEngine.Graphics.DrawMesh(handsMesh, center + new Vector3(0, 0.301f, z),
                // Quaternion.AngleAxis(0, Vector3.up), centerMat, 0);
                // UnityEngine.Graphics.DrawMesh(handsMesh, center + new Vector3(0, 0.301f, z2),
                // Quaternion.AngleAxis(0, Vector3.up), centerMat, 0);
            }
        }

        protected WalkCycleDef walkCycle = WalkCycleDefOf.Human_Walk;


        public override void Initialize()
        {

            base.Initialize();
        }

        public Quaternion QuatBody(Quaternion quat, float movedPercent)
        {
            if (this.bodyFacing.IsHorizontal)
            {
                quat *= Quaternion.AngleAxis(
                    (this.bodyFacing == Rot4.West ? -1 : 1) * this.walkCycle.BodyAngle.Evaluate(movedPercent),
                    Vector3.up);
            }
            else
            {
                quat *= Quaternion.AngleAxis(
                    (this.bodyFacing == Rot4.South ? -1 : 1)
                    * this.walkCycle.BodyAngleVertical.Evaluate(movedPercent),
                    Vector3.up);
            }

            return quat;
        }


        public override void Tick(Rot4 bodyFacing, PawnGraphicSet graphics)
        {
            base.Tick(bodyFacing, graphics);

            this.isMoving = this.CompAnimator.BodyAnimator.IsMoving(out this.movedPercent);
            var curve = bodyFacing.IsHorizontal ? this.walkCycle.BodyOffsetZ : this.walkCycle.BodyOffsetVerticalZ;
            this.BodyWobble = curve.Evaluate(this.movedPercent);

            this.SelectWalkcycle();

        }

        public virtual void SelectWalkcycle()
        {
            if (this.CompAnimator.AnimatorOpen)
            {
                this.walkCycle = MainTabWindow_Animator.EditorWalkcycle;
            }
            else if (this.Pawn.CurJob != null)
            {
                // switch (this.Pawn.mindState.duty.locomotion)
                // {
                //         
                // }

                switch (this.Pawn.CurJob.locomotionUrgency)
                {
                    case LocomotionUrgency.None:
                    case LocomotionUrgency.Amble:
                        this.walkCycle = WalkCycleDefOf.Human_Amble;
                        break;
                    case LocomotionUrgency.Walk:
                        this.walkCycle = WalkCycleDefOf.Human_Walk;
                        break;
                    case LocomotionUrgency.Jog:
                        this.walkCycle = WalkCycleDefOf.Human_Jog;
                        break;
                    case LocomotionUrgency.Sprint:
                        this.walkCycle = WalkCycleDefOf.Human_Sprint;
                        break;
                }
            }
        }

        #endregion Public Methods

        #region Protected Methods

        #endregion Protected Methods
    }
}