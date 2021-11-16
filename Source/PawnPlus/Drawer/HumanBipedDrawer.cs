namespace PawnPlus
{
    using System.Collections.Generic;
    using System.Linq;

    using PawnPlus.AnimatorWindows;
    using PawnPlus.Harmony;
    using PawnPlus.Tweener;

    using RimWorld;

    using UnityEngine;

    using Verse;
    using Verse.AI;

    public class HumanBipedDrawer : PawnBodyDrawer
    {
        #region Protected Fields

        protected const float OffsetGroundZ = -0.575f;

        protected DamageFlasher Flasher;

        #endregion Protected Fields

        #region Private Fields

        // private PawnFeetTweener feetTweener;
        private float _animatedPercent;

        #endregion Private Fields

        #region Public Properties

        public Material LeftHandMat =>
        Flasher.GetDamagedMat(CompAnimator.PawnBodyGraphic?.HandGraphicLeft?.MatSingle);

        public Material LeftHandShadowMat => Flasher.GetDamagedMat(CompAnimator.PawnBodyGraphic
                                                                           ?.HandGraphicLeftShadow?.MatSingle);

        public Material RightHandMat =>
        Flasher.GetDamagedMat(CompAnimator.PawnBodyGraphic?.HandGraphicRight?.MatSingle);

        public Material RightHandShadowMat => Flasher.GetDamagedMat(CompAnimator.PawnBodyGraphic
                                                                            ?.HandGraphicRightShadow?.MatSingle);

        #endregion Public Properties

        #region Public Methods

        public override void ApplyBodyWobble(ref Vector3 rootLoc, ref Vector3 footPos, ref Quaternion quat)
        {
            if(CompAnimator.IsMoving)
            {
                WalkCycleDef walkCycle = CompAnimator.WalkCycle;
                if(walkCycle != null)
                {
                    float bam = CompAnimator.BodyOffsetZ;

                    rootLoc.z += bam;
                    quat = QuatBody(quat, CompAnimator.MovedPercent);
                }
            }

            // Adds the leg length to the rootloc and relocates the feet to keep the pawn in center, e.g. for shields
            if(CompAnimator.BodyAnim != null)
            {
                float legModifier = CompAnimator.BodyAnim.extraLegLength;
                float posModB = legModifier * 0.75f;
                float posModF = -legModifier * 0.25f;
                Vector3 vector3 = new Vector3(0, 0, posModB);
                Vector3 vector4 = new Vector3(0, 0, posModF);

                // No rotation when moving
                if(!CompAnimator.IsMoving)
                {
                    vector3 = quat * vector3;
                    vector4 = quat * vector4;
                }

                if(!CompAnimator.IsRider)
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
            if(CompAnimator.IsMoving)
            {
                WalkCycleDef walkCycle = CompAnimator.WalkCycle;
                if(walkCycle != null)
                {
                    float bam = CompAnimator.BodyOffsetZ;
                    rootLoc.z += bam;
                }
            }

            // Adds the leg length to the rootloc and relocates the feet to keep the pawn in center, e.g. for shields
            if(CompAnimator.BodyAnim != null)
            {
                float legModifier = CompAnimator.BodyAnim.extraLegLength;
                float posModB = legModifier * 0.85f;
                Vector3 vector3 = new Vector3(0, 0, posModB);
                rootLoc += vector3;
            }
        }

        public override List<Material> BodyBaseAt(
            PawnGraphicSet graphics,
            Rot4 bodyFacing,
            RotDrawMode bodyDrawType,
            MaxLayerToShow layer)
        {
            switch(layer)
            {
                case MaxLayerToShow.Naked: return CompAnimator?.NakedMatsBodyBaseAt(bodyFacing, bodyDrawType);
                case MaxLayerToShow.OnSkin: return CompAnimator?.UnderwearMatsBodyBaseAt(bodyFacing, bodyDrawType);
                default: return graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
            }
        }

        public override bool CarryStuff()
        {
            Pawn pawn = Pawn;

            Thing carriedThing = pawn.carryTracker?.CarriedThing;
            if(carriedThing != null)
            {
                return true;
            }

            return base.CarryStuff();
        }

        public void DoAttackAnimationHandOffsets(ref List<float> weaponAngle, ref Vector3 weaponPosition, bool flipped)
        {
            Pawn pawn = Pawn;
            if(pawn.story != null && ((pawn.story.DisabledWorkTagsBackstoryAndTraits & WorkTags.Violent) != 0))
            {
                return;
            }

            if(pawn.health?.capacities != null)
            {
                if(!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    if(pawn.RaceProps != null && pawn.RaceProps.ToolUser)
                    {
                        return;
                    }
                }
            }

            // total weapon angle change during animation sequence
            int totalSwingAngle = 0;
            Vector3 currentOffset = CompAnimator.Jitterer.CurrentOffset;

            float jitterMax = CompAnimator.JitterMax;
            float magnitude = currentOffset.magnitude;
            float animationPhasePercent = magnitude / jitterMax;
            weaponPosition += currentOffset;

            float angle = animationPhasePercent * totalSwingAngle;
            weaponAngle[0] += (flipped ? -1f : 1f) * angle;
            weaponAngle[1] += (flipped ? -1f : 1f) * angle;
        }
        
        public override void DrawFeet(Quaternion bodyQuat, Quaternion footQuat, Vector3 rootLoc, bool portrait, float factor = 1f)
        {
            if(ShouldBeIgnored())
            {
                return;
            }

            /// No feet while sitting at a table
            Job curJob = Pawn.CurJob;
            if(curJob != null)
            {
                if(curJob.def == JobDefOf.Ingest && !Pawn.Rotation.IsHorizontal)
                {
                    if(curJob.targetB.IsValid)
                    {
                        for(int i = 0; i < 4; i++)
                        {
                            Rot4 rotty = new Rot4(i);
                            IntVec3 intVec = Pawn.Position + rotty.FacingCell;
                            if(intVec == curJob.targetB)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            if(portrait && !HarmonyPatchesFS.AnimatorIsOpen() && !Pawn.IsChild())
            {
                return;
            }

            Quaternion drawQuat = CompAnimator.IsMoving ? footQuat : bodyQuat;

            Rot4 rot = BodyFacing;

            // Basic values
            BodyAnimDef body = CompAnimator.BodyAnim;
            if(body == null)
            {
                return;
            }

            JointLister groundPos = GetJointPositions(
                JointType.Hip,
                body.hipOffsets[rot.AsInt],
                body.hipOffsets[Rot4.North.AsInt].x);

            Vector3 rightFootCycle = Vector3.zero;
            Vector3 leftFootCycle = Vector3.zero;
            float footAngleRight = 0;
            float footAngleLeft = 0;
            float offsetJoint = 0;
            WalkCycleDef cycle = CompAnimator.WalkCycle;
            if (cycle != null)
            {
                offsetJoint = cycle.HipOffsetHorizontalX.Evaluate(CompAnimator.MovedPercent);

                DoWalkCycleOffsets(
                    ref rightFootCycle,
                    ref leftFootCycle,
                    ref footAngleRight,
                    ref footAngleLeft,
                    ref offsetJoint,
                    cycle.FootPositionX,
                    cycle.FootPositionZ,
                    cycle.FootAngle, factor);
            }

            GetBipedMesh(out Mesh footMeshRight, out Mesh footMeshLeft);

            Material matRight;
            Material matLeft;

            if(MainTabWindow_BaseAnimator.Colored)
            {
                matRight = CompAnimator.PawnBodyGraphic?.FootGraphicRightCol?.MatAt(rot);
                matLeft = CompAnimator.PawnBodyGraphic?.FootGraphicLeftCol?.MatAt(rot);
            }
            else
            {
                Material rightFoot = CompAnimator.PawnBodyGraphic?.FootGraphicRight?.MatAt(rot);
                Material leftFoot = CompAnimator.PawnBodyGraphic?.FootGraphicLeft?.MatAt(rot);
                Material leftShadow = CompAnimator.PawnBodyGraphic?.FootGraphicLeftShadow?.MatAt(rot);
                Material rightShadow = CompAnimator.PawnBodyGraphic?.FootGraphicRightShadow?.MatAt(rot);

                switch(rot.AsInt)
                {
                    default:
                        matRight = Flasher.GetDamagedMat(rightFoot);
                        matLeft = Flasher.GetDamagedMat(leftFoot);
                        break;

                    case 1:
                        matRight = Flasher.GetDamagedMat(rightFoot);

                        matLeft = Flasher.GetDamagedMat(leftShadow);
                        break;

                    case 3:

                        matRight = Flasher.GetDamagedMat(rightShadow);
                        matLeft = Flasher.GetDamagedMat(leftFoot);
                        break;
                }
            }

            bool drawRight = matRight != null && CompAnimator.BodyStat.FootRight != PartStatus.Missing;
            bool drawLeft = matLeft != null && CompAnimator.BodyStat.FootLeft != PartStatus.Missing;

            groundPos.LeftJoint = drawQuat * groundPos.LeftJoint;
            groundPos.RightJoint = drawQuat * groundPos.RightJoint;
            leftFootCycle = drawQuat * leftFootCycle;
            rightFootCycle = drawQuat * rightFootCycle;
            Vector3 ground = rootLoc + drawQuat * new Vector3(0, 0, OffsetGroundZ) * factor;

            if(drawLeft)
            {
                Vector3 position = ground + (groundPos.LeftJoint + leftFootCycle) * factor;
                position.y = rootLoc.y;
                GenDraw.DrawMeshNowOrLater(
                    footMeshLeft,
                    position,
                    drawQuat * Quaternion.AngleAxis(footAngleLeft, Vector3.up),
                    matLeft,
                    portrait);
            }

            if (drawRight)
            {
                Vector3 position = ground + (groundPos.RightJoint + rightFootCycle) * factor;
                position.y = rootLoc.y;
                GenDraw.DrawMeshNowOrLater(
                    footMeshRight,
                    position,
                    drawQuat * Quaternion.AngleAxis(footAngleRight, Vector3.up),
                    matRight,
                    portrait);
            }

            if(MainTabWindow_BaseAnimator.Develop)
            {
                // for debug
                Material centerMat = GraphicDatabase.Get<Graphic_Single>(
                    "Hands/Ground", 
                    ShaderDatabase.Transparent, 
                    Vector2.one, 
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

                Material hipMat = GraphicDatabase.Get<Graphic_Single>(
                    "Hands/Human_Hand_dev", 
                    ShaderDatabase.Transparent, 
                    Vector2.one, 
                    Color.blue).MatSingle;

                GenDraw.DrawMeshNowOrLater(
                    footMeshLeft,
                    groundPos.LeftJoint +
                    new Vector3(offsetJoint, -0.301f, 0),
                    drawQuat * Quaternion.AngleAxis(0, Vector3.up),
                    hipMat,
                    portrait);
            }
        }

        public override void DrawHands(
            Quaternion bodyQuat, 
            Vector3 drawPos,
            bool portrait,
            Thing carriedThing = null, 
            bool flip = false, 
            float factor = 1f)
        {
            if(ShouldBeIgnored())
            {
                return;
            }

            if(portrait && !HarmonyPatchesFS.AnimatorIsOpen() && !Pawn.IsChild())
            {
                return;
            }

            if(!CompAnimator.Props.bipedWithHands)
            {
                return;
            }

            // return if hands already drawn on carrything
            bool carrying = carriedThing != null;

            if(CarryStuff() && !carrying)
            {
                return;
            }

            BodyAnimDef body = CompAnimator.BodyAnim;

            if(carrying)
            {
                ApplyEquipmentWobble(ref drawPos);

                Vector3 handVector = drawPos;

                // Arms too far away from body
                while(Vector3.Distance(Pawn.DrawPos, handVector) > body.armLength * factor)
                {
                    float step = 0.025f;
                    handVector = Vector3.MoveTowards(handVector, Pawn.DrawPos, step);
                }

                carriedThing.DrawAt(drawPos, flip); 
                handVector.y = drawPos.y;
                drawPos = handVector;
            }


            Rot4 rot = BodyFacing;

            if(body == null)
            {
                return;
            }

            JointLister shoulperPos = GetJointPositions(
                JointType.Shoulder,
                body.shoulderOffsets[rot.AsInt],
                body.shoulderOffsets[Rot4.North.AsInt].x,
                carrying, Pawn.ShowWeaponOpenly());

            List<float> handSwingAngle = new List<float> { 0f, 0f };
            List<float> shoulderAngle = new List<float> { 0f, 0f };
            Vector3 rightHand = Vector3.zero;
            Vector3 leftHand = Vector3.zero;
            WalkCycleDef walkCycle = CompAnimator.WalkCycle;
            PoseCycleDef poseCycle = CompAnimator.PoseCycle;

            if(walkCycle != null && !carrying)
            {
                float offsetJoint = walkCycle.ShoulderOffsetHorizontalX.Evaluate(CompAnimator.MovedPercent);

                // Children's arms are way too long
                DoWalkCycleOffsets(
                    body.armLength * (factor < 1f ? factor * 0.75f : 1f),
                    ref rightHand,
                    ref leftHand,
                    ref shoulderAngle,
                    ref handSwingAngle,
                    ref shoulperPos,
                    carrying,
                    walkCycle.HandsSwingAngle,
                    offsetJoint);
            }

            if(poseCycle != null)
            {
                DoPoseCycleOffsets(
                    ref rightHand,
                    ref shoulderAngle,
                    ref handSwingAngle, poseCycle);
            }

            DoAttackAnimationHandOffsets(ref handSwingAngle, ref rightHand, false);

            GetBipedMesh(out Mesh handMeshRight, out Mesh handMeshLeft);

            Material matLeft = LeftHandMat;
            Material matRight = RightHandMat;

            if(MainTabWindow_BaseAnimator.Colored)
            {
                matLeft = CompAnimator.PawnBodyGraphic?.HandGraphicLeftCol?.MatSingle;
                matRight = CompAnimator.PawnBodyGraphic?.HandGraphicRightCol?.MatSingle;
            }
            else if(carriedThing == null)
            {
                // Should draw shadow if inner side of the palm is facing to camera?
                switch (rot.AsInt)
                {
                    case 1:
                        matLeft = LeftHandShadowMat;
                        break;

                    case 3:
                        matRight = RightHandShadowMat;
                        break;
                }
            }

            bool drawLeft = matLeft != null && CompAnimator.BodyStat.HandLeft != PartStatus.Missing;
            bool drawRight = matRight != null && CompAnimator.BodyStat.HandRight != PartStatus.Missing;

            if(drawLeft)
            {
                Quaternion quat;
                Vector3 position;
                bool noTween = false;
                if(!CompAnimator.IsMoving && CompAnimator.HasLeftHandPosition)
                {
                    position = CompAnimator.SecondHandPosition;
                    quat = CompAnimator.WeaponQuat;
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
                DrawTweenedHand(position, handMeshLeft, matLeft, quat, handLeft, portrait, noTween);
            }

            if(drawRight)
            {
                Quaternion quat;
                Vector3 position;
                bool noTween = false;
                if(CompAnimator.FirstHandPosition != Vector3.zero)
                {
                    quat = CompAnimator.WeaponQuat;
                    position = CompAnimator.FirstHandPosition;
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
                DrawTweenedHand(position, handMeshRight, matRight, quat, handRight, portrait, noTween);
            }

            if(MainTabWindow_BaseAnimator.Develop)
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
            Flasher = Pawn.Drawer.renderer.graphics.flasher;

            base.Initialize();
        }

        public Quaternion QuatBody(Quaternion quat, float movedPercent)
        {
            WalkCycleDef walkCycle = CompAnimator.WalkCycle;
            if(walkCycle != null)
            {
                float angle;
                if (BodyFacing.IsHorizontal)
                {
                    angle = (BodyFacing == Rot4.West ? -1 : 1)
                          * walkCycle.BodyAngle.Evaluate(movedPercent);
                }
                else
                {
                    angle = (BodyFacing == Rot4.South ? -1 : 1)
                          * walkCycle.BodyAngleVertical.Evaluate(movedPercent);
                }

                quat *= Quaternion.AngleAxis(angle, Vector3.up);
                CompAnimator.BodyAngle = angle;
            }

            return quat;
        }

        public Job lastJob;

        public virtual void SelectWalkcycle(bool pawnInEditor)
        {
            if(pawnInEditor)
            {
                CompAnimator.SetWalkCycle(Find.WindowStack.WindowOfType<MainTabWindow_WalkAnimator>().EditorWalkcycle);
                return;
            }


            if(Pawn.CurJob != null && Pawn.CurJob != lastJob)
            {
                BodyAnimDef animDef = CompAnimator.BodyAnim;

                Dictionary<LocomotionUrgency, WalkCycleDef> cycles = animDef?.walkCycles;

                if(cycles != null && cycles.Count > 0)
                {
                    if(cycles.TryGetValue(Pawn.CurJob.locomotionUrgency, out WalkCycleDef cycle))
                    {
                        if(cycle != null)
                        {
                            CompAnimator.SetWalkCycle(cycle);
                        }
                    }
                    else
                    {
                        CompAnimator.SetWalkCycle(animDef.walkCycles.FirstOrDefault().Value);
                    }
                }

                lastJob = Pawn.CurJob;
            }
        }

        public virtual void SelectPosecycle()
        {
        }

        public override void Tick(Rot4 bodyFacing, PawnGraphicSet graphics)
        {
            base.Tick(bodyFacing, graphics);
            
            bool pawnInEditor = HarmonyPatchesFS.AnimatorIsOpen() && MainTabWindow_BaseAnimator.Pawn == Pawn;
            if(!Find.TickManager.Paused || pawnInEditor)
            {
                SelectWalkcycle(pawnInEditor);
                SelectPosecycle();
                CompAnimator.FirstHandPosition = Vector3.zero;
                CompAnimator.SecondHandPosition = Vector3.zero;
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

            if (!CompAnimator.IsMoving)
            {
                return;
            }

            float percent = CompAnimator.MovedPercent;
            float flot = percent;
            if (flot <= 0.5f)
            {
                flot += 0.5f;
            }
            else
            {
                flot -= 0.5f;
            }

            Rot4 rot = BodyFacing;
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
            Rot4 rot = BodyFacing;

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
            if(carrying)
            {
                return;
            }

            Rot4 rot = BodyFacing;

            // Basic values if pawn is carrying stuff
            float x = 0;
            float x2 = -x;
            float y = Offsets.YOffset_Behind;
            float y2 = y;
            float z;
            float z2;

            // Offsets for hands from the pawn center
            z = z2 = -armLength;

            if(rot.IsHorizontal)
            {
                x = x2 = 0f;
                if(rot == Rot4.East)
                {
                    y2 = -0.5f;
                }
                else
                {
                    y = -0.05f;
                }
            }
            else if(rot == Rot4.North)
            {
                y = y2 = -0.02f;
                x *= -1;
                x2 *= -1;
            }

            // Swing the hands, try complete the cycle
            if(CompAnimator.IsMoving)
            {
                WalkCycleDef walkCycle = CompAnimator.WalkCycle;
                float percent = CompAnimator.MovedPercent;
                if(rot.IsHorizontal)
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

            if(MainTabWindow_BaseAnimator.Panic || Pawn.Fleeing() || Pawn.IsBurning())
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

        private void DoPoseCycleOffsets(
            ref Vector3 rightHand,
            ref List<float> shoulderAngle,
            ref List<float> handSwingAngle, PoseCycleDef pose)
        {
            if(!HarmonyPatchesFS.AnimatorIsOpen())
            {
                return;
            }

            SimpleCurve cycleHandsSwingAngle = pose.HandsSwingAngle;
            SimpleCurve rHandX = pose.HandPositionX;
            SimpleCurve rHandZ = pose.HandPositionZ;

            Rot4 rot = BodyFacing;

            // Basic values if pawn is carrying stuff
            float x = 0;
            float z;

            float percent = _animatedPercent;
            PoseCycleDef poseCycle = CompAnimator.PoseCycle;
            float lookie = rot == Rot4.West ? -1f : 1f;

            shoulderAngle[1] = lookie * poseCycle?.shoulderAngle ?? 0f;

            handSwingAngle[1] = (rot == Rot4.West ? -1 : 1) * cycleHandsSwingAngle.Evaluate(percent);

            x = rHandX.Evaluate(percent) * lookie;
            z = rHandZ.Evaluate(percent);

            rightHand += new Vector3(x, 0, z);
        }

        private void DrawTweenedHand(
            Vector3 position, 
            Mesh handsMesh, 
            Material material, 
            Quaternion quat,
            TweenThing tweenThing,
            bool portrait, bool noTween)
        {
            if(position == Vector3.zero || handsMesh == null || material == null)
            {
                return;
            }

            if(ShouldBeIgnored())
            {
                return;
            }

            if(!HarmonyPatchesFS.AnimatorIsOpen() &&
                Find.TickManager.TicksGame == CompAnimator.LastPosUpdate[(int) tweenThing] ||
                HarmonyPatchesFS.AnimatorIsOpen() && MainTabWindow_BaseAnimator.Pawn != Pawn)
            {
                position = CompAnimator.LastPosition[(int) tweenThing];
            }
            else
            {
                Pawn_PathFollower pawnPathFollower = Pawn.pather;
                if(pawnPathFollower != null && pawnPathFollower.MovedRecently(5))
                {
                    noTween = true;
                }

                CompAnimator.LastPosUpdate[(int) tweenThing] = Find.TickManager.TicksGame;


                Vector3Tween tween = CompAnimator.Vector3Tweens[(int) tweenThing];


                switch(tween.State)
                {
                    case TweenState.Running:
                        if(noTween || CompAnimator.IsMoving)
                        {
                            tween.Stop(StopBehavior.ForceComplete);
                        }

                        position = tween.CurrentValue;
                        break;

                    case TweenState.Paused:
                        break;

                    case TweenState.Stopped:
                        if(noTween || (CompAnimator.IsMoving))
                        {
                            break;
                        }

                        ScaleFunc scaleFunc = ScaleFuncs.SineEaseOut;

                        Vector3 start = CompAnimator.LastPosition[(int) tweenThing];
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

                CompAnimator.LastPosition[(int) tweenThing] = position;
            }
            
            GenDraw.DrawMeshNowOrLater(
                handsMesh, position,
                quat,
                material,
                portrait);
        }

        public bool ShouldBeIgnored()
        {
            return Pawn.Dead || !Pawn.Spawned || Pawn.InContainerEnclosed;
        }

        #endregion Private Methods
    }
}