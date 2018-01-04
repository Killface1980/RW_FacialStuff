using System;
using System.Collections.Generic;
using System.Linq;
using FacialStuff.Animator;
using FacialStuff.Components;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FacialStuff
{
    public class HumanBipedDrawer : PawnBodyDrawer
    {
        #region Public Fields

        public PawnHandsTweener handTweener;

        #endregion Public Fields

        #region Protected Fields

        protected const float OffsetGroundZ = -0.575f;

        protected DamageFlasher Flasher;

        #endregion Protected Fields

        #region Private Fields

        private Vector3         _firstHandPosition;
        private Vector3         _secondHandPosition;
        private PawnFeetTweener feetTweener;
        private Vector3         rightHand;
        private Quaternion      weaponQuat = new Quaternion();

        #endregion Private Fields

        #region Public Properties

        public Material LeftHandMat =>
        this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.HandGraphicLeft?.MatSingle);

        public Material LeftHandShadowMat => this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic
                                                                           ?.HandGraphicLeftShadow?.MatSingle);

        public Material RightHandMat =>
        this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.HandGraphicRight?.MatSingle);


        public Material RightHandShadowMat => this.Flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic
                                                                            ?.HandGraphicRightShadow?.MatSingle);

        #endregion Public Properties

        #region Public Methods

        public virtual bool Aiming()
        {
            return this.Pawn.stances.curStance is Stance_Busy stanceBusy && !stanceBusy.neverAimWeapon &&
                   stanceBusy.focusTarg.IsValid;
        }

        public override void ApplyBodyWobble(ref Vector3 rootLoc, ref Vector3 footPos, ref Quaternion quat)
        {
            if (this.IsMoving)
            {
                if (this.CompAnimator.WalkCycle != null)
                {
                    SimpleCurve curve = this.CompAnimator.WalkCycle.BodyOffsetZ;

                    float bam = curve.Evaluate(this.MovedPercent);
                    rootLoc.z += bam;
                    quat      =  this.QuatBody(quat, this.MovedPercent);

                    // Log.Message(CompFace.Pawn + " - " + this.movedPercent + " - " + bam.ToString());
                }
            }

            // Adds the leg length to the rootloc and relocates the feet to keep the pawn in center, e.g. for shields
            if (this.CompAnimator.BodyAnim != null)
            {
                float   legModifier = this.CompAnimator.BodyAnim.extraLegLength;
                float   posModB     = legModifier  * 0.7f;
                float   posModF     = -legModifier * 0.3f;
                Vector3 vector3     = new Vector3(0, 0, posModB);
                Vector3 vector4     = new Vector3(0, 0, posModF);

                // No rotation when moving
                if (!this.IsMoving)
                {
                    vector3 = quat * vector3;
                    vector4 = quat * vector4;
                }

                rootLoc += vector3;
                footPos += vector4;
            }

            base.ApplyBodyWobble(ref rootLoc, ref footPos, ref quat);
        }

        public override List<Material> BodyBaseAt(
        [NotNull] PawnGraphicSet graphics,
        Rot4                     bodyFacing,
        RotDrawMode              bodyDrawType,
        MaxLayerToShow           layer)
        {
            switch (layer)
            {
                case MaxLayerToShow.Naked:  return this.CompAnimator?.NakedMatsBodyBaseAt(bodyFacing, bodyDrawType);
                case MaxLayerToShow.OnSkin: return this.CompAnimator?.UnderwearMatsBodyBaseAt(bodyFacing, bodyDrawType);
                default:                    return graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
            }

            return base.BodyBaseAt(graphics, bodyFacing, bodyDrawType, layer);
        }

        public void CalculateHandsAiming(
        Vector3                                     weaponPosition,
        bool                                        flipped,
        float                                       weaponAngle,
        [CanBeNull] CompProperties_WeaponExtensions compWeaponExtensions)
        {
            // Prepare everything for DrawHands, but do nothing

            if (compWeaponExtensions == null) return;

            this._firstHandPosition  = compWeaponExtensions.RightHandPosition;
            this._secondHandPosition = compWeaponExtensions.LeftHandPosition;

            if (this._firstHandPosition != Vector3.zero)
            {
                float x = this._firstHandPosition.x;
                float y = this._firstHandPosition.y;
                float z = this._firstHandPosition.z;
                if (flipped)
                {
                    x = -x;
                }
                if (this.BodyFacing == Rot4.North)
                {
                    y *= -1f;
                }

                this._firstHandPosition =
                weaponPosition + new Vector3(x, y, z).RotatedBy(weaponAngle);
            }

            if (this._secondHandPosition != Vector3.zero)
            {
                float x2 = this._secondHandPosition.x;
                float y2 = this._secondHandPosition.y;
                float z2 = this._secondHandPosition.z;
                if (flipped)
                {
                    x2 = -x2;
                }

                if (this.BodyFacing == Rot4.North)
                {
                    y2 *= -1f;
                }

                this._secondHandPosition =
                weaponPosition + new Vector3(x2, y2, z2).RotatedBy(weaponAngle);
            }


            this.weaponQuat = Quaternion.AngleAxis(weaponAngle, Vector3.up);
        }

        public override bool CarryStuff()
        {
            Pawn pawn = this.Pawn;

            Thing carriedThing = pawn.carryTracker?.CarriedThing;
            if (carriedThing != null)
            {
                return true;
            }

            return base.CarryStuff();
        }

        public virtual bool CarryWeaponOpenly()
        {
            return this.Pawn.carryTracker?.CarriedThing == null &&
                   (this.Pawn.Drafted                                                           ||
                    (this.Pawn.CurJob         != null && this.Pawn.CurJob.def.alwaysShowWeapon) ||
                    (this.Pawn.mindState.duty != null && this.Pawn.mindState.duty.def.alwaysShowWeapon));
        }

        public void DoAttackAnimationHandOffsets(ref float weaponAngle, ref Vector3 weaponPosition, bool flipped)
        {
            Pawn pawn = this.Pawn;
            if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
            {
                return;
            }

            if (pawn.health?.capacities != null)
            {
                if (!pawn.health.capacities
                         .CapableOf(PawnCapacityDefOf
                                   .Manipulation))
                {
                    if (pawn.RaceProps != null && pawn.RaceProps.ToolUser)
                    {
                        return;
                    }
                }
            }

            // total weapon angle change during animation sequence
            int     totalSwingAngle = 0;
            Vector3 currentOffset   = this.CompAnimator.Jitterer.CurrentOffset;

            float jitterMax             = this.CompAnimator.JitterMax;
            float magnitude             = currentOffset.magnitude;
            float animationPhasePercent = magnitude / jitterMax;
            {
                // if (damageDef == DamageDefOf.Stab)
                weaponPosition += currentOffset;
            }

            // else if (damageDef == DamageDefOf.Blunt || damageDef == DamageDefOf.Cut)
            // {
            // totalSwingAngle = 120;
            // weaponPosition += currentOffset + new Vector3(0, 0, Mathf.Sin(magnitude * Mathf.PI / jitterMax) / 10);
            // }
            weaponAngle += flipped ? -animationPhasePercent * totalSwingAngle : animationPhasePercent * totalSwingAngle;
        }

        public virtual void DoAttackAnimationOffsets(ref float weaponAngle, ref Vector3 weaponPosition, bool flipped)
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
            int     totalSwingAngle = 0;
            Vector3 currentOffset   = this.CompAnimator.Jitterer.CurrentOffset;

            float jitterMax             = this.CompAnimator.JitterMax;
            float magnitude             = currentOffset.magnitude;
            float animationPhasePercent = magnitude / jitterMax;

            if (damageDef == DamageDefOf.Stab)
            {
                weaponPosition += currentOffset;

                // + new Vector3(0, 0, Mathf.Pow(this.CompFace.Jitterer.CurrentOffset.magnitude, 0.25f))/2;
            }
            else if (damageDef == DamageDefOf.Blunt || damageDef == DamageDefOf.Cut)
            {
                totalSwingAngle =  120;
                weaponPosition  += currentOffset + new Vector3(0, 0, Mathf.Sin(magnitude * Mathf.PI / jitterMax) / 10);
            }

            weaponAngle += flipped ? -animationPhasePercent * totalSwingAngle : animationPhasePercent * totalSwingAngle;
        }

        public override void DrawApparel(Quaternion quat, Vector3 vector, bool renderBody, bool portrait)
        {
            if (portrait || renderBody                    && !this.CompAnimator.HideShellLayer || !renderBody
             && !Controller.settings.HideShellWhileRoofed && Controller.settings.IgnoreRenderBody)
            {
                for (int index = 0; index < this.Graphics.apparelGraphics.Count; index++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = this.Graphics.apparelGraphics[index];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        Mesh     bodyMesh  = this.GetPawnMesh(true, portrait);
                        Material material3 = apparelGraphicRecord.graphic.MatAt(this.BodyFacing);
                        material3          = this.Graphics.flasher.GetDamagedMat(material3);
                        GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, material3, portrait);

                        // possible fix for phasing apparel
                        vector.y += Offsets.YOffsetInterval_Clothes;
                    }
                }
            }
        }

        public override void DrawBody(
        PawnWoundDrawer woundDrawer,
        Vector3         rootLoc,
        Quaternion      quat,
        RotDrawMode     bodyDrawType,
        bool            renderBody,
        bool            portrait)
        {
            // renderBody is AFAIK only used for beds, so ignore it and undress
            if (renderBody || Controller.settings.IgnoreRenderBody)
            {
                Vector3 loc = rootLoc;
                loc.y       += Offsets.YOffset_Body;

                Mesh bodyMesh = this.GetPawnMesh(true, portrait);

                List<Material> bodyBaseAt = null;
                bool           flag       = true;
                if (!portrait && Controller.settings.HideShellWhileRoofed)
                {
                    if (this.CompAnimator.InRoom)
                    {
                        MaxLayerToShow layer;
                        if (this.CompAnimator.InPrivateRoom)
                        {
                            layer = renderBody
                                    ? Controller.settings.LayerInPrivateRoom
                                    : Controller.settings.LayerInOwnedBed;
                        }
                        else
                        {
                            layer = renderBody ? Controller.settings.LayerInRoom : Controller.settings.LayerInBed;
                        }

                        bodyBaseAt = this.BodyBaseAt(this.Graphics, this.BodyFacing, bodyDrawType, layer);
                        flag       = false;
                    }
                }

                if (flag)
                {
                    bodyBaseAt = this.Graphics.MatsBodyBaseAt(this.BodyFacing, bodyDrawType);
                }

                for (int i = 0; i < bodyBaseAt.Count; i++)
                {
                    Material damagedMat = this.Graphics.flasher.GetDamagedMat(bodyBaseAt[i]);
                    GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quat, damagedMat, portrait);
                    loc.y += Offsets.YOffsetInterval_Clothes;
                }

                if (bodyDrawType == RotDrawMode.Fresh)
                {
                    Vector3 drawLoc = rootLoc;
                    drawLoc.y       += Offsets.YOffset_Wounds;

                    woundDrawer?.RenderOverBody(drawLoc, bodyMesh, quat, portrait);
                }
            }
        }

        public override void DrawEquipment(Vector3 rootLoc, bool portrait)
        {
            Pawn pawn = this.Pawn;

            if (portrait && !this.CompAnimator.AnimatorOpen)
            {
                return;
            }

            if (pawn.Dead || !pawn.Spawned)
            {
                return;
            }

            if (pawn.equipment == null || pawn.equipment.Primary == null)
            {
                return;
            }

            if (pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon)
            {
                return;
            }

            if (pawn.stances.curStance is Stance_Busy stanceBusy && !stanceBusy.neverAimWeapon
                                                                 && stanceBusy.focusTarg.IsValid)
            {
                Vector3 aimVector = stanceBusy.focusTarg.HasThing
                                    ? stanceBusy.focusTarg.Thing.DrawPos
                                    : stanceBusy.focusTarg.Cell.ToVector3Shifted();

                float aimAngle = 0f;
                if ((aimVector - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                {
                    aimAngle = (aimVector - pawn.DrawPos).AngleFlat();
                }

                Vector3 b       = new Vector3(0f, 0f, 0.4f).RotatedBy(aimAngle);
                Vector3 drawLoc = rootLoc + b;
                drawLoc.y       += Offsets.YOffset_PrimaryEquipmentOver;

                // default weapon angle axis is upward, but all weapons are facing right, so we turn base weapon angle by 90°
                bool dummy = false;
                this.DrawEquipmentAiming(pawn.equipment.Primary, ref drawLoc, ref aimAngle, portrait,
                                         ref dummy);
            }
            else if (this.CarryWeaponOpenly() || MainTabWindow_Animator.Equipment)
            {
                float   aimAngle = 143f;
                Vector3 drawLoc2 = rootLoc;
                Rot4    rotation = this.BodyFacing;

                switch (rotation.AsInt)
                {
                    case 0:
                        drawLoc2 += new Vector3(0, Offsets.YOffset_PrimaryEquipmentUnder, -0.11f);
                        aimAngle =  217f;
                        break;
                    case 1:
                        drawLoc2 += new Vector3(0.2f, Offsets.YOffset_PrimaryEquipmentOver, -0.22f);
                        break;
                    case 2:
                        drawLoc2 += new Vector3(0, Offsets.YOffset_PrimaryEquipmentOver, -0.22f);
                        break;
                    case 3:
                        drawLoc2 += new Vector3(-0.2f, Offsets.YOffset_PrimaryEquipmentOver, -0.22f);
                        aimAngle =  217f;
                        break;
                }

                bool dummy = false;
                this.DrawEquipmentAiming(pawn.equipment.Primary, ref drawLoc2, ref aimAngle, portrait,
                                         ref dummy);
            }
        }

        // Verse.PawnRenderer
        public void DrawEquipmentAiming(
        Thing       equipment,
        ref Vector3 weaponDrawLoc,
        ref float   aimAngle,
        bool        portrait, ref bool flipped)
        {
            // New
            aimAngle -= 90f;

            bool aiming = this.Aiming();

            Mesh    weaponMesh;
            Vector3 weaponPositionOffset = Vector3.zero;

            CompProperties_WeaponExtensions compWeaponExtensions =
            this.Pawn.equipment.Primary.def.GetCompProperties<CompProperties_WeaponExtensions>();

            if (this.BodyFacing == Rot4.West || this.BodyFacing == Rot4.North)
            {
                weaponPositionOffset += new Vector3(0, -0.5f, 0);
            }

            // if if (aimAngle > 200f && aimAngle < 340f)
            if (aimAngle > 110f && aimAngle < 250f)
            {
                weaponMesh =  MeshPool.plane10Flip;
                aimAngle   -= 180f;
                aimAngle   -= equipment.def.equippedAngleOffset;
                if (aiming)
                {
                    aimAngle -= compWeaponExtensions?.AttackAngleOffset ?? 0;
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
                weaponMesh =  MeshPool.plane10;
                aimAngle   += equipment.def.equippedAngleOffset;
                if (aiming)
                {
                    aimAngle += compWeaponExtensions?.AttackAngleOffset ?? 0;
                }

                flipped = false;

                if (!aiming && compWeaponExtensions != null)
                {
                    weaponPositionOffset += compWeaponExtensions.WeaponPositionOffset;
                }
            }

            aimAngle %= 360f;

            // weapon angle and position offsets based on current attack keyframes sequence
            this.DoAttackAnimationOffsets(ref aimAngle, ref weaponPositionOffset, flipped);

            Material matSingle = equipment.Graphic is Graphic_StackCount graphicStackCount
                                 ? graphicStackCount.SubGraphicForStackCount(1, equipment.def)
                                                    .MatSingle
                                 : equipment.Graphic.MatSingle;
            weaponDrawLoc += weaponPositionOffset;

            this.handTweener.HandPositions[(int) TweenThing.Equipment] = weaponDrawLoc;


            this.handTweener.PreHandPosCalculation(TweenThing.Equipment, this.IsMoving);

            Vector3 weaponPosition = this.handTweener.TweenedHandsPos[(int) TweenThing.Equipment];
            GenDraw.DrawMeshNowOrLater(
                                       weaponMesh,
                                       weaponPosition,
                                       Quaternion.AngleAxis(aimAngle, Vector3.up),
                                       matSingle,
                                       portrait);

            // Now the remaining hands if possible
            if (this.CompAnimator.Props.bipedWithHands && Controller.settings.UseHands)
            {
                this.CalculateHandsAiming(
                                          weaponPosition,
                                          flipped,
                                          aimAngle,
                                          compWeaponExtensions);
            }
        }

        public override void DrawFeet(Quaternion bodyQuat, Quaternion footQuat, Vector3 rootLoc, bool portrait)
        {
            if (portrait && !this.CompAnimator.AnimatorOpen)
            {
                return;
            }

            Quaternion drawQuat = this.IsMoving ? footQuat : bodyQuat;

            Rot4 rot = this.BodyFacing;

            // Basic values
            BodyAnimDef body      = this.CompAnimator.BodyAnim;
            JointLister groundPos = this.GetJointPositions(
                                                           body.hipOffsets[rot.AsInt],
                                                           body.hipOffsets[Rot4.North.AsInt].x);

            Vector3      rightFootCycle = Vector3.zero;
            Vector3      leftFootCycle  = Vector3.zero;
            float        footAngleRight = 0;
            float        footAngleLeft  = 0;
            float        offsetJoint    = 0;
            WalkCycleDef cycle          = this.CompAnimator.WalkCycle;
            if (cycle != null)
            {
                offsetJoint = cycle.HipOffsetHorizontalX.Evaluate(this.MovedPercent);

                this.DoWalkCycleOffsets(
                                        ref rightFootCycle,
                                        ref leftFootCycle,
                                        ref footAngleRight,
                                        ref footAngleLeft,
                                        ref offsetJoint,
                                        cycle.FootPositionX,
                                        cycle.FootPositionZ,
                                        cycle.FootAngle);
            }

            this.GetMeshesFoot(out Mesh footMeshRight, out Mesh footMeshLeft);

            Material matRight;
            Material matLeft;

            if (MainTabWindow_Animator.Colored)
            {
                matRight = this.CompAnimator.PawnBodyGraphic?.FootGraphicRightCol?.MatAt(rot);
                matLeft  = this.CompAnimator.PawnBodyGraphic?.FootGraphicLeftCol?.MatAt(rot);
            }
            else
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

            bool drawRight = matRight != null && this.CompAnimator.BodyStat.FootRight != PartStatus.Missing;

            bool drawLeft = matLeft != null && this.CompAnimator.BodyStat.FootLeft != PartStatus.Missing;

            groundPos.LeftJoint  = drawQuat           * groundPos.LeftJoint;
            groundPos.RightJoint = drawQuat           * groundPos.RightJoint;
            leftFootCycle        = drawQuat           * leftFootCycle;
            rightFootCycle       = drawQuat           * rightFootCycle;
            Vector3 ground       = rootLoc + drawQuat * new Vector3(0, 0, OffsetGroundZ);

            this.feetTweener.FootPositions[0] = ground + groundPos.LeftJoint  + leftFootCycle;
            this.feetTweener.FootPositions[1] = ground + groundPos.RightJoint + rightFootCycle;

            this.feetTweener.PreFootPosCalculation();


            if (drawLeft)
            {
                GenDraw.DrawMeshNowOrLater(
                                           footMeshLeft,
                                           this.feetTweener.TweenedFootPos[0],
                                           drawQuat * Quaternion.AngleAxis(footAngleLeft, Vector3.up),
                                           matLeft,
                                           portrait);
            }

            if (drawRight)
            {
                GenDraw.DrawMeshNowOrLater(
                                           footMeshRight,
                                           this.feetTweener.TweenedFootPos[1],
                                           drawQuat * Quaternion.AngleAxis(footAngleRight, Vector3.up),
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
                                           ground + groundPos.LeftJoint +
                                           new Vector3(offsetJoint, -0.301f, 0),
                                           drawQuat * Quaternion.AngleAxis(0, Vector3.up),
                                           centerMat,
                                           portrait);

                GenDraw.DrawMeshNowOrLater(
                                           footMeshRight,
                                           ground + groundPos.RightJoint +
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

        public override void DrawHands(Quaternion bodyQuat, Vector3 drawPos,
                                       bool       portrait,
                                       bool       carrying = false)
        {
            if (portrait && !this.CompAnimator.AnimatorOpen)
            {
                return;
            }

            if (!this.CompAnimator.Props.bipedWithHands)
            {
                return;
            }

            // return if hands already drawn on carrything
            if (this.CarryStuff() && !carrying)
            {
                return;
            }

            BodyAnimDef body = this.CompAnimator.BodyAnim;

            Rot4 rot = this.BodyFacing;

            JointLister shoulperPos = this.GetJointPositions(
                                                             body.shoulderOffsets[rot.AsInt],
                                                             body.shoulderOffsets[Rot4.North.AsInt].x,
                                                             carrying);

            float        handSwingAngle = 0f;
            float        shoulderAngle  = 0f;
            Vector3      rightHandCycle = Vector3.zero;
            Vector3      leftHandCycle  = Vector3.zero;
            WalkCycleDef cycle          = this.CompAnimator.WalkCycle;

            if (cycle != null)
            {
                float offsetJoint = cycle.ShoulderOffsetHorizontalX.Evaluate(this.MovedPercent);

                this.DoWalkCycleOffsets(
                                        body.armLength,
                                        ref rightHandCycle,
                                        ref leftHandCycle,
                                        ref shoulderAngle,
                                        ref handSwingAngle,
                                        ref shoulperPos,
                                        carrying,
                                        cycle.HandsSwingAngle,
                                        offsetJoint);
            }

            this.DoAttackAnimationHandOffsets(ref handSwingAngle, ref rightHandCycle, false);

            Mesh handsMesh = this.HandMesh;

            Material matLeft  = this.LeftHandMat;
            Material matRight = this.RightHandMat;

            if (MainTabWindow_Animator.Colored)
            {
                matLeft  = this.CompAnimator.PawnBodyGraphic?.HandGraphicLeftCol?.MatSingle;
                matRight = this.CompAnimator.PawnBodyGraphic?.HandGraphicRightCol?.MatSingle;
            }
            else if (!carrying)
            {
                switch (rot.AsInt)
                {
                    case 1:
                        matLeft = this.LeftHandShadowMat;
                        break;

                    case 3:
                        matRight = this.RightHandShadowMat;
                        break;
                }
            }

            bool drawLeft  = matLeft  != null && this.CompAnimator.BodyStat.HandLeft  != PartStatus.Missing;
            bool drawRight = matRight != null && this.CompAnimator.BodyStat.HandRight != PartStatus.Missing;

            if (drawLeft)
            {
                Quaternion quat;
                Vector3    position;
                if (!this.IsMoving && this._secondHandPosition != Vector3.zero)
                {
                    position = this._secondHandPosition;
                    quat     = this.weaponQuat;
                }
                else
                {
                    shoulperPos.LeftJoint = bodyQuat * shoulperPos.LeftJoint;
                    leftHandCycle         = bodyQuat * leftHandCycle.RotatedBy(-handSwingAngle - shoulderAngle);

                    position = drawPos + shoulperPos.LeftJoint + leftHandCycle;
                    quat     = bodyQuat * Quaternion.AngleAxis(-handSwingAngle, Vector3.up);
                }

                TweenThing handLeft = TweenThing.HandLeft;
                this.DrawTweenedHand(handLeft, handsMesh, matLeft, quat, portrait, position);
            }

            if (drawRight)
            {
                TweenThing handRight = TweenThing.HandRight;
                Quaternion quat;
                Vector3    position;
                if (this._firstHandPosition != Vector3.zero)
                {
                    quat     = this.weaponQuat;
                    position = this._firstHandPosition;
                }
                else
                {
                    shoulperPos.RightJoint = bodyQuat * shoulperPos.RightJoint;
                    rightHandCycle         =
                    bodyQuat * rightHandCycle.RotatedBy(handSwingAngle - shoulderAngle);

                    position = drawPos + shoulperPos.RightJoint + rightHandCycle;
                    quat     = bodyQuat * Quaternion.AngleAxis(handSwingAngle, Vector3.up);
                }

                this.DrawTweenedHand(handRight, handsMesh, matRight, quat, portrait, position);
            }

            if (MainTabWindow_Animator.Develop)
            {
                // for debug
                Material centerMat = GraphicDatabase.Get<Graphic_Single>(
                                                                         "Hands/Human_Hand_dev",
                                                                         ShaderDatabase.CutoutSkin,
                                                                         Vector2.one,
                                                                         Color.white).MatSingle;

                GenDraw.DrawMeshNowOrLater(
                                           handsMesh,
                                           drawPos + shoulperPos.LeftJoint + new Vector3(0, -0.301f, 0),
                                           bodyQuat * Quaternion.AngleAxis(-shoulderAngle, Vector3.up),
                                           centerMat,
                                           portrait);

                GenDraw.DrawMeshNowOrLater(
                                           handsMesh,
                                           drawPos + shoulperPos.RightJoint + new Vector3(0, 0.301f, 0),
                                           bodyQuat * Quaternion.AngleAxis(-shoulderAngle, Vector3.up),
                                           centerMat,
                                           portrait);
            }
        }

        public override void Initialize()
        {
            this.Flasher     = this.Pawn.Drawer.renderer.graphics.flasher;
            this.handTweener = new PawnHandsTweener();
            this.feetTweener = new PawnFeetTweener();
            base.Initialize();
        }

        public Quaternion QuatBody(Quaternion quat, float movedPercent)
        {
            if (this.CompAnimator.WalkCycle != null)
            {
                if (this.BodyFacing.IsHorizontal)
                {
                    quat *= Quaternion.AngleAxis(
                                                 (this.BodyFacing == Rot4.West ? -1 : 1)
                                               * this.CompAnimator.WalkCycle.BodyAngle.Evaluate(movedPercent),
                                                 Vector3.up);
                }
                else
                {
                    quat *= Quaternion.AngleAxis(
                                                 (this.BodyFacing == Rot4.South ? -1 : 1)
                                               * this.CompAnimator.WalkCycle.BodyAngleVertical.Evaluate(movedPercent),
                                                 Vector3.up);
                }
            }

            return quat;
        }

        public virtual void SelectWalkcycle()
        {
            if (this.CompAnimator.AnimatorOpen)
            {
                this.CompAnimator.WalkCycle = MainTabWindow_Animator.EditorWalkcycle;
            }
            else if (this.Pawn.CurJob != null)
            {
                BodyAnimDef animDef = this.CompAnimator.BodyAnim;

                Dictionary<LocomotionUrgency, WalkCycleDef> cycles = animDef.walkCycles;

                if (cycles.Count > 0)
                {
                    if (cycles.TryGetValue(this.Pawn.CurJob.locomotionUrgency, out WalkCycleDef cycle))
                    {
                        if (cycle != null)
                        {
                            this.CompAnimator.WalkCycle = cycle;
                        }
                    }
                    else
                    {
                        this.CompAnimator.WalkCycle = animDef.walkCycles.FirstOrDefault().Value;
                    }
                }

                // switch (this.Pawn.CurJob.locomotionUrgency)
                // {
                // case LocomotionUrgency.None:
                // case LocomotionUrgency.Amble:
                // this.walkCycle = WalkCycleDefOf.Biped_Amble;
                // break;
                // case LocomotionUrgency.Walk:
                // this.walkCycle = WalkCycleDefOf.Biped_Walk;
                // break;
                // case LocomotionUrgency.Jog:
                // this.walkCycle = WalkCycleDefOf.Biped_Jog;
                // break;
                // case LocomotionUrgency.Sprint:
                // this.walkCycle = WalkCycleDefOf.Biped_Sprint;
                // break;
                // }
            }
        }

        public override void Tick(Rot4 bodyFacing, PawnGraphicSet graphics)
        {
            base.Tick(bodyFacing, graphics);

            BodyAnimator animator = this.CompAnimator.BodyAnimator;
            if (animator != null)
            {
                this.IsMoving = animator.IsMoving(out this.MovedPercent);
            }

            // var curve = bodyFacing.IsHorizontal ? this.walkCycle.BodyOffsetZ : this.walkCycle.BodyOffsetVerticalZ;
            this.SelectWalkcycle();
            this._firstHandPosition  = Vector3.zero;
            this._secondHandPosition = Vector3.zero;
        }

        #endregion Public Methods

        #region Protected Methods

        protected void DoWalkCycleOffsets(ref Vector3 rightFoot,
                                          ref Vector3 leftFoot,
                                          ref float   footAngleRight,
                                          ref float   footAngleLeft,
                                          ref float   offsetJoint,
                                          SimpleCurve offsetX,
                                          SimpleCurve offsetZ,
                                          SimpleCurve angle)
        {
            rightFoot      = Vector3.zero;
            leftFoot       = Vector3.zero;
            footAngleRight = 0;
            footAngleLeft  = 0;

            if (!this.IsMoving)
            {
                return;
            }

            float flot = this.MovedPercent;
            if (flot <= 0.5f)
            {
                flot += 0.5f;
            }
            else
            {
                flot -= 0.5f;
            }

            Rot4 rot = this.BodyFacing;
            if (rot.IsHorizontal)
            {
                rightFoot.x = offsetX.Evaluate(this.MovedPercent);
                leftFoot.x  = offsetX.Evaluate(flot);

                footAngleRight = angle.Evaluate(this.MovedPercent);
                footAngleLeft  = angle.Evaluate(flot);
                rightFoot.z    = offsetZ.Evaluate(this.MovedPercent);
                leftFoot.z     = offsetZ.Evaluate(flot);

                rightFoot.x += offsetJoint;
                leftFoot.x  += offsetJoint;

                if (rot == Rot4.West)
                {
                    rightFoot.x    *= -1f;
                    leftFoot.x     *= -1f;
                    footAngleLeft  *= -1f;
                    footAngleRight *= -1f;
                    offsetJoint    *= -1;
                }
            }
            else
            {
                rightFoot.z = offsetZ.Evaluate(this.MovedPercent);
                leftFoot.z  = offsetZ.Evaluate(flot);
                offsetJoint = 0;
            }

            // smaller steps for smaller pawns
            float bodySize = this.Pawn.def.race.baseBodySize;
            if (Math.Abs(bodySize - 1f) > 0.05f)
            {
                var   curve = new SimpleCurve {new CurvePoint(0f, 0.5f), new CurvePoint(1f, 1f)};
                float mod   = curve.Evaluate(bodySize);
                rightFoot.x *= mod;
                rightFoot.z *= mod;
                leftFoot.x  *= mod;
                leftFoot.z  *= mod;
            }
        }

        protected void GetMeshesFoot(out Mesh footMeshRight, out Mesh footMeshLeft)
        {
            Rot4 rot = this.BodyFacing;

            switch (rot.AsInt)
            {
                default:
                    footMeshRight = MeshPool.plane10;
                    footMeshLeft  = MeshPool.plane10Flip;
                    break;

                case 1:
                    footMeshRight = MeshPool.plane10;
                    footMeshLeft  = MeshPool.plane10;
                    break;

                case 3:
                    footMeshRight = MeshPool.plane10Flip;
                    footMeshLeft  = MeshPool.plane10Flip;
                    break;
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private void DoWalkCycleOffsets(
        float           armLength,
        ref Vector3     rightHand,
        ref Vector3     leftHand,
        ref float       shoulderAngle,
        ref float       handSwingAngle,
        ref JointLister shoulderPos,
        bool            carrying,
        SimpleCurve     cycleHandsSwingAngle,
        float           offsetJoint)
        {
            // Has the pawn something in his hands?
            if (carrying)
            {
                return;
            }

            Rot4 rot = this.BodyFacing;

            // Basic values if pawn is carrying stuff
            float x  = 0;
            float x2 = -x;
            float y  = Offsets.YOffset_Behind;
            float y2 = y;
            float z;
            float z2;

            // Offsets for hands from the pawn center
            z = z2 = -armLength;

            if (rot.IsHorizontal)
            {
                x = x2 = 0f;
                if (rot == Rot4.East)
                {
                    y2 = -0.5f;
                }
                else
                {
                    y = -0.05f;
                }
            }
            else if (rot == Rot4.North)
            {
                y  =  y2 = -0.02f;
                x  *= -1;
                x2 *= -1;
            }

            // Swing the hands, try complete the cycle
            if (this.IsMoving)
            {
                if (rot.IsHorizontal)
                {
                    float lookie = rot == Rot4.West ? -1f : 1f;
                    float f      = lookie * offsetJoint;

                    shoulderAngle = lookie * this.CompAnimator.WalkCycle?.shoulderAngle ?? 0f;

                    shoulderPos.RightJoint.x += f;
                    shoulderPos.LeftJoint.x  += f;

                    handSwingAngle = (rot == Rot4.West ? -1 : 1) * cycleHandsSwingAngle.Evaluate(this.MovedPercent);
                }
                else
                {
                    z  += cycleHandsSwingAngle.Evaluate(this.MovedPercent) / 500;
                    z2 -= cycleHandsSwingAngle.Evaluate(this.MovedPercent) / 500;

                    z  += this.CompAnimator.WalkCycle?.shoulderAngle / 800 ?? 0f;
                    z2 += this.CompAnimator.WalkCycle?.shoulderAngle / 800 ?? 0f;
                }
            }

            if (MainTabWindow_Animator.Panic || this.Pawn.Fleeing() || this.Pawn.IsBurning())
            {
                float offset   = 1f + armLength;
                x              *= offset;
                z              *= offset;
                x2             *= offset;
                z2             *= offset;
                handSwingAngle += 180f;
                shoulderAngle  =  0f;
            }

            rightHand = new Vector3(x,  y,  z);
            leftHand  = new Vector3(x2, y2, z2);
        }

        private void DrawTweenedHand(TweenThing tweenThing, Mesh  handsMesh, Material material, Quaternion quat,
                                     bool       portrait, Vector3 position)
        {
            this.handTweener.HandPositions[(int) tweenThing] = position;

            this.handTweener.PreHandPosCalculation(tweenThing, this.IsMoving);

            GenDraw.DrawMeshNowOrLater(
                                       handsMesh,
                                       this.handTweener.TweenedHandsPos[(int) tweenThing],
                                       quat,
                                       material,
                                       portrait);
        }

        #endregion Private Methods
    }
}