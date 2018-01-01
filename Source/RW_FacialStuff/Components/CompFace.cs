namespace FacialStuff
{
    using System;
    using System.Collections.Generic;

    using FacialStuff.Animator;
    using FacialStuff.DefOfs;
    using FacialStuff.Defs;
    using FacialStuff.Graphics;
    using FacialStuff.Utilities;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class CompFace : ThingComp
    {
        #region Public Fields

        public bool Deactivated;

        [CanBeNull]
        public PawnFaceGraphic PawnFaceGraphic;

        public bool IgnoreRenderer;

        public bool IsChild;

        public bool NeedsStyling = true;

        #endregion Public Fields

        #region Private Fields

        private Vector2 eyeOffset = Vector2.zero;

        [NotNull]
        public List<PawnHeadDrawer> PawnDrawers { get; private set; }

        private Faction factionInt;

        private float factionMelanin;

        // private float blinkRate;
        // public PawnHeadWiggler headWiggler;
        private Vector2 mouthOffset = Vector2.zero;

        // must be null, always initialize with pawn
        private PawnFace pawnFace;

        #endregion Private Fields

        #region Public Properties

        // public bool IgnoreRenderer;
        public GraphicVectorMeshSet EyeMeshSet => MeshPoolFS.HumanEyeSet[(int)this.FullHeadType];

        [NotNull]
        public PawnEyeWiggler EyeWiggler { get; private set; }

        public FaceMaterial FaceMaterial { get; private set; }

        public float FactionMelanin
        {
            get => this.factionMelanin;
            set => this.factionMelanin = value;
        }

        public FullHead FullHeadType { get; set; } = FullHead.Undefined;

        public PawnHeadRotator HeadRotator { get; private set; }

        [NotNull]
        public GraphicVectorMeshSet MouthMeshSet => MeshPoolFS.HumanlikeMouthSet[(int)this.FullHeadType];

        public Faction originFaction
        {
            get
            {
                return this.factionInt;
            }
        }

        [NotNull]
        public Pawn Pawn => this.parent as Pawn;

        public CrownType PawnCrownType => this.Pawn?.story.crownType ?? CrownType.Average;

        [CanBeNull]
        public PawnFace PawnFace => this.pawnFace;

        public HeadType PawnHeadType
        {
            get
            {
                if (this.Pawn.story?.HeadGraphicPath == null)
                {
                    return HeadType.Normal;
                }

                if (this.Pawn.story.HeadGraphicPath.Contains("Pointy"))
                {
                    return HeadType.Pointy;
                }

                if (this.Pawn.story.HeadGraphicPath.Contains("Wide"))
                {
                    return HeadType.Wide;
                }

                return HeadType.Normal;
            }
        }

        public CompProperties_Face Props
        {
            get
            {
                return (CompProperties_Face)this.props;
            }
        }

        #endregion Public Properties



        #region Public Methods

        public void ApplyHeadRotation(bool renderBody, ref Quaternion headQuat)
        {
            if (!this.PawnDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].ApplyHeadRotation(renderBody, ref headQuat);
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
                            this.eyeOffset = MeshPoolFS.EyeMaleAverageNormalOffset;
                        }
                        else
                        {
                            this.eyeOffset = MeshPoolFS.EyeFemaleAverageNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this.eyeOffset = MeshPoolFS.EyeMaleAveragePointyOffset;
                        }
                        else
                        {
                            this.eyeOffset = MeshPoolFS.EyeFemaleAveragePointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this.eyeOffset = MeshPoolFS.EyeMaleAverageWideOffset;
                        }
                        else
                        {
                            this.eyeOffset = MeshPoolFS.EyeFemaleAverageWideOffset;
                        }

                        break;
                }
            }
            else
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            this.eyeOffset = MeshPoolFS.EyeMaleNarrowNormalOffset;
                        }
                        else
                        {
                            this.eyeOffset = MeshPoolFS.EyeFemaleNarrowNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this.eyeOffset = MeshPoolFS.EyeMaleNarrowPointyOffset;
                        }
                        else
                        {
                            this.eyeOffset = MeshPoolFS.EyeFemaleNarrowPointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this.eyeOffset = MeshPoolFS.EyeMaleNarrowWideOffset;
                        }
                        else
                        {
                            this.eyeOffset = MeshPoolFS.EyeFemaleNarrowWideOffset;
                        }

                        break;
                }
            }

            switch (rotation.AsInt)
            {
                case 1: return new Vector3(this.eyeOffset.x, 0f, -this.eyeOffset.y);
                case 2: return new Vector3(0, 0f, -this.eyeOffset.y);
                case 3: return new Vector3(-this.eyeOffset.x, 0f, -this.eyeOffset.y);
                default: return Vector3.zero;
            }
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
                            this.mouthOffset = MeshPoolFS.MouthMaleAverageNormalOffset;
                        }
                        else
                        {
                            this.mouthOffset = MeshPoolFS.MouthFemaleAverageNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this.mouthOffset = MeshPoolFS.MouthMaleAveragePointyOffset;
                        }
                        else
                        {
                            this.mouthOffset = MeshPoolFS.MouthFemaleAveragePointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this.mouthOffset = MeshPoolFS.MouthMaleAverageWideOffset;
                        }
                        else
                        {
                            this.mouthOffset = MeshPoolFS.MouthFemaleAverageWideOffset;
                        }

                        break;
                }
            }
            else
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        this.mouthOffset = male ? MeshPoolFS.MouthMaleNarrowNormalOffset : MeshPoolFS.MouthFemaleNarrowNormalOffset;

                        break;

                    case HeadType.Pointy:
                        this.mouthOffset = male ? MeshPoolFS.MouthMaleNarrowPointyOffset : MeshPoolFS.MouthFemaleNarrowPointyOffset;

                        break;

                    case HeadType.Wide:
                        this.mouthOffset = male ? MeshPoolFS.MouthMaleNarrowWideOffset : MeshPoolFS.MouthFemaleNarrowWideOffset;

                        break;
                }
            }

            switch (rotation.AsInt)
            {
                case 1: return new Vector3(this.mouthOffset.x, 0f, -this.mouthOffset.y);
                case 2: return new Vector3(0, 0f, -this.mouthOffset.y);
                case 3: return new Vector3(-this.mouthOffset.x, 0f, -this.mouthOffset.y);
                default: return Vector3.zero;
            }
        }

        [NotNull]
        public string BrowTexPath([NotNull] BrowDef browDef)
        {
            return "Brows/Brow_" + this.Pawn.gender + "_" + browDef.texPath;
        }


        // Can be called externally
        public void DrawAlienBodyAddons(Quaternion quat, Vector3 vector, bool portrait, bool renderBody)
        {
            if (this.PawnDrawers != null)
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawAlienBodyAddons(quat, vector, portrait, renderBody);
                    i++;
                }
            }
        }

        public void DrawAlienHeadAddons(bool portrait, Quaternion headQuat, Vector3 currentLoc)
        {
            if (this.PawnDrawers != null)
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawAlienHeadAddons(portrait, headQuat, currentLoc);
                    i++;
                }
            }
        }

        public void DrawApparel(Quaternion quat, Vector3 vector, bool portrait, bool renderBody)
        {
            if (this.PawnDrawers != null)
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawApparel(quat, vector, renderBody, portrait);
                    i++;
                }
            }
        }

        public void DrawBasicHead(out bool headDrawn, RotDrawMode bodyDrawType, bool portrait, bool headStump, ref Vector3 locFacialY, Quaternion headQuat)
        {
            headDrawn = false;
            if (this.PawnDrawers != null)
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawBasicHead(
                        headQuat,
                        bodyDrawType,
                        headStump,
                        portrait,
                        ref locFacialY,
                        out headDrawn);
                    i++;
                }
            }
        }

        public void DrawBeardAndTache(ref Vector3 locFacialY, bool portrait, Quaternion headQuat)
        {
            if (!this.PawnDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawBeardAndTache(headQuat, portrait, ref locFacialY);
                    i++;
                }
            }
        }

        public void DrawBrows(ref Vector3 locFacialY, Quaternion headQuat, bool portrait)
        {
            if (!this.PawnDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawBrows(headQuat, portrait, ref locFacialY);
                    i++;
                }
            }
        }

        public void DrawHairAndHeadGear(Vector3 rootLoc, RotDrawMode bodyDrawType, ref Vector3 currentLoc, Vector3 b, bool portrait, bool renderBody, Quaternion headQuat)
        {
            if (!this.PawnDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawHairAndHeadGear(
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
            if (this.PawnDrawers != null)
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawHeadOverlays(headOverlays, bodyLoc, headQuat);
                    i++;
                }
            }
        }

        public void DrawNaturalEyes(ref Vector3 locFacialY, bool portrait, Quaternion headQuat)
        {
            if (this.PawnDrawers != null)
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawNaturalEyes(headQuat, portrait, ref locFacialY);
                    i++;
                }
            }
        }

        public void DrawNaturalMouth(ref Vector3 locFacialY, bool portrait, Quaternion headQuat)
        {
            if (this.PawnDrawers != null)
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawNaturalMouth(headQuat, portrait, ref locFacialY);
                    i++;
                }
            }
        }

        public void DrawUnnaturalEyeParts(ref Vector3 locFacialY, Quaternion headQuat, bool portrait)
        {
            if (this.PawnDrawers != null)
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawUnnaturalEyeParts(headQuat, portrait, ref locFacialY);
                    i++;
                }
            }
        }

        public void DrawWrinkles(RotDrawMode bodyDrawType, ref Vector3 locFacialY, Quaternion headQuat, bool portrait)
        {
            if (this.PawnDrawers != null)
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawWrinkles(headQuat, bodyDrawType, portrait, ref locFacialY);
                    i++;
                }
            }
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
            string path = "Eyes/Eye_" + eyeDefPath + "_" + this.Pawn.gender + "_" + side;

            return path;
        }

        [NotNull]
        public string GetBeardPath(BeardDef def)
        {
            if (def == BeardDefOf.Beard_Shaved)
            {
                return "Beards/Beard_Shaved";
            }

            return "Beards/Beard_" + this.PawnHeadType + "_" + def.texPath + "_" + this.PawnCrownType;
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

        public void InitializePawnDrawer()
        {
            if (this.Props.drawers.Any())
            {
                this.PawnDrawers = new List<PawnHeadDrawer>();
                for (int i = 0; i < this.Props.drawers.Count; i++)
                {
                    PawnHeadDrawer thingComp =
                        (PawnHeadDrawer)Activator.CreateInstance(this.Props.drawers[i].GetType());
                    thingComp.CompFace = this;
                    thingComp.Pawn = this.Pawn;
                    this.PawnDrawers.Add(thingComp);
                    thingComp.Initialize();
                }
            }
            else
            {
                this.PawnDrawers = new List<PawnHeadDrawer>();
                PawnHeadDrawer thingComp =
                    (PawnHeadDrawer)Activator.CreateInstance(typeof(HumanHeadDrawer));
                thingComp.CompFace = this;
                thingComp.Pawn = this.Pawn;
                this.PawnDrawers.Add(thingComp);
                thingComp.Initialize();

            }
        }

        public bool IsAsleep;

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

            if (!IsAsleep)
            {
                this.HeadRotator.RotatorTick();
            }

            if (this.Props.hasMouth)
            {
                if (Find.TickManager.TicksGame % 30 == 0)
                {
                    this.PawnFaceGraphic.SetMouthAccordingToMoodLevel();
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref this.factionInt, "pawnFaction");

            // Scribe_Values.Look(ref this.pawnFace.MelaninOrg, "MelaninOrg");

            // Log.Message(
            // "Facial Stuff updated pawn " + this.parent.Label + "-" + face.BeardDef + "-" + face.EyeDef);

            // Force ResolveAllGraphics
            Scribe_Deep.Look(ref this.pawnFace, "pawnFace");

            // Scribe_References.Look(ref this.pawn, "pawn");
            Scribe_Values.Look(ref this.IsChild, "isChild");

            // Scribe_References.Look(ref this.theRoom, "theRoom");
            Scribe_Values.Look(ref this.Deactivated, "dontrender");

            // Scribe_Values.Look(ref this.roofed, "roofed");
            Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");

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

        /// <summary>
        /// Basic pawn initialization.
        /// </summary>
        /// <returns>
        /// Success if all initialized.
        /// </returns>
        public bool InitializeCompFace()
        {
            if (this.originFaction == null)
            {
                this.factionInt = this.Pawn.Faction ?? Faction.OfPlayer;
            }

            if (this.PawnFace == null)
            {
                this.SetPawnFace(new PawnFace(this, this.originFaction.def));
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

        public void SetPawnFace([NotNull] PawnFace importedFace)
        {
            this.pawnFace = importedFace;
        }

        // Verse.PawnGraphicSet
        #endregion Public Methods

        #region Private Methods

        [CanBeNull]
        public string texPathEyeLeftPatch;

        [CanBeNull]
        public string texPathJawAddedPart;

        [CanBeNull]
        public string texPathEyeLeft;

        [CanBeNull]
        public string texPathEyeRight;

        [CanBeNull]
        public string texPathEyeRightPatch;

        [NotNull]
        public string EyeClosedTexPath(Side side)
        {
            return this.EyeTexPath("Closed", side);
        }

        #endregion Private Methods

        // public Vector3 RightHandPosition;
        // public Vector3 LeftHandPosition;
        public Vector3 BaseHeadOffsetAt(bool portrait)
        {
            Vector3 offset = Vector3.zero;

            if (!this.PawnDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].BaseHeadOffsetAt(ref offset, portrait);
                    i++;
                }
            }

            return offset;
        }

        public FacePartStats bodyStat;

        private bool initialized;

        public void TickDrawers(Rot4 bodyFacing, Rot4 headFacing, PawnGraphicSet graphics)
        {
            if (!this.initialized)
            {
                this.InitializePawnDrawer();
                this.initialized = true;
            }

            if (!this.PawnDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].Tick(bodyFacing, headFacing, graphics);
                    i++;
                }
            }
        }
    }
}