namespace FacialStuff
{
    using System.Collections.Generic;

    using FacialStuff.Defs;
    using FacialStuff.Enums;
    using FacialStuff.Genetics;
    using FacialStuff.Graphics_FS;
    using FacialStuff.Wiggler;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class CompFace : ThingComp
    {

        #region Public Fields

        public bool IgnoreRenderer;

        public int rotationInt;

        #endregion Public Fields

        #region Private Fields

        private Color beardColor = Color.clear;
        private BeardDef beardDef = BeardDefOf.Beard_Shaved;
        private BrowDef browDef;
        private Graphic browGraphic;
        private float cuticula;
        private Graphic deadEyeGraphic;
        private bool drawMouth = true;
        private float euMelanin;
        private EyeDef eyeDef;
        private Graphic_Multi_NaturalEyes eyeLeftClosedGraphic;
        private Graphic_Multi_NaturalEyes eyeLeftGraphic;
        private Graphic_Multi_AddedHeadParts eyeLeftPatchGraphic;
        private Graphic_Multi_NaturalEyes eyeRightClosedGraphic;
        private Graphic_Multi_NaturalEyes eyeRightGraphic;
        private Graphic_Multi_AddedHeadParts eyeRightPatchGraphic;
        private Pawn facePawn;
        private float factionMelanin;
        private DamageFlasher flasher;
        private Color hairColorOrg;
        private bool hasSameBeardColor;
        private float headTypeX;
        private float headTypeY;
        private bool isDNAoptimized;
        // private float blinkRate;
        // public PawnHeadWiggler headWiggler;
        // todo: make proper dead eyes
        private bool isLeftEyeSolid;

        private bool isOld;
        private bool isOptimized = false;
        private bool isRightEyeSolid;
        private bool isSkinDNAoptimized;
        private Graphic mainBeardGraphic;
        private float melaninOrg;
        private float mood = 0.5f;
        private MoustacheDef moustacheDef = MoustacheDefOf.Shaved;
        private Graphic moustacheGraphic;
        private Graphic_Multi_NaturalHeadParts mouthGraphic;
        private CrownType pawnCrownType = CrownType.Undefined;
        private HeadType pawnHeadType = HeadType.Undefined;
        private float pheoMelanin;
        private Graphic rottingWrinkleGraphic;
        private string skinColorHex;
        private string texPathBrow;
        private string texPathEyeLeft;
        private string texPathEyeLeftClosed;
        private string texPathEyeLeftPatch;
        private string texPathEyeRight;
        private string texPathEyeRightClosed;
        private string texPathEyeRightPatch;
        private string texPathMouth;
        private WrinkleDef wrinkleDef;
        private Graphic wrinkleGraphic;

        private bool hasEyePatchLeft;

        private bool hasEyePatchRight;

        private bool oldEnough;

        public bool OldEnough
        {
            get => oldEnough;
            set => this.oldEnough = value;
        }

        #endregion Private Fields

        #region Public Properties

        public Color BeardColor => this.beardColor;

        public BeardDef BeardDef
        {
            get => this.beardDef;
            set => this.beardDef = value;
        }

        public BrowDef BrowDef
        {
            get => this.browDef;
            set => this.browDef = value;
        }

        public float Cuticula => this.cuticula;
        public bool DrawMouth
        {
            get => this.drawMouth;
            set => this.drawMouth = value;
        }

        public float EuMelanin
        {
            get => this.euMelanin;
            set => this.euMelanin = value;
        }

        public EyeDef EyeDef
        {
            get => this.eyeDef;
            set => this.eyeDef = value;
        }

        public PawnEyeWiggler EyeWiggler { get; set; }
        public Pawn FacePawn
        {
            get
            {
                if (this.facePawn == null)
                {
                    this.facePawn = this.parent as Pawn;
                    this.flasher = this.FacePawn.Drawer.renderer.graphics.flasher;
                }

                return this.facePawn;
            }
        }

        public float FactionMelanin
        {
            get => this.factionMelanin;
            set => this.factionMelanin = value;
        }

        public FullHead FullHeadType { get; set; } = FullHead.Undefined;
        public Color HairColorOrg
        {
            get => this.hairColorOrg;
            set => this.hairColorOrg = value;
        }

        public bool HasEyePatchLeft => this.hasEyePatchLeft;


        public bool HasNaturalMouth { get; private set; } = true;

        public bool HasEyePatchRight => this.hasEyePatchRight;


        public bool HasSameBeardColor
        {
            get => this.hasSameBeardColor;
            set => this.hasSameBeardColor = value;
        }

        public bool IsDNAoptimized
        {
            get => this.isDNAoptimized;
            set => this.isDNAoptimized = value;
        }

        public bool IsOptimized => this.isOptimized;

        public bool IsSkinDNAoptimized
        {
            get => this.isSkinDNAoptimized;
            set => this.isSkinDNAoptimized = value;
        }

        public float MelaninOrg
        {
            get => this.melaninOrg;
            set => this.melaninOrg = value;
        }

        public MoustacheDef MoustacheDef
        {
            get => this.moustacheDef;
            set => this.moustacheDef = value;
        }

        public Graphic_Multi_NaturalHeadParts MouthGraphic => this.mouthGraphic;
        public GraphicMeshSet MouthMeshSet => MeshPoolFS.HumanlikeMouthSet[(int)this.FullHeadType];

        public CrownType PawnCrownType
        {
            get
            {
                {
                    if (this.FacePawn.story.HeadGraphicPath.Contains("Narrow"))
                    {
                        this.pawnCrownType = CrownType.Narrow;
                        this.FacePawn.story.crownType = CrownType.Narrow;
                    }
                    else
                    {
                        this.pawnCrownType = CrownType.Average;
                        this.FacePawn.story.crownType = CrownType.Average;

                    }
                }
                return this.pawnCrownType;

            }
        }

        public HeadType PawnHeadType
        {
            get
            {
                {
                    if (this.FacePawn.story.HeadGraphicPath.Contains("Normal"))
                    {
                        this.pawnHeadType = HeadType.Normal;
                    }

                    if (this.FacePawn.story.HeadGraphicPath.Contains("Pointy"))
                    {
                        this.pawnHeadType = HeadType.Pointy;
                    }

                    if (this.FacePawn.story.HeadGraphicPath.Contains("Wide"))
                    {
                        this.pawnHeadType = HeadType.Wide;
                    }
                }

                return this.pawnHeadType;
            }
        }

        public float PheoMelanin
        {
            get => this.pheoMelanin;
            set => this.pheoMelanin = value;
        }

        #endregion Public Properties

        #region Private Properties

        private bool EyeLeftBlinkNow
        {
            get
            {
                bool blinkNow = Find.TickManager.TicksGame >= this.EyeWiggler.NextBlink + this.EyeWiggler.JitterLeft;
                return blinkNow;
            }
        }

        private bool EyeRightBlinkNow
        {
            get
            {
                bool blinkNow = Find.TickManager.TicksGame >= this.EyeWiggler.NextBlink + this.EyeWiggler.JitterRight;
                return blinkNow;
            }
        }

        #endregion Private Properties

        #region Public Methods

        public Vector3 BaseMouthOffsetAt(Rot4 rotation)
        {
#if develop

            var male = pawn.gender == Gender.Male;

            if (crownType == CrownType.Average)
            {
                switch (headType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            headTypeX = Controller.settings.MaleAverageNormalOffsetX;
                            headTypeY = Controller.settings.MaleAverageNormalOffsetY;
                        }
                        else
                        {
                            headTypeX = Controller.settings.FemaleAverageNormalOffsetX;
                            headTypeY = Controller.settings.FemaleAverageNormalOffsetY;
                        }
                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            headTypeX = Controller.settings.MaleAveragePointyOffsetX;
                            headTypeY = Controller.settings.MaleAveragePointyOffsetY;
                        }
                        else
                        {
                            headTypeX = Controller.settings.FemaleAveragePointyOffsetX;
                            headTypeY = Controller.settings.FemaleAveragePointyOffsetY;
                        }
                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            headTypeX = Controller.settings.MaleAverageWideOffsetX;
                            headTypeY = Controller.settings.MaleAverageWideOffsetY;
                        }
                        else
                        {
                            headTypeX = Controller.settings.FemaleAverageWideOffsetX;
                            headTypeY = Controller.settings.FemaleAverageWideOffsetY;
                        }
                        break;
                }
            }
            else
            {
                switch (headType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            headTypeX = Controller.settings.MaleNarrowNormalOffsetX;
                            headTypeY = Controller.settings.MaleNarrowNormalOffsetY;
                        }
                        else
                        {
                            headTypeX = Controller.settings.FemaleNarrowNormalOffsetX;
                            headTypeY = Controller.settings.FemaleNarrowNormalOffsetY;
                        }
                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            headTypeX = Controller.settings.MaleNarrowPointyOffsetX;
                            headTypeY = Controller.settings.MaleNarrowPointyOffsetY;
                        }
                        else
                        {
                            headTypeX = Controller.settings.FemaleNarrowPointyOffsetX;
                            headTypeY = Controller.settings.FemaleNarrowPointyOffsetY;
                        }
                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            headTypeX = Controller.settings.MaleNarrowWideOffsetX;
                            headTypeY = Controller.settings.MaleNarrowWideOffsetY;
                        }
                        else
                        {
                            headTypeX = Controller.settings.FemaleNarrowWideOffsetX;
                            headTypeY = Controller.settings.FemaleNarrowWideOffsetY;
                        }
                        break;
                }
            }

#else
#endif
            switch (rotation.AsInt)
            {
                case 1: return new Vector3(this.headTypeX, 0f, -this.headTypeY);
                case 2: return new Vector3(0, 0f, -this.headTypeY);
                case 3: return new Vector3(-this.headTypeX, 0f, -this.headTypeY);
                default: return Vector3.zero;
            }
        }

        public Material BeardMatAt(Rot4 facing)
        {
            if (!this.HasNaturalMouth || this.FacePawn.gender == Gender.Female)
            {
                return null;
            }

            Material material = null;
            {
                material = this.mainBeardGraphic.MatAt(facing);

                if (material != null)
                {
                    material = this.flasher.GetDamagedMat(material);
                }
            }

            return material;
        }

        public Material BrowMatAt(Rot4 facing)
        {
            Material material;
            material = this.browGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public string BrowTexPath(BrowDef browDef)
        {
            return "Brows/" + this.FacePawn.gender + "/Brow_" + this.FacePawn.gender + "_" + browDef.texPath;
        }

        public void CheckForAddedOrMissingParts()
        {
            List<BodyPartRecord> body = this.FacePawn.RaceProps.body.AllParts;
            foreach (Hediff hediff in this.FacePawn.health.hediffSet.hediffs)
            {
                BodyPartRecord leftEye = body.Find(x => x.def == BodyPartDefOf.LeftEye);
                BodyPartRecord rightEye = body.Find(x => x.def == BodyPartDefOf.RightEye);
                BodyPartRecord jaw = body.Find(x => x.def == BodyPartDefOf.Jaw);
                AddedBodyPartProps addedPartProps = hediff.def.addedPartProps;

                if (addedPartProps != null)
                {
                    if (hediff.Part == leftEye)
                    {
                        this.texPathEyeLeftPatch = "AddedParts/" + hediff.def.defName + "_Left" + "_"
                                                   + this.PawnCrownType;
                        this.isLeftEyeSolid = addedPartProps.isSolid;
                    }

                    if (hediff.Part == rightEye)
                    {
                        this.texPathEyeRightPatch = "AddedParts/" + hediff.def.defName + "_Right" + "_"
                                                    + this.PawnCrownType;
                        this.isRightEyeSolid = addedPartProps.isSolid;
                    }

                    if (hediff.Part == jaw)
                    {
                        this.texPathMouth = "Mouth/Mouth_" + hediff.def.defName;
                        this.HasNaturalMouth = false;
                    }
                }

                if (hediff.def == HediffDefOf.MissingBodyPart)
                {
                    if (hediff.Part == leftEye)
                    {
                        this.texPathEyeLeft = this.EyeTexPath("Missing", Enums.Side.Left);
                        this.EyeWiggler.EyeLeftCanBlink = false;
                    }

                    if (hediff.Part == rightEye)
                    {
                        this.texPathEyeRight = this.EyeTexPath("Missing", Enums.Side.Right);
                        this.EyeWiggler.EyeRightCanBlink = false;
                    }
                }
            }
        }

        // Deactivated for now
        public Material DeadEyeMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            Material material = null;
            if (bodyCondition == RotDrawMode.Fresh)
            {
                material = this.deadEyeGraphic.MatAt(facing);
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                material = this.deadEyeGraphic.MatAt(facing);
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        /// <summary>
        ///     Basic face definition. Nothing works without this.
        /// </summary>
        public void DefineFace()
        {
            // Log.Message("FS " + this.pawn);
            if (this.FacePawn == null)
            {
                // Log.Message("Pawn is null, returning. ");
                return;
            }

            // Log.Message("Choosing eyes... ");
            FactionDef faction = this.FacePawn.Faction?.def;

            this.EyeDef = PawnFaceChooser.RandomEyeDefFor(this.FacePawn, faction);

            // Log.Message(EyeDef.defName);
            this.BrowDef = PawnFaceChooser.RandomBrowDefFor(this.FacePawn, faction);

            // Log.Message(BrowDef.defName);
            this.wrinkleDef = PawnFaceChooser.AssignWrinkleDefFor(this.FacePawn);

            // Log.Message(WrinkleDef.defName);

            // this.MouthDef = PawnFaceChooser.RandomMouthDefFor(this.pawn, this.pawn.Faction.def);
            PawnFaceChooser.RandomBeardDefFor(this.FacePawn, faction, out this.beardDef, out this.moustacheDef);

            // Log.Message(BeardDef.defName);
            this.HairColorOrg = this.FacePawn.story.hairColor;

            this.HasSameBeardColor = true;
            this.beardColor = FacialGraphics.DarkerBeardColor(this.FacePawn.story.hairColor);

            this.isOptimized = true;
        }

        /// <summary>
        ///     Sets the hair melanin for pawns. Checks for relatives by blood to be consistent within the family.
        /// </summary>
        public void DefineHairDNA()
        {
            HairMelanin.GenerateHairMelaninAndCuticula(
                this.FacePawn,
                this,
                out this.euMelanin,
                out this.pheoMelanin,
                out this.cuticula);
            this.IsDNAoptimized = true;
        }

        // TODO: Remove or make usable
        public void DefineSkinDNA()
        {
            HairMelanin.SkinGenetics(this.FacePawn, this, out this.factionMelanin);
            this.IsSkinDNAoptimized = true;
        }

        [CanBeNull]
        public Material EyeLeftMatAt(Rot4 facing, bool portrait)
        {
            if (this.HasEyePatchLeft)
            {
                return null;
            }

            if (!this.HasEyePatchLeft && this.isLeftEyeSolid)
            {
                return null;
            }

            Material material = this.eyeLeftGraphic.MatAt(facing);

            if (!portrait)
            {
                if (Controller.settings.MakeThemBlink && this.EyeWiggler.EyeLeftCanBlink)
                {
                    if (this.EyeWiggler.IsAsleep || this.EyeLeftBlinkNow)
                    {
                        material = this.eyeLeftClosedGraphic.MatAt(facing);
                    }
                }
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public Material EyeLeftPatchMatAt(Rot4 facing)
        {
            Material material = this.eyeLeftPatchGraphic.MatAt(facing);

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

            if (!this.HasEyePatchRight && this.isRightEyeSolid)
            {
                return null;
            }

            Material material = this.eyeRightGraphic.MatAt(facing);

            if (!portrait)
            {
                if (Controller.settings.MakeThemBlink && this.EyeWiggler.EyeRightCanBlink)
                {
                    if (this.EyeWiggler.IsAsleep || this.EyeRightBlinkNow)
                    {
                        material = this.eyeRightClosedGraphic.MatAt(facing);
                    }
                }
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public Material EyeRightPatchMatAt(Rot4 facing)
        {
            Material material = this.eyeRightPatchGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public string EyeTexPath(string eyeDefPath, Enums.Side side)
        {
            string path = "Eyes/Eye_" + eyeDefPath + "_" + this.FacePawn.gender + "_" + side;

            return path;
        }

        /// <summary>
        ///     Initializes Facial stuff graphics.
        /// </summary>
        /// <returns>True if all went well.</returns>
        public bool InitializeGraphics()
        {
            if (this.FacePawn == null)
            {
                return false;
            }

            if (this.FacePawn.Dead)
            {
            }

            this.InitializeGraphicsWrinkles();

            this.InitializeGraphicsBeard();

            this.browGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.texPathBrow,
                ShaderDatabase.Transparent,
                Vector2.one,
                Color.black);

            this.InitializeGraphicsMouth();

            this.InitializeGraphicsEyes();

            return true;
        }

        public Material MoustacheMatAt(Rot4 facing)
        {
            if (!this.HasNaturalMouth || this.MoustacheDef == MoustacheDefOf.Shaved || this.MoustacheDef == null || this.FacePawn.gender == Gender.Female)
            {
                return null;
            }

            Material material = null;
            material = this.moustacheGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public Material MouthMatAt(Rot4 facing, bool portrait, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            Material material = null;
            if (!Controller.settings.UseMouth || !this.DrawMouth)
            {
                return null;
            }
            if (this.FacePawn.gender == Gender.Male && (!this.BeardDef.drawMouth || this.MoustacheDef != MoustacheDefOf.Shaved))
            {
                return null;
            }
            if (portrait)
            {
                material = FacialGraphics.MouthGraphic03.MatAt(facing);
            }
            else
            {
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    material = this.MouthGraphic.MatAt(facing);
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    material = this.MouthGraphic.MatAt(facing);
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
            if (!this.FacePawn.Spawned || this.FacePawn.Dead || !this.IsOptimized || !this.OldEnough)
            {
                return;
            }

            if (this.HasNaturalMouth)
            {
                if (Find.TickManager.TicksGame > this.EyeWiggler.NextBlinkEnd)
                {
                    this.SetMouthAccordingToMoodLevel();
                }
            }

            // todo: head wiggler? move eyes to eyewiggler
            // this.headWiggler.WigglerTick();
            this.EyeWiggler.WigglerTick();

        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Defs.Look(ref this.eyeDef, "EyeDef");
            Scribe_Defs.Look(ref this.browDef, "BrowDef");

            // Scribe_Defs.Look(ref this.MouthDef, "MouthDef");
            Scribe_Defs.Look(ref this.wrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref this.beardDef, "BeardDef");
            Scribe_Defs.Look(ref this.moustacheDef, "MoustacheDef");
            Scribe_Values.Look(ref this.isOptimized, "optimized");
            Scribe_Values.Look(ref this.drawMouth, "drawMouth");

            Scribe_Values.Look(ref this.pawnHeadType, "headType");
            Scribe_Values.Look(ref this.pawnCrownType, "crownType");
            Scribe_Values.Look(ref this.skinColorHex, "SkinColorHex");
            Scribe_Values.Look(ref this.hairColorOrg, "HairColorOrg");
            Scribe_Values.Look(ref this.beardColor, "BeardColor");
            Scribe_Values.Look(ref this.hasSameBeardColor, "sameBeardColor");

            Scribe_Values.Look(ref this.euMelanin, "euMelanin");
            Scribe_Values.Look(ref this.pheoMelanin, "pheoMelanin");

            Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");
            Scribe_Values.Look(ref this.isSkinDNAoptimized, "IsSkinDNAoptimized");
            Scribe_Values.Look(ref this.isDNAoptimized, "DNAoptimized");

            Scribe_Values.Look(ref this.melaninOrg, "MelaninOrg");

            // TODO: Old values for updating
            Scribe_Values.Look(ref this.euMelanin, "melanin1");
            Scribe_Values.Look(ref this.pheoMelanin, "melanin2");
            if (this.moustacheDef == null)
            {
                this.moustacheDef = MoustacheDefOf.Shaved;
            }
        }

        /// <summary>
        ///     Basic pawn initialization.
        /// </summary>
        /// <returns>Success if all initialized.</returns>
        public bool SetHeadType()
        {
            if (this.FacePawn == null)
            {
                return false;
            }
            this.EyeWiggler = new PawnEyeWiggler(this.FacePawn);

            // this.headWiggler = new PawnHeadWiggler(this.pawn);
            this.isOld = this.FacePawn.ageTracker.AgeBiologicalYearsFloat >= 50f;

            this.ResetBoolsAndPaths();

            if (Controller.settings.ShowExtraParts)
            {
                this.CheckForAddedOrMissingParts();
            }
            this.SetHeadOffsets();

            return true;
        }

        public Material WrinkleMatAt(Rot4 facing, RotDrawMode bodyCondition)
        {
            Material material = null;
            if (this.isOld && Controller.settings.UseWrinkles)
            {
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    material = this.wrinkleGraphic.MatAt(facing);
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    material = this.rottingWrinkleGraphic.MatAt(facing);
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
                    this.headTypeX = Controller.settings.FemaleAverageNormalOffsetX;
                    this.headTypeY = Controller.settings.FemaleAverageNormalOffsetY;
                    break;

                case HeadType.Pointy:
                    this.FullHeadType = FullHead.FemaleAveragePointy;
                    this.headTypeX = Controller.settings.FemaleAveragePointyOffsetX;
                    this.headTypeY = Controller.settings.FemaleAveragePointyOffsetY;
                    break;

                case HeadType.Wide:
                    this.FullHeadType = FullHead.FemaleAverageWide;
                    this.headTypeX = Controller.settings.FemaleAverageWideOffsetX;
                    this.headTypeY = Controller.settings.FemaleAverageWideOffsetY;
                    break;
            }
        }

        private void CheckFemaleCrownTypeNarrow()
        {
            switch (this.PawnHeadType)
            {
                case HeadType.Normal:
                    this.FullHeadType = FullHead.FemaleNarrowNormal;
                    this.headTypeX = Controller.settings.FemaleNarrowNormalOffsetX;
                    this.headTypeY = Controller.settings.FemaleNarrowNormalOffsetY;
                    break;

                case HeadType.Pointy:
                    this.FullHeadType = FullHead.FemaleNarrowPointy;
                    this.headTypeX = Controller.settings.FemaleNarrowPointyOffsetX;
                    this.headTypeY = Controller.settings.FemaleNarrowPointyOffsetY;
                    break;

                case HeadType.Wide:
                    this.FullHeadType = FullHead.FemaleNarrowWide;
                    this.headTypeX = Controller.settings.FemaleNarrowWideOffsetX;
                    this.headTypeY = Controller.settings.FemaleNarrowWideOffsetY;
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
                    this.headTypeX = Controller.settings.MaleAverageNormalOffsetX;
                    this.headTypeY = Controller.settings.MaleAverageNormalOffsetY;
                    break;

                case HeadType.Pointy:
                    this.FullHeadType = FullHead.MaleAveragePointy;
                    this.headTypeX = Controller.settings.MaleAveragePointyOffsetX;
                    this.headTypeY = Controller.settings.MaleAveragePointyOffsetY;
                    break;

                case HeadType.Wide:
                    this.FullHeadType = FullHead.MaleAverageWide;
                    this.headTypeX = Controller.settings.MaleAverageWideOffsetX;
                    this.headTypeY = Controller.settings.MaleAverageWideOffsetY;
                    break;
            }
        }

        private void CheckMaleCrownTypeNarrow()
        {
            switch (this.PawnHeadType)
            {
                case HeadType.Normal:
                    this.FullHeadType = FullHead.MaleNarrowNormal;
                    this.headTypeX = Controller.settings.MaleNarrowNormalOffsetX;
                    this.headTypeY = Controller.settings.MaleNarrowNormalOffsetY;
                    break;

                case HeadType.Pointy:
                    this.FullHeadType = FullHead.MaleNarrowPointy;
                    this.headTypeX = Controller.settings.MaleNarrowPointyOffsetX;
                    this.headTypeY = Controller.settings.MaleNarrowPointyOffsetY;
                    break;

                case HeadType.Wide:
                    this.FullHeadType = FullHead.MaleNarrowWide;
                    this.headTypeX = Controller.settings.MaleNarrowWideOffsetX;
                    this.headTypeY = Controller.settings.MaleNarrowWideOffsetY;
                    break;
            }
        }

        private string EyeClosedTexPath(Enums.Side side)
        {
            return this.EyeTexPath("Closed", side);
        }

        private void InitializeGraphicsBeard()
        {
            string mainBeardDefTexPath = this.BeardDef.texPath + "_" + this.PawnCrownType + "_" + this.PawnHeadType;

            if (this.MoustacheDef != null)
            {
                string moustacheDefTexPath;

                if (this.MoustacheDef == MoustacheDefOf.Shaved)
                {
                    moustacheDefTexPath = this.MoustacheDef.texPath;
                }
                else
                {
                    moustacheDefTexPath = this.MoustacheDef.texPath + "_" + this.PawnCrownType;
                }

                this.moustacheGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    moustacheDefTexPath,
                    ShaderDatabase.Transparent,
                    Vector2.one,
                    this.BeardColor);
            }

            if (this.BeardDef == BeardDefOf.Beard_Shaved)
            {
                mainBeardDefTexPath = this.BeardDef.texPath;
            }

            this.mainBeardGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                mainBeardDefTexPath,
                ShaderDatabase.Transparent,
                Vector2.one,
                this.BeardColor);
        }

        private void InitializeGraphicsEyes()
        {
            this.deadEyeGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                "Eyes/Eyes_Dead",
                ShaderDatabase.Transparent,
                Vector2.one,
                Color.white);

            this.SetEyePatches();

            this.eyeLeftGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                      this.texPathEyeLeft,
                                      ShaderDatabase.Transparent,
                                      Vector2.one,
                                      this.FacePawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.eyeLeftClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                            this.texPathEyeLeftClosed,
                                            ShaderDatabase.Transparent,
                                            Vector2.one,
                                            Color.black) as Graphic_Multi_NaturalEyes;

            this.eyeRightGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                       this.texPathEyeRight,
                                       ShaderDatabase.Transparent,
                                       Vector2.one,
                                       this.FacePawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.eyeRightClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                             this.texPathEyeRightClosed,
                                             ShaderDatabase.Transparent,
                                             Vector2.one,
                                             Color.black) as Graphic_Multi_NaturalEyes;
        }

        private void InitializeGraphicsMouth()
        {
            if (!this.HasNaturalMouth)
            {
                this.mouthGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                        this.texPathMouth,
                                        ShaderDatabase.Transparent,
                                        Vector2.one,
                                        Color.white) as Graphic_Multi_NaturalHeadParts;
            }
            else
            {
                this.mouthGraphic = FacialGraphics.MouthGraphic03;
            }
        }

        private void InitializeGraphicsWrinkles()
        {
            Color wrinkleColor = Color.Lerp(
                this.FacePawn.story.SkinColor,
                this.FacePawn.story.SkinColor * this.FacePawn.story.SkinColor,
                Mathf.InverseLerp(50f, 100f, this.FacePawn.ageTracker.AgeBiologicalYearsFloat));

            this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.wrinkleDef.texPath + "_" + this.PawnCrownType + "_" + this.PawnHeadType,
                ShaderDatabase.Transparent,
                Vector2.one,
                wrinkleColor);

            this.rottingWrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.wrinkleDef.texPath + "_" + this.PawnCrownType + "_" + this.PawnHeadType,
                ShaderDatabase.Transparent,
                Vector2.one,
                wrinkleColor * FacialGraphics.SkinRottingMultiplyColor);
        }

        private void ResetBoolsAndPaths()
        {
            this.hasEyePatchLeft = false;
            this.hasEyePatchRight = false;
            this.isLeftEyeSolid = false;
            this.isRightEyeSolid = false;

            this.texPathEyeLeftPatch = null;
            this.texPathEyeRightPatch = null;

            this.texPathEyeRight = this.EyeTexPath(this.EyeDef.texPath, Enums.Side.Right);
            this.texPathEyeRightClosed = this.EyeClosedTexPath(Enums.Side.Right);

            this.texPathEyeLeft = this.EyeTexPath(this.EyeDef.texPath, Enums.Side.Left);
            this.texPathEyeLeftClosed = this.EyeClosedTexPath(Enums.Side.Left);

            this.EyeWiggler.EyeLeftCanBlink = true;
            this.EyeWiggler.EyeRightCanBlink = true;

            // #if develop
            // {
            // HasNaturalMouth = false;
            // this.texPathMouth = "Mouth/Mouth_BionicJaw";
            // }
            // #else
            // #endif
            this.HasNaturalMouth = true;
            this.texPathBrow = this.BrowTexPath(this.BrowDef);
        }

        private void SetEyePatches()
        {
            if (this.texPathEyeLeftPatch != null)
            {
                this.eyeLeftPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                                               this.texPathEyeLeftPatch,
                                               ShaderDatabase.Transparent,
                                               Vector2.one,
                                               Color.white) as Graphic_Multi_AddedHeadParts;
                if (this.eyeLeftPatchGraphic != null)
                {
                    this.hasEyePatchLeft = true;
                }
            }

            if (this.texPathEyeRightPatch != null)
            {
                this.eyeRightPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                                                this.texPathEyeRightPatch,
                                                ShaderDatabase.Transparent,
                                                Vector2.one,
                                                Color.white) as Graphic_Multi_AddedHeadParts;
                if (this.eyeRightPatchGraphic != null)
                {
                    this.hasEyePatchRight = true;
                }
            }
        }

        private void SetHeadOffsets()
        {
            switch (this.FacePawn.gender)
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

        private void SetMouthAccordingToMoodLevel()
        {
            this.mood = this.FacePawn.needs?.mood?.CurInstantLevel ?? 0.5f;

            int mouthTextureIndexOfMood = HumanMouthGraphics.GetMouthTextureIndexOfMood(this.mood);

            this.mouthGraphic = HumanMouthGraphics.HumanMouthGraphic[mouthTextureIndexOfMood].graphic;
        }

        #endregion Private Methods

        public void SetBeardColor(Color newBeardColour)
        {
            this.beardColor = newBeardColour;
        }
    }
}