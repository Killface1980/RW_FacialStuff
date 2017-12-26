using System.Collections.Generic;
using System.Linq;

namespace FacialStuff
{
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

    public class HumanHeadDrawer : PawnHeadDrawer
    {
        #region Private Fields


        #endregion Private Fields

        #region Public Constructors

        public HumanHeadDrawer()
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

        private WalkCycleDef walkCycle = WalkCycleDefOf.Biped_Walk;


        public override void Initialize()
        {
            base.Initialize();
            this.CompAnimator = this.Pawn.GetComp<CompBodyAnimator>();
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

            this.isMoving = this.CompAnimator.BodyAnimator.IsMoving(out this.movedPercent);
          //  var curve = bodyFacing.IsHorizontal ? this.walkCycle.BodyOffsetZ : this.walkCycle.BodyOffsetVerticalZ;
            var curve = this.walkCycle.BodyOffsetZ;
            this.BodyWobble = curve.Evaluate(this.movedPercent);

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
                        this.walkCycle = WalkCycleDefOf.Biped_Amble;
                        break;
                    case LocomotionUrgency.Walk:
                        this.walkCycle = WalkCycleDefOf.Biped_Walk;
                        break;
                    case LocomotionUrgency.Jog:
                        this.walkCycle = WalkCycleDefOf.Biped_Jog;
                        break;
                    case LocomotionUrgency.Sprint:
                        this.walkCycle = WalkCycleDefOf.Biped_Sprint;
                        break;
                }
            }

        }

        #endregion Public Methods

        #region Protected Methods




        #endregion Protected Methods
    }
}