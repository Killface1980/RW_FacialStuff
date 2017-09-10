namespace FacialStuff
{
    using System.Collections.Generic;
    using System.Linq;

    using FacialStuff.Defs;
    using FacialStuff.Enums;
    using FacialStuff.Genetics;
    using FacialStuff.Graphics_FS;

    using JetBrains.Annotations;

    using RimWorld;
    using UnityEngine;
    using Verse;

    public class CompFace : ThingComp
    {
        // public int rotationInt;
        #region Private Fields

        [NotNull]
        private PawnEyeWiggler eyeWiggler;

        [NotNull]
        private FaceGraphicParts faceGraphicPart = new FaceGraphicParts();
        private float factionMelanin;

        [NotNull]
        private DamageFlasher flasher = new DamageFlasher(null);

        private bool hasNaturalMouth = true;

        private Vector2 mouthOffset = Vector2.zero;
        private Vector2 eyeOffset = Vector2.zero;

        // private float blinkRate;
        // public PawnHeadWiggler headWiggler;
        private float mood = 0.5f;

        [NotNull]
        private Pawn pawn;

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
        private string texPathMouth;

        public bool Roofed;

        #endregion Private Fields

        #region Public Properties

        public Faction pawnFaction;

        public bool DontRender;

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

        [NotNull]
        public GraphicVectorMeshSet MouthMeshSet => MeshPoolFS.HumanlikeMouthSet[(int)this.FullHeadType];

        public GraphicVectorMeshSet EyeMeshSet => MeshPoolFS.HumanEyeSet[(int)this.FullHeadType];

        public bool IsChild;

        public CrownType PawnCrownType => this.pawn?.story.crownType ?? CrownType.Average;

        [NotNull]
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

        #region Private Properties



        #endregion Private Properties

        #region Public Methods

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

        // only for development
        public Vector3 BaseEyeOffsetAt(Rot4 rotation)
        {
            var male = this.pawn.gender == Gender.Male;

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

        public Quaternion HeadQuat(Rot4 rotation)
        {
            float num = 1f;
            Quaternion asQuat = rotation.AsQuat;
            float x = 1f * Mathf.Sin(num * (this.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            float z = 1f * Mathf.Cos(num * (this.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            asQuat.SetLookRotation(new Vector3(x, 0f, z), Vector3.up);
            return asQuat;
        }

        [CanBeNull]
        public Material BeardMatAt(Rot4 facing)
        {
            if (!this.hasNaturalMouth || this.pawn.gender == Gender.Female)
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
            Material material;
            material = this.faceGraphicPart.BrowGraphic?.MatAt(facing);

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

        // Can be called external
        public void CheckForAddedOrMissingParts()
        {
            this.pawn = this.parent as Pawn;
            if (this.pawn == null)
            {
                return;
            }

            List<BodyPartRecord> body = this.pawn.RaceProps?.body?.AllParts;
            List<Hediff> hediffSetHediffs = this.pawn.health?.hediffSet?.hediffs;
            if (hediffSetHediffs == null || body == null)
            {
                return;
            }

            foreach (Hediff hediff in hediffSetHediffs.Where(
                hediff => hediff?.def?.defName != null))
            {
                this.CheckPart(body, hediff);
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

        // TODO: Remove or make usable
        // public void DefineSkinDNA()
        // {
        // HairMelanin.SkinGenetics(this.pawn, this, out this.factionMelanin);
        // this.IsSkinDNAoptimized = true;
        // }
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

        private void InitializeGraphicsBrows()
        {
            Color color = this.pawn.story.hairColor * Color.gray;
            this.faceGraphicPart.BrowGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.texPathBrow,
                ShaderDatabase.Cutout,
                Vector2.one,
                color);
        }

        [CanBeNull]
        public Material MoustacheMatAt(Rot4 facing)
        {
            if (!this.hasNaturalMouth || this.PawnFace.MoustacheDef == MoustacheDefOf.Shaved
                                      || this.PawnFace.MoustacheDef == null || this.pawn.gender == Gender.Female)
            {
                return null;
            }

            Material material = null;
            material = this.faceGraphicPart.MoustacheGraphic?.MatAt(facing);

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

            if (!this.hasNaturalMouth && Controller.settings.ShowExtraParts)
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

        public override void PostDraw()
        {
            base.PostDraw();

            if (this.pawn == null)
            {
                return;
            }

            // Children & Pregnancy || Werewolves transformed
            if (!this.pawn.Spawned || this.pawn.Dead || this.IsChild || this.DontRender)
            {
                return;
            }

            if (Find.TickManager.Paused)
            {
                return;
            }


            // todo: head wiggler? move eyes to eyewiggler
            if (Controller.settings.MakeThemBlink)
            {
                this.eyeWiggler.WigglerTick();
            }
            if (Controller.settings.UseHeadRotator)
            {
                if (!this.eyeWiggler.IsAsleep)
                {
                    this.headRotator.RotatorTick();
                }
            }
            // Low-prio stats
            if (Find.TickManager.TicksGame % 30 != 0)
            {
                return;
            }

            this.Roofed = this.pawn.Position.Roofed(this.pawn.Map);


            if (Controller.settings.UseMouth)
            {
                if (this.hasNaturalMouth)
                {
                    this.SetMouthAccordingToMoodLevel();
                }
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
                Scribe_Values.Look(ref this.HairColor, "HairColorOrg");
                Scribe_References.Look(ref this.pawnFaction, "pawnFaction");
                Scribe_Values.Look(ref this.Roofed, "Roofed");
            }

            // Scribe_Values.Look(ref this.pawnFace.MelaninOrg, "MelaninOrg");

            // Log.Message(
            // "Facial Stuff updated pawn " + this.parent.Label + "-" + face.BeardDef + "-" + face.EyeDef);

            // Force ResolveAllGraphics
            Scribe_Deep.Look(ref this.pawnFace, "pawnFace");

            // Scribe_References.Look(ref this.pawn, "pawn");
            Scribe_Values.Look(ref this.IsChild, "isChild");
            Scribe_Values.Look(ref this.DontRender, "dontrender");
            Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");
        }

        public Color HairColor;

        public BeardDef BeardDef;

        public BrowDef BrowDef;

        public EyeDef EyeDef;

        public HumanMouthGraphics mouthgraphic;

        public bool IgnoreRenderer;

        public int rotationInt;

        private PawnHeadRotator headRotator;


        public PawnHeadRotator HeadRotator => this.headRotator;

        /// <summary>
        ///     Basic pawn initialization.
        /// </summary>
        /// <returns>Success if all initialized.</returns>
        public bool SetHeadType([CanBeNull] Pawn p)
        {
            if (p == null)
            {
                return false;
            }

            this.pawn = p;

            // PrepC removes the faction - fix
            this.pawnFaction = this.pawn.Faction ?? Faction.OfPlayer;

            if (this.pawnFace == null)
            {
                this.SetPawnFace(new PawnFace(this.pawn, this.pawnFaction.def));

                // check for pre-0.17.3 pawns
                if (this.EyeDef != null)
                {
                    this.PawnFace.EyeDef = this.EyeDef;
                    this.PawnFace.BrowDef = this.BrowDef;
                    this.PawnFace.HairColor = this.HairColor;
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

            this.isMasochist = this.pawn.story.traits.HasTrait(TraitDef.Named("Masochist"));
            this.mouthgraphic = new HumanMouthGraphics(this.pawn);
            this.flasher = this.pawn.Drawer.renderer.graphics.flasher;
            this.eyeWiggler = new PawnEyeWiggler(this.pawn);
            this.headRotator = new PawnHeadRotator(this.pawn);

            // this.headWiggler = new PawnHeadWiggler(this.pawn);

            // ReSharper disable once PossibleNullReferenceException
            this.ResetBoolsAndPaths();

            if (Controller.settings.ShowExtraParts)
            {
                this.CheckForAddedOrMissingParts();
            }

            this.SetHeadOffsets();
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

        private void CheckFemaleCrownType()
        {
            switch (this.PawnCrownType)
            {
                case CrownType.Average:
                    this.CheckFemaleCrownTypeAverage();
                    break;

                case CrownType.Narrow:
                    this.CheckFemaleCrownTypeNarrow();
                    break;
            }
        }

        private void CheckFemaleCrownTypeAverage()
        {
            switch (this.PawnHeadType)
            {
                case HeadType.Normal:
                    this.FullHeadType = FullHead.FemaleAverageNormal;
                    break;

                case HeadType.Pointy:
                    this.FullHeadType = FullHead.FemaleAveragePointy;
                    break;

                case HeadType.Wide:
                    this.FullHeadType = FullHead.FemaleAverageWide;
                    break;
            }
        }

        private void CheckFemaleCrownTypeNarrow()
        {
            switch (this.PawnHeadType)
            {
                case HeadType.Normal:
                    this.FullHeadType = FullHead.FemaleNarrowNormal;
                    break;

                case HeadType.Pointy:
                    this.FullHeadType = FullHead.FemaleNarrowPointy;
                    break;

                case HeadType.Wide:
                    this.FullHeadType = FullHead.FemaleNarrowWide;
                    break;
            }
        }

        private void CheckMaleCrownType()
        {
            switch (this.PawnCrownType)
            {
                case CrownType.Average:
                    this.CheckMaleCrownTypeAverage();
                    break;

                case CrownType.Narrow:
                    this.CheckMaleCrownTypeNarrow();
                    break;
            }
        }

        private void CheckMaleCrownTypeAverage()
        {
            switch (this.PawnHeadType)
            {
                case HeadType.Normal:
                    this.FullHeadType = FullHead.MaleAverageNormal;
                    break;

                case HeadType.Pointy:
                    this.FullHeadType = FullHead.MaleAveragePointy;
                    break;

                case HeadType.Wide:
                    this.FullHeadType = FullHead.MaleAverageWide;
                    break;
            }
        }

        private void CheckMaleCrownTypeNarrow()
        {
            switch (this.PawnHeadType)
            {
                case HeadType.Normal:
                    this.FullHeadType = FullHead.MaleNarrowNormal;
                    break;

                case HeadType.Pointy:
                    this.FullHeadType = FullHead.MaleNarrowPointy;
                    break;

                case HeadType.Wide:
                    this.FullHeadType = FullHead.MaleNarrowWide;
                    break;
            }
        }

        private void CheckPart([NotNull] List<BodyPartRecord> body, [NotNull] Hediff hediff)
        {
            BodyPartRecord leftEye = body.Find(x => x?.def == BodyPartDefOf.LeftEye);
            BodyPartRecord rightEye = body.Find(x => x?.def == BodyPartDefOf.RightEye);
            BodyPartRecord jaw = body.Find(x => x?.def == BodyPartDefOf.Jaw);
            AddedBodyPartProps addedPartProps = hediff.def?.addedPartProps;

            if (addedPartProps != null)
            {
                if (hediff.def.defName != null && hediff.Part != null)
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
                        this.texPathMouth = "Mouth/Mouth_" + hediff.def.defName;
                    }
                }
            }

            if (hediff.def != HediffDefOf.MissingBodyPart)
            {
                return;
            }

            if (hediff.Part == leftEye)
            {
                this.texPathEyeLeft = this.EyeTexPath("Missing", Side.Left);
                this.eyeWiggler.EyeLeftCanBlink = false;
            }

            // ReSharper disable once InvertIf
            if (hediff.Part == rightEye)
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
            string mainBeardDefTexPath = "Beards/Beard_" + this.PawnHeadType + "_" +  this.PawnFace.BeardDef.texPath + "_"
                                         + this.PawnCrownType;

            string moustacheDefTexPath = this.PawnFace.MoustacheDef.texPath + "_" + this.PawnCrownType;

            string shavedPath = "Beards/Beard_" + BeardDefOf.Beard_Shaved.texPath;

            Color beardColor = this.PawnFace.BeardColor;
            Color tacheColor = this.PawnFace.BeardColor;

            if (this.PawnFace.MoustacheDef == MoustacheDefOf.Shaved)
            {
                // no error, only use the beard def shaved as texture
                moustacheDefTexPath = shavedPath;
                tacheColor = Color.white;
            }

            if (this.PawnFace.BeardDef == BeardDefOf.Beard_Shaved)
            {
                mainBeardDefTexPath = shavedPath;
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

        private void InitializeGraphicsEyePatches()
        {
            if (this.texPathEyeLeftPatch != null)
            {
                bool flag = ContentFinder<Texture2D>.Get(this.texPathEyeLeftPatch + "_front", false) != null;
                if (flag)
                {
                    this.faceGraphicPart.EyeLeftPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                                                                   this.texPathEyeLeftPatch,
                                                                   ShaderDatabase.Cutout,
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

            if (this.texPathEyeRightPatch != null)
            {
                bool flag2 = ContentFinder<Texture2D>.Get(this.texPathEyeRightPatch + "_front", false) != null;
                if (flag2)
                {
                    this.faceGraphicPart.EyeRightPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                                                                    this.texPathEyeRightPatch,
                                                                    ShaderDatabase.Cutout,
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
            if (this.texPathMouth != null)
            {
                bool flag = ContentFinder<Texture2D>.Get(this.texPathMouth + "_front", false) != null;
                if (flag)
                {
                    this.faceGraphicPart.JawGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                          this.texPathMouth,
                                                          ShaderDatabase.CutoutSkin,
                                                          Vector2.one,
                                                          Color.white) as Graphic_Multi_NaturalHeadParts;
                    this.hasNaturalMouth = false;

                    // all done, return
                    return;
                }

                // texture for added/extra part not found, log and default
                Log.Message(
                    "Facial Stuff: No texture for added part: " + this.texPathMouth + " - Graphic_Multi_NaturalHeadParts");
            }

            this.hasNaturalMouth = true;
            this.faceGraphicPart.MouthGraphic = this.mouthgraphic.HumanMouthGraphic[this.pawn.Dead || this.pawn.Downed ? 2 : 3].Graphic;
        }

        private void InitializeGraphicsWrinkles()
        {
            Color wrinkleColor = this.pawn.story.SkinColor * new Color(0.1f, 0.1f, 0.1f);

            wrinkleColor.a = this.PawnFace.wrinkles;

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
            this.texPathEyeLeftPatch = null;
            this.texPathEyeRightPatch = null;
            this.texPathMouth = null;

            EyeDef pawnFaceEyeDef = this.PawnFace.EyeDef;
            if (pawnFaceEyeDef != null)
            {
                this.texPathEyeRight = this.EyeTexPath(pawnFaceEyeDef.texPath, Side.Right);
                this.texPathEyeLeft = this.EyeTexPath(pawnFaceEyeDef.texPath, Side.Left);
            }

            this.texPathEyeRightClosed = this.EyeClosedTexPath(Side.Right);
            this.texPathEyeLeftClosed = this.EyeClosedTexPath(Side.Left);

            this.eyeWiggler.EyeLeftCanBlink = true;
            this.eyeWiggler.EyeRightCanBlink = true;

            this.texPathBrow = this.BrowTexPath(this.PawnFace.BrowDef);
        }

        private void SetHeadOffsets()
        {
            switch (this.pawn.gender)
            {
                case Gender.Male:
                    this.CheckMaleCrownType();
                    break;

                case Gender.Female:
                    this.CheckFemaleCrownType();
                    break;

                default:
                    this.FullHeadType = FullHead.MaleAverageNormal;
                    break;
            }
        }

        private bool isMasochist;

        private void SetMouthAccordingToMoodLevel()
        {

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

            #endregion Private Methods

        }
    }
}