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

namespace FacialStuff
{
    public class CompFace : ThingComp
    {
        #region Public Fields

        public FacePartStats BodyStat;

        [CanBeNull] public PawnFaceGraphic PawnFaceGraphic;

        public bool IsAsleep;

        public bool IsChild
        {
            get
            {
                return LoadedModManager.RunningModsListForReading.Any(x => x.PackageId == "Dylan.CSL") &&
                       Pawn.RaceProps.Humanlike && Pawn.ageTracker.CurLifeStage.bodySizeFactor < 1f;

            }
        }
        public bool NeedsStyling = true;

        [CanBeNull] public string TexPathEyeLeft;
        [CanBeNull] public string TexPathEyeLeftPatch;
        [CanBeNull] public string TexPathEyeLeftMissing;
        [CanBeNull] public string TexPathEyeRight;
        [CanBeNull] public string TexPathEyeRightPatch;
        [CanBeNull] public string TexPathEyeRightMissing;

        [CanBeNull] public string TexPathEarLeft;
        [CanBeNull] public string TexPathEarLeftPatch;
        [CanBeNull] public string TexPathEarLeftMissing;
        [CanBeNull] public string TexPathEarRight;
        [CanBeNull] public string TexPathEarRightPatch;
        [CanBeNull] public string TexPathEarRightMissing;
        
        [CanBeNull] public string TexPathJawAddedPart;

        #endregion Public Fields

        #region Private Fields

        private Vector2 _eyeOffset = Vector2.zero;

        private Faction _factionInt;

        private float _factionMelanin;

        private bool _initialized;

        private Vector2 _mouthOffset = Vector2.zero;

        // must be null, always initialize with pawn
        private PawnFace _pawnFace;

        #endregion Private Fields

        #region Public Properties

        // public bool IgnoreRenderer;
        public GraphicVectorMeshSet EyeMeshSet => MeshPoolFS.HumanEyeSet[(int)this.FullHeadType];

        [NotNull]
        public PawnEyeWiggler EyeWiggler { get; set; }

        public FaceMaterial FaceMaterial { get; set; }

        public float FactionMelanin
        {
            get => this._factionMelanin;
            set => this._factionMelanin = value;
        }

        public FullHead FullHeadType { get; set; } = FullHead.Undefined;

        public PawnHeadRotator HeadRotator { get; private set; }

        [NotNull]
        public GraphicVectorMeshSet MouthMeshSet => MeshPoolFS.HumanlikeMouthSet[(int)this.FullHeadType];

        public Faction OriginFaction => this._factionInt;

        public virtual CrownType PawnCrownType => this.Pawn?.story.crownType ?? CrownType.Average;

        [CanBeNull]
        public PawnFace PawnFace => this._pawnFace;

        public HeadType PawnHeadType
        {
            get
            {
                if (this.Pawn.story?.HeadGraphicPath != null)
                {
                    if (this.Pawn.story.HeadGraphicPath.Contains("Pointy"))
                    {
                        return HeadType.Pointy;
                    }

                    if (this.Pawn.story.HeadGraphicPath.Contains("Wide"))
                    {
                        return HeadType.Wide;
                    }
                }

                return HeadType.Normal;
            }
        }

        public CompProperties_Face Props => (CompProperties_Face)this.props;

        [NotNull]
        public Pawn Pawn => this.parent as Pawn;

        #endregion Public Properties

        #region Private Properties

        [NotNull]
        private List<PawnHeadDrawer> PawnHeadDrawers { get; set; }

        #endregion Private Properties

        #region Public Methods

        public void ApplyHeadRotation(bool renderBody, ref Quaternion headQuat)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i].ApplyHeadRotation(renderBody, ref headQuat);
                i++;
            }
        }

        // only for development
        public Vector3 BaseEyeOffsetAt(Rot4 rotation)
        {
            bool male = this.Pawn.gender == Gender.Male;

            switch (this.PawnCrownType)
            {
                default:
                    switch (this.PawnHeadType)
                    {
                        case HeadType.Normal:
                            this._eyeOffset = male ? MeshPoolFS.EyeMaleAverageNormalOffset : MeshPoolFS.EyeFemaleAverageNormalOffset;

                            break;

                        case HeadType.Pointy:
                            this._eyeOffset = male ? MeshPoolFS.EyeMaleAveragePointyOffset : MeshPoolFS.EyeFemaleAveragePointyOffset;

                            break;

                        case HeadType.Wide:
                            this._eyeOffset = male ? MeshPoolFS.EyeMaleAverageWideOffset : MeshPoolFS.EyeFemaleAverageWideOffset;

                            break;
                    }

                    break;

                case CrownType.Narrow:
                    switch (this.PawnHeadType)
                    {
                        case HeadType.Normal:
                            this._eyeOffset = male
                                              ? MeshPoolFS.EyeMaleNarrowNormalOffset
                                              : MeshPoolFS.EyeFemaleNarrowNormalOffset;
                            break;

                        case HeadType.Pointy:
                            this._eyeOffset = male
                                              ? MeshPoolFS.EyeMaleNarrowPointyOffset
                                              : MeshPoolFS.EyeFemaleNarrowPointyOffset;
                            break;

                        case HeadType.Wide:
                            this._eyeOffset =
                            male ? MeshPoolFS.EyeMaleNarrowWideOffset : MeshPoolFS.EyeFemaleNarrowWideOffset;
                            break;
                    }

                    break;
            }

            switch (rotation.AsInt)
            {
                case 1: return new Vector3(this._eyeOffset.x, 0f, -this._eyeOffset.y);
                case 2: return new Vector3(0, 0f, -this._eyeOffset.y);
                case 3: return new Vector3(-this._eyeOffset.x, 0f, -this._eyeOffset.y);
                default: return Vector3.zero;
            }
        }

        // public Vector3 RightHandPosition;
        // public Vector3 LeftHandPosition;
        public Vector3 BaseHeadOffsetAt(bool portrait, Pawn pawn)
        {
            Vector3 offset = Vector3.zero;

            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return offset;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i].BaseHeadOffsetAt(ref offset, portrait, pawn);
                i++;
            }

            return offset;
        }

        // only for development
        public Vector3 BaseMouthOffsetAtDevelop(Rot4 rotation)
        {
            bool male = this.Pawn.gender == Gender.Male;

            if (this.PawnCrownType == CrownType.Average)
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            this._mouthOffset = MeshPoolFS.MouthMaleAverageNormalOffset;
                        }
                        else
                        {
                            this._mouthOffset = MeshPoolFS.MouthFemaleAverageNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this._mouthOffset = MeshPoolFS.MouthMaleAveragePointyOffset;
                        }
                        else
                        {
                            this._mouthOffset = MeshPoolFS.MouthFemaleAveragePointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this._mouthOffset = MeshPoolFS.MouthMaleAverageWideOffset;
                        }
                        else
                        {
                            this._mouthOffset = MeshPoolFS.MouthFemaleAverageWideOffset;
                        }

                        break;
                }
            }
            else
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        this._mouthOffset =
                        male ? MeshPoolFS.MouthMaleNarrowNormalOffset : MeshPoolFS.MouthFemaleNarrowNormalOffset;

                        break;

                    case HeadType.Pointy:
                        this._mouthOffset =
                        male ? MeshPoolFS.MouthMaleNarrowPointyOffset : MeshPoolFS.MouthFemaleNarrowPointyOffset;

                        break;

                    case HeadType.Wide:
                        this._mouthOffset =
                        male ? MeshPoolFS.MouthMaleNarrowWideOffset : MeshPoolFS.MouthFemaleNarrowWideOffset;

                        break;
                }
            }

            switch (rotation.AsInt)
            {
                case 1: return new Vector3(this._mouthOffset.x, 0f, -this._mouthOffset.y);
                case 2: return new Vector3(0, 0f, -this._mouthOffset.y);
                case 3: return new Vector3(-this._mouthOffset.x, 0f, -this._mouthOffset.y);
                default: return Vector3.zero;
            }
        }

        [NotNull]
        public string BrowTexPath([NotNull] BrowDef browDef)
        {
            string browPath = browDef.texBasePath.NullOrEmpty() ? StringsFS.PathHumanlike + "Brows/" : browDef.texBasePath;
            string browTexPath = browPath + "Brow_" + this.Pawn.gender + "_" + browDef.texName;
            return browTexPath;
        }

        // Can be called externally

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

        public void DrawBasicHead(out bool headDrawn, RotDrawMode bodyDrawType, bool portrait, bool headStump,
                                  Vector3 drawLoc, Quaternion headQuat)
        {
            headDrawn = false;
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i].DrawBasicHead(
                    drawLoc,
                    headQuat,
                    bodyDrawType,
                    headStump,
                    portrait,
                    out headDrawn);
                i++;
            }
        }

        public void DrawBeardAndTache(Vector3 beardLoc, Vector3 tacheLoc, bool portrait, Quaternion headQuat)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i].DrawBeardAndTache(beardLoc, tacheLoc, headQuat, portrait);
                i++;
            }
        }

        public void DrawBrows(Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawBrows(drawLoc, headQuat, portrait);
                i++;
            }
        }

        public void DrawHairAndHeadGear(Vector3 hairLoc, Vector3 headgearLoc, RotDrawMode bodyDrawType,
                                        bool portrait, bool renderBody, Quaternion headQuat,
                                        Vector3 hatInFrontOfFace)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawHairAndHeadGear(hairLoc, headgearLoc, bodyDrawType, headQuat, renderBody, portrait, hatInFrontOfFace);
                i++;
            }
        }

        // public void SetFaceRender(bool portrait, Quaternion headQuat, Rot4 headFacing, bool renderBody, PawnGraphicSet graphics)
        // {
        // this.portrait = portrait;
        // this.headQuat = headQuat;
        // this.headFacing = headFacing;
        // this.graphics = graphics;
        // this.renderBody = renderBody;
        // }
        public void DrawHeadOverlays(PawnHeadOverlays headOverlays, Vector3 bodyLoc, Quaternion headQuat)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawHeadOverlays(headOverlays, bodyLoc, headQuat);
                i++;
            }
        }

        public void DrawNaturalEyes(Vector3 drawLoc, bool portrait, Quaternion headQuat)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawNaturalEyes(drawLoc, headQuat, portrait);
                i++;
            }
        }
        public void DrawNaturalEars(Vector3 drawLoc, bool portrait, Quaternion headQuat)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawNaturalEars(drawLoc, headQuat, portrait);
                i++;
            }
        }

        public void DrawNaturalMouth(Vector3 drawLoc, bool portrait, Quaternion headQuat)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawNaturalMouth(drawLoc, headQuat, portrait);
                i++;
            }
        }

        public void DrawUnnaturalEyeParts(Vector3 locFacialY, Quaternion headQuat, bool portrait)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawUnnaturalEyeParts(locFacialY, headQuat, portrait);
                i++;
            }
        }
        public void DrawUnnaturalEarParts(Vector3 locFacialY, Quaternion headQuat, bool portrait)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawUnnaturalEarParts(locFacialY, headQuat, portrait);
                i++;
            }
        }

        public void DrawWrinkles(RotDrawMode bodyDrawType, Vector3 drawLoc, Quaternion headQuat, bool portrait)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawWrinkles(drawLoc, bodyDrawType, headQuat, portrait);
                i++;
            }
        }

        // TODO: Remove or make usable
        // public void DefineSkinDNA()
        // {
        // HairMelanin.SkinGenetics(this.pawn, this, out this.factionMelanin);
        // this.IsSkinDNAoptimized = true;
        // }
        [NotNull]
        public string EyeTexPath(Side side, [NotNull] EyeDef eyeDef = null)
        {
            if (eyeDef == null)
            {
                eyeDef = this.PawnFace?.EyeDef;
            }
            // ReSharper disable once PossibleNullReferenceException
            string eyePath = eyeDef.texBasePath.NullOrEmpty() ? StringsFS.PathHumanlike + "Eyes/" : eyeDef.texBasePath;
            string path = eyePath + "Eye_" + eyeDef.texName + "_" + this.Pawn.gender + "_" + side;

            return path.Replace(@"\", @"/");
        }
    
        // }
        [NotNull]
        public string EarTexPath(Side side, [NotNull] EarDef ear = null)
        {
            if (ear == null)
            {
                ear = this.PawnFace?.EarDef;
            }
            // ReSharper disable once PossibleNullReferenceException
            string earPath = ear.texBasePath.NullOrEmpty() ? StringsFS.PathHumanlike + "Ears/" : ear.texBasePath;
            string path = earPath + "Ear_" + ear.texName + "_" + this.Pawn.gender + "_" + side;

            return path.Replace(@"\", @"/");
        }

        [NotNull]
        public string GetBeardPath(BeardDef def = null)
        {
            if (def == null)
            {
                if (this.PawnFace?.BeardDef != null)
                {
                    def = this.PawnFace?.BeardDef;
                }
                else
                {
                    return string.Empty;
                }
            }

            if (def == BeardDefOf.Beard_Shaved)
            {
                return StringsFS.PathHumanlike + "Beards/Beard_Shaved";
            }

            if (def.IsBeardNotHair())
            {
                return StringsFS.PathHumanlike + "Beards/" + def.texPath;
            }

            return StringsFS.PathHumanlike + "Beards/Beard_" + this.PawnHeadType + "_" + def.texPath + "_" + this.PawnCrownType;
        }

        [NotNull]
        public string GetMoustachePath(MoustacheDef def = null)
        {
            if (def == null)
            {
                if (this.PawnFace?.MoustacheDef != null)
                {
                    def = this.PawnFace?.MoustacheDef;
                }
                else
                {
                    return string.Empty;
                }
            }

            if (def == MoustacheDefOf.Shaved)
            {
                return this.GetBeardPath(BeardDefOf.Beard_Shaved);
            }

            return def.texPath + "_" + this.PawnCrownType;
        }

        /// <summary>
        /// Basic pawn initialization.
        /// </summary>
        /// <returns>
        /// Success if all initialized.
        /// </returns>
        public virtual bool InitializeCompFace()
        {
            if (this.OriginFaction == null)
            {
                this._factionInt = this.Pawn.Faction ?? Faction.OfPlayer;
            }

            if (this.PawnFace == null)
            {
                this.SetPawnFace(new PawnFace(this, this.OriginFaction?.def));
            }

            // Fix for PrepC for pre-FS pawns, also sometimes the brows are not defined?!?
            if (this.PawnFace?.EyeDef == null || this.PawnFace.BrowDef == null || this.PawnFace.BeardDef == null)
            {
                this.SetPawnFace(new PawnFace(this, Faction.OfPlayer.def));
            }

            // Only for the crowntype ...
            CrownTypeChecker.SetHeadOffsets(this.Pawn, this);

            if (this.Props.hasEyes)
            {
                this.EyeWiggler = new PawnEyeWiggler(this);
            }

            this.PawnFaceGraphic = new PawnFaceGraphic(this);
            this.FaceMaterial = new FaceMaterial(this, this.PawnFaceGraphic);

            // this.isMasochist = this.pawn.story.traits.HasTrait(TraitDef.Named("Masochist"));
            this.HeadRotator = new PawnHeadRotator(this.Pawn);

            // this.headWiggler = new PawnHeadWiggler(this.pawn);
            return true;
        }

        public void InitializePawnDrawer()
        {
            if (this.Props.headDrawers.Any())
            {
                this.PawnHeadDrawers = new List<PawnHeadDrawer>();
                for (int i = 0; i < this.Props.headDrawers.Count; i++)
                {
                    PawnHeadDrawer thingComp =
                    (PawnHeadDrawer)Activator.CreateInstance(this.Props.headDrawers[i].GetType());
                    thingComp.CompFace = this;
                    thingComp.Pawn = this.Pawn;
                    this.PawnHeadDrawers.Add(thingComp);
                    thingComp.Initialize();
                }
            }
            else
            {
                this.PawnHeadDrawers = new List<PawnHeadDrawer>();
                PawnHeadDrawer thingComp =
                (PawnHeadDrawer)Activator.CreateInstance(typeof(HumanHeadDrawer));
                thingComp.CompFace = this;
                thingComp.Pawn = this.Pawn;
                this.PawnHeadDrawers.Add(thingComp);
                thingComp.Initialize();
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();

            if (Find.TickManager.Paused)
            {
                return;
            }
            // Children & Pregnancy || Werewolves transformed
            if (this.Pawn.Map == null || Pawn.InContainerEnclosed || !this.Pawn.Spawned || this.Pawn.Dead || this.IsChild || this.Pawn.GetCompAnim().Deactivated)
            {
                return;
            }


            // CellRect viewRect = Find.CameraDriver.CurrentViewRect;
            // viewRect = viewRect.ExpandedBy(5);
            // if (!viewRect.Contains(this.pawn.Position))
            // {
            // return;
            // }
            if (this.Props.hasEyes)
            {
                this.EyeWiggler.WigglerTick();
            }

            if (Find.TickManager.TicksGame % 180 == 0)
            {
                this.IsAsleep = !this.Pawn.Awake();
            }

            if (!this.IsAsleep)
            {
                this.HeadRotator.RotatorTick();
            }

            if (this.Props.hasMouth)
            {
                if (Find.TickManager.TicksGame % 90 == 0)
                {
                    this.PawnFaceGraphic?.SetMouthAccordingToMoodLevel();
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref this._factionInt, "pawnFaction");

            // Scribe_Values.Look(ref this.pawnFace.MelaninOrg, "MelaninOrg");

            // Log.Message(
            // "Facial Stuff updated pawn " + this.parent.Label + "-" + face.BeardDef + "-" + face.EyeDef);

            // Force ResolveAllGraphics
            Scribe_Deep.Look(ref this._pawnFace, "pawnFace");

            // Scribe_References.Look(ref this.pawn, "pawn");
            //Scribe_Values.Look(ref this.IsChild, "isChild");

            // Scribe_References.Look(ref this.theRoom, "theRoom");

            // Scribe_Values.Look(ref this.roofed, "roofed");
            Scribe_Values.Look(ref this._factionMelanin, "factionMelanin");

            // Faction needs to be saved like in Thing.ExposeData
            // string facID = (this.factionInt == null) ? "null" : this.factionInt.GetUniqueLoadID();
            // Scribe_Values.Look(ref facID, "pawnFaction", "null", false);
            // if (Scribe.mode != LoadSaveMode.LoadingVars && Scribe.mode != LoadSaveMode.ResolvingCrossRefs && Scribe.mode != LoadSaveMode.PostLoadInit)
            // return;
            // if (facID == "null")
            // {
            // this.factionInt = null;
            // }
            // else if (Find.World != null && Find.FactionManager != null)
            // {
            // this.factionInt = Find.FactionManager.AllFactions.FirstOrDefault((Faction fa) => fa.GetUniqueLoadID() == facID);
            // }
        }

        public void SetPawnFace([NotNull] PawnFace importedFace)
        {
            this._pawnFace = importedFace;
        }

        public void TickDrawers(Rot4 bodyFacing, Rot4 headFacing, PawnGraphicSet graphics)
        {
            if (!this._initialized)
            {
                this.InitializePawnDrawer();
                this._initialized = true;
            }

            if (!this.PawnHeadDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnHeadDrawers.Count;
                while (i < count)
                {
                    this.PawnHeadDrawers[i].Tick(bodyFacing, headFacing, graphics);
                    i++;
                }
            }
        }

        #endregion Public Methods
    }
}