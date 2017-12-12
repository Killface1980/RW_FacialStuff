using UnityEngine;

using Verse;

namespace FacialStuff
{
    using System.Collections.Generic;
    using System.Linq;

    using FacialStuff.Components;
    using FacialStuff.Enums;
    using FacialStuff.Graphics;
    using FacialStuff.Harmony;

    using JetBrains.Annotations;

    using RimWorld;

    public abstract class PawnDrawer
    {

        #region Public Fields

        public const float YOffset_Behind = 0.004f;
        public const float YOffset_Body = 0.0078125f;
        public const float YOffset_PostHead = 0.03515625f;
        public const float YOffsetOnFace = 0.0001f;
        public CompFace CompFace;

        #endregion Public Fields

        #region Protected Constructors

        protected PawnDrawer() { }

        #endregion Protected Constructors

        #region Public Methods

        public virtual void ApplyHeadRotation(bool renderBody, ref Rot4 headFacing, ref Quaternion headQuat)
        {
            if (this.CompFace.Props.canRotateHead && Controller.settings.UseHeadRotator)
            {
                headFacing = this.CompFace.HeadRotator.Rotation(headFacing, renderBody);
                headQuat *= this.HeadQuat(headFacing);

                // * Quaternion.AngleAxis(faceComp.headWiggler.downedAngle, Vector3.up);
            }

        }

        public virtual List<Material> BodyBaseAt(
            PawnGraphicSet graphics,
            Rot4 bodyFacing,
            RotDrawMode bodyDrawType,
            MaxLayerToShow layer)
        {
            switch (layer)
            {
                case MaxLayerToShow.Naked:
                    return CompFace.NakedMatsBodyBaseAt(bodyFacing, bodyDrawType);
                case MaxLayerToShow.OnSkin:
                    return CompFace.UnderwearMatsBodyBaseAt(bodyFacing, bodyDrawType);
                default:
                    return graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
            }
        }


        public virtual bool CarryWeaponOpenly()
        {
            Pawn pawn = this.CompFace.Pawn;
            return (pawn.carryTracker == null || pawn.carryTracker.CarriedThing == null)
                   && (pawn.Drafted || (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon)
                       || (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon));
        }

        public virtual void DrawAlienBodyAddons(Quaternion quat, Rot4 bodyFacing, Vector3 rootLoc, bool portrait, bool renderBody, PawnGraphicSet graphics)
        {
            // Just for the Aliens
        }
        public virtual void DrawAlienHeadAddons(bool portrait, Quaternion headQuat, Rot4 headFacing, Vector3 currentLoc)
        {
            // Just for the Aliens
        }

        public virtual void DrawApparel(PawnGraphicSet graphics, Quaternion quat, Rot4 bodyFacing, Vector3 vector, bool renderBody, bool portrait)
        {
            if (portrait || renderBody && !this.CompFace.HideShellLayer || !renderBody && !Controller.settings.HideShellWhileRoofed
                && Controller.settings.IgnoreRenderBody)
            {
                for (int index = 0; index < graphics.apparelGraphics.Count; index++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = graphics.apparelGraphics[index];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        var bodyMesh = this.GetPawnMesh(bodyFacing, true);
                        Material material3 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
                        material3 = graphics.flasher.GetDamagedMat(material3);
                        GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, material3, portrait);

                        // possible fix for phasing apparel
                        vector.y += YOffsetOnFace;
                    }
                }
            }
        }

        public virtual void DrawBasicHead(PawnGraphicSet graphics, Quaternion headQuat, Rot4 headFacing, RotDrawMode bodyDrawType, bool headStump, bool portrait, ref Vector3 locFacialY, out bool headDrawn)
        {
            Material headMaterial = graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
            if (headMaterial != null)
            {
                GenDraw.DrawMeshNowOrLater(
                    this.GetPawnMesh(headFacing, false),
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

        public virtual void DrawBeardAndTache(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            Mesh headMesh = this.GetPawnMesh(headFacing, false);

            Material beardMat = this.CompFace.FaceMaterial.BeardMatAt(headFacing);
            Material moustacheMatAt = this.CompFace.FaceMaterial.MoustacheMatAt(headFacing);

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

        public virtual void DrawBody(
                    PawnGraphicSet graphics,
            [CanBeNull] PawnWoundDrawer woundDrawer,
            Vector3 rootLoc,
            Quaternion quat,
            Rot4 bodyFacing,
            RotDrawMode bodyDrawType,
            bool renderBody,
            bool portrait)
        {
            // renderBody is AFAIK only used for beds, so ignore it and undress
            if (renderBody || Controller.settings.IgnoreRenderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += YOffset_Body;

                var bodyMesh = this.GetPawnMesh(bodyFacing, true);

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

                        bodyBaseAt = this.BodyBaseAt(graphics, bodyFacing, bodyDrawType, layer);
                        flag = false;
                    }
                }

                if (flag)
                {
                    bodyBaseAt = graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                }

                for (int i = 0; i < bodyBaseAt.Count; i++)
                {
                    Material damagedMat = graphics.flasher.GetDamagedMat(bodyBaseAt[i]);
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

        public virtual void DrawBrows(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            Material browMat = this.CompFace.FaceMaterial.BrowMatAt(headFacing);
            if (browMat != null)
            {
                Mesh eyeMesh = this.CompFace.EyeMeshSet.mesh.MeshAt(headFacing);
                GenDraw.DrawMeshNowOrLater(
                    eyeMesh,
                    locFacialY + this.EyeOffset(headFacing),
                    headQuat,
                    browMat,
                    portrait);
                locFacialY.y += YOffsetOnFace;
            }
        }

        // Verse.PawnRenderer - Vanilla with flava
        public virtual void DrawEquipment(Vector3 rootLoc)
        {
            var pawn = this.CompFace.Pawn;

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


            // Continue vanilla

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
                num -= 90f;
                this.DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, num);

            }
            else if (this.CarryWeaponOpenly())
            {
                float aimAngle = 143f;
                Vector3 drawLoc2 = rootLoc;
                if (pawn.Rotation == Rot4.South)
                {
                    drawLoc2 += new Vector3(0f, 0f, -0.22f);
                    drawLoc2.y += 0.04f;
                }
                else if (pawn.Rotation == Rot4.North)
                {
                    drawLoc2 += rootLoc + new Vector3(0f, 0f, -0.11f);
                }
                else if (pawn.Rotation == Rot4.East)
                {
                    drawLoc2 += new Vector3(0.2f, 0f, -0.22f);
                    drawLoc2.y += 0.04f;
                }
                else if (pawn.Rotation == Rot4.West)
                {
                    drawLoc2 = rootLoc + new Vector3(-0.2f, 0f, -0.22f);
                    drawLoc2.y += 0.04f;
                    aimAngle = 217f;
                }

                this.DrawEquipmentAiming(pawn.equipment.Primary, drawLoc2, aimAngle);

            }
        }
        public bool Aiming()
        {
            var stance_Busy = this.CompFace.Pawn.stances.curStance as Stance_Busy;
            return stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid;
        }

        public virtual void DoAttackAnimationOffsets(ref float weaponAngle, ref Vector3 weaponPosition, bool flipped)
        {
            CompEquippable primaryEq = this.CompFace.Pawn.equipment?.PrimaryEq;

            //   DamageDef damageDef = primaryEq?.PrimaryVerb?.verbProps?.meleeDamageDef;
            if (primaryEq == null)
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
               //  + new Vector3(0, 0, Mathf.Pow(this.CompFace.Jitterer.CurrentOffset.magnitude, 0.25f))/2;
            }
            else if (damageDef == DamageDefOf.Blunt || damageDef == DamageDefOf.Cut)
            {
                totalSwingAngle = 120;
                weaponPosition += this.CompFace.Jitterer.CurrentOffset +
                                  new Vector3(0, 0,
                                      Mathf.Sin(this.CompFace.Jitterer.CurrentOffset.magnitude * Mathf.PI / this.CompFace.JitterMax) /
                                      10);
            }
            weaponAngle += flipped
                               ? -animationPhasePercent * totalSwingAngle
                               : animationPhasePercent * totalSwingAngle;
        }

        // Verse.PawnRenderer - Vanilla code with flava at the end
        public virtual void DrawEquipmentAiming(Thing equipment, Vector3 weaponDrawLoc, float aimAngle)
        {
            // New
            float weaponAngle = aimAngle;

            Mesh weaponMesh;
            bool flipped;
            var weaponPositionOffset = Vector3.zero;
            var aiming = Aiming();

            CompProperties_WeaponExtensions compWeaponExtensions = this.CompFace.Pawn.equipment.Primary.def
                .GetCompProperties<CompProperties_WeaponExtensions>();


            if (this.CompFace.Pawn.Rotation == Rot4.West || this.CompFace.Pawn.Rotation == Rot4.North)
            {
                // draw weapon beneath the pawn
                weaponPositionOffset += new Vector3(0, -0.5f, 0);
            }

            //   if (aimAngle > 110 && aimAngle < 340)
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
            DoAttackAnimationOffsets(ref weaponAngle, ref weaponPositionOffset, flipped);


            Graphic_StackCount graphic_StackCount = equipment.Graphic as Graphic_StackCount;
            Material matSingle;
            matSingle = graphic_StackCount != null
                            ? graphic_StackCount.SubGraphicForStackCount(1, equipment.def).MatSingle
                            : equipment.Graphic.MatSingle;

            UnityEngine.Graphics.DrawMesh(
                weaponMesh,
                weaponDrawLoc + weaponPositionOffset,
                Quaternion.AngleAxis(weaponAngle, Vector3.up),
                matSingle,
                0);
            

            // Now the hands if possible
            if (this.CompFace.Props.hasHands && Controller.settings.UseHands)
            {
                this.DrawHands(weaponDrawLoc + weaponPositionOffset, flipped, weaponAngle, compWeaponExtensions);
            }
        }

        public virtual void DrawHairAndHeadGear(
                            PawnGraphicSet graphics,
            Vector3 rootLoc,
            Quaternion headQuat,
            Rot4 bodyFacing,
            RotDrawMode bodyDrawType,
            Rot4 headFacing,
            bool renderBody,
            bool portrait,
            Vector3 b,
            ref Vector3 currentLoc)
        {
            Mesh hairMesh = this.GetPawnHairMesh(headFacing, graphics);
            List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
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

                    if (noRenderBed || filterHeadgear || !apCoversFullHead && !apCoversUpperHead && noRenderGoggles)
                    {
                        Material mat = graphics.HairMatAt(headFacing);
                        GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, mat, portrait);
                        currentLoc.y += YOffsetOnFace;
                    }
                    else if (Controller.settings.MergeHair && !apCoversFullHead)
                    {
                        // If not, display the hair cut
                        HairCutPawn hairPawn = CutHairDB.GetHairCache(this.CompFace.Pawn);
                        Material hairCutMat = hairPawn.HairCutMatAt(headFacing);
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
                        Material headGearMat = headgearGraphic.graphic.MatAt(headFacing);
                        headGearMat = graphics.flasher.GetDamagedMat(headGearMat);

                        Vector3 thisLoc = currentLoc;
                        if (headgearGraphic.sourceApparel.def.apparel.hatRenderedFrontOfFace)
                        {
                            thisLoc = rootLoc + b;
                            thisLoc.y += !(bodyFacing == Rot4.North) ? YOffset_PostHead : YOffset_Behind;
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
                    Material hairMat = graphics.HairMatAt(headFacing);
                    GenDraw.DrawMeshNowOrLater(hairMesh, currentLoc, headQuat, hairMat, portrait);
                }
            }
        }

        public virtual void DrawHands(Vector3 weaponPosition, bool flipped, float weaponAngle, CompProperties_WeaponExtensions compWeaponExtensions)
        {
            if (compWeaponExtensions != null)
            {

                Material handGraphicMatSingle = this.CompFace.PawnGraphic.HandGraphic.MatSingle;

                if (handGraphicMatSingle != null)
                {
                    handGraphicMatSingle.color = this.CompFace.Pawn.story.SkinColor;
                    Vector3 firstHandPosition = compWeaponExtensions.FirstHandPosition;
                    Mesh handsMesh = MeshPool.plane10;
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
                            handGraphicMatSingle,
                            0);
                    }

                    Vector3 secondHandPosition = compWeaponExtensions.SecondHandPosition;
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
                            handGraphicMatSingle,
                            0);
                    }
                    //// for debug
                   // var centerMat =
                   //     GraphicDatabase.Get<Graphic_Single>("Hands/Human_Hand", ShaderDatabase.CutoutSkin, Vector2.one,
                   //         Color.red).MatSingle;
                   //
                   // UnityEngine.Graphics.DrawMesh(handsMesh, weaponPosition + new Vector3(0, 0.001f, 0),
                   //     Quaternion.AngleAxis(weaponAngle, Vector3.up), centerMat, 0);
                }
            }
        }
        public virtual void DrawHeadOverlays(Rot4 headFacing, PawnHeadOverlays headOverlays, Vector3 bodyLoc, Quaternion headQuat)
        {
            headOverlays?.RenderStatusOverlays(bodyLoc, headQuat, this.GetPawnMesh(headFacing, false));
        }

        public virtual void DrawNaturalEyes(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            Mesh eyeMesh = this.CompFace.EyeMeshSet.mesh.MeshAt(headFacing);

            // natural eyes
            if (!this.CompFace.HasEyePatchLeft)
            {
                Material leftEyeMat =
                    this.CompFace.FaceMaterial.EyeLeftMatAt(headFacing, portrait);
                if (leftEyeMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        eyeMesh,
                        locFacialY + this.EyeOffset(headFacing) + this.CompFace.EyeWiggler.EyeMoveL,
                       headQuat,
                        leftEyeMat,
                        portrait);
                    locFacialY.y += YOffsetOnFace;
                }
            }

            if (!this.CompFace.HasEyePatchRight)
            {
                Material rightEyeMat =
                    this.CompFace.FaceMaterial.EyeRightMatAt(headFacing, portrait);
                if (rightEyeMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        eyeMesh,
                        locFacialY + this.EyeOffset(headFacing) + this.CompFace.EyeWiggler.EyeMoveR,
                       headQuat,
                        rightEyeMat,
                        portrait);
                    locFacialY.y += YOffsetOnFace;
                }
            }
        }

        public virtual void DrawNaturalMouth(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            Material mouthMat = this.CompFace.FaceMaterial.MouthMatAt(headFacing, portrait);
            if (mouthMat != null)
            {
                // Mesh meshMouth = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                Mesh meshMouth = this.CompFace.MouthMeshSet.mesh.MeshAt(headFacing);
#if develop
                            Vector3 mouthOffset = compFace.BaseMouthOffsetAt(headFacing);
#else
                Vector3 mouthOffset = this.CompFace.MouthMeshSet.OffsetAt(headFacing);
#endif

                Vector3 drawLoc = locFacialY + headQuat * mouthOffset;
                GenDraw.DrawMeshNowOrLater(meshMouth, drawLoc, headQuat, mouthMat, portrait);
                locFacialY.y += YOffsetOnFace;
            }
        }

        public virtual void DrawUnnaturalEyeParts(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            Mesh headMesh = this.GetPawnMesh(headFacing, false);
            if (this.CompFace.HasEyePatchLeft)
            {
                Material leftBionicMat = this.CompFace.FaceMaterial.EyeLeftPatchMatAt(headFacing);
                if (leftBionicMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(
                        headMesh,
                        locFacialY + this.EyeOffset(headFacing),
                       headQuat,
                        leftBionicMat,
                        portrait);
                    locFacialY.y += YOffsetOnFace;
                }
            }

            if (this.CompFace.HasEyePatchRight)
            {
                Material rightBionicMat = this.CompFace.FaceMaterial.EyeRightPatchMatAt(headFacing);

                if (rightBionicMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(headMesh, locFacialY + this.EyeOffset(headFacing), headQuat, rightBionicMat, portrait);
                    locFacialY.y += YOffsetOnFace;
                }
            }
        }

        public virtual void DrawWrinkles(Quaternion headQuat, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, ref Vector3 locFacialY)
        {
            if (!Controller.settings.UseWrinkles)
            {
                return;
            }

            Material wrinkleMat = this.CompFace.FaceMaterial.WrinkleMatAt(headFacing, bodyDrawType);

            if (wrinkleMat == null)
            {
                return;
            }

            Mesh headMesh = this.GetPawnMesh(headFacing, false);
            GenDraw.DrawMeshNowOrLater(headMesh, locFacialY, headQuat, wrinkleMat, portrait);
            locFacialY.y += YOffsetOnFace;
        }

        public virtual Vector3 EyeOffset(Rot4 headFacing)
        {

#if develop
                    faceComp.BaseEyeOffsetAt(headFacing);
#else
            return this.CompFace.EyeMeshSet.OffsetAt(headFacing);
#endif 
        }

        public virtual Mesh GetPawnHairMesh(Rot4 headFacing, PawnGraphicSet graphics)
        {
            return graphics.HairMeshSet.MeshAt(headFacing);
        }

        public virtual Mesh GetPawnMesh(Rot4 facing, bool wantsBody)
        {
            return wantsBody ? MeshPool.humanlikeBodySet.MeshAt(facing) : MeshPool.humanlikeHeadSet.MeshAt(facing);
        }

        public virtual Quaternion HeadQuat(Rot4 rotation)
        {
            float num = 1f;
            Quaternion asQuat = rotation.AsQuat;
            float x = 1f * Mathf.Sin(num * (this.CompFace.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            float z = 1f * Mathf.Cos(num * (this.CompFace.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            asQuat.SetLookRotation(new Vector3(x, 0f, z), Vector3.up);
            return asQuat;
        }
        public virtual void Initialize()
        {
        }

        #endregion Public Methods

    }
}