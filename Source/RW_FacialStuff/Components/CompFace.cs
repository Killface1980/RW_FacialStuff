using FacialStuff.Animator;
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
        public FaceGraphic PawnFaceGraphic;

        private Faction _originFactionInt;
        private FaceData _faceData;
        private IMouthBehavior.Params _cachedMouthParam = new IMouthBehavior.Params();
        private List<IEyeBehavior.Params> _cachedEyeParam;
        private PawnState _pawnState = new PawnState();
        // Used for distance culling of face details
        private GameComponent_FacialStuff _fsGameComp;

        public bool Initialized { get; private set; }

        public IHeadBehavior HeadBehavior { get; private set; }

        public IMouthBehavior MouthBehavior { get; private set; }

        public IEyeBehavior EyeBehavior { get; private set; }

        public FaceMaterial FaceMaterial { get; set; }
        
        public FullHead FullHeadType { get; set; } = FullHead.Undefined;
        
        public PartStatusTracker PartStatusTracker { get; private set; }
                                
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
                        if(EyeBehavior.NumEyes > 0)
                        {
                            Vector3 eyeLoc = headPos;
                            eyeLoc.y += YOffset_Eyes;
                            // Draw natural eyes
                            DrawEyes(eyeLoc, headFacing, headQuat, portrait);
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
                            if(EyeBehavior.NumEyes > 0)
                            {
                                Vector3 browLoc = headPos;
                                browLoc.y += YOffset_Brows;
                                // Draw brows above eyes
                                DrawBrows(browLoc, headFacing, headQuat, portrait);
                            }
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

        public void DrawEyes(Vector3 drawPos, Rot4 headFacing, Quaternion headQuat, bool portrait)
		{
            Vector3 drawLoc = drawPos;
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
            if(!_cachedMouthParam.render || _cachedMouthParam.mouthTextureIdx < 0)
			{
                return;
			}
            int mouthTextureIdx = _cachedMouthParam.mouthTextureIdx;
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
            drawPos += headQuat * mouthOffset;
            GenDraw.DrawMeshNowOrLater(meshMouth, drawPos, headQuat, mouthMat, portrait);
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
            if(PartStatusTracker == null)
            {
                PartStatusTracker = new PartStatusTracker(Pawn.RaceProps.body);
            }
            HeadBehavior = (IHeadBehavior)Props.headBehavior.Clone();
            MouthBehavior = (IMouthBehavior)Props.mouthBehavior.Clone();
            EyeBehavior = (IEyeBehavior)Props.eyeBehavior.Clone();
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
            bool canUpdatePawn =
                Pawn.Map != null &&
                !Pawn.InContainerEnclosed &&
                Pawn.Spawned &&
                !Find.TickManager.Paused;
            if(canUpdatePawn && !portrait)
            {
                _pawnState.alive = !Pawn.Dead;
                _pawnState.aiming = Pawn.Aiming();
                _pawnState.inPainShock = Pawn.health.InPainShock;
                _pawnState.fleeing = Pawn.Fleeing();
                _pawnState.burning = Pawn.IsBurning();
                if(Find.TickManager.TicksGame % 180 == 0)
                {
                    _pawnState.sleeping = !Pawn.Awake();
                }
                
                _cachedMouthParam.Reset();
                foreach(var eyeParam in _cachedEyeParam)
				{
                    eyeParam.Reset();
				}
                HeadBehavior.Update(Pawn, _pawnState, out headFacing);
                MouthBehavior.Update(Pawn, headFacing, _pawnState, _cachedMouthParam);
                EyeBehavior.Update(Pawn, headFacing, _pawnState, _cachedEyeParam);
            }
            if(portrait)
			{
                headFacing = HeadBehavior.GetRotationForPortrait();
                _cachedMouthParam.render = true;
                _cachedMouthParam.mouthTextureIdx = MouthBehavior.GetTextureIndexForPortrait(out _cachedMouthParam.mirror);
                for(int i = 0; i < EyeBehavior.NumEyes; ++i)
				{
                    _cachedEyeParam[i].render = true;
                    _cachedEyeParam[i].openEye = true;
                    _cachedEyeParam[i].mirror = EyeBehavior.GetEyeMirrorFlagForPortrait(i);
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
