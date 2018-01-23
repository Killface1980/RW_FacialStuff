using System;
using System.Collections.Generic;
using FacialStuff.Animator;
using FacialStuff.DefOfs;
using FacialStuff.Defs;
using FacialStuff.GraphicsFS;
using FacialStuff.Utilities;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class CompFace : ThingComp
    {
        #region Public Fields

        public FacePartStats BodyStat;

        [CanBeNull] public PawnFaceGraphic PawnFaceGraphic;

        public bool Deactivated;
        public bool IgnoreRenderer;
        public bool IsAsleep;
        public bool IsChild;
        public bool NeedsStyling = true;

        [CanBeNull] public string TexPathEyeLeft;
        [CanBeNull] public string TexPathEyeLeftPatch;
        [CanBeNull] public string TexPathEyeRight;
        [CanBeNull] public string TexPathEyeRightPatch;
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
        public GraphicVectorMeshSet EyeMeshSet => MeshPoolFs.HumanEyeSet[(int)this.FullHeadType];

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
        public GraphicVectorMeshSet MouthMeshSet => MeshPoolFs.HumanlikeMouthSet[(int)this.FullHeadType];

        public Faction OriginFaction
        {
            get { return this._factionInt; }
        }

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

        public CompProperties_Face Props
        {
            get { return (CompProperties_Face)this.props; }
        }

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
            if (!this.PawnHeadDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnHeadDrawers.Count;
                while (i < count)
                {
                    this.PawnHeadDrawers[i].ApplyHeadRotation(renderBody, ref headQuat);
                    i++;
                }
            }
        }

        // only for development
        public Vector3 BaseEyeOffsetAt(Rot4 rotation)
        {
            bool male = this.Pawn.gender == Gender.Male;

            if (this.PawnCrownType == CrownType.Average)
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            this._eyeOffset = MeshPoolFs.EyeMaleAverageNormalOffset;
                        }
                        else
                        {
                            this._eyeOffset = MeshPoolFs.EyeFemaleAverageNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this._eyeOffset = MeshPoolFs.EyeMaleAveragePointyOffset;
                        }
                        else
                        {
                            this._eyeOffset = MeshPoolFs.EyeFemaleAveragePointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this._eyeOffset = MeshPoolFs.EyeMaleAverageWideOffset;
                        }
                        else
                        {
                            this._eyeOffset = MeshPoolFs.EyeFemaleAverageWideOffset;
                        }

                        break;
                }
            }
            else
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        this._eyeOffset = male
                                          ? MeshPoolFs.EyeMaleNarrowNormalOffset
                                          : MeshPoolFs.EyeFemaleNarrowNormalOffset;
                        break;

                    case HeadType.Pointy:
                        this._eyeOffset = male
                                          ? MeshPoolFs.EyeMaleNarrowPointyOffset
                                          : MeshPoolFs.EyeFemaleNarrowPointyOffset;
                        break;

                    case HeadType.Wide:
                        this._eyeOffset =
                        male ? MeshPoolFs.EyeMaleNarrowWideOffset : MeshPoolFs.EyeFemaleNarrowWideOffset;
                        break;
                }
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
        public Vector3 BaseHeadOffsetAt(bool portrait)
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
                this.PawnHeadDrawers[i].BaseHeadOffsetAt(ref offset, portrait);
                i++;
            }

            return offset;
        }

        // only for development
        public Vector3 BaseMouthOffsetAt(Rot4 rotation)
        {
            bool male = this.Pawn.gender == Gender.Male;

            if (this.PawnCrownType == CrownType.Average)
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            this._mouthOffset = MeshPoolFs.MouthMaleAverageNormalOffset;
                        }
                        else
                        {
                            this._mouthOffset = MeshPoolFs.MouthFemaleAverageNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this._mouthOffset = MeshPoolFs.MouthMaleAveragePointyOffset;
                        }
                        else
                        {
                            this._mouthOffset = MeshPoolFs.MouthFemaleAveragePointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this._mouthOffset = MeshPoolFs.MouthMaleAverageWideOffset;
                        }
                        else
                        {
                            this._mouthOffset = MeshPoolFs.MouthFemaleAverageWideOffset;
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
                        male ? MeshPoolFs.MouthMaleNarrowNormalOffset : MeshPoolFs.MouthFemaleNarrowNormalOffset;

                        break;

                    case HeadType.Pointy:
                        this._mouthOffset =
                        male ? MeshPoolFs.MouthMaleNarrowPointyOffset : MeshPoolFs.MouthFemaleNarrowPointyOffset;

                        break;

                    case HeadType.Wide:
                        this._mouthOffset =
                        male ? MeshPoolFs.MouthMaleNarrowWideOffset : MeshPoolFs.MouthFemaleNarrowWideOffset;

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
            return "Things/Pawn/Humanlike/Brows/Brow_" + this.Pawn.gender + "_" + browDef.texPath;
        }


        // Can be called externally
        public void DrawAlienBodyAddons(Quaternion quat, Vector3 vector, bool portrait, bool renderBody)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i].DrawAlienBodyAddons(quat, vector, portrait, renderBody);
                i++;
            }
        }

        public void DrawAlienHeadAddons(bool portrait, Quaternion headQuat, Vector3 currentLoc)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i].DrawAlienHeadAddons(portrait, headQuat, currentLoc);
                i++;
            }
        }


        public void DrawBasicHead(out bool headDrawn, RotDrawMode bodyDrawType, bool portrait, bool headStump,
                                  ref Vector3 locFacialY, Quaternion headQuat)
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
                                                      headQuat,
                                                      bodyDrawType,
                                                      headStump,
                                                      portrait,
                                                      ref locFacialY,
                                                      out headDrawn);
                i++;
            }
        }

        public void DrawBeardAndTache(ref Vector3 locFacialY, bool portrait, Quaternion headQuat)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i].DrawBeardAndTache(headQuat, portrait, ref locFacialY);
                i++;
            }
        }

        public void DrawBrows(ref Vector3 locFacialY, Quaternion headQuat, bool portrait)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawBrows(headQuat, portrait, ref locFacialY);
                i++;
            }
        }

        public void DrawHairAndHeadGear(Vector3 rootLoc, RotDrawMode bodyDrawType, ref Vector3 currentLoc, Vector3 b,
                                        bool portrait, bool renderBody, Quaternion headQuat)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawHairAndHeadGear(
                                                             rootLoc,
                                                             headQuat,
                                                             bodyDrawType,
                                                             renderBody,
                                                             portrait,
                                                             b,
                                                             ref currentLoc);
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

        public void DrawNaturalEyes(ref Vector3 locFacialY, bool portrait, Quaternion headQuat)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawNaturalEyes(headQuat, portrait, ref locFacialY);
                i++;
            }
        }

        public void DrawNaturalMouth(ref Vector3 locFacialY, bool portrait, Quaternion headQuat)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawNaturalMouth(headQuat, portrait, ref locFacialY);
                i++;
            }
        }

        public void DrawUnnaturalEyeParts(ref Vector3 locFacialY, Quaternion headQuat, bool portrait)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawUnnaturalEyeParts(headQuat, portrait, ref locFacialY);
                i++;
            }
        }

        public void DrawWrinkles(RotDrawMode bodyDrawType, ref Vector3 locFacialY, Quaternion headQuat, bool portrait)
        {
            if (this.PawnHeadDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnHeadDrawers.Count;
            while (i < count)
            {
                this.PawnHeadDrawers[i]?.DrawWrinkles(headQuat, bodyDrawType, portrait, ref locFacialY);
                i++;
            }
        }

        [NotNull]
        public string EyeClosedTexPath(Side side)
        {
            return this.EyeTexPath("Closed", side);
        }

        // TODO: Remove or make usable
        // public void DefineSkinDNA()
        // {
        // HairMelanin.SkinGenetics(this.pawn, this, out this.factionMelanin);
        // this.IsSkinDNAoptimized = true;
        // }
        [NotNull]
        public string EyeTexPath([NotNull] string eyeDefPath, Side side)
        {
            // ReSharper disable once PossibleNullReferenceException
            string path = "Things/Pawn/Humanlike/Eyes/Eye_" + eyeDefPath + "_" + this.Pawn.gender + "_" + side;

            return path;
        }

        [NotNull]
        public string GetBeardPath(BeardDef def)
        {
            if (def == BeardDefOf.Beard_Shaved)
            {
                return "Things/Pawn/Humanlike/Beards/Beard_Shaved";
            }

            return "Things/Pawn/Humanlike/Beards/Beard_" + this.PawnHeadType + "_" + def.texPath + "_" + this.PawnCrownType;
        }

        [NotNull]
        public string GetMoustachePath(MoustacheDef def)
        {
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
            if (this.Props.drawers.Any())
            {
                this.PawnHeadDrawers = new List<PawnHeadDrawer>();
                for (int i = 0; i < this.Props.drawers.Count; i++)
                {
                    PawnHeadDrawer thingComp =
                    (PawnHeadDrawer)Activator.CreateInstance(this.Props.drawers[i].GetType());
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

            // Children & Pregnancy || Werewolves transformed
            if (this.Pawn.Map == null || !this.Pawn.Spawned || this.Pawn.Dead || this.IsChild || this.Deactivated)
            {
                return;
            }

            if (Find.TickManager.Paused)
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
                if (Find.TickManager.TicksGame % 30 == 0)
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
            Scribe_Values.Look(ref this.IsChild, "isChild");

            // Scribe_References.Look(ref this.theRoom, "theRoom");
            Scribe_Values.Look(ref this.Deactivated, "dontrender");

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