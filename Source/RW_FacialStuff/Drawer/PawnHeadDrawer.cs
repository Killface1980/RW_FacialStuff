using UnityEngine;

using Verse;

namespace FacialStuff
{
    using JetBrains.Annotations;
    using RimWorld;
    using System.Collections.Generic;

    using FacialStuff.Drawer;

    public class PawnHeadDrawer : BasicDrawer
    {

        #region Protected Fields

        protected static readonly float[] HorHeadOffsets = { 0f, 0.04f, 0.1f, 0.09f, 0.1f, 0.09f };
        protected static readonly float YOffsetBodyParts = 0.01f;

        protected bool isMoving;
        protected float movedPercent;

        protected Mesh HandMesh = MeshPool.plane10;

        #endregion Protected Fields

        #region Private Fields

        #endregion Private Fields

        #region Protected Constructors

        protected PawnHeadDrawer()
        {
        }
        #endregion Protected Constructors

        #region Public Properties

        public CompFace CompFace { get; set; }

        [CanBeNull]
        public CompBodyAnimator CompAnimator { get; set; }

        #endregion Public Properties

        #region Public Methods

        public virtual void ApplyHeadRotation(bool renderBody, ref Quaternion headQuat)
        {
        }

        public virtual void BaseHeadOffsetAt(ref Vector3 offset, bool portrait)
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



        public virtual void DrawBrows(Quaternion headQuat, bool portrait, ref Vector3 locFacialY)
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




        public virtual Quaternion QuatHead(Rot4 rotation)
        {
            return rotation.AsQuat;
        }

        public Pawn Pawn;

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