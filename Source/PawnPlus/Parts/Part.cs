﻿using PawnPlus.Defs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.Parts
{
    public class Part
    {
        private class SinglePart
        {
            public string _renderNodeName;
            public int _partIdentifier;
            public RootType _rootType;
            public bool _occluded;
            public RenderParam[] _renderParams;
        }

        private PartDef _partDef;
        private IPartRenderer _partRenderer;
        private TickDelegate _tickDelegate;
        private List<SinglePart> _parts;
        
        public static Part Create(Pawn pawn, PartDef partDef, BodyPartSignals bodyPartSignals)
		{
            if(partDef.partRenderer == null)
			{
                Log.Warning(
                    "Pawn Plus: no graphic provider is specified for one of the parts in PartDef " +
                    partDef.defName +
                    " . The part will not be shown.");
                return null;
            }
            return new Part(pawn, partDef, bodyPartSignals);
		}

        private Part(Pawn pawn, PartDef partDef, BodyPartSignals bodyPartSignals)
		{
            _partDef = partDef;
            _partRenderer = (IPartRenderer)partDef.partRenderer.Clone();
            _partRenderer.Initialize(
                pawn,
                pawn.RaceProps.body,
                partDef.defaultTexPath,
                partDef.namedTexPaths,
                bodyPartSignals,
                ref _tickDelegate);
            _parts = new List<Part.SinglePart>();
            foreach(var part in partDef.parts)
            {
                Part.SinglePart singlePartData = new Part.SinglePart();
                singlePartData._renderNodeName = part.renderNodeName;
                singlePartData._partIdentifier = part.partIdentifier;
                RenderParamManager.GetRenderParams(
                    pawn,
                    part.renderNodeName,
                    out singlePartData._rootType,
                    out singlePartData._renderParams);
                _parts.Add(singlePartData);
            }
        }
        
        public void Update(PawnState pawnState, BodyPartStatus bodyPartStatus, ref bool updatePortrait)
		{
            if(_tickDelegate.NormalUpdate != null)
            {
                _tickDelegate.NormalUpdate(
                    pawnState,
                    bodyPartStatus,
                    ref updatePortrait);
            }
        }

        public void UpdateRare(PawnState pawnState, BodyPartStatus bodyPartStatus, ref bool updatePortrait)
        {
            if(_tickDelegate.RareUpdate != null)
            {
                _tickDelegate.RareUpdate(
                    pawnState,
                    bodyPartStatus,
                    ref updatePortrait);
            }
        }

        public void UpdateLong(PawnState pawnState, BodyPartStatus bodyPartStatus, ref bool updatePortrait)
        {
            if(_tickDelegate.LongUpdate != null)
            {
                _tickDelegate.LongUpdate(
                    pawnState,
                    bodyPartStatus,
                    ref updatePortrait);
            }
        }

        public void RenderPart(
            PawnGraphicSet graphicSet,
            RotDrawMode bodyDrawType,
            bool portrait,
            bool headStump,
            Rot4 bodyFacing,
            Rot4 headFacing,
            Vector3 headPos,
            Quaternion headQuat)
		{
            foreach(var part in _parts)
            {
                if(part._renderParams == null || part._occluded)
                {
                    continue;
                }
                // If portrait is being rendered, follow the body rotation.
                Rot4 partRot4 = bodyFacing;
                if(!portrait && part._rootType == RootType.Head)
                {
                    partRot4 = headFacing;
                }
                if(part._renderParams[partRot4.AsInt].render)
                {
                    _partRenderer.Render(
                        headPos,
                        headQuat,
                        partRot4,
                        part._renderParams[partRot4.AsInt].offset,
                        part._renderParams[partRot4.AsInt].mesh,
                        part._partIdentifier,
                        portrait);
                }
            }
        }

        public void UpdatePartOcclusion(HashSet<string> occludedRenderNodes)
		{
            foreach(var singlePart in _parts)
            {
                if(occludedRenderNodes.Contains(singlePart._renderNodeName))
                {
                    singlePart._occluded = true;
                }
                else
                {
                    singlePart._occluded = false;
                }
            }
        }
    }
}
