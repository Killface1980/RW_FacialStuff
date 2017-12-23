using UnityEngine;

using Verse;

namespace FacialStuff
{
    using JetBrains.Annotations;
    using RimWorld;
    using System.Collections.Generic;

    using FacialStuff.Components;

    public abstract class PawnBodyDrawer
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

        public virtual void DrawFeet(Vector3 rootLoc, bool portrait)
        {
        }



        protected Rot4 bodyFacing;

        public Pawn Pawn;

        public virtual void Initialize()
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