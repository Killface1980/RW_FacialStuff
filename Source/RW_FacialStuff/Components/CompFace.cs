﻿using FacialStuff.Animator;
using FacialStuff.DefOfs;
using FacialStuff.Defs;
using FacialStuff.GraphicsFS;
using FacialStuff.Utilities;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using FacialStuff.Harmony;
using UnityEngine;
using Verse;
using static FacialStuff.Offsets;
using FacialStuff.AI;

namespace FacialStuff
{
    public class CompFace : ThingComp
    {
        public FacePartStats BodyStat;
        public FaceGraphic PawnFaceGraphic;
        
        private Faction _originFactionInt;
        private FaceData _faceData;
        private IMouthBehavior.Params _cachedMouthParam = new IMouthBehavior.Params();
        private List<IEyeBehavior.Params> _cachedEyeParam;

        public bool Initialized { get; private set; }

        public IMouthBehavior MouthBehavior { get; private set; }

        public IEyeBehavior EyeBehavior { get; private set; }

        public FaceMaterial FaceMaterial { get; set; }
        
        public FullHead FullHeadType { get; set; } = FullHead.Undefined;
        
        public PartStatusTracker PartStatusTracker { get; private set; }

        public PawnHeadRotationAI HeadRotationAI { get; private set; }
                        
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

        public CompProperties_Face Props => (CompProperties_Face)this.props;
        
        public Pawn Pawn => this.parent as Pawn;
        
        public HeadCoverage CurrentHeadCoverage { get; set; }

        private List<PawnHeadDrawer> PawnHeadDrawers { get; set; }
                
		// Return true if head was drawn. False if not.
		public bool DrawHead(
            RotDrawMode bodyDrawType, 
            bool portrait, 
            bool headStump,
            Rot4 bodyFacing, 
            Rot4 headFacing,
            Vector3 headPos,
            Quaternion headQuat)
        {
            if(!PawnHeadDrawers.Any() || Pawn.IsChild() || Pawn.GetCompAnim().Deactivated)
			{
                return false;
			}
            bool headDrawn = false;
            // Draw head
            foreach(var headDrawer in PawnHeadDrawers)
            {
                headDrawer.DrawBasicHead(
                    headPos,
                    headQuat,
                    bodyDrawType,
                    headStump,
                    portrait,
                    out bool headDrawnTemp);
                headDrawn |= headDrawnTemp;
            }
            if(headDrawn)
            {
                if(bodyDrawType != RotDrawMode.Dessicated && !headStump)
                {
                    if(Props.hasWrinkles)
                    {
                        Vector3 wrinkleLoc = headPos;
                        wrinkleLoc.y += YOffset_Wrinkles;
                        // Draw wrinkles
                        foreach(var headDrawer in PawnHeadDrawers)
                        {
                            headDrawer.DrawWrinkles(wrinkleLoc, bodyDrawType, headQuat, portrait);
                        }
                    }
                    if(EyeBehavior.NumEyes > 0)
                    {
                        Vector3 eyeLoc = headPos;
                        eyeLoc.y += YOffset_Eyes;
                        // Draw natural eyes
                        DrawEyes(eyeLoc, headFacing, headQuat, portrait);

                        Vector3 browLoc = headPos;
                        browLoc.y += YOffset_Brows;
                        // Draw brows above eyes
                        foreach(var headDrawer in PawnHeadDrawers)
                        {
                            headDrawer.DrawBrows(browLoc, headQuat, portrait);
                        }
                        // Draw added eye parts on top of natural eyes
                        Vector3 unnaturalEyeLoc = headPos;
                        unnaturalEyeLoc.y += YOffset_UnnaturalEyes;
                        foreach(var headDrawer in PawnHeadDrawers)
                        {
                            headDrawer.DrawUnnaturalEyeParts(unnaturalEyeLoc, headQuat, portrait);
                        }
                    }
                    // Portrait obviously ignores the y offset, thus render the beard after the body apparel (again)
                    if(Props.hasBeard)
                    {
                        Vector3 beardLoc = headPos;
                        Vector3 tacheLoc = headPos;
                        beardLoc.y += headFacing == Rot4.North ? -YOffset_Head - YOffset_Beard : YOffset_Beard;
                        tacheLoc.y += headFacing == Rot4.North ? -YOffset_Head - YOffset_Tache : YOffset_Tache;
                        // Draw beard and mustache
                        foreach(var headDrawer in PawnHeadDrawers)
                        {
                            headDrawer.DrawBeardAndTache(beardLoc, tacheLoc, headQuat, portrait);
                        }
                    }
                    if(Props.hasMouth && 
                        _cachedMouthParam.render && 
                        _cachedMouthParam.mouthTextureIdx >= 0 && 
                        FaceData.BeardDef.drawMouth && 
                        Controller.settings.UseMouth)
                    {
                        Vector3 mouthLoc = headPos;
                        mouthLoc.y += YOffset_Mouth;
                        DrawMouth(mouthLoc, headFacing, headQuat, portrait);
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

        public void DrawEyes(Vector3 headPos, Rot4 headFacing, Quaternion headQuat, bool portrait)
		{
            Vector3 drawLoc = headPos;
            FaceGraphic faceGraphic = PawnFaceGraphic;
            if(faceGraphic == null)
            {
                return;
            }
            for(int partIdx = 0; partIdx < EyeBehavior.NumEyes; ++partIdx)
            {
                if(_cachedEyeParam[partIdx].render)
                {
                    Mesh eyeMesh = MeshPoolFS.GetFaceMesh(PawnCrownType, headFacing, _cachedEyeParam[partIdx].mirror);
                    Material eyeMat = faceGraphic.EyeMatAt(
                        partIdx,
                        headFacing,
                        portrait,
                        _cachedEyeParam[partIdx].openEye,
                        PartStatusTracker.GetEyePartLevel(partIdx));
                    if(eyeMat != null)
                    {
                        drawLoc.y += Offsets.YOffset_LeftPart;
                        GenDraw.DrawMeshNowOrLater(
                            eyeMesh,
                            drawLoc,
                            headQuat,
                            eyeMat,
                            portrait);
                    }
                }
            }
        }

        public void DrawMouth(Vector3 headPos, Rot4 headFacing, Quaternion headQuat, bool portrait)
		{
            Vector3 mouthLoc = headPos;
            mouthLoc.y += YOffset_Mouth;
            int mouthTextureIdx = _cachedMouthParam.mouthTextureIdx;
            if(portrait)
			{
                mouthTextureIdx = MouthBehavior.GetTextureIndexForPortrait();
			}
            Material mouthMat = PawnFaceGraphic.MouthMatAt(headFacing, mouthTextureIdx);
            if(mouthMat == null)
            {
                return;
            }
            Mesh meshMouth = MeshPoolFS.GetFaceMesh(PawnCrownType, headFacing, _cachedMouthParam.mirror);
            Vector3 mouthOffset = MeshPoolFS.mouthOffsetsHeadType[(int)FullHeadType];
            switch(headFacing.AsInt)
            {
                case 1:
                    mouthOffset = new Vector3(mouthOffset.x, 0f, mouthOffset.y);
                    break;
                case 2:
                    mouthOffset = new Vector3(0, 0f, mouthOffset.y);
                    break;
                case 3:
                    mouthOffset = new Vector3(-mouthOffset.x, 0f, mouthOffset.y);
                    break;
                default:
                    mouthOffset = Vector3.zero;
                    break;
            }
            mouthLoc += headQuat * mouthOffset;
            GenDraw.DrawMeshNowOrLater(meshMouth, mouthLoc, headQuat, mouthMat, portrait);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            try
			{
                if(HeadRotationAI == null)
                {
                    HeadRotationAI = new PawnHeadRotationAI(Pawn);
                }
                if(PartStatusTracker == null)
                {
                    PartStatusTracker = new PartStatusTracker(Pawn.RaceProps.body);
                }
                InitializePawnDrawer();
            } 
            catch(Exception e)
			{
                Log.Message("Facial Stuff: error while initializing CompFace: " + e.Message);
			}
        }

        // Faction data isn't available during ThingComp.Initialize(). Initialize faction-related members here.
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
            MouthBehavior = (IMouthBehavior)Props.mouthBehavior.Clone();
            EyeBehavior = (IEyeBehavior)Props.eyeBehavior.Clone();
            MouthBehavior.InitializeTextureIndex(FaceData.MouthSetDef.texNames.AsReadOnly());
            _cachedEyeParam = new List<IEyeBehavior.Params>(EyeBehavior.NumEyes);
            for(int i = 0; i < EyeBehavior.NumEyes; ++i)
			{
                _cachedEyeParam.Add(new IEyeBehavior.Params()); 
			}
            FullHeadType = MeshPoolFS.GetFullHeadType(Pawn.gender, PawnCrownType, PawnHeadType);
            PawnFaceGraphic = new FaceGraphic(this, EyeBehavior.NumEyes);
            FaceMaterial = new FaceMaterial(this, PawnFaceGraphic);
            Initialized = true;
        }
        
        public void TickDrawers(Rot4 bodyFacing, ref Rot4 headFacing, PawnGraphicSet graphics, bool portrait)
        {
            PawnState pawnState = new PawnState();
            pawnState.alive = !Pawn.Dead;
            pawnState.aiming = Pawn.Aiming();
            pawnState.inPainShock = Pawn.health.InPainShock;
            pawnState.fleeing = Pawn.Fleeing();
            pawnState.burning = Pawn.IsBurning();
            if(Find.TickManager.TicksGame % 180 == 0)
            {
                pawnState.sleeping = !Pawn.Awake();
            }
            
            bool canUpdatePawn =
                Pawn.Map != null &&
                !Pawn.InContainerEnclosed &&
                Pawn.Spawned &&
                !Find.TickManager.Paused;
            HeadRotationAI.Tick(canUpdatePawn, bodyFacing, pawnState);
            if(canUpdatePawn)
            {
                _cachedMouthParam.Reset();
                foreach(var eyeParam in _cachedEyeParam)
				{
                    eyeParam.Reset();
				}
                MouthBehavior.Update(Pawn, headFacing, pawnState, _cachedMouthParam);
                EyeBehavior.Update(Pawn, headFacing, pawnState, _cachedEyeParam);
            }

            if(!portrait)
			{
                headFacing = HeadRotationAI.CurrentRotation;
            }

            if(!this.PawnHeadDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnHeadDrawers.Count;
                while(i < count)
                {
                    this.PawnHeadDrawers[i].Tick(bodyFacing, headFacing, graphics);
                    i++;
                }
            }
        }
        
        public void DrawAlienHeadAddons(Vector3 headPos, bool portrait, Quaternion headQuat, Vector3 currentLoc)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i].DrawAlienHeadAddons(headPos, portrait, headQuat, currentLoc);
                i++;
            }
        }
        
        public void InitializePawnDrawer()
        {
            if(Props.headDrawers.Any())
            {
                PawnHeadDrawers = new List<PawnHeadDrawer>();
                for(int i = 0; i < Props.headDrawers.Count; i++)
                {
                    PawnHeadDrawer thingComp =
                        (PawnHeadDrawer)Activator.CreateInstance(Props.headDrawers[i].GetType());
                    thingComp.CompFace = this;
                    thingComp.Pawn = Pawn;
                    PawnHeadDrawers.Add(thingComp);
                    thingComp.Initialize();
                }
            }
        }
                        
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref this._originFactionInt, "pawnFaction");
            Scribe_Deep.Look(ref this._faceData, "pawnFace");
        }
    }
}