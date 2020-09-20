using System.Collections.Generic;
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


        #endregion Protected Fields

        #region Protected Constructors

        protected PawnBodyDrawer()
        {
        }

        #endregion Protected Constructors

        #region Public Properties


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
        public virtual void DrawAlienBodyAddons(bool portrait, Vector3 rootLoc, Quaternion quat, bool renderBody,
            Rot4 rotation, bool invisible)
        {
            // Just for the Aliens
        }
        public virtual void DrawFeet(Quaternion bodyQuat, Quaternion footQuat, Vector3 rootLoc, bool portrait, float factor = 1f)
        {
        }

        public virtual void DrawHands(Quaternion bodyQuat, Vector3 drawPos, bool portrait, Thing carriedThing = null,
            bool flip = false, float factor = 1f)
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