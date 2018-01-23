using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class PawnHeadDrawer : BasicDrawer
    {
        #region Public Fields



        #endregion Public Fields

        #region Protected Fields

        // original values
        // protected static readonly float[] HorHeadOffsets = { 0f, 0.04f, 0.1f, 0.09f, 0.1f, 0.09f };

        // Undefined -0, Male -1, Female -2, Thin -3, Hulk -4, Fat -5
        protected static readonly float[] HorHeadOffsets = { 0f,    0.06f, 0.11f,  0.11f, 0.26f,  0.17f };

        protected static readonly float[] VerHeadOffsets = { 0.34f, 0.37f, 0.36f, 0.37f, 0.41f, 0.35f };


        #endregion Protected Fields

        #region Protected Constructors

        #endregion Protected Constructors

        #region Public Properties

        [CanBeNull]
        public CompBodyAnimator CompAnimator { get; set; }

        public CompFace CompFace { get; set; }

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
            return this.Graphics.HairMeshSet.MeshAt(this.HeadFacing);
        }




        public virtual void Initialize()
        {
        }

        public virtual Quaternion QuatHead(Rot4 rotation)
        {
            return rotation.AsQuat;
        }

        public virtual void Tick(Rot4 bodyFacing, Rot4 headFacing, PawnGraphicSet graphics)
        {
            this.Graphics = graphics;
            this.BodyFacing = bodyFacing;
            this.HeadFacing = headFacing;
        }

        #endregion Public Methods
    }
}