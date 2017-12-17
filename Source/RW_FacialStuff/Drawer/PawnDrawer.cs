using UnityEngine;

using Verse;

namespace FacialStuff
{
    using System.Collections.Generic;

    using FacialStuff.Components;
    using FacialStuff.Enums;

    using JetBrains.Annotations;

    using RimWorld;

    public abstract class PawnDrawer
    {

        #region Public Fields

        public const float YOffset_Behind = 0.004f;
        public const float YOffset_Body = 0.0078125f;
        public const float YOffset_PostHead = 0.035f;
        public const float YOffsetOnFace = 0.0001f;
        public static readonly float[] HorHeadOffsets = new float[] { 0f, 0.04f, 0.1f, 0.09f, 0.1f, 0.09f };
        public static readonly float YOffsetBodyParts = 0.05f;


        public readonly SimpleCurve swingCurveFeet =
            new SimpleCurve
                {
                    new CurvePoint(0f, 0f),
                    new CurvePoint(0.25f, 40f),
                    new CurvePoint(0.5f, 0f),
                    new CurvePoint(0.75f, -40f),
                    new CurvePoint(1f, 0f)
                };

        public readonly SimpleCurve posCurveFeet = new SimpleCurve
                                                       {
                                                           //// Passing
                                                           new CurvePoint(0f, 0f),
                                                           //// Recoil
                                                           new CurvePoint(0.25f, 40f),
                                                           //// Passing
                                                           new CurvePoint(0.5f, 0f),
                                                           //// Recoil
                                                           new CurvePoint(0.75f, -40f),
                                                           //// Passing
                                                           new CurvePoint(1f, 0f)
                                                       };

        public readonly SimpleCurve upDownCurve =
            new SimpleCurve
                {
                    new CurvePoint(0f, 0f),
                    new CurvePoint(0.25f, 0.125f),
                    new CurvePoint(0.5f, 0f),
                    new CurvePoint(0.75f, 0.125f),
                    new CurvePoint(1f, 0f)
                };

        public readonly SimpleCurve swingCurveHands =
            new SimpleCurve
                {
                    new CurvePoint(0f, 0f),
                    new CurvePoint(0.25f, 60f),
                    new CurvePoint(0.5f, 0f),
                    new CurvePoint(0.75f, -60f),
                    new CurvePoint(1f, 0f)
                };

        public readonly SimpleCurve swingCurveHandsVertical =
            new SimpleCurve
                {
                    new CurvePoint(0f, 0f),
                    new CurvePoint(0.25f, 0.125f),
                    new CurvePoint(0.5f, 0f),
                    new CurvePoint(0.75f, -0.125f),
                    new CurvePoint(1f, 0f)
                };

        public CompFace CompFace;
        public Mesh FootMesh = MeshPool.plane10;

        public Mesh HandMesh = MeshPool.plane10;

        public SimpleCurve swingCurveFeetNorthSouth =
                    new SimpleCurve
        {
                    new CurvePoint(0f, 0f),
                    new CurvePoint(0.25f, 0.075f),
                    new CurvePoint(0.5f, 0f),
                    new CurvePoint(0.75f, -0.075f),
                    new CurvePoint(1f, 0f)
        };

        #endregion Public Fields

        #region Protected Constructors

        protected PawnDrawer() { }

        #endregion Protected Constructors

        #region Public Methods

        public virtual void ApplyHeadRotation(bool renderBody, ref Rot4 headFacing, ref Quaternion headQuat)
        {
        }

        public virtual void BaseHeadOffsetAt(Rot4 rotation, ref Vector3 offset)
        {
        }

        public virtual List<Material> BodyBaseAt(
            PawnGraphicSet graphics,
            Rot4 bodyFacing,
            RotDrawMode bodyDrawType,
            MaxLayerToShow layer)
        {
            return new List<Material>();
        }


        public virtual bool CarryStuff(out Vector3 drawPos)
        {
            drawPos = Vector3.zero;
            return false;
        }

        public virtual bool CarryWeaponOpenly()
        {
            return false;
        }

        public virtual void DoAttackAnimationOffsets(ref float weaponAngle, ref Vector3 weaponPosition, bool flipped)
        {
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
        }

        public virtual void DrawBasicHead(PawnGraphicSet graphics, Quaternion headQuat, Rot4 headFacing, RotDrawMode bodyDrawType, bool headStump, bool portrait, ref Vector3 locFacialY, out bool headDrawn)
        {
            headDrawn = false;
        }

        public virtual void DrawBeardAndTache(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
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
        }

        public virtual void DrawBrows(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
        }
        // Verse.PawnRenderer - Vanilla with flava
        public virtual void DrawEquipment(Vector3 rootLoc, Rot4 bodyFacing)
        {
        }

        public virtual void DrawEquipmentAiming(Thing equipment, Vector3 weaponDrawLoc, float aimAngle)
        {
        }

        public virtual void DrawFeet(Vector3 drawPos, Rot4 bodyFacing)
        {
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
        }

        public virtual void DrawHands(Vector3 drawPos, Rot4 bodyFacing, bool carrying)
        {
        }
        public virtual void DrawHandsAiming(Vector3 weaponPosition, bool flipped, float weaponAngle,
                                            [CanBeNull] CompProperties_WeaponExtensions compWeaponExtensions)
        {
        }
        public virtual void DrawHeadOverlays(Rot4 headFacing, PawnHeadOverlays headOverlays, Vector3 bodyLoc, Quaternion headQuat)
        {
            headOverlays?.RenderStatusOverlays(bodyLoc, headQuat, this.GetPawnMesh(headFacing, false, false));
        }

        public virtual void DrawNaturalEyes(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
        }

        public virtual void DrawNaturalMouth(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
        }

        public virtual void DrawUnnaturalEyeParts(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
        }

        public virtual void DrawWrinkles(Quaternion headQuat, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, ref Vector3 locFacialY)
        {
        }

        public virtual Vector3 EyeOffset(Rot4 headFacing)
        {
            return Vector3.zero;
        }

        public virtual Mesh GetPawnHairMesh(PawnGraphicSet graphics, Rot4 headFacing, bool portrait)
        {
            return graphics.HairMeshSet.MeshAt(headFacing);
        }

        public virtual Mesh GetPawnMesh(Rot4 facing, bool wantsBody, bool portrait)
        {
            return wantsBody ? MeshPool.humanlikeBodySet.MeshAt(facing) : MeshPool.humanlikeHeadSet.MeshAt(facing);
        }

        public virtual Quaternion HeadQuat(Rot4 rotation)
        {
            return rotation.AsQuat;
        }
        public virtual void Initialize()
        {
        }

        #endregion Public Methods

        public virtual void ApplyBodyWobble(ref Vector3 rootLoc)
        {

        }
    }
}