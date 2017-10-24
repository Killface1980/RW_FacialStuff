namespace FacialStuff
{
    using System.Collections.Generic;
    using System.Linq;

    using FacialStuff.Animator;
    using FacialStuff.Defs;
    using FacialStuff.Enums;
    using FacialStuff.Graphics;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class CompFace : ThingComp
    {

        #region Public Fields

        public bool DontRender;

        [NotNull]
        public FaceGraphicParts faceGraphicPart = new FaceGraphicParts();

        public bool IgnoreRenderer;

        public bool IsChild;

        public Faction originFaction;

        [NotNull]
        public Pawn pawn;

        public bool Roofed;

        public int rotationInt;

        #endregion Public Fields

        #region Private Fields

        // old, remove 0.18
        private BeardDef BeardDef;

        // old, remove 0.18
        private BrowDef BrowDef;

        // old, remove 0.18
        private EyeDef EyeDef;

        private Vector2 eyeOffset = Vector2.zero;

        [NotNull]
        private PawnEyeWiggler eyeWiggler;

        private float factionMelanin;

        [NotNull]
        // ReSharper disable once NotNullMemberIsNotInitialized
        private DamageFlasher flasher;

        private Color hairColor;

        private bool hasNaturalJaw = true;

        private PawnHeadRotator headRotator;

        // private float blinkRate;
        // public PawnHeadWiggler headWiggler;
        private float mood = 0.5f;

        private HumanMouthGraphics mouthgraphic;

        private Vector2 mouthOffset = Vector2.zero;

        // must be null, always initialize with pawn
        [CanBeNull]
        private PawnFace pawnFace;

        // public bool IgnoreRenderer;
        [CanBeNull]
        private string texPathBrow;

        [CanBeNull]
        private string texPathEyeLeft;

        [CanBeNull]
        private string texPathEyeLeftClosed;

        [CanBeNull]
        private string texPathEyeLeftPatch;

        [CanBeNull]
        private string texPathEyeRight;

        [CanBeNull]
        private string texPathEyeRightClosed;

        [CanBeNull]
        private string texPathEyeRightPatch;

        [CanBeNull]
        private string texPathJawAddedPart;

        #endregion Private Fields

        #region Public Properties

        public GraphicVectorMeshSet EyeMeshSet => MeshPoolFS.HumanEyeSet[(int)this.FullHeadType];

        [NotNull]
        public PawnEyeWiggler EyeWiggler => this.eyeWiggler;

        public float FactionMelanin
        {
            get => this.factionMelanin;
            set => this.factionMelanin = value;
        }

        public FullHead FullHeadType { get; set; } = FullHead.Undefined;

        public bool HasEyePatchLeft { get; private set; }

        public bool HasEyePatchRight { get; private set; }

        public PawnHeadRotator HeadRotator => this.headRotator;

        [NotNull]
        public GraphicVectorMeshSet MouthMeshSet => MeshPoolFS.HumanlikeMouthSet[(int)this.FullHeadType];

        public CrownType PawnCrownType => this.pawn?.story.crownType ?? CrownType.Average;

        [CanBeNull]
        public PawnFace PawnFace => this.pawnFace;

        public HeadType PawnHeadType
        {
            get
            {
                if (this.pawn.story?.HeadGraphicPath == null)
                {
                    return HeadType.Normal;
                }

                if (this.pawn.story.HeadGraphicPath.Contains("Pointy"))
                {
                    return HeadType.Pointy;
                }

                if (this.pawn.story.HeadGraphicPath.Contains("Wide"))
                {
                    return HeadType.Wide;
                }

                return HeadType.Normal;
            }
        }

        #endregion Public Properties

        #region Public Methods

        // only for development
        public Vector3 BaseEyeOffsetAt(Rot4 rotation)
        {
            bool male = this.pawn.gender == Gender.Male;

            if (this.PawnCrownType == CrownType.Average)
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            this.eyeOffset = Settings.EyeMaleAverageNormalOffset;
                        }
                        else
                        {
                            this.eyeOffset = Settings.EyeFemaleAverageNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this.eyeOffset = Settings.EyeMaleAveragePointyOffset;
                        }
                        else
                        {
                            this.eyeOffset = Settings.EyeFemaleAveragePointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this.eyeOffset = Settings.EyeMaleAverageWideOffset;
                        }
                        else
                        {
                            this.eyeOffset = Settings.EyeFemaleAverageWideOffset;
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
                            this.eyeOffset = Settings.EyeMaleNarrowNormalOffset;
                        }
                        else
                        {
                            this.eyeOffset = Settings.EyeFemaleNarrowNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this.eyeOffset = Settings.EyeMaleNarrowPointyOffset;
                        }
                        else
                        {
                            this.eyeOffset = Settings.EyeFemaleNarrowPointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this.eyeOffset = Settings.EyeMaleNarrowWideOffset;
                        }
                        else
                        {
                            this.eyeOffset = Settings.EyeFemaleNarrowWideOffset;
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
            var male = this.pawn.gender == Gender.Male;

            if (this.PawnCrownType == CrownType.Average)
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            this.mouthOffset = Settings.MouthMaleAverageNormalOffset;
                        }
                        else
                        {
                            this.mouthOffset = Settings.MouthFemaleAverageNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this.mouthOffset = Settings.MouthMaleAveragePointyOffset;
                        }
                        else
                        {
                            this.mouthOffset = Settings.MouthFemaleAveragePointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this.mouthOffset = Settings.MouthMaleAverageWideOffset;
                        }
                        else
                        {
                            this.mouthOffset = Settings.MouthFemaleAverageWideOffset;
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
                            this.mouthOffset = Settings.MouthMaleNarrowNormalOffset;
                        }
                        else
                        {
                            this.mouthOffset = Settings.MouthFemaleNarrowNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this.mouthOffset = Settings.MouthMaleNarrowPointyOffset;
                        }
                        else
                        {
                            this.mouthOffset = Settings.MouthFemaleNarrowPointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this.mouthOffset = Settings.MouthMaleNarrowWideOffset;
                        }
                        else
                        {
                            this.mouthOffset = Settings.MouthFemaleNarrowWideOffset;
                        }

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

        [CanBeNull]
        public Material BeardMatAt(Rot4 facing)
        {
            if (this.pawn.gender != Gender.Male || this.PawnFace?.BeardDef == BeardDefOf.Beard_Shaved || !this.hasNaturalJaw)
            {
                return null;
            }

            Material material = this.faceGraphicPart.MainBeardGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material BrowMatAt(Rot4 facing)
        {
            Material material = this.faceGraphicPart.BrowGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [NotNull]
        public string BrowTexPath([NotNull] BrowDef browDef)
        {
            return "Brows/Brow_" + this.pawn.gender + "_" + browDef.texPath;
        }

        // Can be called externally
        public void CheckForAddedOrMissingParts(Pawn p)
        {
            if (!Controller.settings.ShowExtraParts)
            {
                return;
            }

            this.pawn = p;

            // no head => no face
            if (!this.pawn?.health.hediffSet.HasHead != true)
            {
                return;
            }

            List<BodyPartRecord> body = this.pawn?.RaceProps?.body?.AllParts;
            List<Hediff> hediffs = this.pawn?.health?.hediffSet?.hediffs;

            if (hediffs.NullOrEmpty() || body.NullOrEmpty())
            //   || hediffs.Any(x => x.def == HediffDefOf.MissingBodyPart && x.Part.def == BodyPartDefOf.Head))
            {
                return;
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            foreach (Hediff diff in hediffs.Where(diff => diff?.def?.defName != null))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                this.CheckPart(body, diff);
            }
        }

        // Deactivated for now
        // ReSharper disable once FlagArgument
        [CanBeNull]
        public Material DeadEyeMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            Material material = null;
            if (bodyCondition == RotDrawMode.Fresh)
            {
                material = this.faceGraphicPart.DeadEyeGraphic?.MatAt(facing);
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                material = this.faceGraphicPart.DeadEyeGraphic?.MatAt(facing);
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material EyeLeftMatAt(Rot4 facing, bool portrait)
        {
            if (this.HasEyePatchLeft)
            {
                return null;
            }

            if (facing == Rot4.East)
            {
                return null;
            }

            Material material = this.faceGraphicPart.EyeLeftGraphic?.MatAt(facing);

            if (!portrait)
            {
                if (Controller.settings.MakeThemBlink && this.eyeWiggler.EyeLeftCanBlink)
                {
                    if (this.eyeWiggler.IsAsleep || this.eyeWiggler.EyeLeftBlinkNow)
                    {
                        material = this.faceGraphicPart.EyeLeftClosedGraphic.MatAt(facing);
                    }
                }
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        // TODO: Remove or make usable
        // public void DefineSkinDNA()
        // {
        // HairMelanin.SkinGenetics(this.pawn, this, out this.factionMelanin);
        // this.IsSkinDNAoptimized = true;
        // }
        [CanBeNull]
        public Material EyeLeftPatchMatAt(Rot4 facing)
        {
            Material material = this.faceGraphicPart.EyeLeftPatchGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material EyeRightMatAt(Rot4 facing, bool portrait)
        {
            if (this.HasEyePatchRight)
            {
                return null;
            }

            if (facing == Rot4.West)
            {
                return null;
            }

            Material material = this.faceGraphicPart.EyeRightGraphic?.MatAt(facing);

            if (!portrait)
            {
                if (Controller.settings.MakeThemBlink && this.eyeWiggler.EyeRightCanBlink)
                {
                    if (this.eyeWiggler.IsAsleep || this.eyeWiggler.EyeRightBlinkNow)
                    {
                        material = this.faceGraphicPart.EyeRightClosedGraphic?.MatAt(facing);
                    }
                }
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material EyeRightPatchMatAt(Rot4 facing)
        {
            Material material = this.faceGraphicPart.EyeRightPatchGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [NotNull]
        public string EyeTexPath([NotNull] string eyeDefPath, Side side)
        {
            // ReSharper disable once PossibleNullReferenceException
            string path = "Eyes/Eye_" + eyeDefPath + "_" + this.pawn.gender + "_" + side;

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

        public Quaternion HeadQuat(Rot4 rotation)
        {
            float num = 1f;
            Quaternion asQuat = rotation.AsQuat;
            float x = 1f * Mathf.Sin(num * (this.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            float z = 1f * Mathf.Cos(num * (this.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            asQuat.SetLookRotation(new Vector3(x, 0f, z), Vector3.up);
            return asQuat;
        }

        /// <summary>
        ///     Initializes Facial stuff graphics.
        /// </summary>
        /// <returns>True if all went well.</returns>
        public bool InitializeGraphics()
        {
            this.InitializeGraphicsWrinkles();

            this.InitializeGraphicsBeard();

            this.InitializeGraphicsBrows();

            this.InitializeGraphicsMouth();

            this.InitializeGraphicsEyes();

            return true;
        }

        [CanBeNull]
        public Material MoustacheMatAt(Rot4 facing)
        {
            if (this.pawn.gender != Gender.Male || this.PawnFace.MoustacheDef == MoustacheDefOf.Shaved || !this.hasNaturalJaw)
            {
                return null;
            }

            Material material = this.faceGraphicPart.MoustacheGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material MouthMatAt(Rot4 facing, bool portrait)
        {
            Material material = null;

            if (!this.hasNaturalJaw && Controller.settings.ShowExtraParts)
            {
                material = this.faceGraphicPart.JawGraphic?.MatAt(facing);
            }
            else
            {
                if (!Controller.settings.UseMouth || !this.PawnFace.DrawMouth)
                {
                    return null;
                }

                if (this.pawn.gender == Gender.Male)
                {
                    if (!this.PawnFace.BeardDef.drawMouth || this.PawnFace.MoustacheDef != MoustacheDefOf.Shaved)
                    {
                        return null;
                    }
                }

                if (portrait)
                {
                    material = this.mouthgraphic.HumanMouthGraphic[3].Graphic.MatAt(facing);
                }
                else
                {
                    material = this.faceGraphicPart.MouthGraphic?.MatAt(facing);

                    // if (bodyCondition == RotDrawMode.Fresh)
                    // {
                    // material = this.faceGraphicPart.JawGraphic?.MatAt(facing);
                    // }
                    // else if (bodyCondition == RotDrawMode.Rotting)
                    // {
                    // }
                }
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public bool PawnFaceIsNull()
        {
            return this.pawnFace == null;
        }
        public override void PostDraw()
        {
            base.PostDraw();

            // Children & Pregnancy || Werewolves transformed
            if (this.pawn?.Map == null || !this.pawn.Spawned || this.pawn.Dead || this.IsChild || this.DontRender)
            {
                return;
            }

            this.Roofed = this.pawn.Position.Roofed(this.pawn.Map);

            if (Find.TickManager.Paused)
            {
                return;
            }

            // CellRect viewRect = Find.CameraDriver.CurrentViewRect;
            // viewRect = viewRect.ExpandedBy(5);
            // if (!viewRect.Contains(this.pawn.Position))
            // {
            //     return;
            // }

            this.eyeWiggler.WigglerTick();

            if (!this.eyeWiggler.IsAsleep)
            {
                this.headRotator.RotatorTick();
            }

            // Low-prio stats
            if (Find.TickManager.TicksGame % 30 == 0)
            {
                this.SetMouthAccordingToMoodLevel();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            {
                // Legacy, remove in A18
                Scribe_References.Look(ref this.pawn, "Pawn");
                Scribe_Defs.Look(ref this.EyeDef, "EyeDef");
                Scribe_Defs.Look(ref this.BrowDef, "BrowDef");
                Scribe_Defs.Look(ref this.BeardDef, "BeardDef");
                Scribe_Values.Look(ref this.hairColor, "HairColorOrg");
                Scribe_References.Look(ref this.originFaction, "pawnFaction");
            }

            // Scribe_Values.Look(ref this.pawnFace.MelaninOrg, "MelaninOrg");

            // Log.Message(
            // "Facial Stuff updated pawn " + this.parent.Label + "-" + face.BeardDef + "-" + face.EyeDef);

            // Force ResolveAllGraphics
            Scribe_Deep.Look(ref this.pawnFace, "pawnFace");

            // Scribe_References.Look(ref this.pawn, "pawn");
            Scribe_Values.Look(ref this.IsChild, "isChild");
            Scribe_Values.Look(ref this.DontRender, "dontrender");
            Scribe_Values.Look(ref this.Roofed, "roofed");
            Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");
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
        public bool SetHeadType([NotNull] Pawn p)
        {
            this.pawn = p;

            if (this.originFaction == null)
            {
                this.originFaction = this.pawn.Faction ?? Faction.OfPlayer;
            }

            if (this.PawnFace == null)
            {
                this.SetPawnFace(new PawnFace(this.pawn, this.originFaction.def));

                // check for pre-0.17.3 pawns
                if (this.EyeDef != null)
                {
                    this.PawnFace.EyeDef = this.EyeDef;
                    this.PawnFace.BrowDef = this.BrowDef;
                    this.PawnFace.HairColor = this.hairColor;
                    this.pawn.story.melanin = Mathf.Abs(1f - this.pawn.story.melanin);
                }
                else
                {
                    // set the hair color here, the only suitable place
                    this.pawn.story.hairColor = this.PawnFace.HairColor;
                }

                if (this.BeardDef != null)
                {
                    this.PawnFace.BeardDef = this.BeardDef;
                    this.PawnFace.MoustacheDef = MoustacheDefOf.Shaved;
                }

                this.EyeDef = null;
                this.BrowDef = null;
                this.BeardDef = null;
            }

            // this.isMasochist = this.pawn.story.traits.HasTrait(TraitDef.Named("Masochist"));
            this.mouthgraphic = new HumanMouthGraphics(this.pawn);
            this.flasher = this.pawn.Drawer.renderer.graphics.flasher;
            this.eyeWiggler = new PawnEyeWiggler(this.pawn);
            this.headRotator = new PawnHeadRotator(this.pawn);

            // this.headWiggler = new PawnHeadWiggler(this.pawn);

            // ReSharper disable once PossibleNullReferenceException
            this.ResetBoolsAndPaths();
            this.CheckForAddedOrMissingParts();

            // Only for the crowntype ...
            CrownTypeChecker.SetHeadOffsets(this.pawn, this);

            return true;
        }

        public void SetPawnFace([NotNull] PawnFace inportedFace)
        {
            this.pawnFace = inportedFace;
        }

        [CanBeNull]
        public Material WrinkleMatAt(Rot4 facing, RotDrawMode bodyCondition)
        {
            Material material = null;
            if (Controller.settings.UseWrinkles)
            {
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    material = this.faceGraphicPart.WrinkleGraphic?.MatAt(facing);
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    material = this.faceGraphicPart.RottingWrinkleGraphic?.MatAt(facing);
                }
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        #endregion Public Methods

        #region Private Methods

        private void CheckForAddedOrMissingParts()
        {
            this.CheckForAddedOrMissingParts(this.parent as Pawn);
        }
        private void CheckPart([NotNull] List<BodyPartRecord> body, [NotNull] Hediff hediff)
        {
            if (body.NullOrEmpty() || hediff.def == null)
            {
                return;
            }

            BodyPartRecord leftEye = body.Find(x => x.def == BodyPartDefOf.LeftEye);
            BodyPartRecord rightEye = body.Find(x => x.def == BodyPartDefOf.RightEye);
            BodyPartRecord jaw = body.Find(x => x.def == BodyPartDefOf.Jaw);
            AddedBodyPartProps addedPartProps = hediff.def?.addedPartProps;

            if (addedPartProps != null)
            {
                if (hediff.def?.defName != null && hediff.Part != null)
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

                    if (hediff.Part == jaw)
                    {
                        this.texPathJawAddedPart = "Mouth/Mouth_" + hediff.def.defName;
                    }
                }
            }

            if (hediff.def != HediffDefOf.MissingBodyPart)
            {
                return;
            }

            if (leftEye != null && hediff.Part == leftEye)
            {
                this.texPathEyeLeft = this.EyeTexPath("Missing", Side.Left);
                this.eyeWiggler.EyeLeftCanBlink = false;
            }

            // ReSharper disable once InvertIf
            if (rightEye != null && hediff.Part == rightEye)
            {
                this.texPathEyeRight = this.EyeTexPath("Missing", Side.Right);
                this.eyeWiggler.EyeRightCanBlink = false;
            }
        }

        [NotNull]
        private string EyeClosedTexPath(Side side)
        {
            return this.EyeTexPath("Closed", side);
        }

        private void InitializeGraphicsBeard()
        {
            string mainBeardDefTexPath = this.GetBeardPath(this.PawnFace.BeardDef);

            string moustacheDefTexPath = this.GetMoustachePath(this.PawnFace.MoustacheDef);

            Color beardColor = this.PawnFace.BeardColor;
            Color tacheColor = this.PawnFace.BeardColor;

            if (this.PawnFace.MoustacheDef == MoustacheDefOf.Shaved)
            {
                // no error, only use the beard def shaved as texture
                tacheColor = Color.white;
            }

            if (this.PawnFace.BeardDef == BeardDefOf.Beard_Shaved)
            {
                beardColor = Color.white;
            }

            this.faceGraphicPart.MoustacheGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                moustacheDefTexPath,
                ShaderDatabase.Cutout,
                Vector2.one,
                tacheColor);

            this.faceGraphicPart.MainBeardGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                mainBeardDefTexPath,
                ShaderDatabase.Cutout,
                Vector2.one,
                beardColor);
        }

        private void InitializeGraphicsBrows()
        {
            Color color = this.pawn.story.hairColor * Color.gray;
            this.faceGraphicPart.BrowGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.texPathBrow,
                ShaderDatabase.Cutout,
                Vector2.one,
                color);
        }

        private void InitializeGraphicsEyePatches()
        {
            if (!this.texPathEyeLeftPatch.NullOrEmpty())
            {
                bool flag = !ContentFinder<Texture2D>.Get(this.texPathEyeLeftPatch + "_front", false).NullOrBad();
                if (flag)
                {
                    this.faceGraphicPart.EyeLeftPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                                                                   this.texPathEyeLeftPatch,
                                                                   ShaderDatabase.Transparent,
                                                                   Vector2.one,
                                                                   Color.white) as Graphic_Multi_AddedHeadParts;
                    this.HasEyePatchLeft = true;
                }
                else
                {
                    this.HasEyePatchLeft = false;
                    Log.Message(
                        "Facial Stuff: No texture for added part: " + this.texPathEyeLeftPatch
                        + " - Graphic_Multi_AddedHeadParts");
                }
            }
            else
            {
                this.HasEyePatchLeft = false;
            }

            if (!this.texPathEyeRightPatch.NullOrEmpty())
            {
                bool flag2 = !ContentFinder<Texture2D>.Get(this.texPathEyeRightPatch + "_front", false).NullOrBad();
                if (flag2)
                {
                    this.faceGraphicPart.EyeRightPatchGraphic =
                        GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                            this.texPathEyeRightPatch,
                            ShaderDatabase.Transparent,
                            Vector2.one,
                            Color.white) as Graphic_Multi_AddedHeadParts;
                    this.HasEyePatchRight = true;
                }
                else
                {
                    Log.Message(
                        "Facial Stuff: No texture for added part: " + this.texPathEyeRightPatch
                        + " - Graphic_Multi_AddedHeadParts");
                    this.HasEyePatchRight = false;
                }
            }
            else
            {
                this.HasEyePatchRight = false;
            }
        }

        private void InitializeGraphicsEyes()
        {
            this.InitializeGraphicsEyePatches();

            this.faceGraphicPart.EyeLeftGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                                      this.texPathEyeLeft,
                                                      ShaderDatabase.Cutout,
                                                      Vector2.one,
                                                      this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.faceGraphicPart.EyeRightGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                                       this.texPathEyeRight,
                                                       ShaderDatabase.Cutout,
                                                       Vector2.one,
                                                       this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.faceGraphicPart.EyeLeftClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                                            this.texPathEyeLeftClosed,
                                                            ShaderDatabase.Cutout,
                                                            Vector2.one,
                                                            this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.faceGraphicPart.EyeRightClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                                             this.texPathEyeRightClosed,
                                                             ShaderDatabase.Cutout,
                                                             Vector2.one,
                                                             this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.faceGraphicPart.DeadEyeGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                "Eyes/Eyes_Dead",
                ShaderDatabase.Cutout,
                Vector2.one,
                this.pawn.story.SkinColor);
        }

        private void InitializeGraphicsMouth()
        {
            if (!this.texPathJawAddedPart.NullOrEmpty())
            {
                bool flag = ContentFinder<Texture2D>.Get(this.texPathJawAddedPart + "_front", false) != null;
                if (flag)
                {
                    this.faceGraphicPart.JawGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                          this.texPathJawAddedPart,
                                                          ShaderDatabase.CutoutSkin,
                                                          Vector2.one,
                                                          Color.white) as Graphic_Multi_NaturalHeadParts;
                    this.hasNaturalJaw = false;

                    // all done, return
                    return;
                }

                // texture for added/extra part not found, log and default
                Log.Message(
                    "Facial Stuff: No texture for added part: " + this.texPathJawAddedPart
                    + " - Graphic_Multi_NaturalHeadParts. This is not an error, just an info.");
            }

            this.hasNaturalJaw = true;
            this.faceGraphicPart.MouthGraphic = this.mouthgraphic
                .HumanMouthGraphic[this.pawn.Dead || this.pawn.Downed ? 2 : 3].Graphic;
        }

        private void InitializeGraphicsWrinkles()
        {
            Color wrinkleColor = this.pawn.story.SkinColor * new Color(0.1f, 0.1f, 0.1f);

            wrinkleColor.a = this.PawnFace.wrinkleIntensity;

            WrinkleDef pawnFaceWrinkleDef = this.PawnFace.WrinkleDef;

            this.faceGraphicPart.WrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                pawnFaceWrinkleDef.texPath + "_" + this.PawnCrownType + "_" + this.PawnHeadType,
                ShaderDatabase.TransparentPostLight,
                Vector2.one,
                wrinkleColor);

            this.faceGraphicPart.RottingWrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                pawnFaceWrinkleDef.texPath + "_" + this.PawnCrownType + "_" + this.PawnHeadType,
                ShaderDatabase.TransparentPostLight,
                Vector2.one,
                wrinkleColor * FaceTextures.SkinRottingMultiplyColor);
        }

        private void ResetBoolsAndPaths()
        {
            // Fix for PrepC for pre-FS pawns, also sometimes the brows are not defined?!?
            {
                if (this.PawnFace?.EyeDef == null)
                {
                    SetPawnFace(new PawnFace(this.pawn, Faction.OfPlayer.def));
                }

                if (this.PawnFace.BrowDef == null)
                {
                    this.PawnFace.BrowDef = PawnFaceMaker.RandomBrowDefFor(this.pawn, Faction.OfPlayer.def);
                }
            }

            this.texPathEyeLeftPatch = null;
            this.texPathEyeRightPatch = null;
            this.texPathJawAddedPart = null;

            EyeDef pawnFaceEyeDef = this.PawnFace.EyeDef;
            this.texPathEyeRight = this.EyeTexPath(pawnFaceEyeDef.texPath, Side.Right);
            this.texPathEyeLeft = this.EyeTexPath(pawnFaceEyeDef.texPath, Side.Left);

            this.texPathEyeRightClosed = this.EyeClosedTexPath(Side.Right);
            this.texPathEyeLeftClosed = this.EyeClosedTexPath(Side.Left);

            this.eyeWiggler.EyeLeftCanBlink = true;
            this.eyeWiggler.EyeRightCanBlink = true;

            this.texPathBrow = this.BrowTexPath(this.PawnFace.BrowDef);
        }

        private void SetMouthAccordingToMoodLevel()
        {
            if (this.pawn == null)
            {
                return;
            }
            if (!Controller.settings.UseMouth || !this.hasNaturalJaw)
            {
                return;
            }

            if (this.pawn.health.InPainShock && !this.eyeWiggler.IsAsleep)
            {
                if (this.eyeWiggler.EyeRightBlinkNow && this.eyeWiggler.EyeLeftBlinkNow)
                {
                    this.faceGraphicPart.MouthGraphic = this.mouthgraphic.mouthGraphicCrying;
                    return;
                }
            }

            if (this.pawn.needs?.mood?.thoughts != null)
            {
                this.mood = this.pawn.needs.mood.CurInstantLevel;
            }

            int mouthTextureIndexOfMood = this.mouthgraphic.GetMouthTextureIndexOfMood(this.mood);

            this.faceGraphicPart.MouthGraphic = this.mouthgraphic.HumanMouthGraphic[mouthTextureIndexOfMood].Graphic;
        }

        #endregion Private Methods

    }
}