using UnityEngine;

using Verse;

namespace FacialStuff
{
    using System.Collections.Generic;

    using FacialStuff.Drawer;
    using FacialStuff.Harmony;

    using JetBrains.Annotations;

    using RimWorld;

    public class PawnBodyDrawer : BasicDrawer
    {

        #region Protected Fields


        protected bool isMoving;
        protected float movedPercent;

        protected Mesh HandMesh = MeshPool.plane10;

        #endregion Protected Fields

        #region Private Fields

        #endregion Private Fields

        #region Protected Constructors

        protected PawnBodyDrawer()
        {
        }
        #endregion Protected Constructors

        #region Public Properties

        public CompBodyAnimator CompAnimator
        {
            get => this.compAnimator;
            set => this.compAnimator = value;
        }

        #endregion Public Properties

        #region Public Methods

        public virtual void ApplyBodyWobble(ref Vector3 rootLoc, ref Quaternion quat)
        {
        }

        public virtual void DrawEquipment(Vector3 rootLoc, bool portrait)
        {
        }

        public virtual void DrawHands(Vector3 drawPos, bool portrait, bool carrying = false, HandsToDraw drawSide = HandsToDraw.Both)
        {
            
        }

        public virtual void DrawEquipmentAiming(Thing equipment, Vector3 weaponDrawLoc, Vector3 rootLoc, float aimAngle, bool portrait)
        {
        }



        public virtual bool CarryWeaponOpenly()
        {
            return false;
        }

        public virtual void DoAttackAnimationOffsets(ref float weaponAngle, ref Vector3 weaponPosition, bool flipped)
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

        public virtual bool CarryStuff()
        {
            return false;
        }

        public virtual void DrawFeet(Vector3 rootLoc, bool portrait)
        {
        }




        public Pawn Pawn;

        public virtual void Initialize()
        {
        }

        public virtual void DrawBody([CanBeNull] PawnWoundDrawer woundDrawer, Vector3 rootLoc, Quaternion quat, RotDrawMode bodyDrawType, bool renderBody, bool portrait)
        {
        }

        #endregion Public Methods
        public PawnGraphicSet graphics;

        private CompBodyAnimator compAnimator;

        public virtual void Tick(Rot4 bodyFacing, PawnGraphicSet graphics)
        {
            this.graphics = graphics;
            this.bodyFacing = bodyFacing;
        }
    }
}