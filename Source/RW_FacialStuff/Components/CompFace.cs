namespace FacialStuff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using FacialStuff.Animator;
    using FacialStuff.Defs;
    using FacialStuff.Enums;
    using FacialStuff.Graphics;
    using FacialStuff.Harmony;
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
        public PawnGraphic PawnGraphic;

        public bool IgnoreRenderer;

        public bool IsChild;

        public int lastRoomCheck;

        public bool NeedsStyling = true;

        public Rot4 rotation = Rot4.East;

        #endregion Public Fields

        #region Private Fields

        private List<Material> cachedNakedMatsBodyBase = new List<Material>();

        private int cachedNakedMatsBodyBaseHash = -1;

        private List<Material> cachedSkinMatsBodyBase = new List<Material>();

        private int cachedSkinMatsBodyBaseHash = -1;

        private Vector2 eyeOffset = Vector2.zero;

        [NotNull]
        private PawnEyeWiggler eyeWiggler;

        private List<PawnDrawer> pawnDrawers;

        private FaceMaterial faceMaterial;

        private Faction factionInt;

        private float factionMelanin;

        private PawnHeadRotator headRotator;

        // private float blinkRate;
        // public PawnHeadWiggler headWiggler;
        private Vector2 mouthOffset = Vector2.zero;

        // must be null, always initialize with pawn
        [CanBeNull]
        private PawnFace pawnFace;

        private Room theRoom;

        public static FieldInfo infoJitterer;

        public float JitterMax = 0.35f;

        public JitterHandler Jitterer
            => GetHiddenValue(typeof(Pawn_DrawTracker), Pawn.Drawer, "jitterer", infoJitterer) as
                   JitterHandler;

        public static object GetHiddenValue(Type type, object instance, string fieldName, [CanBeNull] FieldInfo info)
        {
            if (info == null)
            {
                info = type.GetField(fieldName, GenGeneric.BindingFlagsAll);
            }

            return info?.GetValue(instance);
        }
        #endregion Private Fields

        #region Public Properties

        // public bool IgnoreRenderer;
        public GraphicVectorMeshSet EyeMeshSet => MeshPoolFS.HumanEyeSet[(int)this.FullHeadType];

        [CanBeNull]
        public PawnEyeWiggler EyeWiggler => this.eyeWiggler;

        public FaceMaterial FaceMaterial => this.faceMaterial;

        public float FactionMelanin
        {
            get => this.factionMelanin;
            set => this.factionMelanin = value;
        }

        public FullHead FullHeadType { get; set; } = FullHead.Undefined;

        public PawnHeadRotator HeadRotator => this.headRotator;

        public bool HideShellLayer => this.InRoom && Controller.settings.HideShellWhileRoofed;

        public bool InPrivateRoom
        {
            get
            {
                if (this.InRoom && !this.Pawn.IsPrisoner)
                {
                    Room ownedRoom = this.Pawn.ownership?.OwnedRoom;
                    if (ownedRoom != null)
                    {
                        return ownedRoom == this.TheRoom;
                    }
                }

                return false;
            }
        }

        public bool InRoom
        {
            get
            {

                if (this.TheRoom != null && (!this.TheRoom.Group.UsesOutdoorTemperature))
                {
                    // Pawn is indoors
                    return !this.Pawn.Drafted || !Controller.settings.IgnoreWhileDrafted;
                }

                return false;

                // return !room?.Group.UsesOutdoorTemperature == true && Controller.settings.IgnoreWhileDrafted || !this.pawn.Drafted;
            }
        }

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
        public bool HideHat => this.InRoom && Controller.settings.HideHatWhileRoofed;

        #endregion Public Properties

        #region Private Properties

        [CanBeNull]
        private Room TheRoom
        {
            get
            {
                if (this.Pawn.Dead)
                {
                    return null;
                }

                if ((Find.TickManager.TicksGame < this.lastRoomCheck + 60f))
                {
                    return this.theRoom;
                }

                this.theRoom = this.Pawn.GetRoom();
                this.lastRoomCheck = Find.TickManager.TicksGame;

                return this.theRoom;
            }
        }

        #endregion Private Properties

        #region Public Methods

        public void ApplyHeadRotation(bool renderBody, ref Quaternion headQuat)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].ApplyHeadRotation(renderBody, ref headQuat);
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
        public void CheckForAddedOrMissingParts()
        {
            if (!Controller.settings.ShowExtraParts)
            {
                return;
            }

            // no head => no face
            if (!this.Pawn.health.hediffSet.HasHead)
            {
                return;
            }

            // Reset the stats
            this.bodyStat.eyeLeft = PartStatus.Natural;
            this.bodyStat.eyeRight = PartStatus.Natural;
            this.bodyStat.jaw = PartStatus.Natural;
            this.bodyStat.handLeft = PartStatus.Natural;
            this.bodyStat.handRight = PartStatus.Natural;
            this.bodyStat.footLeft = PartStatus.Natural;
            this.bodyStat.footRight = PartStatus.Natural;

            List<BodyPartRecord> allParts = this.Pawn?.RaceProps?.body?.AllParts;
            if (allParts.NullOrEmpty())
            {
                return;
            }
            List<BodyPartRecord> body = allParts;
            List<Hediff> hediffs = this.Pawn?.health?.hediffSet?.hediffs;

            if (hediffs.NullOrEmpty() || body.NullOrEmpty())
            {
                // || hediffs.Any(x => x.def == HediffDefOf.MissingBodyPart && x.Part.def == BodyPartDefOf.Head))
                return;
            }

            foreach (Hediff diff in hediffs.Where(diff => diff?.def?.defName != null && diff.def == HediffDefOf.MissingBodyPart))
            {
                this.CheckPart(body, diff);
            }
            foreach (Hediff diff in hediffs.Where(diff => diff?.def?.defName != null && diff.def.addedPartProps != null))
            {
                this.CheckPart(body, diff);
            }
        }

        // Verse.PawnGraphicSet
        public void ClearCache()
        {
            this.cachedSkinMatsBodyBaseHash = -1;
            this.cachedNakedMatsBodyBaseHash = -1;
        }

        public void DrawAlienBodyAddons(Quaternion quat, Vector3 vector, bool portrait, bool renderBody)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawAlienBodyAddons(quat, vector, portrait, renderBody);
                    i++;
                }
            }
        }

        public void DrawAlienHeadAddons(bool portrait, Quaternion headQuat, Vector3 currentLoc)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawAlienHeadAddons(portrait, headQuat, currentLoc);
                    i++;
                }
            }
        }

        public void DrawApparel(Quaternion quat, Vector3 vector, bool portrait, bool renderBody)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawApparel(quat, vector, renderBody, portrait);
                    i++;
                }
            }
        }

        public void DrawBasicHead(out bool headDrawn, RotDrawMode bodyDrawType, bool portrait, bool headStump, ref Vector3 locFacialY, Quaternion headQuat)
        {
            headDrawn = false;
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawBasicHead(
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
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawBeardAndTache(headQuat, portrait, ref locFacialY);
                    i++;
                }
            }
        }

        public void DrawBody(Vector3 rootLoc, Quaternion quat, RotDrawMode bodyDrawType, [CanBeNull] PawnWoundDrawer woundDrawer, bool renderBody, bool portrait)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawBody(
                        woundDrawer,
                        rootLoc,
                        quat,
                        bodyDrawType,
                        renderBody,
                        portrait);
                    i++;
                }
            }
        }

        public void DrawBrows(ref Vector3 locFacialY, Quaternion headQuat, bool portrait)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawBrows(headQuat, portrait, ref locFacialY);
                    i++;
                }
            }
        }

        public void DrawHairAndHeadGear(Vector3 rootLoc, RotDrawMode bodyDrawType, ref Vector3 currentLoc, Vector3 b, bool portrait, bool renderBody, Quaternion headQuat)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawHairAndHeadGear(
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
        //             this.portrait = portrait;
        //             this.headQuat = headQuat;
        //             this.headFacing = headFacing;
        //             this.graphics = graphics;
        //     this.renderBody = renderBody;
        // }
        public void DrawHeadOverlays(PawnHeadOverlays headOverlays, Vector3 bodyLoc, Quaternion headQuat)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawHeadOverlays(headOverlays, bodyLoc, headQuat);
                    i++;
                }
            }
        }

        public void DrawNaturalEyes(ref Vector3 locFacialY, bool portrait, Quaternion headQuat)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawNaturalEyes(headQuat, portrait, ref locFacialY);
                    i++;
                }
            }
        }

        public void DrawNaturalMouth(ref Vector3 locFacialY, bool portrait, Quaternion headQuat)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawNaturalMouth(headQuat, portrait, ref locFacialY);
                    i++;
                }
            }
        }

        public void DrawUnnaturalEyeParts(ref Vector3 locFacialY, Quaternion headQuat, bool portrait)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawUnnaturalEyeParts(headQuat, portrait, ref locFacialY);
                    i++;
                }
            }
        }

        public void DrawWrinkles(RotDrawMode bodyDrawType, ref Vector3 locFacialY, Quaternion headQuat, bool portrait)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawWrinkles(headQuat, bodyDrawType, portrait, ref locFacialY);
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
                this.pawnDrawers = new List<PawnDrawer>();
                for (int i = 0; i < this.Props.drawers.Count; i++)
                {
                    PawnDrawer thingComp = (PawnDrawer)Activator.CreateInstance(this.Props.drawers[i].GetType());
                    thingComp.CompFace = this;
                    this.pawnDrawers.Add(thingComp);
                    thingComp.Initialize();
                }
            }
        }

        public List<Material> NakedMatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            int num = facing.AsInt + 1000 * (int)bodyCondition;
            if (num != this.cachedNakedMatsBodyBaseHash)
            {
                this.cachedNakedMatsBodyBase.Clear();
                this.cachedNakedMatsBodyBaseHash = num;
                PawnGraphicSet graphics = this.Pawn.Drawer.renderer.graphics;
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    this.cachedNakedMatsBodyBase.Add(graphics.nakedGraphic.MatAt(facing, null));
                }
                else if (bodyCondition == RotDrawMode.Rotting || graphics.dessicatedGraphic == null)
                {
                    this.cachedNakedMatsBodyBase.Add(graphics.rottingGraphic.MatAt(facing, null));
                }
                else if (bodyCondition == RotDrawMode.Dessicated)
                {
                    this.cachedNakedMatsBodyBase.Add(graphics.dessicatedGraphic.MatAt(facing, null));
                }

                for (int i = 0; i < graphics.apparelGraphics.Count; i++)
                {
                    ApparelLayer lastLayer = graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer;

                    if (this.Pawn.Dead)
                    {
                        if (lastLayer != ApparelLayer.Shell && lastLayer != ApparelLayer.Overhead)
                        {
                            this.cachedNakedMatsBodyBase.Add(graphics.apparelGraphics[i].graphic.MatAt(facing, null));
                        }
                    }
                }
            }

            return this.cachedNakedMatsBodyBase;
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

            if (this.Props.hasHands)
            {
                this.BodyAnimator.AnimatorTick();
            }

            if (Find.TickManager.TicksGame % 180 == 0)
            {
                this.IsAsleep = !this.Pawn.Awake();
            }

            if (!IsAsleep)
            {
                this.headRotator.RotatorTick();
            }

            if (this.Props.hasMouth)
            {
                if (Find.TickManager.TicksGame % 30 == 0)
                {
                    this.PawnGraphic.SetMouthAccordingToMoodLevel();
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

            Scribe_Values.Look(ref this.lastRoomCheck, "lastRoomCheck");
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
        /// <param name="p">
        /// The pawn.
        /// </param>
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
                this.eyeWiggler = new PawnEyeWiggler(this);
            }
            this.CheckForAddedOrMissingParts();

            this.PawnGraphic = new PawnGraphic(this);
            this.faceMaterial = new FaceMaterial(this, this.PawnGraphic);

            // this.isMasochist = this.pawn.story.traits.HasTrait(TraitDef.Named("Masochist"));
            this.headRotator = new PawnHeadRotator(this.Pawn);
            this.bodyAnimator = new BodyAnimator(this.Pawn, this);

            // this.headWiggler = new PawnHeadWiggler(this.pawn);

            switch (this.Pawn.story.bodyType)
            {
                case BodyType.Undefined:
                case BodyType.Male:
                    this.bodySizeDefinition.shoulderOffsetVerFromCenter = 0;
                    this.bodySizeDefinition.shoulderWidth = 0.25f;
                    this.bodySizeDefinition.armLength = 0.275f;
                    this.bodySizeDefinition.hipOffsetVerticalFromCenter = -0.275f;
                    this.bodySizeDefinition.hipWidth = 0.175f;
                    this.bodySizeDefinition.legLength = 0.3f;
                    break;
                case BodyType.Female:
                    this.bodySizeDefinition.shoulderOffsetVerFromCenter = 0f;
                    this.bodySizeDefinition.shoulderWidth = 0.225f;
                    this.bodySizeDefinition.armLength = 0.3f;
                    this.bodySizeDefinition.hipOffsetVerticalFromCenter = -0.275f;
                    this.bodySizeDefinition.hipWidth = 0.175f;
                    this.bodySizeDefinition.legLength = 0.3f;
                    break;
                case BodyType.Hulk:
                    this.bodySizeDefinition.shoulderOffsetVerFromCenter = 0f;
                    this.bodySizeDefinition.shoulderWidth = 0.35f;
                    this.bodySizeDefinition.armLength = 0.35f;
                    this.bodySizeDefinition.hipOffsetVerticalFromCenter = -0.3f;
                    this.bodySizeDefinition.hipWidth = 0.175f;
                    this.bodySizeDefinition.legLength = 0.425f;
                    this.bodySizeDefinition.hipOffsetHorWhenFacingHorizontal = -0.15f;
                    this.bodySizeDefinition.shoulderOffsetWhenFacingHorizontal = -0.05f;
                    break;
                case BodyType.Fat:
                    this.bodySizeDefinition.shoulderOffsetVerFromCenter = 0f;
                    this.bodySizeDefinition.shoulderWidth = 0.3f;
                    this.bodySizeDefinition.armLength = 0.275f;
                    this.bodySizeDefinition.hipOffsetVerticalFromCenter = -0.275f;
                    this.bodySizeDefinition.hipWidth = 0.2f;
                    this.bodySizeDefinition.legLength = 0.35f;
                    break;
                case BodyType.Thin:
                    this.bodySizeDefinition.shoulderOffsetVerFromCenter = 0f;
                    this.bodySizeDefinition.shoulderWidth = 0.15f;
                    this.bodySizeDefinition.armLength = 0.225f;
                    this.bodySizeDefinition.hipOffsetVerticalFromCenter = -0.25f;
                    this.bodySizeDefinition.hipWidth = 0.125f;
                    this.bodySizeDefinition.legLength = 0.3f;
                    break;
            }

            this.bodySizeDefinition.hipOffsetHorWhenFacingHorizontal = -0.05f;

            this.InitializePawnDrawer();

            return true;
        }

        public BodyDefinition bodySizeDefinition;

        public void SetPawnFace([NotNull] PawnFace importedFace)
        {
            this.pawnFace = importedFace;
        }

        // Verse.PawnGraphicSet
        public List<Material> UnderwearMatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            int num = facing.AsInt + 1000 * (int)bodyCondition;
            if (num != this.cachedSkinMatsBodyBaseHash)
            {
                this.cachedSkinMatsBodyBase.Clear();
                this.cachedSkinMatsBodyBaseHash = num;
                PawnGraphicSet graphics = this.Pawn.Drawer.renderer.graphics;
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    this.cachedSkinMatsBodyBase.Add(graphics.nakedGraphic.MatAt(facing, null));
                }
                else if (bodyCondition == RotDrawMode.Rotting || graphics.dessicatedGraphic == null)
                {
                    this.cachedSkinMatsBodyBase.Add(graphics.rottingGraphic.MatAt(facing, null));
                }
                else if (bodyCondition == RotDrawMode.Dessicated)
                {
                    this.cachedSkinMatsBodyBase.Add(graphics.dessicatedGraphic.MatAt(facing, null));
                }

                for (int i = 0; i < graphics.apparelGraphics.Count; i++)
                {
                    ApparelLayer lastLayer = graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer;

                    // if (lastLayer != ApparelLayer.Shell && lastLayer != ApparelLayer.Overhead)
                    if (lastLayer == ApparelLayer.OnSkin)
                    {
                        this.cachedSkinMatsBodyBase.Add(graphics.apparelGraphics[i].graphic.MatAt(facing, null));
                    }
                }
            }

            return this.cachedSkinMatsBodyBase;
        }

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


        private void CheckPart([NotNull] List<BodyPartRecord> body, [NotNull] Hediff hediff)
        {
            if (body.NullOrEmpty() || hediff.def == null)
            {
                return;
            }

            BodyPartRecord leftEye = body.Find(x => x.def == BodyPartDefOf.LeftEye);
            BodyPartRecord rightEye = body.Find(x => x.def == BodyPartDefOf.RightEye);
            BodyPartRecord jaw = body.Find(x => x.def == BodyPartDefOf.Jaw);
            BodyPartRecord leftArm = body.Find(x => x.def == BodyPartDefOf.LeftArm);
            BodyPartRecord rightArm = body.Find(x => x.def == DefDatabase<BodyPartDef>.GetNamed("RightShoulder"));
            BodyPartRecord leftHand = body.Find(x => x.def == DefDatabase<BodyPartDef>.GetNamed("LeftShoulder"));
            BodyPartRecord rightHand = body.Find(x => x.def == BodyPartDefOf.RightHand);
            BodyPartRecord leftLeg = body.Find(x => x.def == BodyPartDefOf.LeftLeg);
            BodyPartRecord rightLeg = body.Find(x => x.def == BodyPartDefOf.RightLeg);
            BodyPartRecord leftFoot = body.Find(x => x.def == DefDatabase<BodyPartDef>.GetNamed("LeftFoot"));
            BodyPartRecord rightFoot = body.Find(x => x.def == DefDatabase<BodyPartDef>.GetNamed("RightFoot"));

            // Missing parts firs, hands and feet can be replaced by arms/legs
            if (hediff.def == HediffDefOf.MissingBodyPart)
            {
                if (this.Props.hasEyes)
                {
                    if (leftEye != null && hediff.Part == leftEye)
                    {
                        this.bodyStat.eyeLeft = PartStatus.Missing;
                        this.texPathEyeLeft = this.EyeTexPath("Missing", Side.Left);
                    }

                    // ReSharper disable once InvertIf
                    if (rightEye != null && hediff.Part == rightEye)
                    {
                        this.bodyStat.eyeRight = PartStatus.Missing;
                        this.texPathEyeRight = this.EyeTexPath("Missing", Side.Right);
                    }
                }

                if (this.Props.hasHands)
                {
                    if (hediff.Part == leftHand)
                    {
                        this.bodyStat.handLeft = PartStatus.Missing;
                    }
                    if (hediff.Part == rightHand)
                    {
                        this.bodyStat.handRight = PartStatus.Missing;
                    }
                    if (hediff.Part == leftFoot)
                    {
                        this.bodyStat.footLeft = PartStatus.Missing;
                    }
                    if (hediff.Part == rightFoot)
                    {
                        this.bodyStat.footRight = PartStatus.Missing;
                    }
                }
            }

            AddedBodyPartProps addedPartProps = hediff.def?.addedPartProps;
            if (addedPartProps != null)
            {
                if (hediff.def?.defName != null && hediff.Part != null)
                {
                    if (this.Props.hasEyes)
                    {
                        if (hediff.Part == leftEye)
                        {
                            this.texPathEyeLeftPatch = "AddedParts/" + hediff.def.defName + "_Left" + "_"
                                                       + this.PawnCrownType;
                        }

                        if (hediff.Part == rightEye)
                        {
                            this.texPathEyeRightPatch = "AddedParts/" + hediff.def.defName + "_Right" + "_"
                                                        + this.PawnCrownType;
                        }
                    }

                    if (this.Props.hasMouth)
                    {
                        if (hediff.Part == jaw)
                        {
                            this.texPathJawAddedPart = "Mouth/Mouth_" + hediff.def.defName;
                        }
                    }

                    if (this.Props.hasHands)
                    {
                        if (hediff.Part == leftHand || hediff.Part == leftArm)
                        {
                            this.bodyStat.handLeft = PartStatus.Artificial;
                        }
                        if (hediff.Part == rightHand || hediff.Part == rightArm)
                        {
                            this.bodyStat.handRight = PartStatus.Artificial;
                        }
                        if (hediff.Part == leftFoot || hediff.Part == leftLeg)
                        {
                            this.bodyStat.footLeft = PartStatus.Artificial;
                        }
                        if (hediff.Part == rightFoot || hediff.Part == rightLeg)
                        {
                            this.bodyStat.footRight = PartStatus.Artificial;
                        }
                    }
                }
            }


        }

        [CanBeNull]
        public string texPathEyeRightPatch;

        private BodyAnimator bodyAnimator;

        public BodyAnimator BodyAnimator => bodyAnimator;

        [NotNull]
        public string EyeClosedTexPath(Side side)
        {
            return this.EyeTexPath("Closed", side);
        }

        #endregion Private Methods

        // public Vector3 RightHandPosition;
        //
        // public Vector3 LeftHandPosition;


        public void DrawEquipment(Vector3 rootLoc, bool portrait)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawEquipment(rootLoc, portrait);
                    i++;
                }
            }
        }

        public Vector3 BaseHeadOffsetAt(bool portrait)
        {
            var offset = Vector3.zero;

            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].BaseHeadOffsetAt(ref offset, portrait);
                    i++;
                }
            }
            return offset;
        }

        public BodyPartStats bodyStat;

        public bool AnimatorOpen;

        public float AnimationPercent;

        public WalkCycleDef walkCycle = WalkCycleDefOf.Human_Walk;

        public void DrawFeet(Vector3 rootLoc, bool portrait)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawFeet(rootLoc, portrait);
                    i++;
                }
            }
        }
        public void ApplyBodyWobble(ref Vector3 rootLoc, ref Quaternion quat)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].ApplyBodyWobble(ref rootLoc, ref quat);
                    i++;
                }
            }
        }

        public void TickDrawers(Rot4 bodyFacing, Rot4 headFacing, PawnGraphicSet graphics)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].Tick(bodyFacing, headFacing, graphics);
                    i++;
                }
            }
        }
    }
}