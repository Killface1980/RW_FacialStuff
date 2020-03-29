using System;
using System.Collections.Generic;
using System.Linq;
using FacialStuff.Animator;
using FacialStuff.AnimatorWindows;
using FacialStuff.Harmony;
using FacialStuff.Tweener;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FacialStuff
{
    public class HumanBipedDrawer : PawnBodyDrawer
    {
        #region Protected Fields

        protected const float OffsetGroundZ = -0.575f;

        protected DamageFlasher Flasher;

        #endregion Protected Fields

        #region Private Fields

        //  private PawnFeetTweener feetTweener;
        private float _animatedPercent;

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

        public override void ApplyBodyWobble(ref Vector3 rootLoc, ref Vector3 footPos, ref Quaternion quat)
        {
            if (this.CompAnimator.IsMoving)
            {
                WalkCycleDef walkCycle = this.CompAnimator.WalkCycle;
                if (walkCycle != null)
                {
                    float bam = this.CompAnimator.BodyOffsetZ;

                    rootLoc.z += bam;
                    quat = this.QuatBody(quat, this.CompAnimator.MovedPercent);

                    // Log.Message(CompFace.Pawn + " - " + this.movedPercent + " - " + bam.ToString());
                }
            }

            // Adds the leg length to the rootloc and relocates the feet to keep the pawn in center, e.g. for shields
            if (this.CompAnimator.BodyAnim != null)
            {
                float legModifier = this.CompAnimator.BodyAnim.extraLegLength;
                float posModB = legModifier * 0.75f;
                float posModF = -legModifier * 0.25f;
                Vector3 vector3 = new Vector3(0, 0, posModB);
                Vector3 vector4 = new Vector3(0, 0, posModF);

                // No rotation when moving
                if (!this.CompAnimator.IsMoving)
                {
                    vector3 = quat * vector3;
                    vector4 = quat * vector4;
                }
                if (!this.CompAnimator.IsRider)
                {
                    rootLoc += vector3;
                }
                else
                {
                    footPos -= vector3;
                }
                footPos += vector4;

            }

            base.ApplyBodyWobble(ref rootLoc, ref footPos, ref quat);
        }

        public void ApplyEquipmentWobble(ref Vector3 rootLoc)
        {
            if (this.CompAnimator.IsMoving)
            {
                WalkCycleDef walkCycle = this.CompAnimator.WalkCycle;
                if (walkCycle != null)
                {
                    float bam = this.CompAnimator.BodyOffsetZ;
                    rootLoc.z += bam;

                    // Log.Message(CompFace.Pawn + " - " + this.movedPercent + " - " + bam.ToString());
                }
            }

            // Adds the leg length to the rootloc and relocates the feet to keep the pawn in center, e.g. for shields
            if (this.CompAnimator.BodyAnim != null)
            {
                float legModifier = this.CompAnimator.BodyAnim.extraLegLength;
                float posModB = legModifier * 0.85f;
                Vector3 vector3 = new Vector3(0, 0, posModB);

                rootLoc += vector3;
            }
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
            if (pawn.story != null && ((pawn.story.DisabledWorkTagsBackstoryAndTraits & WorkTags.Violent) != 0))
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
                foreach (ApparelGraphicRecord apparelGraphicRecord in this.Graphics.apparelGraphics)
                {
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Shell)
                    {
                        continue;
                    }

                    Mesh bodyMesh = this.GetPawnMesh(true, portrait);
                    Material material3 = apparelGraphicRecord.graphic.MatAt(this.BodyFacing);
                    material3 = this.Graphics.flasher.GetDamagedMat(material3);
                    GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, material3, portrait);

                    // possible fix for phasing apparel
                    vector.y += Offsets.YOffsetInterval_Clothes;
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
                Vector3 bodyLoc = rootLoc;
                bodyLoc.x += this.CompAnimator.BodyAnim?.offCenterX ?? 0f;
                bodyLoc.y += Offsets.YOffset_Body;

                if (bodyDrawType == RotDrawMode.Dessicated &&
                    !this.Pawn.RaceProps.Humanlike
                 && this.Pawn.Drawer.renderer.graphics.dessicatedGraphic != null && !portrait)
                {
                    this.Pawn.Drawer.renderer.graphics.dessicatedGraphic.Draw(bodyLoc, this.BodyFacing, this.Pawn);
                }
                else
                {
                    Mesh bodyMesh;
                    if (this.Pawn.RaceProps.Humanlike)
                    {
                        bodyMesh = this.GetPawnMesh(true, portrait);
                    }
                    else
                    {
                        bodyMesh = this.Pawn.Drawer.renderer.graphics.nakedGraphic.MeshAt(this.BodyFacing);
                    }

                    List<Material> bodyBaseAt = null;
                    bool flag = true;
                    if (!portrait && Controller.settings.HideShellWhileRoofed)
                    {
                        if (this.CompAnimator.InRoom && CompAnimator.HideShellLayer)
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

                    foreach (Material material in bodyBaseAt)
                    {
                        Material damagedMat = this.Graphics.flasher.GetDamagedMat(material);
                        GenDraw.DrawMeshNowOrLater(bodyMesh, bodyLoc, quat, damagedMat, portrait);
                        bodyLoc.y += Offsets.YOffsetInterval_Clothes;
                    }

                    if (bodyDrawType == RotDrawMode.Fresh)
                    {
                        Vector3 drawLoc = rootLoc;
                        drawLoc.y += Offsets.YOffset_Wounds;

                        woundDrawer?.RenderOverBody(drawLoc, bodyMesh, quat, portrait);
                    }
                }
            }
        }

        public override void DrawFeet(Quaternion bodyQuat, Quaternion footQuat, Vector3 rootLoc, bool portrait, float factor = 1f)
        {
            if (ShouldBeIgnored())
            {
                return;
            }
            /// No feet while sitting at a table
            Job curJob = this.Pawn.CurJob;
            if (curJob != null)
            {
                if (curJob.def == JobDefOf.Ingest && !this.Pawn.Rotation.IsHorizontal)
                {
                    if (curJob.targetB.IsValid)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Rot4 rotty = new Rot4(i);
                            IntVec3 intVec = this.Pawn.Position + rotty.FacingCell;
                            if (intVec == curJob.targetB)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            if (portrait && !HarmonyPatchesFS.AnimatorIsOpen())
            {
                return;
            }

            Quaternion drawQuat = this.CompAnimator.IsMoving ? footQuat : bodyQuat;

            Rot4 rot = this.BodyFacing;

            // Basic values
            BodyAnimDef body = this.CompAnimator.BodyAnim;
            if (body == null)
            {
                return;
            }

            JointLister groundPos = this.GetJointPositions(JointType.Hip,
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
                offsetJoint = cycle.HipOffsetHorizontalX.Evaluate(this.CompAnimator.MovedPercent);

                this.DoWalkCycleOffsets(
                                        ref rightFootCycle,
                                        ref leftFootCycle,
                                        ref footAngleRight,
                                        ref footAngleLeft,
                                        ref offsetJoint,
                                        cycle.FootPositionX,
                                        cycle.FootPositionZ,
                                        cycle.FootAngle, factor);
            }

            this.GetBipedMesh(out Mesh footMeshRight, out Mesh footMeshLeft);

            Material matRight;
            Material matLeft;

            if (MainTabWindow_BaseAnimator.Colored)
            {
                matRight = this.CompAnimator.PawnBodyGraphic?.FootGraphicRightCol?.MatAt(rot);
                matLeft = this.CompAnimator.PawnBodyGraphic?.FootGraphicLeftCol?.MatAt(rot);
            }
            else
            {
                Material rightFoot = this.CompAnimator.PawnBodyGraphic?.FootGraphicRight?.MatAt(rot);
                Material leftFoot = this.CompAnimator.PawnBodyGraphic?.FootGraphicLeft?.MatAt(rot);
                Material leftShadow = this.CompAnimator.PawnBodyGraphic?.FootGraphicLeftShadow?.MatAt(rot);
                Material rightShadow = this.CompAnimator.PawnBodyGraphic?.FootGraphicRightShadow?.MatAt(rot);

                switch (rot.AsInt)
                {
                    default:
                        matRight = this.Flasher.GetDamagedMat(rightFoot);
                        matLeft = this.Flasher.GetDamagedMat(leftFoot);
                        break;

                    case 1:
                        matRight = this.Flasher.GetDamagedMat(rightFoot);

                        matLeft = this.Flasher.GetDamagedMat(leftShadow);
                        break;

                    case 3:

                        matRight = this.Flasher.GetDamagedMat(rightShadow);
                        matLeft = this.Flasher.GetDamagedMat(leftFoot);
                        break;
                }
            }

            bool drawRight = matRight != null && this.CompAnimator.BodyStat.FootRight != PartStatus.Missing;

            bool drawLeft = matLeft != null && this.CompAnimator.BodyStat.FootLeft != PartStatus.Missing;

            groundPos.LeftJoint = drawQuat * groundPos.LeftJoint;
            groundPos.RightJoint = drawQuat * groundPos.RightJoint;
            leftFootCycle = drawQuat * leftFootCycle;
            rightFootCycle = drawQuat * rightFootCycle;
            Vector3 ground = rootLoc + drawQuat * new Vector3(0, 0, OffsetGroundZ) * factor;

            if (drawLeft)
            {
                // TweenThing leftFoot = TweenThing.FootLeft;
                // PawnPartsTweener tweener = this.CompAnimator.PartTweener;
                // if (tweener != null)
                {
                    Vector3 position = ground + (groundPos.LeftJoint + leftFootCycle) * factor;

                    // tweener.PartPositions[(int)leftFoot] = position;
                    // tweener.PreThingPosCalculation(leftFoot, spring: SpringTightness.Stff);

                    GenDraw.DrawMeshNowOrLater(
                                               footMeshLeft,
                                               position, // tweener.TweenedPartsPos[(int)leftFoot],
                                               drawQuat * Quaternion.AngleAxis(footAngleLeft, Vector3.up),
                                               matLeft,
                                               portrait);
                }
            }

            if (drawRight)
            {
                // TweenThing rightFoot = TweenThing.FootRight;
                // PawnPartsTweener tweener = this.CompAnimator.PartTweener;
                // if (tweener != null)
                // {
                Vector3 position = ground + (groundPos.RightJoint + rightFootCycle) * factor;

                // tweener.PartPositions[(int)rightFoot] = position;
                //     tweener.PreThingPosCalculation(rightFoot, spring: SpringTightness.Stff);
                GenDraw.DrawMeshNowOrLater(
                                           footMeshRight,
                                           position, // tweener.TweenedPartsPos[(int)rightFoot],
                                           drawQuat * Quaternion.AngleAxis(footAngleRight, Vector3.up),
                                           matRight,
                                           portrait);

                // }
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

                Material hipMat = GraphicDatabase
                    .Get<Graphic_Single>("Hands/Human_Hand_dev", ShaderDatabase.Transparent, Vector2.one,
                        Color.blue).MatSingle;

                GenDraw.DrawMeshNowOrLater(
                    footMeshLeft,
                    groundPos.LeftJoint +
                    new Vector3(offsetJoint, -0.301f, 0),
                    drawQuat * Quaternion.AngleAxis(0, Vector3.up),
                    hipMat,
                    portrait);

                // UnityEngine.Graphics.DrawMesh(handsMesh, center + new Vector3(0, 0.301f, z),
                // Quaternion.AngleAxis(0, Vector3.up), centerMat, 0);
                // UnityEngine.Graphics.DrawMesh(handsMesh, center + new Vector3(0, 0.301f, z2),
                // Quaternion.AngleAxis(0, Vector3.up), centerMat, 0);
            }
        }

        public override void DrawHands(Quaternion bodyQuat, Vector3 drawPos,
                                       bool portrait,
                                       Thing carriedThing = null, bool flip = false, float factor = 1f)
        {
            if (ShouldBeIgnored())
            {
                return;
            }

            if (portrait && !HarmonyPatchesFS.AnimatorIsOpen())
            {
                return;
            }

            if (!this.CompAnimator.Props.bipedWithHands)
            {
                return;
            }

            // return if hands already drawn on carrything
            bool carrying = carriedThing != null;

            if (this.CarryStuff() && !carrying)
            {
                return;
            }

            BodyAnimDef body = this.CompAnimator.BodyAnim;

            if (carrying)
            {
                this.ApplyEquipmentWobble(ref drawPos);

                Vector3 handVector = drawPos;

                // Arms too far away from body
                while (Vector3.Distance(Pawn.DrawPos, handVector) > body.armLength)
                {
                    float step = 0.025f;
                    handVector = Vector3.MoveTowards(handVector, Pawn.DrawPos, step);
                }

                carriedThing.DrawAt(drawPos, flip); 
                handVector.y = drawPos.y;
                drawPos = handVector;
            }


            Rot4 rot = this.BodyFacing;

            if (body == null)
            {
                return;
            }

            JointLister shoulperPos = this.GetJointPositions(JointType.Shoulder,
                                                             body.shoulderOffsets[rot.AsInt],
                                                             body.shoulderOffsets[Rot4.North.AsInt].x,
                                                             carrying, this.Pawn.ShowWeaponOpenly());

            List<float> handSwingAngle = new List<float> { 0f, 0f };
            List<float> shoulderAngle = new List<float> { 0f, 0f };
            Vector3 rightHand = Vector3.zero;
            Vector3 leftHand = Vector3.zero;
            WalkCycleDef walkCycle = this.CompAnimator.WalkCycle;
            PoseCycleDef poseCycle = this.CompAnimator.PoseCycle;

            if (walkCycle != null && !carrying)
            {
                float offsetJoint = walkCycle.ShoulderOffsetHorizontalX.Evaluate(this.CompAnimator.MovedPercent);

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

            this.GetBipedMesh(out Mesh handMeshRight, out Mesh handMeshLeft);

            Material matLeft = this.LeftHandMat;
            Material matRight = this.RightHandMat;

            if (MainTabWindow_BaseAnimator.Colored)
            {
                matLeft = this.CompAnimator.PawnBodyGraphic?.HandGraphicLeftCol?.MatSingle;
                matRight = this.CompAnimator.PawnBodyGraphic?.HandGraphicRightCol?.MatSingle;
            }
            else if (carriedThing == null)
            {
                // Should draw shadow if inner side of the palm is facing to camera?
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
                bool noTween = false;
                if (!this.CompAnimator.IsMoving && this.CompAnimator.HasLeftHandPosition)
                {
                    position = this.CompAnimator.SecondHandPosition;
                    quat = this.CompAnimator.WeaponQuat;
                    noTween = true;
                }
                else
                {
                    shoulperPos.LeftJoint = bodyQuat * shoulperPos.LeftJoint;
                    leftHand = bodyQuat * leftHand.RotatedBy(-handSwingAngle[0] - shoulderAngle[0]);

                    position = drawPos + (shoulperPos.LeftJoint + leftHand) * factor;
                    quat = bodyQuat * Quaternion.AngleAxis(-handSwingAngle[0], Vector3.up);
                }

                TweenThing handLeft = TweenThing.HandLeft;
                this.DrawTweenedHand(position, handMeshLeft, matLeft, quat, handLeft, portrait, noTween);
                //GenDraw.DrawMeshNowOrLater(
                //                           handMeshLeft, position,
                //                           quat,
                //                           matLeft,
                //                           portrait);
            }

            if (drawRight)
            {
                Quaternion quat;
                Vector3 position;
                bool noTween = false;
                if (this.CompAnimator.FirstHandPosition != Vector3.zero)
                {
                    quat = this.CompAnimator.WeaponQuat;
                    position = this.CompAnimator.FirstHandPosition;
                    noTween = true;
                }
                else
                {
                    shoulperPos.RightJoint = bodyQuat * shoulperPos.RightJoint;
                    rightHand = bodyQuat * rightHand.RotatedBy(handSwingAngle[1] - shoulderAngle[1]);

                    position = drawPos + (shoulperPos.RightJoint + rightHand) * factor;
                    quat = bodyQuat * Quaternion.AngleAxis(handSwingAngle[1], Vector3.up);
                }

                TweenThing handRight = TweenThing.HandRight;
                this.DrawTweenedHand(position, handMeshRight, matRight, quat, handRight, portrait, noTween);
                // GenDraw.DrawMeshNowOrLater(
                //                            handMeshRight, position,
                //                            quat,
                //                            matRight,
                //                            portrait);
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
                                           handMeshLeft,
                                           drawPos + shoulperPos.LeftJoint + new Vector3(0, -0.301f, 0),
                                           bodyQuat * Quaternion.AngleAxis(-shoulderAngle[0], Vector3.up),
                                           centerMat,
                                           portrait);

                GenDraw.DrawMeshNowOrLater(
                                           handMeshRight,
                                           drawPos + shoulperPos.RightJoint + new Vector3(0, 0.301f, 0),
                                           bodyQuat * Quaternion.AngleAxis(-shoulderAngle[1], Vector3.up),
                                           centerMat,
                                           portrait);
            }
        }

        public override void Initialize()
        {
            this.Flasher = this.Pawn.Drawer.renderer.graphics.flasher;

            // this.feetTweener = new PawnFeetTweener();
            base.Initialize();
        }

        public Quaternion QuatBody(Quaternion quat, float movedPercent)
        {
            WalkCycleDef walkCycle = this.CompAnimator.WalkCycle;
            if (walkCycle != null)
            {
                float angle;
                if (this.BodyFacing.IsHorizontal)
                {
                    angle = (this.BodyFacing == Rot4.West ? -1 : 1)
                          * walkCycle.BodyAngle.Evaluate(movedPercent);
                }
                else
                {
                    angle = (this.BodyFacing == Rot4.South ? -1 : 1)
                          * walkCycle.BodyAngleVertical.Evaluate(movedPercent);
                }

                quat *= Quaternion.AngleAxis(angle, Vector3.up);
                this.CompAnimator.BodyAngle = angle;
            }

            return quat;
        }

        public Job lastJob;

        public virtual void SelectWalkcycle(bool pawnInEditor)
        {
            if (pawnInEditor)
            {
                this.CompAnimator.SetWalkCycle(Find.WindowStack.WindowOfType<MainTabWindow_WalkAnimator>().EditorWalkcycle);
                return;
            }


            if (this.Pawn.CurJob != null && this.Pawn.CurJob != this.lastJob)
            {
                BodyAnimDef animDef = this.CompAnimator.BodyAnim;

                Dictionary<LocomotionUrgency, WalkCycleDef> cycles = animDef?.walkCycles;

                if (cycles != null && cycles.Count > 0)
                {
                    if (cycles.TryGetValue(this.Pawn.CurJob.locomotionUrgency, out WalkCycleDef cycle))
                    {
                        if (cycle != null)
                        {
                            this.CompAnimator.SetWalkCycle(cycle);
                        }
                    }
                    else
                    {
                        this.CompAnimator.SetWalkCycle(animDef.walkCycles.FirstOrDefault().Value);
                    }
                }

                this.lastJob = this.Pawn.CurJob;
            }
        }

        public virtual void SelectPosecycle()
        {
            return;
            if (HarmonyPatchesFS.AnimatorIsOpen())
            {
                //  this.CompAnimator.PoseCycle = MainTabWindow_PoseAnimator.EditorPoseCycle;
            }

            if (this.Pawn.CurJob != null)
            {
                BodyAnimDef animDef = this.CompAnimator.BodyAnim;

                List<PoseCycleDef> cycles = animDef?.poseCycles;

                if (cycles != null && cycles.Count > 0)
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
                animator.IsPosing(out this._animatedPercent);
            }

            // var curve = bodyFacing.IsHorizontal ? this.walkCycle.BodyOffsetZ : this.walkCycle.BodyOffsetVerticalZ;
            bool pawnInEditor = HarmonyPatchesFS.AnimatorIsOpen() && MainTabWindow_BaseAnimator.Pawn == this.Pawn;
            if (!Find.TickManager.Paused || pawnInEditor)
            {
                this.SelectWalkcycle(pawnInEditor);
                this.SelectPosecycle();

                this.CompAnimator.FirstHandPosition = Vector3.zero;
                this.CompAnimator.SecondHandPosition = Vector3.zero;
            }
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
                                          SimpleCurve angle, float factor = 1f)
        {
            rightFoot = Vector3.zero;
            leftFoot = Vector3.zero;
            footAngleRight = 0;
            footAngleLeft = 0;

            if (!this.CompAnimator.IsMoving)
            {
                return;
            }

            float percent = this.CompAnimator.MovedPercent;
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
            if (factor < 1f)
            {
                SimpleCurve curve = new SimpleCurve { new CurvePoint(0f, 0.5f), new CurvePoint(1f, 1f) };
                float mod = curve.Evaluate(factor);
                rightFoot.x *= mod;
                rightFoot.z *= mod;
                leftFoot.x *= mod;
                leftFoot.z *= mod;
            }
        }

        protected void GetBipedMesh(out Mesh meshRight, out Mesh meshLeft)
        {
            Rot4 rot = this.BodyFacing;

            switch (rot.AsInt)
            {
                default:
                    meshRight = MeshPool.plane10;
                    meshLeft = MeshPool.plane10Flip;
                    break;

                case 1:
                    meshRight = MeshPool.plane10;
                    meshLeft = MeshPool.plane10;
                    break;

                case 3:
                    meshRight = MeshPool.plane10Flip;
                    meshLeft = MeshPool.plane10Flip;
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
            if (this.CompAnimator.IsMoving)
            {
                WalkCycleDef walkCycle = this.CompAnimator.WalkCycle;
                float percent = this.CompAnimator.MovedPercent;
                if (rot.IsHorizontal)
                {
                    float lookie = rot == Rot4.West ? -1f : 1f;
                    float f = lookie * offsetJoint;

                    shoulderAngle[0] = shoulderAngle[1] = lookie * walkCycle?.shoulderAngle ?? 0f;

                    shoulderPos.RightJoint.x += f;
                    shoulderPos.LeftJoint.x += f;

                    handSwingAngle[0] = handSwingAngle[1] =
                                        (rot == Rot4.West ? -1 : 1) * cycleHandsSwingAngle.Evaluate(percent);
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
            if (!HarmonyPatchesFS.AnimatorIsOpen())
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
                                     bool portrait, bool noTween)
        {
            if (position == Vector3.zero || handsMesh == null || material == null)
            {
                return;
            }

            if (ShouldBeIgnored())
            {
                return;
            }

            if (!HarmonyPatchesFS.AnimatorIsOpen() &&
                Find.TickManager.TicksGame == this.CompAnimator.LastPosUpdate[(int) tweenThing] ||
                HarmonyPatchesFS.AnimatorIsOpen() && MainTabWindow_BaseAnimator.Pawn != this.Pawn)
            {
                position = this.CompAnimator.LastPosition[(int) tweenThing];
            }
            else
            {
                Pawn_PathFollower pawnPathFollower = this.Pawn.pather;
                if (pawnPathFollower != null && pawnPathFollower.MovedRecently(5))
                {
                    noTween = true;
                }

                this.CompAnimator.LastPosUpdate[(int) tweenThing] = Find.TickManager.TicksGame;


                Vector3Tween tween = this.CompAnimator.Vector3Tweens[(int) tweenThing];


                switch (tween.State)
                {
                    case TweenState.Running:
                        if (noTween || this.CompAnimator.IsMoving)
                        {
                            tween.Stop(StopBehavior.ForceComplete);
                        }

                        position = tween.CurrentValue;
                        break;

                    case TweenState.Paused:
                        break;

                    case TweenState.Stopped:
                        if (noTween || (this.CompAnimator.IsMoving))
                        {
                            break;
                        }

                        ScaleFunc scaleFunc = ScaleFuncs.SineEaseOut;


                        Vector3 start = this.CompAnimator.LastPosition[(int) tweenThing];
                        float distance = Vector3.Distance(start, position);
                        float duration = Mathf.Abs(distance * 50f);
                        if (start != Vector3.zero && duration > 12f)
                        {
                            start.y = position.y;
                            tween.Start(start, position, duration, scaleFunc);
                            position = start;
                        }

                        break;
                }

                this.CompAnimator.LastPosition[(int) tweenThing] = position;
            }

            //  tweener.PreThingPosCalculation(tweenThing, noTween);

            GenDraw.DrawMeshNowOrLater(
                                       handsMesh, position,
                                       quat,
                                       material,
                                       portrait);
        }

        public bool ShouldBeIgnored()
        {
            return this.Pawn.Dead || !this.Pawn.Spawned || this.Pawn.InContainerEnclosed;
        }

        #endregion Private Methods
    }
}