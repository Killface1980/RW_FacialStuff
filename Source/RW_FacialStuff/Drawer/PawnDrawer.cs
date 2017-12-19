using UnityEngine;

using Verse;

namespace FacialStuff
{
    using FacialStuff.Components;
    using FacialStuff.Enums;
    using JetBrains.Annotations;
    using RimWorld;
    using System.Collections.Generic;

    public abstract class PawnDrawer
    {

        #region Protected Fields

        protected const float YOffset_Behind = 0.004f;
        protected const float YOffset_Body = 0.0075f;
        protected const float YOffset_PostHead = 0.035f;
        protected const float YOffsetOnFace = 0.0001f;
        protected static readonly float[] HorHeadOffsets = { 0f, 0.04f, 0.1f, 0.09f, 0.1f, 0.09f };
        protected static readonly float YOffsetBodyParts = 0.01f;

        protected bool isMoving;
        protected float movedPercent;

        protected Mesh HandMesh = MeshPool.plane10;

        #endregion Protected Fields

        #region Private Fields

        private CompFace compFace;

        #endregion Private Fields

        #region Protected Constructors

        protected PawnDrawer()
        {
        }
        #endregion Protected Constructors

        #region Public Properties

        public CompFace CompFace { get => this.compFace; set => this.compFace = value; }

        #endregion Public Properties

        #region Public Methods

        public virtual void ApplyBodyWobble(ref Vector3 rootLoc, ref Quaternion quat)
        {
        }

        public virtual void ApplyHeadRotation(bool renderBody, ref Quaternion headQuat)
        {
        }

        public virtual void BaseHeadOffsetAt(ref Vector3 offset)
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

        public virtual void DrawAlienBodyAddons(Quaternion quat, Vector3 rootLoc, bool portrait, bool renderBody)
        {
            // Just for the Aliens
        }

        public virtual void DrawAlienHeadAddons(bool portrait, Quaternion headQuat, Vector3 currentLoc)
        {
            // Just for the Aliens
        }

        public virtual void DrawApparel(Quaternion quat, Vector3 vector, bool renderBody, bool portrait)
        {
        }

        public virtual void DrawBasicHead(Quaternion headQuat, RotDrawMode bodyDrawType, bool headStump, bool portrait, ref Vector3 locFacialY, out bool headDrawn)
        {
            headDrawn = false;
        }

        public virtual void DrawBeardAndTache(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
        }

        public virtual void DrawBody([CanBeNull] PawnWoundDrawer woundDrawer, Vector3 rootLoc, Quaternion quat, RotDrawMode bodyDrawType, bool renderBody, bool portrait)
        {
        }

        public virtual void DrawBrows(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
        }

        // Verse.PawnRenderer - Vanilla with flava
        public virtual void DrawEquipment(Vector3 rootLoc)
        {
        }

        public virtual void DrawEquipmentAiming(Thing equipment, Vector3 weaponDrawLoc, Vector3 rootLoc, float aimAngle)
        {
        }

        public virtual void DrawFeet(Vector3 drawPos, Rot4 bodyFacing)
        {
        }

        public virtual void DrawHairAndHeadGear(Vector3 rootLoc, Quaternion headQuat, RotDrawMode bodyDrawType, bool renderBody, bool portrait, Vector3 b, ref Vector3 currentLoc)
        {
        }

        public virtual void DrawHeadOverlays(PawnHeadOverlays headOverlays, Vector3 bodyLoc, Quaternion headQuat)
        {
            headOverlays?.RenderStatusOverlays(bodyLoc, headQuat, this.GetPawnMesh(false, false));
        }

        public virtual void DrawNaturalEyes(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
        }

        public virtual void DrawNaturalMouth(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
        }

        public virtual void DrawUnnaturalEyeParts(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
        {
        }

        public virtual void DrawWrinkles(Quaternion headQuat, RotDrawMode bodyDrawType, bool portrait, ref Vector3 locFacialY)
        {
        }

        public virtual Vector3 EyeOffset(Rot4 headFacing)
        {
            return Vector3.zero;
        }

        public virtual Mesh GetPawnHairMesh(bool portrait)
        {
            return graphics.HairMeshSet.MeshAt(headFacing);
        }

        protected Rot4 bodyFacing;

        protected Rot4 headFacing;
        public virtual Mesh GetPawnMesh(bool wantsBody, bool portrait)
        {
            return wantsBody ? MeshPool.humanlikeBodySet.MeshAt(bodyFacing) : MeshPool.humanlikeHeadSet.MeshAt(headFacing);
        }

        public virtual Quaternion QuatHead(Rot4 rotation)
        {
            return rotation.AsQuat;
        }
        protected Pawn Pawn;

        public virtual void Initialize()
        {
        }

        #endregion Public Methods
        public PawnGraphicSet graphics;

        public virtual void Tick(Rot4 bodyFacing, Rot4 headFacing, PawnGraphicSet graphics)
        {
            this.graphics = graphics;
            this.bodyFacing = bodyFacing;
            this.headFacing = headFacing;
        }
    }
}