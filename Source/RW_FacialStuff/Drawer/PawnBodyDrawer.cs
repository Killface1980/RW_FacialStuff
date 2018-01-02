using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class PawnBodyDrawer : BasicDrawer
    {
        #region Public Fields

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

        [NotNull]
        public CompBodyAnimator CompAnimator { get; set; }


        #endregion Public Properties

        #region Public Methods

        public virtual void ApplyBodyWobble(ref Vector3 rootLoc, ref Vector3 footPos, ref Quaternion quat)
        {
        }
        public virtual void DrawApparel(Quaternion quat, Vector3 vector, bool renderBody, bool portrait)
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
            this.BodyFacing = bodyFacing;
        }

        #endregion Public Methods
    }
}