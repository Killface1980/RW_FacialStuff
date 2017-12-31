using UnityEngine;

using Verse;

namespace FacialStuff
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using RimWorld;

    public class PawnBodyDrawer : BasicDrawer
    {
        #region Public Fields

        public PawnGraphicSet Graphics;

        #endregion Public Fields

        #region Protected Fields

        protected Mesh HandMesh = MeshPool.plane10;
        protected bool IsMoving;
        protected float MovedPercent;

        #endregion Protected Fields

        #region Protected Constructors

        protected PawnBodyDrawer()
        {
        }

        #endregion Protected Constructors

        #region Public Properties

        public CompBodyAnimator CompAnimator { get; set; }

        public Pawn Pawn { get; set; }

        #endregion Public Properties

        #region Public Methods

        public virtual void ApplyBodyWobble(ref Vector3 rootLoc, ref Quaternion quat)
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

        public virtual void DrawBody(PawnWoundDrawer woundDrawer, Vector3 rootLoc, Quaternion quat, RotDrawMode bodyDrawType, bool renderBody, bool portrait)
        {
        }

        public virtual void DrawEquipment(Vector3 rootLoc, bool portrait)
        {
        }

        public virtual void DrawFeet(Vector3 rootLoc, bool portrait)
        {
        }

        public virtual void DrawHands(Vector3 drawPos, bool portrait, bool carrying = false, HandsToDraw drawSide = HandsToDraw.Both)
        {

        }

        public virtual void Initialize()
        {
        }

        public virtual void Tick(Rot4 bodyFacing, PawnGraphicSet graphics)
        {
            this.Graphics = graphics;
            this.bodyFacing = bodyFacing;
        }

        #endregion Public Methods
    }
}