using System;
using System.Collections.Generic;
using System.Linq;
using FacialStuff.Animator;
using FacialStuff.AnimatorWindows;
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


        #endregion Public Fields

        #region Protected Fields

        protected const float OffsetGroundZ = -0.575f;

        protected DamageFlasher Flasher;

        #endregion Protected Fields

        #region Private Fields


        //  private PawnFeetTweener feetTweener;
        private float _animatedPercent;
        private bool _isPosing;

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
                WalkCycleDef walkCycle = this.CompAnimator.WalkCycle;
                if (walkCycle != null)
                {
                    SimpleCurve curve = walkCycle.BodyOffsetZ;

                    float percent = this.MovedPercent;
                    float bam = curve.Evaluate(percent);
                    rootLoc.z += bam;
                    quat = this.QuatBody(quat, percent);

                    // Log.Message(CompFace.Pawn + " - " + this.movedPercent + " - " + bam.ToString());
                }
            }

            // Adds the leg length to the rootloc and relocates the feet to keep the pawn in center, e.g. for shields
            if (this.CompAnimator.BodyAnim != null)
            {
                float legModifier = this.CompAnimator.BodyAnim.extraLegLength;
                float posModB = legModifier * 0.85f;
                float posModF = -legModifier * 0.15f;
                Vector3 vector3 = new Vector3(0, 0, posModB);
                Vector3 vector4 = new Vector3(0, 0, posModF);

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


        public void DoAttackAnimationHandOffsets(ref List<float> weaponAngle, ref Vector3 weaponPosition, bool flipped)
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
            float angle = animationPhasePercent * totalSwingAngle;
            weaponAngle[0] += (flipped ? -1f : 1f) * angle;
            weaponAngle[1] += (flipped ? -1f : 1f) * angle;
        }


        public override void DrawApparel(Quaternion quat, Vector3 vector, bool renderBody, bool portrait)
        {
            if (portrait || renderBody && !this.CompAnimator.HideShellLayer || !renderBody
             && !Controller.settings.HideShellWhileRoofed && Controller.settings.IgnoreRenderBody)
            {
                for (int index = 0; index < this.Graphics.apparelGraphics.Count; index++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = this.Graphics.apparelGraphics[index];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        Mesh bodyMesh = this.GetPawnMesh(true, portrait);
                        Material material3 = apparelGraphicRecord.graphic.MatAt(this.BodyFacing);
                        material3 = this.Graphics.flasher.GetDamagedMat(material3);
                        GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, material3, portrait);

                        // possible fix for phasing apparel
                        vector.y += Offsets.YOffsetInterval_Clothes;
                    }
                }
            }
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

                        bodyBaseAt = this.BodyBaseAt(this.Graphics, this.BodyFacing, bodyDrawType, layer);
                        flag = false;
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
                    drawLoc.y += Offsets.YOffset_Wounds;

                    woundDrawer?.RenderOverBody(drawLoc, bodyMesh, quat, portrait);
                }
            }
        }
        /*
        public override void DrawEquipment(Vector3 rootLoc, bool portrait)
        {
            Pawn pawn = this.Pawn;

            if (portrait && !this.CompAnimator.AnyOpen())
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

                Vector3 b = new Vector3(0f, 0f, 0.4f).RotatedBy(aimAngle);
                Vector3 drawLoc = rootLoc + b;
                drawLoc.y += Offsets.YOffset_PrimaryEquipmentOver;

                // default weapon angle axis is upward, but all weapons are facing right, so we turn base weapon angle by 90°
                bool dummy = false;
                this.DrawEquipmentAiming(pawn.equipment.Primary, ref drawLoc, ref aimAngle, portrait,
                                         ref dummy);
            }
            else if (this.CarryWeaponOpenly() || MainTabWindow_WalkAnimator.Equipment)
            {
                float aimAngle = 143f;
                Vector3 drawLoc2 = rootLoc;
                Rot4 rotation = this.BodyFacing;

                switch (rotation.AsInt)
                {
                    case 0:
                        drawLoc2 += new Vector3(0, Offsets.YOffset_PrimaryEquipmentUnder, -0.11f);
                        aimAngle = 217f;
                        break;
                    case 1:
                        drawLoc2 += new Vector3(0.2f, Offsets.YOffset_PrimaryEquipmentOver, -0.22f);
                        break;
                    case 2:
                        drawLoc2 += new Vector3(0, Offsets.YOffset_PrimaryEquipmentOver, -0.22f);
                        break;
                    case 3:
                        drawLoc2 += new Vector3(-0.2f, Offsets.YOffset_PrimaryEquipmentOver, -0.22f);
                        aimAngle = 217f;
                        break;
                }

                bool dummy = false;
                this.DrawEquipmentAiming(pawn.equipment.Primary, ref drawLoc2, ref aimAngle, portrait,
                                         ref dummy);
            }
        }
        */
        // Verse.PawnRenderer

        public override void DrawFeet(Quaternion bodyQuat, Quaternion footQuat, Vector3 rootLoc, bool portrait)
        {
            if (portrait && !this.CompAnimator.AnyOpen())
            {
                return;
            }

            Quaternion drawQuat = this.IsMoving ? footQuat : bodyQuat;

            Rot4 rot = this.BodyFacing;

            // Basic values
            BodyAnimDef body = this.CompAnimator.BodyAnim;
            JointLister groundPos = this.GetJointPositions(
                                                           body.hipOffsets[rot.AsInt],
                                                           body.hipOffsets[Rot4.North.AsInt].x);

            Vector3 rightFootCycle = Vector3.zero;
            Vector3 leftFootCycle = Vector3.zero;
            float footAngleRight = 0;
            float footAngleLeft = 0;
            float offsetJoint = 0;
            WalkCycleDef cycle = this.CompAnimator.WalkCycle;
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

            if (MainTabWindow_BaseAnimator.Colored)
            {
                matRight = this.CompAnimator.PawnBodyGraphic?.FootGraphicRightCol?.MatAt(rot);
                matLeft = this.CompAnimator.PawnBodyGraphic?.FootGraphicLeftCol?.MatAt(rot);
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

            groundPos.LeftJoint = drawQuat * groundPos.LeftJoint;
            groundPos.RightJoint = drawQuat * groundPos.RightJoint;
            leftFootCycle = drawQuat * leftFootCycle;
            rightFootCycle = drawQuat * rightFootCycle;
            Vector3 ground = rootLoc + drawQuat * new Vector3(0, 0, OffsetGroundZ);

            if (drawLeft)
            {

                TweenThing leftFoot = TweenThing.FootLeft;
                this.CompAnimator.PartTweener.PartPositions[(int)leftFoot] = ground + groundPos.LeftJoint + leftFootCycle;
                this.CompAnimator.PartTweener.PreHandPosCalculation(leftFoot);
                GenDraw.DrawMeshNowOrLater(
                                           footMeshLeft, this.CompAnimator.PartTweener.TweenedPartsPos[(int)leftFoot],
                                           drawQuat * Quaternion.AngleAxis(footAngleLeft, Vector3.up),
                                           matLeft,
                                           portrait);
            }

            if (drawRight)
            {
                TweenThing rightFoot = TweenThing.FootRight;
                this.CompAnimator.PartTweener.PartPositions[(int)rightFoot] = ground + groundPos.RightJoint + rightFootCycle;
                this.CompAnimator.PartTweener.PreHandPosCalculation(rightFoot);
                GenDraw.DrawMeshNowOrLater(
                                           footMeshRight, this.CompAnimator.PartTweener.TweenedPartsPos[(int)rightFoot],
                                           drawQuat * Quaternion.AngleAxis(footAngleRight, Vector3.up),
                                           matRight,
                                           portrait);
            }

            if (MainTabWindow_BaseAnimator.Develop)
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
                                       bool portrait,
                                       bool carrying = false)
        {
            if (portrait && !this.CompAnimator.AnyOpen())
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

            List<float> handSwingAngle = new List<float> { 0f, 0f };
            List<float> shoulderAngle = new List<float> { 0f, 0f };
            Vector3 rightHand = Vector3.zero;
            Vector3 leftHand = Vector3.zero;
            WalkCycleDef walkCycle = this.CompAnimator.WalkCycle;
            PoseCycleDef poseCycle = this.CompAnimator.PoseCycle;

            if (walkCycle != null)
            {
                float offsetJoint = walkCycle.ShoulderOffsetHorizontalX.Evaluate(this.MovedPercent);

                this.DoWalkCycleOffsets(
                                        body.armLength,
                                        ref rightHand,
                                        ref leftHand,
                                        ref shoulderAngle,
                                        ref handSwingAngle,
                                        ref shoulperPos,
                                        carrying,
                                        walkCycle.HandsSwingAngle,
                                        offsetJoint);
            }

            if (poseCycle != null)
            {

                this.DoPoseCycleOffsets(ref rightHand,
                                        ref shoulderAngle,
                                        ref handSwingAngle, poseCycle);
            }

            this.DoAttackAnimationHandOffsets(ref handSwingAngle, ref rightHand, false);

            Mesh handsMesh = this.HandMesh;

            Material matLeft = this.LeftHandMat;
            Material matRight = this.RightHandMat;

            if (MainTabWindow_BaseAnimator.Colored)
            {
                matLeft = this.CompAnimator.PawnBodyGraphic?.HandGraphicLeftCol?.MatSingle;
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

            bool drawLeft = matLeft != null && this.CompAnimator.BodyStat.HandLeft != PartStatus.Missing;
            bool drawRight = matRight != null && this.CompAnimator.BodyStat.HandRight != PartStatus.Missing;

            if (drawLeft)
            {
                Quaternion quat;
                Vector3 position;
                if (!this.IsMoving && this.CompAnimator.SecondHandPosition != Vector3.zero)
                {
                    position = this.CompAnimator.SecondHandPosition;
                    quat = this.CompAnimator.WeaponQuat;
                }
                else
                {
                    shoulperPos.LeftJoint = bodyQuat * shoulperPos.LeftJoint;
                    leftHand = bodyQuat * leftHand.RotatedBy(-handSwingAngle[0] - shoulderAngle[0]);

                    position = drawPos + shoulperPos.LeftJoint + leftHand;
                    quat = bodyQuat * Quaternion.AngleAxis(-handSwingAngle[0], Vector3.up);
                }

                TweenThing handLeft = TweenThing.HandLeft;
                this.DrawTweenedHand(position, handsMesh, matLeft, quat, handLeft, portrait);
            }

            if (drawRight)
            {
                Quaternion quat;
                Vector3 position;
                if (this.CompAnimator.FirstHandPosition != Vector3.zero)
                {
                    quat = this.CompAnimator.WeaponQuat;
                    position = this.CompAnimator.FirstHandPosition;
                }
                else
                {
                    shoulperPos.RightJoint = bodyQuat * shoulperPos.RightJoint;
                    rightHand =
                    bodyQuat * rightHand.RotatedBy(handSwingAngle[1] - shoulderAngle[1]);

                    position = drawPos + shoulperPos.RightJoint + rightHand;
                    quat = bodyQuat * Quaternion.AngleAxis(handSwingAngle[1], Vector3.up);
                }

                TweenThing handRight = TweenThing.HandRight;
                this.DrawTweenedHand(position, handsMesh, matRight, quat, handRight, portrait);
            }

            if (MainTabWindow_BaseAnimator.Develop)
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
                                           bodyQuat * Quaternion.AngleAxis(-shoulderAngle[0], Vector3.up),
                                           centerMat,
                                           portrait);

                GenDraw.DrawMeshNowOrLater(
                                           handsMesh,
                                           drawPos + shoulperPos.RightJoint + new Vector3(0, 0.301f, 0),
                                           bodyQuat * Quaternion.AngleAxis(-shoulderAngle[1], Vector3.up),
                                           centerMat,
                                           portrait);
            }
        }

        public override void Initialize()
        {
            this.Flasher = this.Pawn.Drawer.renderer.graphics.flasher;
            this.CompAnimator.PartTweener = new PawnPartsTweener(this.Pawn);
            // this.feetTweener = new PawnFeetTweener();
            base.Initialize();
        }

        public Quaternion QuatBody(Quaternion quat, float movedPercent)
        {
            WalkCycleDef walkCycle = this.CompAnimator.WalkCycle;
            if (walkCycle != null)
            {
                if (this.BodyFacing.IsHorizontal)
                {
                    quat *= Quaternion.AngleAxis(
                                                 (this.BodyFacing == Rot4.West ? -1 : 1)
                                               * walkCycle.BodyAngle.Evaluate(movedPercent),
                                                 Vector3.up);
                }
                else
                {
                    quat *= Quaternion.AngleAxis(
                                                 (this.BodyFacing == Rot4.South ? -1 : 1)
                                               * walkCycle.BodyAngleVertical.Evaluate(movedPercent),
                                                 Vector3.up);
                }
            }

            return quat;
        }

        public virtual void SelectWalkcycle()
        {
            if (this.CompAnimator.AnimatorWalkOpen)
            {
                this.CompAnimator.WalkCycle = MainTabWindow_WalkAnimator.EditorWalkcycle;
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

        public virtual void SelectPosecycle()
        {
            if (this.CompAnimator.AnimatorPoseOpen)
            {
                this.CompAnimator.PoseCycle = MainTabWindow_PoseAnimator.EditorPosecycle;
            }
            else if (this.Pawn.CurJob != null)
            {
                BodyAnimDef animDef = this.CompAnimator.BodyAnim;

                List<PoseCycleDef> cycles = animDef.poseCycles;

                if (cycles.Count > 0)
                {
                    this.CompAnimator.PoseCycle = animDef.poseCycles.FirstOrDefault();
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
                this._isPosing = animator.IsPosing(out this._animatedPercent);
            }

            // var curve = bodyFacing.IsHorizontal ? this.walkCycle.BodyOffsetZ : this.walkCycle.BodyOffsetVerticalZ;
            this.SelectWalkcycle();
            this.SelectPosecycle();
            this.CompAnimator.FirstHandPosition = Vector3.zero;
            this.CompAnimator.SecondHandPosition = Vector3.zero;


            this.CompAnimator.PartTweener.Update(this.IsMoving, this.MovedPercent);

        }

        #endregion Public Methods

        #region Protected Methods

        protected void DoWalkCycleOffsets(ref Vector3 rightFoot,
                                          ref Vector3 leftFoot,
                                          ref float footAngleRight,
                                          ref float footAngleLeft,
                                          ref float offsetJoint,
                                          SimpleCurve offsetX,
                                          SimpleCurve offsetZ,
                                          SimpleCurve angle)
        {
            rightFoot = Vector3.zero;
            leftFoot = Vector3.zero;
            footAngleRight = 0;
            footAngleLeft = 0;

            if (!this.IsMoving)
            {
                return;
            }

            float percent = this.MovedPercent;
            float flot = percent;
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
                rightFoot.x = offsetX.Evaluate(percent);
                leftFoot.x = offsetX.Evaluate(flot);

                footAngleRight = angle.Evaluate(percent);
                footAngleLeft = angle.Evaluate(flot);
                rightFoot.z = offsetZ.Evaluate(percent);
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
                rightFoot.z = offsetZ.Evaluate(percent);
                leftFoot.z = offsetZ.Evaluate(flot);
                offsetJoint = 0;
            }

            // smaller steps for smaller pawns
            float bodySize = this.Pawn.def.race.baseBodySize;
            if (Math.Abs(bodySize - 1f) > 0.05f)
            {
                var curve = new SimpleCurve { new CurvePoint(0f, 0.5f), new CurvePoint(1f, 1f) };
                float mod = curve.Evaluate(bodySize);
                rightFoot.x *= mod;
                rightFoot.z *= mod;
                leftFoot.x *= mod;
                leftFoot.z *= mod;
            }
        }

        protected void GetMeshesFoot(out Mesh footMeshRight, out Mesh footMeshLeft)
        {
            Rot4 rot = this.BodyFacing;

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

        private void DoWalkCycleOffsets(
        float armLength,
        ref Vector3 rightHand,
        ref Vector3 leftHand,
        ref List<float> shoulderAngle,
        ref List<float> handSwingAngle,
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

            Rot4 rot = this.BodyFacing;

            // Basic values if pawn is carrying stuff
            float x = 0;
            float x2 = -x;
            float y = Offsets.YOffset_Behind;
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
                y = y2 = -0.02f;
                x *= -1;
                x2 *= -1;
            }

            // Swing the hands, try complete the cycle
            if (this.IsMoving)
            {
                WalkCycleDef walkCycle = this.CompAnimator.WalkCycle;
                float percent = this.MovedPercent;
                if (rot.IsHorizontal)
                {
                    float lookie = rot == Rot4.West ? -1f : 1f;
                    float f = lookie * offsetJoint;

                    shoulderAngle[0] = shoulderAngle[1] = lookie * walkCycle?.shoulderAngle ?? 0f;

                    shoulderPos.RightJoint.x += f;
                    shoulderPos.LeftJoint.x += f;

                    handSwingAngle[0] = handSwingAngle[1] = (rot == Rot4.West ? -1 : 1) * cycleHandsSwingAngle.Evaluate(percent);
                }
                else
                {
                    z += cycleHandsSwingAngle.Evaluate(percent) / 500;
                    z2 -= cycleHandsSwingAngle.Evaluate(percent) / 500;

                    z += walkCycle?.shoulderAngle / 800 ?? 0f;
                    z2 += walkCycle?.shoulderAngle / 800 ?? 0f;
                }
            }

            if (MainTabWindow_BaseAnimator.Panic || this.Pawn.Fleeing() || this.Pawn.IsBurning())
            {
                float offset = 1f + armLength;
                x *= offset;
                z *= offset;
                x2 *= offset;
                z2 *= offset;
                handSwingAngle[0] += 180f;
                handSwingAngle[1] += 180f;
                shoulderAngle[0] = shoulderAngle[1] = 0f;
            }

            rightHand = new Vector3(x, y, z);
            leftHand = new Vector3(x2, y2, z2);
        }

        private void DoPoseCycleOffsets(ref Vector3 rightHand,
                                        ref List<float> shoulderAngle,
                                        ref List<float> handSwingAngle, PoseCycleDef pose)
        {
            if (!this.CompAnimator.AnimatorPoseOpen)
            {
                return;
            }

            SimpleCurve cycleHandsSwingAngle = pose.HandsSwingAngle;
            SimpleCurve rHandX = pose.HandPositionX;
            SimpleCurve rHandZ = pose.HandPositionZ;



            Rot4 rot = this.BodyFacing;

            // Basic values if pawn is carrying stuff
            float x = 0;
            float y = Offsets.YOffset_Behind;
            float z;


            float percent = this._animatedPercent;
            PoseCycleDef poseCycle = this.CompAnimator.PoseCycle;
            float lookie = rot == Rot4.West ? -1f : 1f;

            shoulderAngle[1] = lookie * poseCycle?.shoulderAngle ?? 0f;

            handSwingAngle[1] = (rot == Rot4.West ? -1 : 1) * cycleHandsSwingAngle.Evaluate(percent);

            x = rHandX.Evaluate(percent) * lookie;
            z = rHandZ.Evaluate(percent);

            rightHand += new Vector3(x, 0, z);
        }

        private void DrawTweenedHand(Vector3 position, Mesh handsMesh, Material material, Quaternion quat,
                                     TweenThing tweenThing,
                                     bool portrait)
        {
            this.CompAnimator.PartTweener.PartPositions[(int)tweenThing] = position;

            this.CompAnimator.PartTweener.PreHandPosCalculation(tweenThing);

            GenDraw.DrawMeshNowOrLater(
                                       handsMesh, this.CompAnimator.PartTweener.TweenedPartsPos[(int)tweenThing],
                                       quat,
                                       material,
                                       portrait);
        }

        #endregion Private Methods
    }
}