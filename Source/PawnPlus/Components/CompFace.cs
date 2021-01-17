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
            public RootType rootType;
            public IPartRenderer renderer;
            public TickDelegate tickDelegate;
            public string renderNodeName;
            public bool occluded;
            public RenderParam[] renderParams;
        }
        
        private FactionDef _originalFaction;
        private IReadOnlyList<PartData> _perPartData;
        private BodyPartSignals _bodyPartSignals;
        private BodyPartStatus _bodyPartStatus;
        private Rot4 _cachedHeadFacing;
        private PawnState _pawnState;
        private IHeadBehavior _headBehavior;
        private List<IPartBehavior> _partBehaviors;
        private List<PartDef> _partDefs;
        // Used for distance culling of face details
        private GameComponent_PawnPlus _fsGameComp;
        
        public bool Initialized { get; private set; }
                                
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
            headFacing = portrait ? _headBehavior.GetRotationForPortrait() : headFacing;
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
                            // If portrait is being rendered, follow the body rotation.
                            Rot4 partRot4 = bodyFacing;
                            if(!portrait && part.rootType == RootType.Head)
							{
                                partRot4 = headFacing;
                            }
                            if(part.renderParams[partRot4.AsInt].render)
							{
                                part.renderer.Render(
                                    headPos, 
                                    headQuat, 
                                    partRot4, 
                                    part.renderParams[partRot4.AsInt].offset, 
                                    part.renderParams[partRot4.AsInt].mesh, 
                                    portrait);
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

            if(_headBehavior == null)
            {
                _headBehavior = (IHeadBehavior)Props.headBehavior.Clone();
            }
            BuildPartBehaviors();

            _pawnState = new PawnState(Pawn);
            _bodyPartSignals = new BodyPartSignals(Pawn.RaceProps.body);
            _bodyPartStatus = new BodyPartStatus(Pawn);
        }

        // Graphics and faction data aren't available in ThingComp.Initialize(). Initialize the members related to those in this method,
        // which is called by a postfix to ResolveAllGraphics() method.
        public void InitializeFace()
        {
            if(_originalFaction == null)
			{
                _originalFaction = Pawn.Faction.def ?? Faction.OfPlayer.def;
			}

            _pawnState.UpdateState();

            _headBehavior.Initialize(Pawn);
            foreach(var partBehavior in _partBehaviors)
            {
                partBehavior.Initialize(Pawn.RaceProps.body, _bodyPartSignals);
            }
            
            if(_partDefs == null)
			{
                if(Props.partGenHelper == null)
                {
                    Log.Error("Pawn Plus: partGenHelper in CompProperties_Face can't be null. No parts will be generated.");
                    Initialized = false;
                    return;
                }
                Props.partGenHelper.PartsPreGeneration(Pawn);
                Dictionary<string, PartDef> partDefInCategory = 
                    Props.partGenHelper.GeneratePartInCategory(Pawn, _originalFaction, PartDef.GetCategoriesInRace(Pawn.RaceProps.body));                
                _partDefs = new List<PartDef>();
                foreach(var pair in partDefInCategory)
				{
                    _partDefs.Add(pair.Value);
                }
                Props.partGenHelper.PartsPostGeneration(Pawn);
            }

            List<PartData> perPartData = new List<PartData>();
            foreach(var partDef in _partDefs)
			{
                if(partDef.parts.NullOrEmpty())
				{
                    continue;
				}
                foreach(var part in partDef.parts)
                {
                    PartData partData = new PartData();
                    if(part.partRenderer == null)
					{
                        Log.Warning(
                            "Pawn Plus: no graphic provider is specified for one of the parts in PartDef " + 
                            partDef.defName +
                            " . The part will not be shown.");
                        continue;
					}
                    partData.renderer = (IPartRenderer)part.partRenderer.Clone();
                    partData.renderNodeName = part.renderNodeName;
                    RenderParamManager.GetRenderParams(
                        Pawn,
                        part.renderNodeName,
                        out partData.rootType,
                        out partData.renderParams);
                    partData.renderer.Initialize(
                        Pawn,
                        Pawn.RaceProps.body,
                        partDef.defaultTexPath,
                        partDef.namedTexPaths,
                        _bodyPartSignals,
                        ref partData.tickDelegate);
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
                    Pawn.Spawned;
                if(canUpdatePawn)
                {
                    _pawnState.UpdateState();
                    _headBehavior.Update(Pawn, _pawnState, out _cachedHeadFacing);
                    foreach(var partBehavior in _partBehaviors)
					{
                        partBehavior.Update(Pawn, _pawnState);
					}
                    bool updatePortrait = false;
                    foreach(var part in _perPartData)
                    {
                        if(part.tickDelegate.NormalUpdate != null)
						{
                            bool updatePortraitTemp = false;
                            part.tickDelegate.NormalUpdate(
                                _pawnState,
                                _bodyPartStatus,
                                ref updatePortraitTemp);
                            updatePortrait |= updatePortraitTemp;
                        }
                    }
                    if(updatePortrait)
					{
                        PortraitsCache.SetDirty(Pawn);
					}
                }
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if(Initialized)
            {
                bool canUpdatePawn =
                    Pawn.Map != null &&
                    !Pawn.InContainerEnclosed &&
                    Pawn.Spawned;
                if(canUpdatePawn)
                {
                    bool updatePortrait = false;
                    foreach(var part in _perPartData)
                    {
                        if(part.tickDelegate.RareUpdate != null)
                        {
                            bool updatePortraitTemp = false;
                            part.tickDelegate.RareUpdate(
                                _pawnState,
                                _bodyPartStatus,
                                ref updatePortraitTemp);
                            updatePortrait |= updatePortraitTemp;
                        }
                    }
                    if(updatePortrait)
                    {
                        PortraitsCache.SetDirty(Pawn);
                    }
                }
            }
        }

        public override void CompTickLong()
		{
			base.CompTickLong();
            if(Initialized)
            {
                bool canUpdatePawn =
                    Pawn.Map != null &&
                    !Pawn.InContainerEnclosed &&
                    Pawn.Spawned;
                if(canUpdatePawn)
                {
                    bool updatePortrait = false;
                    foreach(var part in _perPartData)
                    {
                        if(part.tickDelegate.LongUpdate != null)
                        {
                            bool updatePortraitTemp = false;
                            part.tickDelegate.LongUpdate(
                                _pawnState,
                                _bodyPartStatus,
                                ref updatePortraitTemp);
                            updatePortrait |= updatePortraitTemp;
                        }
                    }
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
        
        public void NotifyBodyPartHediffGained(BodyPartRecord bodyPart, Hediff hediff)
		{
            _bodyPartStatus.NotifyBodyPartHediffGained(bodyPart, hediff);
        }
        
        public void NotifyBodyPartHediffLost(BodyPartRecord bodyPart, Hediff hediff)
		{
            _bodyPartStatus.NotifyBodyPartHediffLost(bodyPart, hediff);
        }

        public void NotifyBodyPartRestored(BodyPartRecord bodyPart)
        {
            _bodyPartStatus.NotifyBodyPartRestored(bodyPart);
        }

        public void SetHeadTarget(Thing target, IHeadBehavior.TargetType targetType)
		{
            _headBehavior.SetTarget(target, targetType);
		}

        public Rot4 GetHeadFacing()
		{
            return _cachedHeadFacing;
		}
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref _originalFaction, "originalFaction");
            Scribe_Deep.Look(ref _headBehavior, "headBehavior");
            Scribe_Collections.Look(ref _partBehaviors, "partBehaviors", LookMode.Deep);
            Scribe_Collections.Look(ref _partDefs, "partDefs", LookMode.Def);
        }
    }
}
