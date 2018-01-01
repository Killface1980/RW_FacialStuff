namespace FacialStuff
{
    using FacialStuff.Animator;
    using FacialStuff.Components;
    using JetBrains.Annotations;
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using Verse.AI;

    public class HumanBipedDrawer : PawnBodyDrawer
    {
        #region Protected Fields

        protected const float OffsetGround = -0.575f;

        protected DamageFlasher flasher;

        #endregion Protected Fields

        #region Public Methods

        public virtual bool Aiming()
        {
            Stance_Busy stanceBusy = this.Pawn.stances.curStance as Stance_Busy;
            return stanceBusy != null && !stanceBusy.neverAimWeapon && stanceBusy.focusTarg.IsValid;
        }

        public override void ApplyBodyWobble(ref Vector3 rootLoc, ref Vector3 footPos, ref Quaternion quat)
        {
            if (this.IsMoving)
            {
                SimpleCurve curve = this.CompAnimator.WalkCycle.BodyOffsetZ;
                float bam = curve.Evaluate(this.MovedPercent);

                // Log.Message(CompFace.Pawn + " - " + this.movedPercent + " - " + bam.ToString());
                rootLoc.z += bam;
                quat = this.QuatBody(quat, this.MovedPercent);
            }

            // Adds the leg length to the rootloc and relocates the feet to keep the pawn in center, e.g. for shields
            if (this.CompAnimator.bodyAnim != null)
            {
                float legModifier = this.CompAnimator.bodyAnim.extraLegLength;
                rootLoc.z += legModifier * 0.7f;
                footPos.z -= legModifier * 0.3f;
            }

            base.ApplyBodyWobble(ref rootLoc, ref footPos, ref quat);
        }

        public override List<Material> BodyBaseAt(
            [NotNull] PawnGraphicSet graphics,
            Rot4 bodyFacing,
            RotDrawMode bodyDrawType,
            MaxLayerToShow layer)
        {
            switch (layer)
            {
                case MaxLayerToShow.Naked: return this.CompAnimator?.NakedMatsBodyBaseAt(bodyFacing, bodyDrawType);
                case MaxLayerToShow.OnSkin: return this.CompAnimator?.UnderwearMatsBodyBaseAt(bodyFacing, bodyDrawType);
                default: return graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
            }

            return base.BodyBaseAt(graphics, bodyFacing, bodyDrawType, layer);
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
            Pawn pawn = this.Pawn;
            return (pawn.carryTracker == null || pawn.carryTracker.CarriedThing == null)
                   && (pawn.Drafted || pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon
                       || pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon);
        }

        public void DoAttackAnimationHandOffsets(ref float weaponAngle, ref Vector3 weaponPosition, bool flipped)
        {
            Pawn pawn = this.Pawn;
            if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
            {
                return;
            }

            if (pawn.RaceProps.ToolUser && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return;
            }

            // total weapon angle change during animation sequence
            int totalSwingAngle = 0;
            Vector3 currentOffset = this.CompAnimator.Jitterer.CurrentOffset;

            float jitterMax = this.CompAnimator.JitterMax;
            float magnitude = currentOffset.magnitude;
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
            int totalSwingAngle = 0;
            Vector3 currentOffset = this.CompAnimator.Jitterer.CurrentOffset;

            float jitterMax = this.CompAnimator.JitterMax;
            float magnitude = currentOffset.magnitude;
            float animationPhasePercent = magnitude / jitterMax;

            if (damageDef == DamageDefOf.Stab)
            {
                weaponPosition += currentOffset;

                // + new Vector3(0, 0, Mathf.Pow(this.CompFace.Jitterer.CurrentOffset.magnitude, 0.25f))/2;
            }
            else if (damageDef == DamageDefOf.Blunt || damageDef == DamageDefOf.Cut)
            {
                totalSwingAngle = 120;
                weaponPosition += currentOffset + new Vector3(0, 0, Mathf.Sin(magnitude * Mathf.PI / jitterMax) / 10);
            }

            weaponAngle += flipped ? -animationPhasePercent * totalSwingAngle : animationPhasePercent * totalSwingAngle;
        }

        public override void DrawBody(
            PawnWoundDrawer woundDrawer,
            Vector3 rootLoc,
            Quaternion quat,
            RotDrawMode bodyDrawType,
            bool renderBody,
            bool portrait)
        {
            // renderBody is AFAIK only used for beds, so ignore it and undress
            if (renderBody || Controller.settings.IgnoreRenderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += Offsets.YOffset_Body;

                Mesh bodyMesh = this.GetPawnMesh(true, portrait);

                List<Material> bodyBaseAt = null;
                bool flag = true;
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

                        bodyBaseAt = this.BodyBaseAt(this.Graphics, this.bodyFacing, bodyDrawType, layer);
                        flag = false;
                    }
                }

                if (flag)
                {
                    bodyBaseAt = this.Graphics.MatsBodyBaseAt(this.bodyFacing, bodyDrawType);
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
                    drawLoc.y += Offsets.YOffset_Wounds;

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

            if (pawn.stances.curStance is Stance_Busy stance_Busy && !stance_Busy.neverAimWeapon
                && stance_Busy.focusTarg.IsValid)
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
                drawLoc.y += Offsets.YOffset_PrimaryEquipmentOver;

                // default weapon angle axis is upward, but all weapons are facing right, so we turn base weapon angle by 90°
                this.DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, rootLoc, num, portrait);
            }
            else if (this.CarryWeaponOpenly() || MainTabWindow_Animator.Equipment)
            {
                float aimAngle = 143f;
                Vector3 drawLoc2 = rootLoc;
                Rot4 rotation = this.bodyFacing;
                if (rotation.IsHorizontal)
                {
                    if (rotation == Rot4.East)
                    {
                        drawLoc2 += new Vector3(0.2f, Offsets.YOffset_PrimaryEquipmentOver, -0.22f);
                    }
                    else if (rotation == Rot4.West)
                    {
                        drawLoc2 += new Vector3(-0.2f, Offsets.YOffset_PrimaryEquipmentOver, -0.22f);
                        aimAngle = 217f;
                    }
                }
                else if (rotation == Rot4.North)
                {
                    drawLoc2 += new Vector3(0, Offsets.YOffset_PrimaryEquipmentUnder, -0.11f);
                }
                else if (rotation == Rot4.South)
                {
                    drawLoc2 += new Vector3(0, Offsets.YOffset_PrimaryEquipmentOver, -0.22f);
                }

                this.DrawEquipmentAiming(pawn.equipment.Primary, drawLoc2, rootLoc, aimAngle, portrait);
            }
        }

        // Verse.PawnRenderer
        public void DrawEquipmentAiming(
            Thing equipment,
            Vector3 weaponDrawLoc,
            Vector3 rootLoc,
            float aimAngle,
            bool portrait)
        {
            // New
            aimAngle -= 90f;
            float weaponAngle = aimAngle;

            Mesh weaponMesh;
            bool flipped;
            Vector3 weaponPositionOffset = Vector3.zero;
            bool aiming = this.Aiming();

            CompProperties_WeaponExtensions compWeaponExtensions =
                this.Pawn.equipment.Primary.def.GetCompProperties<CompProperties_WeaponExtensions>();

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

            // weapon angle and position offsets based on current attack keyframes sequence
            this.DoAttackAnimationOffsets(ref weaponAngle, ref weaponPositionOffset, flipped);

            Graphic_StackCount graphic_StackCount = equipment.Graphic as Graphic_StackCount;
            Material matSingle = graphic_StackCount != null
                                     ? graphic_StackCount.SubGraphicForStackCount(1, equipment.def).MatSingle
                                     : equipment.Graphic.MatSingle;

            GenDraw.DrawMeshNowOrLater(
                weaponMesh,
                weaponDrawLoc + weaponPositionOffset,
                Quaternion.AngleAxis(weaponAngle, Vector3.up),
                matSingle,
                portrait);

            // Now the remaining hands if possible
            if (this.CompAnimator.Props.bipedWithHands && Controller.settings.UseHands)
            {
                this.DrawHandsAiming(
                    weaponDrawLoc + weaponPositionOffset,
                    rootLoc,
                    flipped,
                    weaponAngle,
                    compWeaponExtensions,
                    false);
            }
        }

        public override void DrawFeet(Vector3 rootLoc, bool portrait)
        {
            if (portrait && !this.CompAnimator.AnimatorOpen)
            {
                return;
            }

            Vector3 ground = rootLoc;
            ground.z += OffsetGround;

            float footAngleRight = 0f;
            float footAngleLeft = 0f;
            Rot4 rot = this.bodyFacing;

            // Basic values
            BodyAnimDef body = this.CompAnimator.bodyAnim;
            JointLister groundPos = this.GetJointPositions(
                body.hipOffsets[rot.AsInt],
                body.hipOffsets[Rot4.North.AsInt].x);

            Vector3 rightFootAnim = Vector3.zero;
            Vector3 leftFootAnim = Vector3.zero;

            float offsetJoint = this.CompAnimator.WalkCycle.HipOffsetHorizontalX.Evaluate(this.MovedPercent);
            WalkCycleDef cycle = this.CompAnimator.WalkCycle;

            this.DoWalkCycleOffsets(
                ref rightFootAnim,
                ref leftFootAnim,
                ref footAngleRight,
                ref footAngleLeft,
                ref offsetJoint,
                cycle.FootPositionX,
                cycle.FootPositionZ,
                cycle.FootAngle);

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
                        matRight = this.flasher.GetDamagedMat(
                            this.CompAnimator.PawnBodyGraphic?.FootGraphicRight?.MatAt(rot));
                        matLeft = this.flasher.GetDamagedMat(
                            this.CompAnimator.PawnBodyGraphic?.FootGraphicLeft?.MatAt(rot));
                        break;

                    case 1:
                        matRight = this.flasher.GetDamagedMat(
                            this.CompAnimator.PawnBodyGraphic?.FootGraphicRight?.MatAt(rot));
                        matLeft = this.flasher.GetDamagedMat(
                            this.CompAnimator.PawnBodyGraphic?.FootGraphicLeftShadow?.MatAt(rot));
                        break;

                    case 3:
                        matRight = this.flasher.GetDamagedMat(
                            this.CompAnimator.PawnBodyGraphic?.FootGraphicRightShadow?.MatAt(rot));
                        matLeft = this.flasher.GetDamagedMat(
                            this.CompAnimator.PawnBodyGraphic?.FootGraphicLeft?.MatAt(rot));
                        break;
                }
            }
            else
            {
                matRight = this.CompAnimator.PawnBodyGraphic?.FootGraphicRightCol?.MatAt(rot);
                matLeft = this.CompAnimator.PawnBodyGraphic?.FootGraphicLeftCol?.MatAt(rot);
            }

            bool drawRight = matRight != null && this.CompAnimator.bodyStat.FootRight != PartStatus.Missing;

            bool drawLeft = matLeft != null && this.CompAnimator.bodyStat.FootLeft != PartStatus.Missing;

            if (drawLeft)
            {
                GenDraw.DrawMeshNowOrLater(
                    footMeshLeft,
                    (ground + groundPos.leftJoint + leftFootAnim).RotatedBy(bodyAngle),
                    Quaternion.AngleAxis(bodyAngle + footAngleLeft, Vector3.up),
                    matLeft,
                    portrait);
            }

            if (drawRight)
            {
                GenDraw.DrawMeshNowOrLater(
                    footMeshRight,
                    (ground + groundPos.rightJoint + rightFootAnim).RotatedBy(bodyAngle),
                    Quaternion.AngleAxis(bodyAngle + footAngleRight, Vector3.up),
                    matRight,
                    portrait);
            }

            if (MainTabWindow_Animator.Develop)
            {
                // for debug
                Material centerMat = GraphicDatabase
                    .Get<Graphic_Single>("Hands/Ground", ShaderDatabase.Transparent, Vector2.one, Color.red).MatSingle;

                GenDraw.DrawMeshNowOrLater(
                    footMeshLeft,
                    ground.RotatedBy(bodyAngle) + groundPos.leftJoint + new Vector3(offsetJoint, -0.301f, 0),
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

        public override void DrawHands(
            Vector3 drawPos,
            bool portrait,
            bool carrying,
            HandsToDraw drawSide = HandsToDraw.Both)
        {
            if (portrait && !this.CompAnimator.AnimatorOpen)
            {
                return;
            }

            // return if hands already drawn on weapon
            if (this.Pawn.equipment.Primary != null
                && this.Pawn.equipment.Primary.def.HasComp(typeof(CompWeaponExtensions)))
            {
                if (this.CarryWeaponOpenly())
                {
                    if (drawSide == HandsToDraw.Both)
                    {
                        return;
                    }
                }
            }

            // return if hands already drawn on carrything
            if (this.CarryStuff() && !carrying)
            {
                return;
            }

            BodyAnimDef body = this.CompAnimator.bodyAnim;

            Rot4 rot = this.bodyFacing;

            JointLister shoulperPos = this.GetJointPositions(
                body.shoulderOffsets[rot.AsInt],
                body.shoulderOffsets[Rot4.North.AsInt].x,
                Offsets.YOffset_PostHead,
                carrying);

            // DoCarriedOffsets(ref shoulperPos);

            // Center = drawpos of carryThing
            Vector3 center = drawPos;

            float handSwingAngle = 0f;
            float shoulderAngle = 0f;

            Vector3 rightHand = Vector3.zero;
            Vector3 leftHand = Vector3.zero;

            // Todo: inclide this
            WalkCycleDef cycle = this.CompAnimator.WalkCycle;
            float offsetJoint = cycle.ShoulderOffsetHorizontalX.Evaluate(this.MovedPercent);

            this.DoWalkCycleOffsets(
                body.armLength,
                ref rightHand,
                ref leftHand,
                ref shoulderAngle,
                ref handSwingAngle,
                ref shoulperPos,
                carrying,
                cycle.HandsSwingAngle,
                offsetJoint);

            this.DoAttackAnimationHandOffsets(ref handSwingAngle, ref rightHand, false);

            Mesh handsMesh = this.HandMesh;
            float bodyAngle = 0f;
            Pawn pawn = this.Pawn;
            if (pawn.Downed || pawn.Dead)
            {
                bodyAngle = pawn.Drawer.renderer.wiggler.downedAngle;
            }

            Material matLeft =
                this.flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.HandGraphicLeft?.MatSingle);
            Material matRight =
                this.flasher.GetDamagedMat(this.CompAnimator.PawnBodyGraphic?.HandGraphicRight?.MatSingle);
            if (!MainTabWindow_Animator.Colored)
            {
                if (!carrying)
                {
                    switch (rot.AsInt)
                    {
                        case 1:
                            matLeft = this.flasher.GetDamagedMat(
                                this.CompAnimator.PawnBodyGraphic?.HandGraphicLeftShadow?.MatAt(rot));
                            break;

                        case 3:
                            matRight = this.flasher.GetDamagedMat(
                                this.CompAnimator.PawnBodyGraphic?.HandGraphicRightShadow?.MatAt(rot));
                            break;
                    }
                }
            }
            else
            {
                matLeft = this.CompAnimator.PawnBodyGraphic?.HandGraphicLeftCol?.MatSingle;
                matRight = this.CompAnimator.PawnBodyGraphic?.HandGraphicRightCol?.MatSingle;
            }

            bool drawRight = matRight != null && drawSide != HandsToDraw.LeftHand
                             && this.CompAnimator.bodyStat.HandRight != PartStatus.Missing;

            bool drawLeft = matLeft != null && drawSide != HandsToDraw.RightHand
                            && this.CompAnimator.bodyStat.HandLeft != PartStatus.Missing;

            if (drawRight)
            {
                GenDraw.DrawMeshNowOrLater(
                    handsMesh,
                    (drawPos + shoulperPos.rightJoint + rightHand.RotatedBy(handSwingAngle - shoulderAngle))
                    .RotatedBy(bodyAngle),
                    Quaternion.AngleAxis(bodyAngle + handSwingAngle, Vector3.up),
                    matRight,
                    portrait);
            }

            if (drawLeft)
            {
                GenDraw.DrawMeshNowOrLater(
                    handsMesh,
                    (drawPos + shoulperPos.leftJoint + leftHand.RotatedBy(-handSwingAngle - shoulderAngle))
                    .RotatedBy(bodyAngle),
                    Quaternion.AngleAxis(bodyAngle - handSwingAngle, Vector3.up),
                    matLeft,
                    portrait);
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
                    drawPos + shoulperPos.leftJoint + new Vector3(0, -0.301f, 0),
                    Quaternion.AngleAxis(-shoulderAngle, Vector3.up),
                    centerMat,
                    portrait);

                GenDraw.DrawMeshNowOrLater(
                    handsMesh,
                    drawPos + shoulperPos.rightJoint + new Vector3(0, 0.301f, 0),
                    Quaternion.AngleAxis(-shoulderAngle, Vector3.up),
                    centerMat,
                    portrait);
            }
        }

        public void DrawHandsAiming(
            Vector3 weaponPosition,
            Vector3 rootLoc,
            bool flipped,
            float weaponAngle,
            [CanBeNull] CompProperties_WeaponExtensions compWeaponExtensions,
            bool portrait)
        {
            if (compWeaponExtensions == null)
            {
                return;
            }

            Material matLeft = this.CompAnimator.PawnBodyGraphic?.HandGraphicLeft?.MatSingle;
            Material matRight = this.CompAnimator.PawnBodyGraphic?.HandGraphicRight?.MatSingle;

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

                    GenDraw.DrawMeshNowOrLater(
                        handsMesh,
                        weaponPosition + new Vector3(x, y, z).RotatedBy(weaponAngle),
                        Quaternion.AngleAxis(weaponAngle, Vector3.up),
                        matRight,
                        portrait);
                }
                else
                {
                    this.DrawHands(rootLoc, portrait, false, HandsToDraw.RightHand);
                }
            }

            if (matLeft != null)
            {
                if (this.IsMoving)
                {
                    // hold the weapon with only one hand while moving
                    this.DrawHands(rootLoc, portrait, false, HandsToDraw.LeftHand);
                }
                else
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

                        GenDraw.DrawMeshNowOrLater(
                            handsMesh,
                            weaponPosition + new Vector3(x2, y2, z2).RotatedBy(weaponAngle),
                            Quaternion.AngleAxis(weaponAngle, Vector3.up),
                            matLeft,
                            portrait);
                    }
                    else
                    {
                        this.DrawHands(rootLoc, portrait, false, HandsToDraw.LeftHand);
                    }
                }
            }

            if (MainTabWindow_Animator.Develop)
            {
                //// for debug
                Material centerMat = GraphicDatabase.Get<Graphic_Single>(
                    "Hands/Human_Hand_dev",
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    Color.white).MatSingle;

                UnityEngine.Graphics.DrawMesh(
                    handsMesh,
                    weaponPosition + new Vector3(0, 0.001f, 0),
                    Quaternion.AngleAxis(weaponAngle, Vector3.up),
                    centerMat,
                    0);
            }
        }

        public override void Initialize()
        {
            this.flasher = this.Pawn.Drawer.renderer.graphics.flasher;

            base.Initialize();
        }

        public Quaternion QuatBody(Quaternion quat, float movedPercent)
        {
            if (this.bodyFacing.IsHorizontal)
            {
                quat *= Quaternion.AngleAxis(
                    (this.bodyFacing == Rot4.West ? -1 : 1)
                    * this.CompAnimator.WalkCycle.BodyAngle.Evaluate(movedPercent),
                    Vector3.up);
            }
            else
            {
                quat *= Quaternion.AngleAxis(
                    (this.bodyFacing == Rot4.South ? -1 : 1)
                    * this.CompAnimator.WalkCycle.BodyAngleVertical.Evaluate(movedPercent),
                    Vector3.up);
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
                BodyAnimDef animDef = this.CompAnimator.bodyAnim;
                if (animDef != null)
                {
                    Dictionary<LocomotionUrgency, WalkCycleDef> cycles = animDef.walkCycles;

                    if (cycles != null)
                    {
                        if (cycles.TryGetValue(this.Pawn.CurJob.locomotionUrgency, out WalkCycleDef cycle))
                        {
                            if (cycle != null)
                            {
                                this.CompAnimator.WalkCycle = cycle;
                            }
                        }
                        else if (cycles.Count > 0)
                        {
                            this.CompAnimator.WalkCycle = animDef.walkCycles.FirstOrDefault().Value;
                        }
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
        }

        #endregion Public Methods

        #region Protected Methods

        protected void DoWalkCycleOffsets(
            ref Vector3 rightFoot,
            ref Vector3 leftFoot,
            ref float footAngleRight,
            ref float footAngleLeft,
            ref float offsetJoint,
            SimpleCurve offsetX,
            SimpleCurve offsetZ,
            SimpleCurve angle)
        {
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

            Rot4 rot = this.bodyFacing;
            if (rot.IsHorizontal)
            {
                rightFoot.x = offsetX.Evaluate(this.MovedPercent);
                leftFoot.x = offsetX.Evaluate(flot);

                footAngleRight = angle.Evaluate(this.MovedPercent);
                footAngleLeft = angle.Evaluate(flot);
                rightFoot.z = offsetZ.Evaluate(this.MovedPercent);
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
                rightFoot.z = offsetZ.Evaluate(this.MovedPercent);
                leftFoot.z = offsetZ.Evaluate(flot);
                offsetJoint = 0;
            }
        }

        protected void GetMeshesFoot(out Mesh footMeshRight, out Mesh footMeshLeft)
        {
            Rot4 rot = this.bodyFacing;

            switch (rot.AsInt)
            {
                default:
                    footMeshRight = MeshPool.plane10;
                    footMeshLeft = MeshPool.plane10Flip;
                    break;

                case 1:
                    footMeshRight = MeshPool.plane10;
                    footMeshLeft = MeshPool.plane10;
                    break;

                case 3:
                    footMeshRight = MeshPool.plane10Flip;
                    footMeshLeft = MeshPool.plane10Flip;
                    break;
            }
        }
        #endregion Protected Methods

        #region Private Methods

        private void DoCarriedOffsets(ref JointLister shoulperPos)
        {
            if (this.bodyFacing == Rot4.North)
            {
                shoulperPos.rightJoint.y = shoulperPos.leftJoint.y;
            }
            else
            {
                shoulperPos.leftJoint.y = shoulperPos.rightJoint.y;
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
            SimpleCurve cycleHandsSwingAngle,
            float offsetJoint)
        {
            // Has the pawn something in his hands?
            if (carrying)
            {
                return;
            }

            Rot4 rot = this.bodyFacing;

            // Basic values if pawn is carrying stuff
            float x = 0;
            float x2 = -x;
            float y = Offsets.YOffset_Behind;
            float y2 = y;
            float z = 0;
            float z2 = -z;

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
                y = y2 = -0.02f;
                x *= -1;
                x2 *= -1;
            }

            // Swing the hands, try complete the cycle
            if (this.IsMoving)
            {
                if (rot.IsHorizontal)
                {
                    float lookie = rot == Rot4.West ? -1f : 1f;
                    float f = lookie * offsetJoint;

                    shoulderAngle = lookie * this.CompAnimator.WalkCycle.shoulderAngle;

                    shoulderPos.rightJoint.x += f;
                    shoulderPos.leftJoint.x += f;

                    handSwingAngle = (rot == Rot4.West ? -1 : 1) * cycleHandsSwingAngle.Evaluate(this.MovedPercent);
                }
                else
                {
                    z += cycleHandsSwingAngle.Evaluate(this.MovedPercent) / 500;
                    z2 -= cycleHandsSwingAngle.Evaluate(this.MovedPercent) / 500;

                    z += this.CompAnimator.WalkCycle.shoulderAngle / 800;
                    z2 += this.CompAnimator.WalkCycle.shoulderAngle / 800;
                }
            }

            if (MainTabWindow_Animator.Panic || this.Pawn.Fleeing() || this.Pawn.IsBurning())
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

        #endregion Private Methods
    }
}