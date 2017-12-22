using System.Collections.Generic;
using System.Linq;

namespace FacialStuff
{
    using FacialStuff.Components;
    using FacialStuff.Defs;
    using FacialStuff.Enums;
    using FacialStuff.Graphics;
    using FacialStuff.Harmony;
    using JetBrains.Annotations;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;

    public class HumanDrawer : PawnDrawer
    {
        #region Private Fields

        private bool develop = false;

        #endregion Private Fields

        #region Public Constructors

        public HumanDrawer()
        {
            // Needs a constructor
        }

        #endregion Public Constructors

        #region Public Methods

        protected float BodyWobble;

        public virtual bool Aiming()
        {
            Stance_Busy stance_Busy = this.Pawn.stances.curStance as Stance_Busy;
            return stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid;
        }

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

        public override void ApplyHeadRotation(bool renderBody, ref Quaternion headQuat)
        {
            if (this.CompFace.Props.canRotateHead && Controller.settings.UseHeadRotator)
            {
                this.headFacing = this.CompFace.HeadRotator.Rotation(this.headFacing, renderBody);
                headQuat *= this.QuatHead(this.headFacing);

                // * Quaternion.AngleAxis(faceComp.headWiggler.downedAngle, Vector3.up);
            }
        }

        public override void BaseHeadOffsetAt(ref Vector3 offset, bool portrait)
        {
            Pawn pawn = this.Pawn;
            float num = HorHeadOffsets[(int)pawn.story.bodyType];
            switch (this.headFacing.AsInt)
            {
                case 0:
                    offset = new Vector3(0f, 0f, 0.34f);
                    break;

                case 1:
                    offset = new Vector3(num, 0f, 0.34f);
                    break;

                case 2:
                    offset = new Vector3(0f, 0f, 0.34f);
                    break;

                case 3:
                    offset = new Vector3(-num, 0f, 0.34f);
                    break;

                default:
                    Log.Error("BaseHeadOffsetAt error in " + pawn);
                    offset = Vector3.zero;
                    return;
            }
            if (!portrait)
            {
                if (this.isMoving)
                {
                    float bam = this.BodyWobble;

                    // Let's try a slightly stiffy head
                    offset.z -= 0.25f * bam;
                }
            }
        }

        public override List<Material> BodyBaseAt(
            PawnGraphicSet graphics,
            Rot4 bodyFacing,
            RotDrawMode bodyDrawType,
            MaxLayerToShow layer)
        {
            switch (layer)
            {
                case MaxLayerToShow.Naked: return this.CompFace.NakedMatsBodyBaseAt(bodyFacing, bodyDrawType);
                case MaxLayerToShow.OnSkin: return this.CompFace.UnderwearMatsBodyBaseAt(bodyFacing, bodyDrawType);
                default: return graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
            }

            return base.BodyBaseAt(graphics, bodyFacing, bodyDrawType, layer);
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
            float animationPhasePercent = this.CompFace.Jitterer.CurrentOffset.magnitude / this.CompFace.JitterMax;
            if (damageDef == DamageDefOf.Stab)
            {
                weaponPosition += this.CompFace.Jitterer.CurrentOffset;

                // + new Vector3(0, 0, Mathf.Pow(this.CompFace.Jitterer.CurrentOffset.magnitude, 0.25f))/2;
            }
            else if (damageDef == DamageDefOf.Blunt || damageDef == DamageDefOf.Cut)
            {
                totalSwingAngle = 120;
                weaponPosition += this.CompFace.Jitterer.CurrentOffset + new Vector3(
                                      0,
                                      0,
                                      Mathf.Sin(
                                          this.CompFace.Jitterer.CurrentOffset.magnitude * Mathf.PI
                                          / this.CompFace.JitterMax) / 10);
            }

            weaponAngle += flipped ? -animationPhasePercent * totalSwingAngle : animationPhasePercent * totalSwingAngle;
        }

        public override void DrawApparel(Quaternion quat, Vector3 vector, bool renderBody, bool portrait)
        {
            if (portrait || renderBody && !this.CompFace.HideShellLayer || !renderBody && !Controller.settings.HideShellWhileRoofed
                && Controller.settings.IgnoreRenderBody)
            {
                for (int index = 0; index < this.graphics.apparelGraphics.Count; index++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = this.graphics.apparelGraphics[index];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        Mesh bodyMesh = this.GetPawnMesh(true, portrait);
                        Material material3 = apparelGraphicRecord.graphic.MatAt(this.bodyFacing);
                        material3 = this.graphics.flasher.GetDamagedMat(material3);
                        GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, material3, portrait);

                        // possible fix for phasing apparel
                        vector.y += YOffsetOnFace;
                    }
                }
            }
        }

        public override void DrawBasicHead(Quaternion headQuat, RotDrawMode bodyDrawType, bool headStump, bool portrait, ref Vector3 locFacialY, out bool headDrawn)
        {
            Material headMaterial = this.graphics.HeadMatAt(this.headFacing, bodyDrawType, headStump);
            if (headMaterial != null)
            {
                GenDraw.DrawMeshNowOrLater(
                    this.GetPawnMesh(false, portrait),
                    locFacialY,
                    headQuat,
                    headMaterial,
                    portrait);
                locFacialY.y += YOffsetOnFace;
                headDrawn = true;
            }
            else
            {
                headDrawn = false;
            }
        }

        public override void DrawBeardAndTache(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Mesh headMesh = this.GetPawnMesh(false, portrait);

            Material beardMat = this.CompFace.FaceMaterial.BeardMatAt(this.headFacing);
            Material moustacheMatAt = this.CompFace.FaceMaterial.MoustacheMatAt(this.headFacing);

            if (beardMat != null)
            {
                GenDraw.DrawMeshNowOrLater(headMesh, locFacialY, headQuat, beardMat, portrait);
                locFacialY.y += YOffsetOnFace;
            }

            if (moustacheMatAt != null)
            {
                GenDraw.DrawMeshNowOrLater(headMesh, locFacialY, headQuat, moustacheMatAt, portrait);
                locFacialY.y += YOffsetOnFace;
            }
        }

        public override void DrawBody(PawnWoundDrawer woundDrawer, Vector3 rootLoc, Quaternion quat, RotDrawMode bodyDrawType, bool renderBody, bool portrait)
        {
            // renderBody is AFAIK only used for beds, so ignore it and undress
            if (renderBody || Controller.settings.IgnoreRenderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += YOffset_Body;

                Mesh bodyMesh = this.GetPawnMesh(true, portrait);

                List<Material> bodyBaseAt = null;
                bool flag = true;
                if (!portrait && Controller.settings.HideShellWhileRoofed)
                {
                    if (this.CompFace.InRoom)
                    {
                        MaxLayerToShow layer;
                        if (this.CompFace.InPrivateRoom)
                        {
                            layer = renderBody
                                        ? Controller.settings.LayerInPrivateRoom
                                        : Controller.settings.LayerInOwnedBed;
                        }
                        else
                        {
                            layer = renderBody ? Controller.settings.LayerInRoom : Controller.settings.LayerInBed;
                        }

                        bodyBaseAt = this.BodyBaseAt(this.graphics, this.bodyFacing, bodyDrawType, layer);
                        flag = false;
                    }
                }

                if (flag)
                {
                    bodyBaseAt = this.graphics.MatsBodyBaseAt(this.bodyFacing, bodyDrawType);
                }

                for (int i = 0; i < bodyBaseAt.Count; i++)
                {
                    Material damagedMat = this.graphics.flasher.GetDamagedMat(bodyBaseAt[i]);
                    GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quat, damagedMat, portrait);
                    loc.y += HarmonyPatch_PawnRenderer.YOffsetInterval_Clothes;
                }

                if (bodyDrawType == RotDrawMode.Fresh)
                {
                    Vector3 drawLoc = rootLoc;
                    drawLoc.y += HarmonyPatch_PawnRenderer.YOffset_Wounds;

                    woundDrawer?.RenderOverBody(drawLoc, bodyMesh, quat, portrait);
                }
            }
        }

        public override void DrawBrows(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Material browMat = this.CompFace.FaceMaterial.BrowMatAt(this.headFacing);
            if (browMat != null)
            {
                Mesh eyeMesh = this.CompFace.EyeMeshSet.mesh.MeshAt(this.headFacing);
                GenDraw.DrawMeshNowOrLater(
                    eyeMesh,
                    locFacialY + this.EyeOffset(this.headFacing),
                    headQuat,
                    browMat,
                    portrait);
                locFacialY.y += YOffsetOnFace;
            }
        }

        public override void DrawEquipment(Vector3 rootLoc, bool portrait)
        {
            Pawn pawn = this.Pawn;

            if (pawn.Dead || !pawn.Spawned)
            {
                return;
            }

            bool showHands = this.CompFace.Props.hasHands && Controller.settings.UseHands;
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
            if (this.CompFace.Props.hasHands && Controller.settings.UseHands)
            {
                this.DrawHandsAiming(weaponDrawLoc + weaponPositionOffset, rootLoc, flipped, weaponAngle, compWeaponExtensions, false);
            }
        }

        public override void DrawFeet(Vector3 rootLoc, bool portrait)
        {
            Material matLeft;
            Material matRight;

            // Basic values
            BodyDefinition body = this.CompFace.bodySizeDefinition;
            float rightFootHorizontal = -body.hipWidth;
            float leftFootHorizontal = -rightFootHorizontal;
            float rightFootDepth = 0.035f;
            float leftFootDepth = rightFootDepth;
            float rightFootVertical = -body.legLength;
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
                    rightFootHorizontal = cycle.FootPositionX.Evaluate(this.movedPercent);
                    leftFootHorizontal = cycle.FootPositionX.Evaluate(flot);

                    footAngleRight = cycle.FootAngle.Evaluate(this.movedPercent);
                    footAngleLeft = cycle.FootAngle.Evaluate(flot);
                    rightFootVertical += cycle.FootPositionY.Evaluate(this.movedPercent);
                    leftFootVertical += cycle.FootPositionY.Evaluate(flot);
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
                    rightFootVertical += cycle.FootPositionVerticalY.Evaluate(this.movedPercent);
                    leftFootVertical += cycle.FootPositionVerticalY.Evaluate(flot);
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

            if (this.CompFace.AnimatorOpen)
            {
                matRight = this.CompFace.PawnGraphic?.FootGraphicRightCol?.MatAt(rot);
                matLeft = this.CompFace.PawnGraphic?.FootGraphicLeftCol?.MatAt(rot);
            }
            else
            {
                matRight = this.CompFace.PawnGraphic?.FootGraphicRight?.MatAt(rot);
                matLeft = this.CompFace.PawnGraphic?.FootGraphicLeft?.MatAt(rot);
            }




            float bodyAngle = 0f;

            Pawn pawn = this.Pawn;
            if (pawn.Downed || pawn.Dead)
            {
                bodyAngle = pawn.Drawer.renderer.wiggler.downedAngle;
            }

            if (matRight != null)
            {
                if (this.CompFace.bodyStat.footRight != PartStatus.Missing)
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
                if (this.CompFace.bodyStat.footLeft != PartStatus.Missing)
                {
                    GenDraw.DrawMeshNowOrLater(
                        footMeshLeft,
                        center.RotatedBy(bodyAngle) + new Vector3(leftFootHorizontal, leftFootDepth, leftFootVertical),
                        Quaternion.AngleAxis(bodyAngle + footAngleLeft, Vector3.up),
                        matLeft,
                        portrait);
                }
            }

            if (this.develop)
            {
                // for debug
                Material centerMat = GraphicDatabase.Get<Graphic_Single>(
                    "Hands/Human_Foot",
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    Color.blue).MatSingle;

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

        public override void DrawHairAndHeadGear(Vector3 rootLoc, Quaternion headQuat, RotDrawMode bodyDrawType, bool renderBody, bool portrait, Vector3 b, ref Vector3 currentLoc)
        {
            Mesh hairMesh = this.GetPawnHairMesh(portrait);
            List<ApparelGraphicRecord> apparelGraphics = this.graphics.apparelGraphics;
            List<ApparelGraphicRecord> headgearGraphics = null;
            if (!apparelGraphics.NullOrEmpty())
            {
                headgearGraphics = apparelGraphics
                    .Where(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead).ToList();
            }

            bool noRenderRoofed = this.CompFace.HideHat;
            bool noRenderBed = Controller.settings.HideHatInBed && (!renderBody);
            bool noRenderGoggles = Controller.settings.FilterHats;

            if (!headgearGraphics.NullOrEmpty())
            {
                bool filterHeadgear = (portrait && Prefs.HatsOnlyOnMap) || (!portrait && noRenderRoofed);

                // Draw regular hair if appparel or environment allows it (FS feature)
                if (bodyDrawType != RotDrawMode.Dessicated)
                {
                    // draw full or partial hair
                    bool apCoversFullHead =
                        headgearGraphics.Any(
                            x => x.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)
                                 && !x.sourceApparel.def.apparel.hatRenderedFrontOfFace);

                    bool apCoversUpperHead =
                        headgearGraphics.Any(
                            x => x.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead)
                                 && !x.sourceApparel.def.apparel.hatRenderedFrontOfFace);

                    if (this.CompFace.Props.hasOrganicHair || noRenderBed || filterHeadgear || !apCoversFullHead && !apCoversUpperHead && noRenderGoggles)
                    {
                        Material mat = this.graphics.HairMatAt(this.headFacing);
                        GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, mat, portrait);
                        currentLoc.y += YOffsetOnFace;
                    }
                    else if (Controller.settings.MergeHair && !apCoversFullHead)
                    {
                        // If not, display the hair cut
                        HairCutPawn hairPawn = CutHairDB.GetHairCache(this.Pawn);
                        Material hairCutMat = hairPawn.HairCutMatAt(this.headFacing);
                        if (hairCutMat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, hairCutMat, portrait);
                            currentLoc.y += YOffsetOnFace;
                        }
                    }
                }
                else
                {
                    filterHeadgear = false;
                }

                if (filterHeadgear)
                {
                    // Filter the head gear to only show non-hats, show nothing while in bed
                    if (noRenderGoggles)
                    {
                        headgearGraphics = headgearGraphics
                            .Where(
                                x => !x.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)
                                     && !x.sourceApparel.def.apparel.bodyPartGroups.Contains(
                                         BodyPartGroupDefOf.UpperHead)).ToList();
                    }
                    else
                    {
                        // Clear if nothing to show
                        headgearGraphics.Clear();
                    }
                }

                if (noRenderBed)
                {
                    headgearGraphics?.Clear();
                }

                // headgearGraphics = headgearGraphics
                // .OrderBy(x => x.sourceApparel.def.apparel.bodyPartGroups.Max(y => y.listOrder)).ToList();
                if (!headgearGraphics.NullOrEmpty())
                {
                    for (int index = 0; index < headgearGraphics?.Count; index++)
                    {
                        ApparelGraphicRecord headgearGraphic = headgearGraphics[index];
                        Material headGearMat = headgearGraphic.graphic.MatAt(this.headFacing);
                        headGearMat = this.graphics.flasher.GetDamagedMat(headGearMat);

                        Vector3 thisLoc = currentLoc;
                        if (headgearGraphic.sourceApparel.def.apparel.hatRenderedFrontOfFace)
                        {
                            thisLoc = rootLoc + b;
                            thisLoc.y += !(this.bodyFacing == Rot4.North) ? YOffset_PostHead : YOffset_Behind;
                        }

                        GenDraw.DrawMeshNowOrLater(hairMesh, thisLoc, headQuat, headGearMat, portrait);
                        currentLoc.y += HarmonyPatch_PawnRenderer.YOffset_Head;
                    }
                }
            }
            else
            {
                // Draw regular hair if no hat worn
                if (bodyDrawType != RotDrawMode.Dessicated)
                {
                    Material hairMat = this.graphics.HairMatAt(this.headFacing);
                    GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, hairMat, portrait);
                }
            }
        }

        public void DrawHandsAiming(Vector3 weaponPosition, Vector3 rootLoc, bool flipped, float weaponAngle, [CanBeNull] CompProperties_WeaponExtensions compWeaponExtensions, bool portrait)
        {
            if (compWeaponExtensions == null)
            {
                return;
            }

            Material matLeft = this.CompFace.PawnGraphic.HandGraphicLeft.MatSingle;
            Material matRight = this.CompFace.PawnGraphic.HandGraphicRight.MatSingle;

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

            //// for debug
            // var centerMat =
            // GraphicDatabase.Get<Graphic_Single>("Hands/Human_Hand", ShaderDatabase.CutoutSkin, Vector2.one,
            // Color.red).MatSingle;
            // UnityEngine.Graphics.DrawMesh(handsMesh, weaponPosition + new Vector3(0, 0.001f, 0),
            // Quaternion.AngleAxis(weaponAngle, Vector3.up), centerMat, 0);
        }

        public override void DrawNaturalEyes(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Mesh eyeMesh = this.CompFace.EyeMeshSet.mesh.MeshAt(this.headFacing);

            // natural eyes
            if (this.CompFace.bodyStat.eyeLeft != PartStatus.Artificial)
            {
                Material leftEyeMat =
                    this.CompFace.FaceMaterial.EyeLeftMatAt(this.headFacing, portrait);
                if (leftEyeMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        eyeMesh,
                        locFacialY + this.EyeOffset(this.headFacing) + this.CompFace.EyeWiggler.EyeMoveL,
                        headQuat,
                        leftEyeMat,
                        portrait);
                    locFacialY.y += YOffsetOnFace;
                }
            }

            if (this.CompFace.bodyStat.eyeRight != PartStatus.Artificial)
            {
                Material rightEyeMat =
                    this.CompFace.FaceMaterial.EyeRightMatAt(this.headFacing, portrait);

                if (rightEyeMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        eyeMesh,
                        locFacialY + this.EyeOffset(this.headFacing) + this.CompFace.EyeWiggler.EyeMoveR,
                        headQuat,
                        rightEyeMat,
                        portrait);
                    locFacialY.y += YOffsetOnFace;
                }
            }
        }

        public override void DrawNaturalMouth(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Material mouthMat = this.CompFace.FaceMaterial.MouthMatAt(this.headFacing, portrait);
            if (mouthMat != null)
            {
                // Mesh meshMouth = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                Mesh meshMouth = this.CompFace.MouthMeshSet.mesh.MeshAt(this.headFacing);
#if develop
                            Vector3 mouthOffset = compFace.BaseMouthOffsetAt(headFacing);
#else
                Vector3 mouthOffset = this.CompFace.MouthMeshSet.OffsetAt(this.headFacing);
#endif

                Vector3 drawLoc = locFacialY + headQuat * mouthOffset;
                GenDraw.DrawMeshNowOrLater(meshMouth, drawLoc, headQuat, mouthMat, portrait);
                locFacialY.y += YOffsetOnFace;
            }
        }

        public override void DrawUnnaturalEyeParts(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
            Mesh headMesh = this.GetPawnMesh(false, portrait);
            if (this.CompFace.bodyStat.eyeLeft == PartStatus.Artificial)
            {
                Material leftBionicMat = this.CompFace.FaceMaterial.EyeLeftPatchMatAt(this.headFacing);
                if (leftBionicMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        headMesh,
                        locFacialY + this.EyeOffset(this.headFacing),
                        headQuat,
                        leftBionicMat,
                        portrait);
                    locFacialY.y += YOffsetOnFace;
                }
            }

            if (this.CompFace.bodyStat.eyeRight == PartStatus.Artificial)
            {
                Material rightBionicMat = this.CompFace.FaceMaterial.EyeRightPatchMatAt(this.headFacing);

                if (rightBionicMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(headMesh, locFacialY + this.EyeOffset(this.headFacing), headQuat, rightBionicMat, portrait);
                    locFacialY.y += YOffsetOnFace;
                }
            }
        }

        public override void DrawWrinkles(Quaternion headQuat, RotDrawMode bodyDrawType, bool portrait, ref Vector3 locFacialY)
        {
            if (!Controller.settings.UseWrinkles)
            {
                return;
            }

            Material wrinkleMat = this.CompFace.FaceMaterial.WrinkleMatAt(this.headFacing, bodyDrawType);

            if (wrinkleMat == null)
            {
                return;
            }

            Mesh headMesh = this.GetPawnMesh(false, portrait);
            GenDraw.DrawMeshNowOrLater(headMesh, locFacialY, headQuat, wrinkleMat, portrait);
            locFacialY.y += YOffsetOnFace;
        }

        public override Vector3 EyeOffset(Rot4 headFacing)
        {
#if develop
                    faceComp.BaseEyeOffsetAt(headFacing);
#else
            return this.CompFace.EyeMeshSet.OffsetAt(headFacing);
#endif
        }

        private WalkCycleDef walkCycle = WalkCycleDefOf.Human_Walk;


        public override void Initialize()
        {
            base.Initialize();
            this.Pawn = this.CompFace.Pawn;
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

        public override Quaternion QuatHead(Rot4 rotation)
        {
            float num = 1f;
            Quaternion asQuat = rotation.AsQuat;
            float x = 1f * Mathf.Sin(num * (this.CompFace.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            float z = 1f * Mathf.Cos(num * (this.CompFace.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            asQuat.SetLookRotation(new Vector3(x, 0f, z), Vector3.up);

            // remove the body rotation
            if (this.isMoving)
            {
                if (this.bodyFacing.IsHorizontal)
                {
                    asQuat *= Quaternion.AngleAxis(
                        (this.bodyFacing == Rot4.West ? 1 : -1) * this.walkCycle.BodyAngle.Evaluate(this.movedPercent),
                        Vector3.up);
                }
                else
                {
                    asQuat *= Quaternion.AngleAxis(
                        (this.bodyFacing == Rot4.South ? 1 : -1)
                        * this.walkCycle.BodyAngleVertical.Evaluate(this.movedPercent),
                        Vector3.up);
                }
            }

            return asQuat;
        }

        public override void Tick(Rot4 bodyFacing, Rot4 headFacing, PawnGraphicSet graphics)
        {
            base.Tick(bodyFacing, headFacing, graphics);

            this.isMoving = this.CompFace.BodyAnimator.IsMoving(out this.movedPercent);
            this.BodyWobble = this.walkCycle.BodyOffsetVertical.Evaluate(this.movedPercent);

            if (this.CompFace.AnimatorOpen)
            {
                this.walkCycle = this.CompFace.walkCycle;
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

        protected void DrawHands(Vector3 drawPos, bool portrait, bool carrying = false, bool rightSide = true, bool leftSide = true)
        {
            Material matLeft = this.CompFace.PawnGraphic?.HandGraphicLeft?.MatSingle;
            Material matRight = this.CompFace.PawnGraphic?.HandGraphicRight?.MatSingle;
            if (this.CompFace.AnimatorOpen)
            {
                 matLeft = this.CompFace.PawnGraphic?.HandGraphicLeftCol?.MatSingle;
                 matRight = this.CompFace.PawnGraphic?.HandGraphicRightCol?.MatSingle;

            }
            BodyDefinition body = this.CompFace.bodySizeDefinition;

            // Basic values if pawn is carrying stuff
            float x = -body.shoulderWidth;
            float x2 = -x;
            float y = YOffsetBodyParts;
            float y2 = y;
            float z = -0.025f;
            float z2 = -z;

            // Center = drawpos of carryThing
            Vector3 center = drawPos;

            float handSwingAngle = 0f;
            Rot4 rot = this.bodyFacing;
            var shoulderAngle = 0f;

            // Has the pawn something in his hands?
            if (!carrying)
            {
                // Offsets for hands from the pawn center
                center.z += body.shoulderOffsetVerFromCenter;
                z = z2 = -body.armLength;

                if (rot.IsHorizontal)
                {
                    center.x += (rot == Rot4.West ? -1 : 1) * body.shoulderOffsetWhenFacingHorizontal;
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
                    WalkCycleDef cycle = this.walkCycle;
                    if (rot.IsHorizontal)
                    {
                        x = x2 = 0;

                        handSwingAngle = (rot == Rot4.West ? -1 : 1) * cycle.HandsSwingAngle.Evaluate(this.movedPercent);

                    }
                    else
                    {
                        z += cycle.HandsSwingPosVertical.Evaluate(this.movedPercent);
                        z2 -= cycle.HandsSwingPosVertical.Evaluate(this.movedPercent);
                    }

                }

                if (this.Pawn.Fleeing() || this.Pawn.IsBurning())
                {
                    float offset = 1f + body.armLength;
                    x *= offset;
                    z *= offset;
                    x2 *= offset;
                    z2 *= offset;
                    handSwingAngle += 180f;
                    shoulderAngle = 0f;
                }
            }

            Mesh handsMesh = this.HandMesh;
            float bodyAngle = 0f;
            Pawn pawn = this.Pawn;
            if (pawn.Downed || pawn.Dead)
            {
                bodyAngle = pawn.Drawer.renderer.wiggler.downedAngle;
            }

            if (matRight != null && rightSide)
            {
                // if (carrying || rot != Rot4.West)
                if (this.CompFace.bodyStat.handRight != PartStatus.Missing)
                {
                    GenDraw.DrawMeshNowOrLater(
                        handsMesh,
                        (center.RotatedBy(bodyAngle) + new Vector3(x, y, z).RotatedBy(handSwingAngle - shoulderAngle)),
                        Quaternion.AngleAxis(bodyAngle + handSwingAngle, Vector3.up),
                        matRight,
                        portrait);
                }
            }

            if (matLeft != null && leftSide)
            {
                // if (carrying || rot != Rot4.East)
                if (this.CompFace.bodyStat.handLeft != PartStatus.Missing)
                {
                    GenDraw.DrawMeshNowOrLater(
                        handsMesh,
                        (center.RotatedBy(bodyAngle)
                         + new Vector3(x2, y2, z2).RotatedBy(-handSwingAngle - shoulderAngle)),
                        Quaternion.AngleAxis(bodyAngle - handSwingAngle, Vector3.up),
                        matLeft,
                        portrait);
                }
            }

            if (this.develop)
            {
                // for debug
                Material centerMat = GraphicDatabase.Get<Graphic_Single>(
                    "Hands/Human_Foot",
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    Color.red).MatSingle;

                GenDraw.DrawMeshNowOrLater(handsMesh, center + new Vector3(0, 0.301f, 0),
                    Quaternion.AngleAxis(0, Vector3.up), centerMat, portrait);
            }
        }

        #endregion Protected Methods
    }
}