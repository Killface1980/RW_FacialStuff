using System.Collections.Generic;
using System.Linq;

namespace FacialStuff
{
    using System;
    using FacialStuff.Components;
    using FacialStuff.Defs;
    using FacialStuff.Drawer;
    using FacialStuff.Graphics;
    using FacialStuff.Harmony;
    using JetBrains.Annotations;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;

    public class HumanBipedDrawer : PawnBodyDrawer
    {
        protected const float OffsetGround = -0.575f;

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

            rootLoc.z += this.CompAnimator.bodySizeDefinition.extraLegLength;

            base.ApplyBodyWobble(ref rootLoc, ref quat);
        }

        public override void DrawFeet(Vector3 rootLoc, bool portrait)
        {
            Material matLeft;
            Material matRight;

            // Basic values
            var body = this.CompAnimator.bodySizeDefinition;

            Vector3 ground = rootLoc;
            ground.z += OffsetGround;

            float footAngleRight = 0f;
            float footAngleLeft = 0f;
            Rot4 rot = bodyFacing;

            JointLister groundPos = this.GetJointPositions(rot, body.hipOffsets[rot.AsInt]);

            Vector3 rightFootAnim = Vector3.zero;
            Vector3 leftFootAnim = Vector3.zero;

            float offsetJoint = this.walkCycle.HipOffsetHorizontalX.Evaluate(this.movedPercent);
            var cycle = this.walkCycle;
            this.DoWalkCycleOffsets(
                ref rightFootAnim,
                ref leftFootAnim,
                ref footAngleRight,
                ref footAngleLeft,
               ref offsetJoint,
                cycle.FootPositionX,
                cycle.FootPositionZ,
                cycle.FootAngle,
                cycle.FootPositionVerticalZ);

            this.GetMeshesFoot(out Mesh footMeshRight, out Mesh footMeshLeft);

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
                        ground.RotatedBy(bodyAngle) + groundPos.rightJoint
                        + rightFootAnim,
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
                        ground.RotatedBy(bodyAngle) + groundPos.leftJoint
                        + leftFootAnim,
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
                    Color.red).MatSingle;

                GenDraw.DrawMeshNowOrLater(
                footMeshLeft,
                ground.RotatedBy(bodyAngle) + groundPos.leftJoint + new Vector3(offsetJoint, 0.301f, 0),
                Quaternion.AngleAxis(0, Vector3.up),
                centerMat,
                portrait);

                GenDraw.DrawMeshNowOrLater(
                    footMeshRight,
                    ground.RotatedBy(bodyAngle) + groundPos.rightJoint + new Vector3(offsetJoint, 0.301f, 0),
                    Quaternion.AngleAxis(0, Vector3.up),
                    centerMat,
                    portrait);

                // UnityEngine.Graphics.DrawMesh(handsMesh, center + new Vector3(0, 0.301f, z),
                // Quaternion.AngleAxis(0, Vector3.up), centerMat, 0);
                // UnityEngine.Graphics.DrawMesh(handsMesh, center + new Vector3(0, 0.301f, z2),
                // Quaternion.AngleAxis(0, Vector3.up), centerMat, 0);
            }
        }

        public override void DrawEquipment(Vector3 rootLoc, bool portrait)
        {
            Pawn pawn = this.Pawn;

            if (pawn.Dead || !pawn.Spawned)
            {
                return;
            }

            bool showHands = this.CompAnimator.Props.bipedWithHands && Controller.settings.UseHands;
            if (showHands)
            {
                if (this.CarryStuff(out Vector3 drawPos))
                {
                    this.DrawHands(drawPos, false, carrying: true);
                    return;
                }
            }

            bool notEquipped = pawn.equipment?.Primary == null;
            if (notEquipped)
            {
                if (showHands)
                {
                    this.DrawHands(rootLoc, portrait);
                }

                return;
            }

            if (pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon)
            {
                if (showHands)
                {
                    this.DrawHands(rootLoc, false);
                }

                return;
            }

            Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
            if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
            {
                Vector3 aimVector;
                aimVector = stance_Busy.focusTarg.HasThing
                                ? stance_Busy.focusTarg.Thing.DrawPos
                                : stance_Busy.focusTarg.Cell.ToVector3Shifted();
                float num = 0f;
                if ((aimVector - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                {
                    num = (aimVector - pawn.DrawPos).AngleFlat();
                }

                Vector3 b = new Vector3(0f, 0f, 0.4f).RotatedBy(num);
                Vector3 drawLoc = rootLoc + b;
                drawLoc.y += 0.04f;

                // default weapon angle axis is upward, but all weapons are facing right, so we turn base weapon angle by 90°
                this.DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, rootLoc, num);
            }
            else if (this.CarryWeaponOpenly())
            {
                float aimAngle = 143f;
                Vector3 drawLoc2 = rootLoc;
                Rot4 rotation = this.bodyFacing;
                if (rotation.IsHorizontal)
                {
                    if (rotation == Rot4.East)
                    {
                        drawLoc2 += new Vector3(YOffsetBodyParts, 0.04f, -0.22f);
                    }
                    else if (rotation == Rot4.West)
                    {
                        drawLoc2 += new Vector3(-YOffsetBodyParts, 0.04f, -0.22f);
                        aimAngle = 217f;
                    }
                }
                else if (rotation == Rot4.North)
                {
                    drawLoc2 += new Vector3(0, 0, -0.11f);
                }
                else if (rotation == Rot4.South)
                {
                    drawLoc2 += new Vector3(0, 0.04f, -0.22f);
                }
                this.DrawEquipmentAiming(pawn.equipment.Primary, drawLoc2, rootLoc, aimAngle);
            }
            else
            {
                this.DrawHands(rootLoc, portrait);
            }
        }
        public virtual bool Aiming()
        {
            Stance_Busy stance_Busy = this.Pawn.stances.curStance as Stance_Busy;
            return stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid;
        }

        // Verse.PawnRenderer - Vanilla code with flava at the end
        public override void DrawEquipmentAiming(Thing equipment, Vector3 weaponDrawLoc, Vector3 rootLoc, float aimAngle)
        {
            // New
            aimAngle -= 90f;
            float weaponAngle = aimAngle;

            Mesh weaponMesh;
            bool flipped;
            Vector3 weaponPositionOffset = Vector3.zero;
            bool aiming = this.Aiming();

            CompProperties_WeaponExtensions compWeaponExtensions = this.Pawn.equipment.Primary.def
                .GetCompProperties<CompProperties_WeaponExtensions>();

            if (this.bodyFacing == Rot4.West || this.bodyFacing == Rot4.North)
            {
                // draw weapon beneath the pawn
                weaponPositionOffset += new Vector3(0, -0.5f, 0);
            }

            // if if (aimAngle > 200f && aimAngle < 340f)
            if (aimAngle > 110f && aimAngle < 250f)
            {
                weaponMesh = MeshPool.plane10Flip;
                weaponAngle -= 180f;
                weaponAngle -= equipment.def.equippedAngleOffset;
                if (aiming)
                {
                    weaponAngle -= compWeaponExtensions?.AttackAngleOffset ?? 0;
                }

                flipped = true;

                if (!aiming && compWeaponExtensions != null)
                {
                    weaponPositionOffset += compWeaponExtensions.WeaponPositionOffset;

                    // flip x position offset
                    weaponPositionOffset.x *= -1;
                }
            }
            else
            {
                weaponMesh = MeshPool.plane10;
                weaponAngle += equipment.def.equippedAngleOffset;
                if (aiming)
                {
                    weaponAngle += compWeaponExtensions?.AttackAngleOffset ?? 0;
                }

                flipped = false;

                if (!aiming && compWeaponExtensions != null)
                {
                    weaponPositionOffset += compWeaponExtensions.WeaponPositionOffset;
                }
            }

            weaponAngle %= 360f;

            // weapon angle and position offsets based on current attack animation sequence
            this.DoAttackAnimationOffsets(ref weaponAngle, ref weaponPositionOffset, flipped);

            Graphic_StackCount graphic_StackCount = equipment.Graphic as Graphic_StackCount;
            Material matSingle = graphic_StackCount != null
                                     ? graphic_StackCount.SubGraphicForStackCount(1, equipment.def).MatSingle
                                     : equipment.Graphic.MatSingle;

            UnityEngine.Graphics.DrawMesh(
                weaponMesh,
                weaponDrawLoc + weaponPositionOffset,
                Quaternion.AngleAxis(weaponAngle, Vector3.up),
                matSingle,
                0);

            // Now the remaining hands if possible
            if (this.CompAnimator.Props.bipedWithHands && Controller.settings.UseHands)
            {
                this.DrawHandsAiming(weaponDrawLoc + weaponPositionOffset, rootLoc, flipped, weaponAngle, compWeaponExtensions, false);
            }
        }

        public void DrawHandsAiming(Vector3 weaponPosition, Vector3 rootLoc, bool flipped, float weaponAngle, [CanBeNull] CompProperties_WeaponExtensions compWeaponExtensions, bool portrait)
        {
            if (compWeaponExtensions == null)
            {
                return;
            }

            Material matLeft = this.CompAnimator.PawnBodyGraphic?.HandGraphicLeft.MatSingle;
            Material matRight = this.CompAnimator.PawnBodyGraphic?.HandGraphicRight.MatSingle;

            Mesh handsMesh = this.HandMesh;
            if (matRight != null)
            {
                Vector3 firstHandPosition = compWeaponExtensions.RightHandPosition;
                if (firstHandPosition != Vector3.zero)
                {
                    float x = firstHandPosition.x;
                    float y = firstHandPosition.y;
                    float z = firstHandPosition.z;
                    if (flipped)
                    {
                        x = -x;
                    }

                    UnityEngine.Graphics.DrawMesh(
                        handsMesh,
                        weaponPosition + new Vector3(x, y, z).RotatedBy(weaponAngle),
                        Quaternion.AngleAxis(weaponAngle, Vector3.up),
                        matRight,
                        0);
                }
                else
                {
                    this.DrawHands(rootLoc, portrait, leftSide: false);
                }
            }

            if (matLeft != null)
            {
                Vector3 secondHandPosition = compWeaponExtensions.LeftHandPosition;
                if (secondHandPosition != Vector3.zero)
                {
                    float x2 = secondHandPosition.x;
                    float y2 = secondHandPosition.y;
                    float z2 = secondHandPosition.z;
                    if (flipped)
                    {
                        x2 = -x2;
                    }

                    UnityEngine.Graphics.DrawMesh(
                        handsMesh,
                        weaponPosition + new Vector3(x2, y2, z2).RotatedBy(weaponAngle),
                        Quaternion.AngleAxis(weaponAngle, Vector3.up),
                        matLeft,
                        0);
                }
                else
                {
                    this.DrawHands(rootLoc, false, rightSide: false);
                }
            }
            if (MainTabWindow_Animator.develop)
            {

                //// for debug
                Material centerMat = GraphicDatabase.Get<Graphic_Single>(
                    "Hands/Human_Hand_dev",
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    Color.white).MatSingle;

                UnityEngine.Graphics.DrawMesh(handsMesh, weaponPosition + new Vector3(0, 0.001f, 0),
             Quaternion.AngleAxis(weaponAngle, Vector3.up), centerMat, 0);
            }
        }
        public override bool CarryStuff(out Vector3 drawPos)
        {
            Pawn pawn = this.Pawn;

            Thing carriedThing = pawn.carryTracker?.CarriedThing;
            if (carriedThing != null)
            {
                drawPos = pawn.DrawPos;
                bool flag = false;
                bool flip = false;
                if (pawn.CurJob == null
                    || !pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref drawPos, ref flag, ref flip))
                {
                    if (carriedThing is Pawn || carriedThing is Corpse)
                    {
                        drawPos += new Vector3(0.44f, 0.02f, 0f);
                    }
                    else
                    {
                        drawPos += new Vector3(0.18f, 0f, 0.02f);
                    }
                }

                if (flag)
                {
                    drawPos.y -= 0.04f;
                }
                else
                {
                    drawPos.y += 0.04f;
                }

                return true;
            }

            return base.CarryStuff(out drawPos);
        }

        public override bool CarryWeaponOpenly()
        {
            Pawn pawn = this.Pawn;
            return (pawn.carryTracker == null || pawn.carryTracker.CarriedThing == null)
                   && (pawn.Drafted || (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon)
                       || (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon));
        }


        public override void DoAttackAnimationOffsets(ref float weaponAngle, ref Vector3 weaponPosition, bool flipped)
        {
            CompEquippable primaryEq = this.Pawn.equipment?.PrimaryEq;

            // DamageDef damageDef = primaryEq?.PrimaryVerb?.verbProps?.meleeDamageDef;
            if (primaryEq?.parent?.def == null)
            {
                return;
            }

            if (primaryEq.AllVerbs.NullOrEmpty())
            {
                return;
            }

            if (!primaryEq.AllVerbs.Any(x => x.verbProps.MeleeRange))
            {
                return;
            }

            DamageDef damageDef = ThingUtility.PrimaryMeleeWeaponDamageType(primaryEq.parent.def);
            if (damageDef == null)
            {
                return;
            }

            // total weapon angle change during animation sequence
            int totalSwingAngle = 0;
            float animationPhasePercent = this.CompAnimator.Jitterer.CurrentOffset.magnitude / this.CompAnimator.JitterMax;
            if (damageDef == DamageDefOf.Stab)
            {
                weaponPosition += this.CompAnimator.Jitterer.CurrentOffset;

                // + new Vector3(0, 0, Mathf.Pow(this.CompFace.Jitterer.CurrentOffset.magnitude, 0.25f))/2;
            }
            else if (damageDef == DamageDefOf.Blunt || damageDef == DamageDefOf.Cut)
            {
                totalSwingAngle = 120;
                weaponPosition += this.CompAnimator.Jitterer.CurrentOffset + new Vector3(
                                      0,
                                      0,
                                      Mathf.Sin(
                                          this.CompAnimator.Jitterer.CurrentOffset.magnitude * Mathf.PI
                                          / this.CompAnimator.JitterMax) / 10);
            }

            weaponAngle += flipped ? -animationPhasePercent * totalSwingAngle : animationPhasePercent * totalSwingAngle;
        }

        protected void DrawHands(Vector3 drawPos, bool portrait, bool carrying = false, bool rightSide = true, bool leftSide = true)
        {
            Material matLeft = this.CompAnimator.PawnBodyGraphic?.HandGraphicLeft?.MatSingle;
            Material matRight = this.CompAnimator.PawnBodyGraphic?.HandGraphicRight?.MatSingle;
            if (MainTabWindow_Animator.Colored)
            {
                matLeft = this.CompAnimator.PawnBodyGraphic?.HandGraphicLeftCol?.MatSingle;
                matRight = this.CompAnimator.PawnBodyGraphic?.HandGraphicRightCol?.MatSingle;

            }
            var body = this.CompAnimator.bodySizeDefinition;


            Rot4 rot = this.bodyFacing;

            JointLister shoulperPos = this.GetJointPositions(rot, body.shoulderOffsets[rot.AsInt]);

            // Center = drawpos of carryThing
            Vector3 center = drawPos;

            float handSwingAngle = 0f;
            var shoulderAngle = 0f;

            var rightHand = Vector3.zero;
            var leftHand = Vector3.zero;

            // Todo: inclide this
            float offsetJoint = this.walkCycle.ShoulderOffsetHorizontalX.Evaluate(this.movedPercent);

            WalkCycleDef cycle = this.walkCycle;

            this.DoWalkCycleOffsets(
                body.armLength,
                ref rightHand,
                ref leftHand,
                ref shoulderAngle,
                ref handSwingAngle,
                ref shoulperPos,
                carrying,
                cycle.HandsSwingPosVertical,
                cycle.HandsSwingAngle, offsetJoint);


            Mesh handsMesh = this.HandMesh;
            float bodyAngle = 0f;
            Pawn pawn = this.Pawn;
            if (pawn.Downed || pawn.Dead)
            {
                bodyAngle = pawn.Drawer.renderer.wiggler.downedAngle;
            }
            {




            }
            if (matRight != null && rightSide)
            {
                // if (carrying || rot != Rot4.West)
                if (this.CompAnimator.bodyStat.handRight != PartStatus.Missing)
                {
                    GenDraw.DrawMeshNowOrLater(
                        handsMesh,
                        (drawPos.RotatedBy(bodyAngle)
                         + shoulperPos.rightJoint + rightHand.RotatedBy(handSwingAngle - shoulderAngle)),
                        Quaternion.AngleAxis(bodyAngle + handSwingAngle, Vector3.up),
                        matRight,
                        portrait);
                }
            }

            if (matLeft != null && leftSide)
            {
                // if (carrying || rot != Rot4.East)
                if (this.CompAnimator.bodyStat.handLeft != PartStatus.Missing)
                {
                    GenDraw.DrawMeshNowOrLater(
                        handsMesh,
                        drawPos.RotatedBy(bodyAngle)
                        + shoulperPos.leftJoint + leftHand.RotatedBy(-handSwingAngle - shoulderAngle),
                        Quaternion.AngleAxis(bodyAngle - handSwingAngle, Vector3.up),
                        matLeft,
                        portrait);
                }
            }

            if (MainTabWindow_Animator.develop)
            {
                // for debug
                Material centerMat = GraphicDatabase.Get<Graphic_Single>(
                    "Hands/Human_Hand_dev",
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    Color.white).MatSingle;

                GenDraw.DrawMeshNowOrLater(
                    handsMesh,
                    drawPos + shoulperPos.leftJoint + new Vector3(0, 0.301f, 0),
                    Quaternion.AngleAxis(shoulderAngle, Vector3.up),
                    centerMat,
                    portrait);

                GenDraw.DrawMeshNowOrLater(
                    handsMesh,
                    drawPos + shoulperPos.rightJoint + new Vector3(0, 0.301f, 0),
                    Quaternion.AngleAxis(0, Vector3.up),
                    centerMat,
                    portrait);
            }
        }

        private void DoWalkCycleOffsets(
            float armLength,
            ref Vector3 rightHand,
            ref Vector3 leftHand,
            ref float shoulderAngle,
            ref float handSwingAngle,
            ref JointLister shoulderPos,
            bool carrying,
            SimpleCurve cycleHandsSwingPosVertical,
            SimpleCurve cycleHandsSwingAngle,
            float offsetJoint)
        {
            var rot = this.bodyFacing;
            // Basic values if pawn is carrying stuff
            float x = 0;
            float x2 = -x;
            float y = YOffsetBodyParts;
            float y2 = y;
            float z = 0;
            float z2 = -z;

            // Has the pawn something in his hands?
            if (!carrying)
            {
                // Offsets for hands from the pawn center
                z = z2 = -armLength;

                if (rot.IsHorizontal)
                {
                    x = x2 = 0f;
                    if (rot == Rot4.East)
                    {
                        shoulderAngle = this.walkCycle.shoulderAngle;
                        y2 = -0.5f;
                    }
                    else
                    {
                        shoulderAngle = -this.walkCycle.shoulderAngle;
                        y = -0.05f;
                    }
                }
                else if (rot == Rot4.North)
                {
                    y = y2 = -0.02f;
                    x *= -1;
                    x2 *= -1;
                }

                // Swing the hands, try complete the cycle
                if (this.isMoving)
                {
                    if (rot.IsHorizontal)
                    {
                        float f = (rot == Rot4.West ? -1 : 1) * offsetJoint;
                        shoulderPos.rightJoint.x += f;
                        shoulderPos.leftJoint.x += f;

                        handSwingAngle = (rot == Rot4.West ? -1 : 1) * cycleHandsSwingAngle.Evaluate(this.movedPercent);

                    }
                    else
                    {
                        z += cycleHandsSwingPosVertical.Evaluate(this.movedPercent);
                        z2 -= cycleHandsSwingPosVertical.Evaluate(this.movedPercent);
                    }

                }

                if (this.Pawn.Fleeing() || this.Pawn.IsBurning())
                {
                    float offset = 1f + armLength;
                    x *= offset;
                    z *= offset;
                    x2 *= offset;
                    z2 *= offset;
                    handSwingAngle += 180f;
                    shoulderAngle = 0f;
                }
                rightHand = new Vector3(x, y, z);
                leftHand = new Vector3(x2, y2, z2);
            }
        }

        protected void DoWalkCycleOffsets(
            ref Vector3 rightFoot,
            ref Vector3 leftFoot,
            ref float footAngleRight,
            ref float footAngleLeft,
            ref float offsetJoint,
            SimpleCurve offsetX,
            SimpleCurve offsetZ,
            SimpleCurve angle,
            SimpleCurve offsetVerticalZ)
        {
            if (!this.isMoving)
            {
                return;
            }

            float flot = this.movedPercent;
            if (flot <= 0.5f)
            {
                flot += 0.5f;
            }
            else
            {
                flot -= 0.5f;
            }

            var rot = this.bodyFacing;
            if (rot.IsHorizontal)
            {
                rightFoot.x = offsetX.Evaluate(this.movedPercent);
                leftFoot.x = offsetX.Evaluate(flot);

                footAngleRight = angle.Evaluate(this.movedPercent);
                footAngleLeft = angle.Evaluate(flot);
                rightFoot.z = offsetZ.Evaluate(this.movedPercent);
                leftFoot.z = offsetZ.Evaluate(flot);

                rightFoot.x += offsetJoint;
                leftFoot.x += offsetJoint;

                if (rot == Rot4.West)
                {
                    rightFoot.x *= -1f;
                    leftFoot.x *= -1f;
                    footAngleLeft *= -1f;
                    footAngleRight *= -1f;
                    offsetJoint *= -1;
                }
            }
            else
            {
                rightFoot.z = offsetVerticalZ.Evaluate(this.movedPercent);
                leftFoot.z = offsetVerticalZ.Evaluate(flot);
                offsetJoint = 0;
            }
        }

        protected void GetMeshesFoot(out Mesh footMeshRight, out Mesh footMeshLeft)
        {
            var rot = this.bodyFacing;
            footMeshRight = MeshPool.plane10;
            footMeshLeft = MeshPool.plane10Flip;
            if (rot.IsHorizontal)
            {
                if (rot == Rot4.East)
                {
                    footMeshRight = footMeshLeft = MeshPool.plane10;
                }
                else
                {
                    footMeshRight = footMeshLeft = MeshPool.plane10Flip;
                }
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