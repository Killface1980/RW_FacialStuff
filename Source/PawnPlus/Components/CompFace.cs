using PawnPlus.Animator;
using PawnPlus.DefOfs;
using PawnPlus.Defs;
using PawnPlus.Graphics;
using PawnPlus.Utilities;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using PawnPlus.Harmony;
using UnityEngine;
using Verse;
using static PawnPlus.Offsets;
using PawnPlus.AI;
using System.Collections.ObjectModel;
using System.Linq;

namespace PawnPlus
{
    public class CompFace : ThingComp
    {
        public FaceGraphic PawnFaceGraphic;

        private class PerBodyPart
        {
            public IGraphicProvider graphicProvider;
            public Queue<PartSignal> signals = new Queue<PartSignal>();
            public PartRender.Attachment attachment;
            public Graphic graphic;
            public Graphic portraitGraphic;
            public RenderParam[] renderParams;
        }

        private class HediffChangeEvent
		{
            public enum Type
			{
                PartRestored,
                PartLost, // This indicates addition of Hediff_Removed hediff
                HediffGained,
                HediffLost
			}

            public HediffChangeEvent.Type eventType;
            public int bodyPartIndex;
            public Hediff hediff;
        }
        
        private IReadOnlyDictionary<int, PerBodyPart> _perBodyPartData;
        private Faction _originFactionInt;
        private FaceData _faceData;
        private RenderParam[] _mouthRenderParams;
        private Rot4 _cachedHeadFacing;
        private IMouthBehavior.Params _cachedMouthParam = new IMouthBehavior.Params();
        private PawnState _pawnState;
        private IHeadBehavior _headBehavior;
        private IMouthBehavior _mouthBehavior;
        private IEyeBehavior _eyeBehavior;
        private Dictionary<int, Queue<PartSignal>> _eyeBehaviorParams = new Dictionary<int, Queue<PartSignal>>();
        private Queue<HediffChangeEvent> _hediffEvents =  new Queue<HediffChangeEvent>();
        // Used for distance culling of face details
        private GameComponent_FacialStuff _fsGameComp;
        
        public bool Initialized { get; private set; }

        public IHeadBehavior HeadBehavior => _headBehavior;

        public IMouthBehavior MouthBehavior => _mouthBehavior;
        
        public FaceMaterial FaceMaterial { get; set; }
        
        public FullHead FullHeadType { get; set; } = FullHead.Undefined;
                                        
        public Faction OriginFaction => _originFactionInt;
        
        public virtual CrownType PawnCrownType => Pawn?.story.crownType ?? CrownType.Average;

        public FaceData FaceData
        {
            get
			{
                return _faceData;
            }

            set
			{
                _faceData = value;
			}
        }

        public HeadType PawnHeadType
        {
            get
            {
                if(Pawn.story?.HeadGraphicPath != null)
                {
                    if(Pawn.story.HeadGraphicPath.Contains("Pointy"))
                    {
                        return HeadType.Pointy;
                    }

                    if(Pawn.story.HeadGraphicPath.Contains("Wide"))
                    {
                        return HeadType.Wide;
                    }
                }

                return HeadType.Normal;
            }
        }

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
                        foreach(KeyValuePair<int, PerBodyPart> pair in _perBodyPartData)
                        {
                            Rot4 partRot4 = portrait ? Rot4.South : headFacing;
                            Vector3 partDrawPos = headPos;
                            Quaternion partQuat = headQuat;
                            switch(pair.Value.attachment)
                            {
                                case PartRender.Attachment.Body:
                                    break;

                                case PartRender.Attachment.Head:
                                    break;
                            }
                            Graphic graphic = portrait ?
                                pair.Value.portraitGraphic :
                                pair.Value.graphic;
                            if(graphic == null)
							{
                                continue;
							}
                            Material partMat = graphic.MatAt(partRot4);
                            if(partMat != null)
                            {
                                Vector3 offset = pair.Value.renderParams[partRot4.AsInt].offset;
                                offset = partQuat * offset;
                                GenDraw.DrawMeshNowOrLater(
                                        pair.Value.renderParams[partRot4.AsInt].mesh,
                                        partDrawPos + offset,
                                        partQuat,
                                        partMat,
                                        portrait);
                            }
                        }

                        if(Props.hasMouth &&
                            FaceData.BeardDef.drawMouth &&
                            Controller.settings.UseMouth)
                        {
                            Vector3 mouthLoc = headPos;
                            mouthLoc.y += YOffset_Mouth;
                            DrawMouth(mouthLoc, headFacing, headQuat, portrait);
                        }
                        if(headFacing != Rot4.North)
						{
                            if(Props.hasWrinkles)
                            {
                                Vector3 wrinkleLoc = headPos;
                                wrinkleLoc.y += YOffset_Wrinkles;
                                // Draw wrinkles
                                DrawWrinkles(wrinkleLoc, headFacing, headQuat, bodyDrawType, portrait);
                            }

                            Vector3 browLoc = headPos;
                            browLoc.y += YOffset_Brows;
                            // Draw brows above eyes
                            DrawBrows(browLoc, headFacing, headQuat, portrait);

                            if(Props.hasBeard)
                            {
                                Vector3 beardLoc = headPos;
                                Vector3 tacheLoc = headPos;
                                beardLoc.y += YOffset_Beard;
                                tacheLoc.y += YOffset_Tache;
                                // Draw beard and mustache
                                DrawBeardAndTache(graphicSet, beardLoc, tacheLoc, headFacing, headQuat, portrait);
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
                    "Facial Stuff: " + Pawn.Name + " has CompFace but doesn't have valid hair graphic of Graphic_Hair class",
                    "FacialStuff_CompFaceNoValidHair".GetHashCode());
            }
        }

        public void DrawWrinkles(Vector3 drawPos, Rot4 headFacing, Quaternion headQuat, RotDrawMode bodyDrawType, bool portrait)
		{
            if(!Controller.settings.UseWrinkles)
            {
                return;
            }
            Material wrinkleMat = FaceMaterial.WrinkleMatAt(headFacing, bodyDrawType);
            if(wrinkleMat == null)
            {
                return;
            }
            Mesh headMesh = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
            GenDraw.DrawMeshNowOrLater(headMesh, drawPos, headQuat, wrinkleMat, portrait);
        }
        
        public void DrawBrows(Vector3 drawPos, Rot4 headFacing, Quaternion headQuat, bool portrait)
		{
            Material browMat = FaceMaterial.BrowMatAt(headFacing);
            if(browMat == null)
            {
                return;
            }
            Mesh eyeMesh = MeshPoolFS.GetFaceMesh(PawnCrownType, headFacing, false);
            GenDraw.DrawMeshNowOrLater(
                eyeMesh,
                drawPos,
                headQuat,
                browMat,
                portrait);
        }

        public void DrawMouth(Vector3 drawPos, Rot4 headFacing, Quaternion headQuat, bool portrait)
		{
            if(PawnFaceGraphic == null || _mouthRenderParams == null || !_mouthRenderParams[headFacing.AsInt].render)
			{
                return;
			}
            Mesh mouthMesh = _mouthRenderParams[headFacing.AsInt].mesh;
            int mouthTextureIdx = portrait ?
                MouthBehavior.GetTextureIndexForPortrait() :
                _cachedMouthParam.mouthTextureIdx;
            Material mouthMat = PawnFaceGraphic.MouthMatAt(headFacing, mouthTextureIdx);
            if(mouthMat != null)
            {
                Vector3 offset = new Vector3(
                    _mouthRenderParams[headFacing.AsInt].offset.x,
                    0,
                    _mouthRenderParams[headFacing.AsInt].offset.y);
                offset = headQuat * offset;
                GenDraw.DrawMeshNowOrLater(
                    mouthMesh,
                    drawPos + offset,
                    headQuat,
                    mouthMat,
                    portrait);
            }
        }

        public void DrawBeardAndTache(PawnGraphicSet graphicSet, Vector3 beardLoc, Vector3 tacheLoc, Rot4 headFacing, Quaternion headQuat, bool portrait)
        {
            Mesh headMesh = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
            if(FaceData.BeardDef.IsBeardNotHair())
            {
                headMesh = graphicSet.HairMeshSet.MeshAt(headFacing);
            }
            Material beardMat = FaceMaterial.BeardMatAt(headFacing);
            Material moustacheMatAt = FaceMaterial.MoustacheMatAt(headFacing);
            if(beardMat != null)
            {
                GenDraw.DrawMeshNowOrLater(headMesh, beardLoc, headQuat, beardMat, portrait);
            }
            if(moustacheMatAt != null)
            {
                GenDraw.DrawMeshNowOrLater(headMesh, tacheLoc, headQuat, moustacheMatAt, portrait);
            }
        }

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
            _fsGameComp = Current.Game.GetComponent<GameComponent_FacialStuff>();
            Props = (CompProperties_Face)props;
            Pawn = (Pawn)parent;
            if(HeadBehavior == null)
            {
                _headBehavior = (IHeadBehavior)Props.headBehavior.Clone();
                HeadBehavior.Initialize(Pawn);
            }
            if(MouthBehavior == null)
            {
                _mouthBehavior = (IMouthBehavior)Props.mouthBehavior.Clone();
            }
            if(_eyeBehavior == null)
			{
                _eyeBehavior = (IEyeBehavior)Props.eyeBehavior.Clone();
            }
            _pawnState = new PawnState(Pawn);
        }
        
		// Graphics and faction data aren't available in ThingComp.Initialize(). Initialize the members related to those in this method,
		// which is called by a postfix to ResolveAllGraphics() method.
		public void InitializeFace()
        {
            if(_originFactionInt == null)
            {
                _originFactionInt = Pawn.Faction ?? Faction.OfPlayer;
            }
            if(FaceData == null)
            {
                FaceData = new FaceData(this, OriginFaction?.def);
            }

            HeadRenderDef.GetCachedHeadRenderParams(
                Pawn.RaceProps.body,
                Pawn.story.HeadGraphicPath,
                out Dictionary<int, RenderParam[]> eyeRenderParams,
                out _mouthRenderParams);
            var perBodyPartData = new SortedDictionary<int, PerBodyPart>();
            foreach(BodyPartLocator partLocator in FaceData.EyeDef.representBodyParts)
            {
                if(partLocator.resolvedPartIndex < 0)
                {
                    continue;
                }
                if(partLocator.bodyDef == Pawn.RaceProps.body)
                {
                    if(!perBodyPartData.ContainsKey(partLocator.resolvedPartIndex))
                    {
                        PerBodyPart perBodyPart = new PerBodyPart();
                        perBodyPart.graphicProvider = FaceData.EyeDef.graphicProvider?.Clone() as IGraphicProvider;
                        if(perBodyPart.graphicProvider == null)
                        {
                            continue;
                        }
                        perBodyPart.renderParams = eyeRenderParams[partLocator.resolvedPartIndex];
                        if(perBodyPart.renderParams == null)
                        {
                            continue;
                        }
                        perBodyPart.graphicProvider.Initialize(
                            Pawn, Pawn.RaceProps.body,
                            partLocator.resolvedBodyPartRecord,
                            FaceData.EyeDef.defaultTexPath,
                            FaceData.EyeDef.namedTexPaths);
                        // TODO: get value from RenderDef properly
                        perBodyPart.attachment = PartRender.Attachment.Head;
                        perBodyPartData.Add(partLocator.resolvedPartIndex, perBodyPart);
                    } else
                    {
                        Log.Warning("Facial Stuff: Pawn " + Pawn.Label + " already has per body part data initialized for index " + partLocator.resolvedPartIndex);
                    }
                }
            }
            // Make the dictionary unmodifiable
            _perBodyPartData = perBodyPartData;

            _eyeBehavior.Initialize(Pawn.RaceProps.body, out List<int> usedBodyPartIndices);
            if(usedBodyPartIndices != null)
            {
                foreach(int bodyPartIndex in usedBodyPartIndices)
				{
                    if(bodyPartIndex >= 0 && _perBodyPartData.ContainsKey(bodyPartIndex))
					{
                        _eyeBehaviorParams.Add(bodyPartIndex, _perBodyPartData[bodyPartIndex].signals);
                    }
                }
            }

            _pawnState.UpdateState();
            // Update the graphic providers to get the portrait graphic
            UpdateGraphicProviders(out bool updatePortrait);

            MouthBehavior.InitializeTextureIndex(FaceData.MouthSetDef.texNames.AsReadOnly());

            FullHeadType = MeshPoolFS.GetFullHeadType(Pawn.gender, PawnCrownType, PawnHeadType);
            PawnFaceGraphic = new FaceGraphic(this, 2);
            FaceMaterial = new FaceMaterial(this, PawnFaceGraphic);
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
                    _eyeBehavior.Update(Pawn, _pawnState, _eyeBehaviorParams);
                    UpdateGraphicProviders(out bool updatePortrait);
                    if(updatePortrait)
					{
                        PortraitsCache.SetDirty(Pawn);
					}

                    _cachedMouthParam.Reset();
                    MouthBehavior.Update(Pawn, _cachedHeadFacing, _pawnState, _cachedMouthParam);
                }
            }
        }
        
        private void UpdateGraphicProviders(out bool updatePortrait)
		{
            updatePortrait = false;
            foreach(KeyValuePair<int, PerBodyPart> pair in _perBodyPartData)
            {
                bool updatePortraitTemp = false;
                pair.Value.graphicProvider.Update(
                    _pawnState,
                    out pair.Value.graphic,
                    out pair.Value.portraitGraphic,
                    ref updatePortraitTemp,
                    pair.Value.signals);
                pair.Value.signals.Clear();
                updatePortrait |= updatePortraitTemp;
            }
        }

        public void NotifyBodyPartHediffGained(BodyPartRecord bodyPart, Hediff hediff)
		{
            // ThingComp.Initialize() is called before Pawn is fully defined (including the hediffs). This means that 
            // _perBodyPartData can't be initialized in Initialize(). Therefore, the hediff events are stored in a queue
            // then replayed when the comp updates.
            // If the hediff is an added part, notify the child parts that the hediff has been added to them also.
            if(hediff is Hediff_AddedPart)
            {
                foreach(var childPart in bodyPart.GetChildParts())
				{
                    // PartRestored event will be fired before AddedPart hediff is added if the part had been missing.
                    // Therefore, there is no need to create PartRestored event here.
                    _hediffEvents.Enqueue(new HediffChangeEvent()
                    {
                        eventType = HediffChangeEvent.Type.HediffGained,
                        bodyPartIndex = bodyPart.Index,
                        hediff = hediff
                    });
                }
            }
            else if(hediff is Hediff_MissingPart)
			{
                foreach(var childPart in bodyPart.GetChildParts())
                {
                    _hediffEvents.Enqueue(new HediffChangeEvent()
                    {
                        eventType = HediffChangeEvent.Type.PartLost,
                        bodyPartIndex = bodyPart.Index,
                        hediff = hediff
                    });
                }
            }
        }
        
        public void NotifyBodyPartHediffLost(BodyPartRecord bodyPart, Hediff hediff)
		{
            // If the removed hediff is an added part, notify the child parts that they've been removed also.
            if(hediff is Hediff_AddedPart)
			{
                foreach(var childPart in bodyPart.GetChildParts())
                {
                    _hediffEvents.Enqueue(new HediffChangeEvent()
                    {
                        eventType = HediffChangeEvent.Type.HediffLost,
                        bodyPartIndex = bodyPart.Index,
                        hediff = hediff
                    });
                }
            }
        }

        public void NotifyBodyPartRestored(BodyPartRecord bodyPart)
        {
            foreach(var childPart in bodyPart.GetChildParts())
            {
                _hediffEvents.Enqueue(new HediffChangeEvent()
                {
                    eventType = HediffChangeEvent.Type.PartRestored,
                    bodyPartIndex = bodyPart.Index,
                    hediff = null
                });
            }
        }

        public Rot4 GetHeadFacing()
		{
            return _cachedHeadFacing;
		}
        
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref this._originFactionInt, "pawnFaction");
            Scribe_Deep.Look(ref this._faceData, "pawnFace");
            Scribe_Deep.Look(ref _headBehavior, "headBehavior");
            Scribe_Deep.Look(ref _mouthBehavior, "mouthBehavior");
            Scribe_Deep.Look(ref _eyeBehavior, "eyeBehavior");
        }
    }
}
