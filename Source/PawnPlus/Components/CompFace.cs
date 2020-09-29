using PawnPlus.Animator;
using PawnPlus.DefOfs;
using PawnPlus.Defs;
using PawnPlus.Graphics;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using PawnPlus.Harmony;
using UnityEngine;
using Verse;
using System.Collections.ObjectModel;
using System.Linq;
using PawnPlus.Parts;

namespace PawnPlus
{
    public class CompFace : ThingComp
    {
        private class PartData
        {
            public int bodyPartIndex;
            public RootType rootType;
            public IGraphicProvider graphicProvider;
            public Graphic graphic;
            public Graphic portraitGraphic;
            public Vector3 additionalOffset;
            public string renderNodeName;
            public bool occluded;
            public RenderParam[] renderParams;
        }
                
        private IReadOnlyList<PartData> _perPartData;
        private BodyPartStatus[] _perPartStatus;
        private BodyPartStatus _defaultPart = new BodyPartStatus
		    {
                missing = false,
                hediffAddedPart = null
		    };
        private Dictionary<int, List<PartSignal>> _bodyPartSignals;
        private List<PartSignal> _defaultEmptyPartSignals = new List<PartSignal>();
        private Rot4 _cachedHeadFacing;
        private PawnState _pawnState;
        private IHeadBehavior _headBehavior;
        private List<IPartBehavior> _partBehaviors;
        private List<PartDef> _partDefs;
        // Used for distance culling of face details
        private GameComponent_PawnPlus _fsGameComp;
        
        public bool Initialized { get; private set; }

        public IHeadBehavior HeadBehavior => _headBehavior;
                        
        public CompProperties_Face Props { get; private set; }
        
        public Pawn Pawn { get; private set; }
        
        public HeadCoverage CurrentHeadCoverage { get; set; }
                                
		// Return true if head was drawn. False if not.
		public bool DrawHead(
            PawnGraphicSet graphicSet,
            RotDrawMode bodyDrawType, 
            bool portrait, 
            bool headStump,
            Rot4 bodyFacing, 
            Rot4 headFacing,
            Vector3 headPos,
            Quaternion headQuat)
        {
            if(Pawn.IsChild())
			{
                return false;
			}
            bool headDrawn = false;
            // Draw head
            headFacing = portrait ? HeadBehavior.GetRotationForPortrait() : headFacing;
            Material headMaterial = graphicSet.HeadMatAt_NewTemp(headFacing, bodyDrawType, headStump);
            if(headMaterial != null)
            {
                GenDraw.DrawMeshNowOrLater(
                    MeshPool.humanlikeHeadSet.MeshAt(headFacing),
                    headPos,
                    headQuat,
                    headMaterial,
                    portrait);
                headDrawn = true;
                if(bodyDrawType != RotDrawMode.Dessicated && !headStump)
                {   
                    if(portrait || _fsGameComp.ShouldRenderFaceDetails)
				    {
                        foreach(var part in _perPartData)
                        {
                            if(part.renderParams == null || part.occluded)
							{
                                continue;
							}
                            Rot4 partRot4 = Rot4.South;
                            if(!portrait)
							{
                                switch(part.rootType)
                                {
                                    case RootType.Body:
                                        partRot4 = bodyFacing;
                                        break;

                                    case RootType.Head:
                                        partRot4 = headFacing;
                                        break;
                                }
                            }
                            if(part.renderParams[partRot4.AsInt].render)
							{
                                Vector3 partDrawPos = headPos;
                                Quaternion partQuat = headQuat;
                                Graphic graphic = portrait ?
                                    part.portraitGraphic :
                                    part.graphic;
                                if(graphic == null)
                                {
                                    continue;
                                }
                                Material partMat = graphic.MatAt(partRot4);
                                if(partMat != null)
                                {
                                    Vector3 offset =
                                        part.renderParams[partRot4.AsInt].offset +
                                        part.additionalOffset;
                                    offset = partQuat * offset;
                                    GenDraw.DrawMeshNowOrLater(
                                            part.renderParams[partRot4.AsInt].mesh,
                                            partDrawPos + offset,
                                            partQuat,
                                            partMat,
                                            portrait);
                                }
                            }
                        }
                    }
                    // When CurrentHeadCoverage == HeadCoverage.None, let the vanilla routine draw the hair
                    if(CurrentHeadCoverage != HeadCoverage.None)
				    {
                        DrawHairBasedOnCoverage(headFacing, headPos, headQuat, portrait);
                    }
                }
            }
            return headDrawn;
        }

        // Draw hair using different cutout masks based on head coverage
        public void DrawHairBasedOnCoverage(
            Rot4 headFacing, 
            Vector3 headDrawLoc, 
            Quaternion headQuat, 
            bool portrait)
		{
            PawnGraphicSet graphics = Pawn.Drawer.renderer.graphics;
            Vector3 hairDrawLoc = headDrawLoc;
            // Constant is "YOffsetIntervalClothes". Adding this will ensure that hair is above the head 
            // and apparel regardless of the head's orientation, but also ensure that it remains below headwear.
            // Kind of arbitrary, but at least it works.
            hairDrawLoc.y += 0.00306122447f;
            Graphic_Hair hairGraphic = graphics.hairGraphic as Graphic_Hair;
            if(hairGraphic != null)
            {
                // Copied from PawnGraphicSet.HairMatAt_NewTemp
                Material hairBasemat = hairGraphic.MatAt(headFacing, CurrentHeadCoverage);
                if(!portrait && Pawn.IsInvisible())
                {
                    // Invisibility shader ignores the mask texture used in this mod's custom hair shader, which
                    // cuts out the parts of hair that are poking through headwear.
                    // However, decompiling vanilla invisibility shader and writing an equivalent shader for this mod's
                    // custom shader is rather difficult. The only downside of using the vanilla invisibility shader
                    // is the graphical artifacts, so fixing it will be a low priority task.
                    hairBasemat = InvisibilityMatPool.GetInvisibleMat(hairBasemat);
                }
                // Similar to the invisibility shader, a separate damaged mat shader needs to be written for this
                // mod's custom hair shader. However, the effect is so subtle that taking time to decompile the vanilla
                // shader and writing a custom shader isn't worth the time.
                // graphics.flasher.GetDamagedMat(baseMat);
                var maskTex = hairBasemat.GetMaskTexture();
                GenDraw.DrawMeshNowOrLater(
                    graphics.HairMeshSet.MeshAt(headFacing),
                    mat: hairBasemat,
                    loc: hairDrawLoc,
                    quat: headQuat,
                    drawNow: portrait);
            } else
            {
                Log.ErrorOnce(
                    "Pawn Plus: " + Pawn.Name + " has CompFace but doesn't have valid hair graphic of Graphic_Hair class",
                    "FacialStuff_CompFaceNoValidHair".GetHashCode());
            }
        }
        
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
            _fsGameComp = Current.Game.GetComponent<GameComponent_PawnPlus>();
            Props = (CompProperties_Face)props;
            Pawn = (Pawn)parent;

            if(HeadBehavior == null)
            {
                _headBehavior = (IHeadBehavior)Props.headBehavior.Clone();
                HeadBehavior.Initialize(Pawn);
            }
            BuildPartBehaviors();

            _pawnState = new PawnState(Pawn);
            _perPartStatus = new BodyPartStatus[Pawn.RaceProps.body.AllParts.Count];
            _bodyPartSignals = new Dictionary<int, List<PartSignal>>();
        }

        // Graphics and faction data aren't available in ThingComp.Initialize(). Initialize the members related to those in this method,
        // which is called by a postfix to ResolveAllGraphics() method.
        public void InitializeFace()
        {
            _pawnState.UpdateState();
            
            foreach(var partBehavior in _partBehaviors)
            {
                partBehavior.Initialize(Pawn.RaceProps.body, out List<int> usedBodyPartIndices);
                if(usedBodyPartIndices == null)
                {
                    continue;
                }
                Dictionary<int, List<PartSignal>> partSignalSink = new Dictionary<int, List<PartSignal>>();
                foreach(int bodyPartIdx in usedBodyPartIndices)
                {
                    if(!_bodyPartSignals.ContainsKey(bodyPartIdx))
                    {
                        _bodyPartSignals.Add(bodyPartIdx, new List<PartSignal>());
                    }
                    if(!partSignalSink.ContainsKey(bodyPartIdx))
					{
                        partSignalSink.Add(bodyPartIdx, _bodyPartSignals[bodyPartIdx]);
                    }
                }
                partBehavior.SetPartSignalSink(partSignalSink);
            }
            
            if(_partDefs == null)
			{
                if(Props.partGenHelper == null)
                {
                    Log.Error("Pawn Plus: partGenHelper in CompProperties_Face can't be null. No parts will be generated.");
                    Initialized = true;
                    return;
                }
                Props.partGenHelper.PartsPreGeneration(Pawn);
                _partDefs = new List<PartDef>();
                foreach(string category in PartDef.GetCategoriesInRace(Pawn.RaceProps.body))
                {
                    List<PartDef> parts = PartDef.GetAllPartsFromCategory(Pawn.RaceProps.body, category);
                    if(parts.NullOrEmpty())
                    {
                        continue;
                    }
                    PartDef partDef = Props.partGenHelper.GeneratePartInCategory(Pawn, category, parts);
                    if(partDef == null)
                    {
                        continue;
                    }
                    _partDefs.Add(partDef);
                }
                Props.partGenHelper.PartsPostGeneration(Pawn);
            }

            List<PartData> perPartData = new List<PartData>();
            foreach(var partDef in _partDefs)
			{
                foreach(var part in partDef.parts)
                {
                    PartData partData = new PartData();
                    partData.bodyPartIndex = part.bodyPartLocator._resolvedPartIndex;
                    if(part.graphicProvider == null)
					{
                        Log.Warning(
                            "Pawn Plus: no graphic provider is specified for one of the parts in PartDef " + 
                            partDef.defName +
                            " . The part will not be shown.");
                        continue;
					}
                    partData.graphicProvider = (IGraphicProvider)part.graphicProvider.Clone();
                    partData.renderNodeName = part.renderNodeName;
                    RenderParamManager.GetRenderParams(
                        Pawn,
                        part.renderNodeName,
                        out partData.rootType,
                        out partData.renderParams);
                    partData.graphicProvider.Initialize(
                        Pawn,
                        Pawn.RaceProps.body,
                        part.bodyPartLocator._resolvedBodyPartRecord,
                        partDef.defaultTexPath,
                        partDef.namedTexPaths);
                    perPartData.Add(partData);
                }
            }
            _perPartData = perPartData;

            HashSet<string> occludedRenderNodes = new HashSet<string>();
            foreach(var partDef in _partDefs)
			{
                foreach(var part in partDef.parts)
				{
                    if(part.occludedRenderNodes == null)
					{
                        continue;
					}
                    foreach(var occludedRenderNode in part.occludedRenderNodes)
					{
                        occludedRenderNodes.Add(occludedRenderNode);
					}
				}
            }
            foreach(var partData in _perPartData)
			{
                if(occludedRenderNodes.Contains(partData.renderNodeName))
				{
                    partData.occluded = true;
				}
                else
				{
                    partData.occluded = false;
				}
			}

            // Update the graphic providers to get the portrait graphic
            UpdateGraphicProviders(out bool updatePortrait);

            Initialized = true;
        }
        
		public override void CompTick()
		{
			base.CompTick();
            if(Initialized)
            {
                bool canUpdatePawn =
                    Pawn.Map != null &&
                    !Pawn.InContainerEnclosed &&
                    Pawn.Spawned &&
                    !Find.TickManager.Paused;
                if(canUpdatePawn)
                {
                    _pawnState.UpdateState();
                    HeadBehavior.Update(Pawn, _pawnState, out _cachedHeadFacing);
                    // Clear signals from previous tick
                    foreach(var pair in _bodyPartSignals)
					{
                        pair.Value.Clear();
					}
                    foreach(var partBehavior in _partBehaviors)
					{
                        partBehavior.Update(Pawn, _pawnState);
					}
                    UpdateGraphicProviders(out bool updatePortrait);
                    if(updatePortrait)
					{
                        PortraitsCache.SetDirty(Pawn);
					}
                }
            }
        }
        
        private void BuildPartBehaviors()
		{
            if(_partBehaviors == null)
            {
                _partBehaviors = new List<IPartBehavior>();
            }
            // Check if there is an existing part behavior class from the previous save. If so, use that instance instead of
            // creating a new one.
            // Also, drop existing part behavior instance if it is not defined in <partBehaviors> list anymore.
            List<IPartBehavior> newPartBehaviors = new List<IPartBehavior>();
            // Note that items in Props.partBehaviors are guaranteed to have unique string IDs (IPartBehavior.UniqueID)
            // Look CompProperties_Face.ResolveReferences()
            for(int i = 0; i < Props.partBehaviors.Count; ++i)
            {
                IPartBehavior partBehavior = null;
                int partBehaviorIdx =
                    _partBehaviors.FindIndex(behavior => behavior.UniqueID == Props.partBehaviors[i].UniqueID);
                if(partBehaviorIdx >= 0)
                {
                    partBehavior = _partBehaviors[partBehaviorIdx];
                    _partBehaviors.RemoveAt(partBehaviorIdx);
                } else
                {
                    partBehavior = (IPartBehavior)Props.partBehaviors[i].Clone();
                }
                newPartBehaviors.Add(partBehavior);
            }
            foreach(var oldPartBehavior in _partBehaviors)
            {
                // Scribe_Collections inserts null item if the IPartBehavior implementation class does not exist anymore in the assembly
                if(oldPartBehavior == null)
                {
                    continue;
                }
                Log.Warning(
                    "Pawn Plus: The previously-saved part behavior class (" +
                    oldPartBehavior.GetType().ToString() +
                    " UniqueID: " +
                    oldPartBehavior.UniqueID +
                    ") for pawn (" +
                    Pawn +
                    ") will be dropped because it is no longer defined in <partBehaviors> list in CompProperties_Face.");
            }
            _partBehaviors = newPartBehaviors;
        }

        private void UpdateGraphicProviders(out bool updatePortrait)
		{
            updatePortrait = false;
            foreach(var part in _perPartData)
            {
                bool updatePortraitTemp = false;
                if(!_bodyPartSignals.TryGetValue(part.bodyPartIndex, out List<PartSignal> partSignal))
				{
                    partSignal = _defaultEmptyPartSignals;
                }
                part.graphicProvider.Update(
                    _pawnState,
                    part.bodyPartIndex >= 0 ? 
                        _perPartStatus[part.bodyPartIndex] : _defaultPart,
                    out part.graphic,
                    out part.portraitGraphic,
                    ref part.additionalOffset,
                    ref updatePortraitTemp,
                    partSignal);
                updatePortrait |= updatePortraitTemp;
            }
        }

        public void NotifyBodyPartHediffGained(BodyPartRecord bodyPart, Hediff hediff)
		{
            if(hediff is Hediff_AddedPart hediffAddedPart)
            {
                foreach(var childPart in bodyPart.GetChildParts())
				{
                    _perPartStatus[childPart.Index] =
						new BodyPartStatus()
						{ 
                            missing = false,
                            hediffAddedPart = hediffAddedPart
                        };
                }
            }
            else if(hediff is Hediff_MissingPart)
			{
                foreach(var childPart in bodyPart.GetChildParts())
                {
                    _perPartStatus[childPart.Index] =
                        new BodyPartStatus()
                        {
                            missing = true,
                            hediffAddedPart = null
                        };
                }
            }
        }
        
        public void NotifyBodyPartHediffLost(BodyPartRecord bodyPart, Hediff hediff)
		{
            if(hediff is Hediff_AddedPart)
			{
                foreach(var childPart in bodyPart.GetChildParts())
                {
                    _perPartStatus[childPart.Index] =
                        new BodyPartStatus()
                        {
                            missing = _perPartStatus[childPart.Index].missing,
                            hediffAddedPart = null
                        };
                }
            }
        }

        public void NotifyBodyPartRestored(BodyPartRecord bodyPart)
        {
            HashSet<int> affectedBodyParts = new HashSet<int>();
            foreach(var childPart in bodyPart.GetChildParts())
            {
                affectedBodyParts.Add(childPart.Index);
                _perPartStatus[childPart.Index] =
                    new BodyPartStatus()
                    {
                        missing = false,
                        hediffAddedPart = null
                    };
            }
            // It is possible that the hediff still exists after restoration due to HediffDef.keepOnBodyPartRestoration.
            foreach(var hediff in Pawn.health.hediffSet.hediffs)
			{
                if(hediff.Part == null)
				{
                    continue;
				}
                if(affectedBodyParts.Contains(hediff.Part.Index) && 
                    hediff is Hediff_AddedPart hediffAddedPart)
				{
                    _perPartStatus[hediff.Part.Index] =
                        new BodyPartStatus()
                        {
                            missing = false,
                            hediffAddedPart = hediffAddedPart
                        };
                }
			}
        }

        public Rot4 GetHeadFacing()
		{
            return _cachedHeadFacing;
		}
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref _headBehavior, "headBehavior");
            Scribe_Collections.Look(ref _partBehaviors, "partBehaviors", LookMode.Deep);
            Scribe_Collections.Look(ref _partDefs, "partDefs", LookMode.Def);
        }
    }
}
