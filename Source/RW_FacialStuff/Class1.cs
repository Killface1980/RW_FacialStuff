using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FacialStuff.Enums;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class SecondHeadDrawer : HumanDrawer
    {
        public override void ApplyHeadRotation(bool renderBody, ref Rot4 headFacing, ref Quaternion headQuat)
        {
        }

        public override void DrawAlienBodyAddons(Quaternion quat, Rot4 bodyFacing, Vector3 rootLoc, bool portrait, bool renderBody, PawnGraphicSet graphics)
        {
        }

        public override void DrawAlienHeadAddons(bool portrait, Quaternion headQuat, Rot4 headFacing, Vector3 currentLoc)
        {
        }

        public override void DrawApparel(PawnGraphicSet graphics, Quaternion quat, Rot4 bodyFacing, Vector3 vector, bool renderBody, bool portrait)
        {
        }

        public override void DrawBasicHead(PawnGraphicSet graphics, Quaternion headQuat, Rot4 headFacing, RotDrawMode bodyDrawType, bool headStump, bool portrait, ref Vector3 locFacialY, out bool headDrawn)
        {
            locFacialY.x += 0.5f;
            base.DrawBasicHead(graphics, headQuat, headFacing, bodyDrawType, headStump, portrait, ref locFacialY, out headDrawn);
            locFacialY.x -= 0.5f;
        }

        public override void DrawBeardAndTache(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            base.DrawBeardAndTache(headQuat, headFacing, portrait, ref locFacialY);
        }

        public override void DrawBody(PawnGraphicSet graphics, [CanBeNull] PawnWoundDrawer woundDrawer, Vector3 rootLoc, Quaternion quat, Rot4 bodyFacing, RotDrawMode bodyDrawType, bool renderBody, bool portrait)
        {
        }

        public override void DrawBrows(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            base.DrawBrows(headQuat, headFacing, portrait, ref locFacialY);
        }

        public override void DrawHairAndHeadGear(PawnGraphicSet graphics, Vector3 rootLoc, Quaternion headQuat, Rot4 bodyFacing, RotDrawMode bodyDrawType, Rot4 headFacing, bool renderBody, bool portrait, Vector3 b, ref Vector3 currentLoc)
        {
            base.DrawHairAndHeadGear(graphics, rootLoc, headQuat, bodyFacing, bodyDrawType, headFacing, renderBody, portrait, b, ref currentLoc);
        }

        public override void DrawHeadOverlays(Rot4 headFacing, PawnHeadOverlays headOverlays, Vector3 bodyLoc, Quaternion headQuat)
        {
            base.DrawHeadOverlays(headFacing, headOverlays, bodyLoc, headQuat);
        }

        public override void DrawNaturalEyes(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            base.DrawNaturalEyes(headQuat, headFacing, portrait, ref locFacialY);
        }

        public override void DrawNaturalMouth(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            base.DrawNaturalMouth(headQuat, headFacing, portrait, ref locFacialY);
        }

        public override void DrawUnnaturalEyeParts(Quaternion headQuat, Rot4 headFacing, bool portrait, ref Vector3 locFacialY)
        {
            base.DrawUnnaturalEyeParts(headQuat, headFacing, portrait, ref locFacialY);
        }

        public override void DrawWrinkles(Quaternion headQuat, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, ref Vector3 locFacialY)
        {
            base.DrawWrinkles(headQuat, headFacing, bodyDrawType, portrait, ref locFacialY);
        }

        public override Vector3 EyeOffset(Rot4 headFacing)
        {
            return base.EyeOffset(headFacing);
        }

        public override Mesh GetPawnHairMesh(Rot4 headFacing, PawnGraphicSet graphics)
        {
            return base.GetPawnHairMesh(headFacing, graphics);
        }

        public override Mesh GetPawnMesh(Rot4 facing, bool wantsBody)
        {
            return base.GetPawnMesh(facing, wantsBody);
        }

        public override Quaternion HeadQuat(Rot4 rotation)
        {
            return base.HeadQuat(rotation);
        }

        public override void Initialize()
        {
            base.Initialize();
        }
    }
}
