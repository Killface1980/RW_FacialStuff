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

    public partial class CompFace : ThingComp
    {

        #region Public Fields

        public bool dontrender;
        public bool IgnoreRenderer;

        public Pawn Pawn;
        public PawnFace pawnFace;
        public int rotationInt;

        #endregion Public Fields

        #region Private Fields



        private FaceGraphicParts faceGraphicPart;
        private float factionMelanin;

        private DamageFlasher flasher;


        private float headTypeX;

        private float headTypeY;

        // private float blinkRate;
        // public PawnHeadWiggler headWiggler;
        // todo: make proper dead eyes
        private bool isLeftEyeSolid;

        private bool isOld;

        private bool isRightEyeSolid;



        private float mood = 0.5f;



        private string texPathBrow;

        private string texPathEyeLeft;

        private string texPathEyeLeftClosed;

        private string texPathEyeLeftPatch;

        private string texPathEyeRight;

        private string texPathEyeRightClosed;

        private string texPathEyeRightPatch;

        private string texPathMouth;


        private bool updated = true;

        #endregion Private Fields

        #region Public Constructors

        #endregion Public Constructors

        #region Public Properties

        // {
        // get => this.eyeDef;
        // set => this.eyeDef = value;
        // }
        public PawnEyeWiggler EyeWiggler { get; set; }

        public float FactionMelanin
        {
            get => this.factionMelanin;
            set => this.factionMelanin = value;
        }

        public FullHead FullHeadType { get; set; } = FullHead.Undefined;



        public bool HasEyePatchLeft { get; private set; }
        public bool HasEyePatchRight { get; private set; }
        public bool HasNaturalMouth { get; private set; } = true;



        public GraphicMeshSet MouthMeshSet => MeshPoolFS.HumanlikeMouthSet[(int)this.FullHeadType];
        public bool OldEnough { get; set; }
        public CrownType PawnCrownType
        {
            get
            {
                return this.Pawn.story.crownType;
            }
        }

        public HeadType PawnHeadType
        {
            get
            {
                if (this.Pawn.story.HeadGraphicPath.Contains("Normal"))
                {
                    this.pawnFace.PawnHeadType = HeadType.Normal;
                }

                if (this.Pawn.story.HeadGraphicPath.Contains("Pointy"))
                {
                    this.pawnFace.PawnHeadType = HeadType.Pointy;
                }

                if (this.Pawn.story.HeadGraphicPath.Contains("Wide"))
                {
                    this.pawnFace.PawnHeadType = HeadType.Wide;
                }

                return this.pawnFace.PawnHeadType;
            }
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
            if (!this.HasNaturalMouth || this.Pawn.gender == Gender.Female)
            {
                return null;
            }

            Material material = null;
            {
                material = this.faceGraphicPart.MainBeardGraphic.MatAt(facing);

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
            material = this.faceGraphicPart.BrowGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public string BrowTexPath(BrowDef browDef)
        {
            return "Brows/" + this.Pawn.gender + "/Brow_" + this.Pawn.gender + "_" + browDef.texPath;
        }

        public void CheckForAddedOrMissingParts()
        {
            this.Pawn = this.parent as Pawn;
            if (this.Pawn == null)
            {
                return;
            }

            List<BodyPartRecord> body = this.Pawn.RaceProps?.body?.AllParts;
            List<Hediff> hediffSetHediffs = this.Pawn.health?.hediffSet?.hediffs;
            if (hediffSetHediffs != null && body != null)
            {
                foreach (Hediff hediff in hediffSetHediffs)
                {
                    if (hediff == null || hediff.def == null || hediff.def.defName == null)
                    {
                        continue;
                    }

                    BodyPartRecord leftEye = body.Find(x => x?.def == BodyPartDefOf.LeftEye);
                    BodyPartRecord rightEye = body.Find(x => x?.def == BodyPartDefOf.RightEye);
                    BodyPartRecord jaw = body.Find(x => x?.def == BodyPartDefOf.Jaw);
                    AddedBodyPartProps addedPartProps = hediff.def?.addedPartProps;

                    if (addedPartProps != null)
                    {
                        if (hediff.def.defName != null)
                        {
                            if (hediff.Part == leftEye)
                            {
                                this.texPathEyeLeftPatch =
                                    "AddedParts/" + hediff.def.defName + "_Left" + "_" + this.PawnCrownType;
                                this.isLeftEyeSolid = addedPartProps.isSolid;
                            }

                            if (hediff.Part == rightEye)
                            {
                                this.texPathEyeRightPatch =
                                    "AddedParts/" + hediff.def.defName + "_Right" + "_" + this.PawnCrownType;
                                this.isRightEyeSolid = addedPartProps.isSolid;
                            }

                            if (hediff.Part == jaw)
                            {
                                this.texPathMouth = "Mouth/Mouth_" + hediff.def.defName;
                                this.HasNaturalMouth = false;
                            }
                        }
                    }

                    if (hediff.def == HediffDefOf.MissingBodyPart)
                    {
                        if (hediff.Part == leftEye)
                        {
                            this.texPathEyeLeft = this.EyeTexPath("Missing", Side.Left);
                            this.EyeWiggler.EyeLeftCanBlink = false;
                        }

                        if (hediff.Part == rightEye)
                        {
                            this.texPathEyeRight = this.EyeTexPath("Missing", Side.Right);
                            this.EyeWiggler.EyeRightCanBlink = false;
                        }
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
                material = this.faceGraphicPart.DeadEyeGraphic.MatAt(facing);
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                material = this.faceGraphicPart.DeadEyeGraphic.MatAt(facing);
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        /// <summary>
        ///     Sets the hair melanin for pawns. Checks for relatives by blood to be consistent within the family.
        /// </summary>

        // TODO: Remove or make usable
        // public void DefineSkinDNA()
        // {
        //     HairMelanin.SkinGenetics(this.Pawn, this, out this.factionMelanin);
        //     this.IsSkinDNAoptimized = true;
        // }

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

            Material material = this.faceGraphicPart.EyeLeftGraphic.MatAt(facing);

            if (!portrait)
            {
                if (Controller.settings.MakeThemBlink && this.EyeWiggler.EyeLeftCanBlink)
                {
                    if (this.EyeWiggler.IsAsleep || this.EyeLeftBlinkNow)
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

        public Material EyeLeftPatchMatAt(Rot4 facing)
        {
            Material material = this.faceGraphicPart.EyeLeftPatchGraphic.MatAt(facing);

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

            Material material = this.faceGraphicPart.EyeRightGraphic.MatAt(facing);

            if (!portrait)
            {
                if (Controller.settings.MakeThemBlink && this.EyeWiggler.EyeRightCanBlink)
                {
                    if (this.EyeWiggler.IsAsleep || this.EyeRightBlinkNow)
                    {
                        material = this.faceGraphicPart.EyeRightClosedGraphic.MatAt(facing);
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
            Material material = this.faceGraphicPart.EyeRightPatchGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public string EyeTexPath(string eyeDefPath, Side side)
        {
            string path = "Eyes/Eye_" + eyeDefPath + "_" + this.Pawn.gender + "_" + side;

            return path;
        }

        /// <summary>
        ///     Initializes Facial stuff graphics.
        /// </summary>
        /// <returns>True if all went well.</returns>
        public bool InitializeGraphics()
        {
            if (this.Pawn == null)
            {
                return false;
            }

            if (this.Pawn.Dead)
            {
            }

            this.InitializeGraphicsWrinkles();

            this.InitializeGraphicsBeard();

            this.faceGraphicPart.BrowGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
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
            if (!this.HasNaturalMouth || this.pawnFace.MoustacheDef == MoustacheDefOf.Shaved
                || this.pawnFace.MoustacheDef == null || this.Pawn.gender == Gender.Female)
            {
                return null;
            }

            Material material = null;
            material = this.faceGraphicPart.MoustacheGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public Material MouthMatAt(Rot4 facing, bool portrait, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            Material material = null;
            if (!Controller.settings.UseMouth || !this.pawnFace.DrawMouth)
            {
                return null;
            }

            if (this.Pawn.gender == Gender.Male && (!this.pawnFace.BeardDef.drawMouth
                                                        || this.pawnFace.MoustacheDef
                                                        != MoustacheDefOf.Shaved))
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
                    material = this.faceGraphicPart.MouthGraphic.MatAt(facing);
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    material = this.faceGraphicPart.MouthGraphic.MatAt(facing);
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

            if (this.Pawn == null || !this.Pawn.Spawned || this.Pawn.Dead || !this.pawnFace.IsOptimized
                || !this.OldEnough || this.dontrender)
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

            Scribe_Values.Look(ref this.updated, "updated");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                if (!updated)
                {
                    var pawn = this.parent as Pawn;
                    if (pawn != null)
                    {

                        this.pawnFace = new PawnFace
                        {
                            IsOptimized = true,
                        };
                        Scribe_Values.Look(ref this.pawnFace.HairColorOrg, "HairColorOrg");

                        Scribe_Defs.Look(ref this.pawnFace.EyeDef, "EyeDef");
                        Scribe_Defs.Look(ref this.pawnFace.BrowDef, "BrowDef");
                        Scribe_Defs.Look(ref this.pawnFace.WrinkleDef, "WrinkleDef");
                        Scribe_Defs.Look(ref this.pawnFace.BeardDef, "BeardDef");
                        Scribe_Defs.Look(ref this.pawnFace.MoustacheDef, "MoustacheDef");
                        Scribe_Values.Look(ref this.pawnFace.IsOptimized, "optimized");
                        Scribe_Values.Look(ref this.pawnFace.DrawMouth, "drawMouth");
                        Scribe_Values.Look(ref this.pawnFace.CrownType, "crownType");


                        Scribe_Values.Look(ref this.pawnFace.HasSameBeardColor, "sameBeardColor");

                        Scribe_Values.Look(ref this.pawnFace.EuMelanin, "euMelanin");
                        Scribe_Values.Look(ref this.pawnFace.BeardColor, "BeardColor");
                        Scribe_Values.Look(ref this.pawnFace.PheoMelanin, "pheoMelanin");
                        Scribe_Values.Look(ref this.pawnFace.EuMelanin, "melanin1");
                        Scribe_Values.Look(ref this.pawnFace.PheoMelanin, "melanin2");
                   //     Scribe_Values.Look(ref this.pawnFace.MelaninOrg, "MelaninOrg");
                        if (this.pawnFace.MoustacheDef == null)
                        {
                            this.pawnFace.MoustacheDef = MoustacheDefOf.Shaved;
                        }
                        updated = true;
                    }
                }

            Scribe_References.Look(ref this.Pawn, "Pawn");

            Scribe_Values.Look(ref this.dontrender, "dontrender");
            Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");

            Scribe_Deep.Look(ref this.pawnFace, "pawnFace");
        }

        public void ExposeFaceData()
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.loader.curXmlParent["PawnFace"] != null)
            {
                Scribe_Deep.Look(ref this.pawnFace, "PawnFace");
            }
        }

        /// <summary>
        ///     Basic pawn initialization.
        /// </summary>
        /// <returns>Success if all initialized.</returns>
        public bool SetHeadType()
        {
            this.Pawn = this.parent as Pawn;
            if (this.Pawn == null)
            {
                return false;
            }

            this.flasher = this.Pawn.Drawer.renderer.graphics.flasher;
            this.EyeWiggler = new PawnEyeWiggler(this.Pawn);

            if (!this.pawnFace.IsOptimized)
            {
                this.pawnFace = new PawnFace(this.Pawn);
            }

            // this.headWiggler = new PawnHeadWiggler(this.pawn);
            this.isOld = this.Pawn.ageTracker.AgeBiologicalYearsFloat >= 50f;

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
                    material = this.faceGraphicPart.WrinkleGraphic.MatAt(facing);
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    material = this.faceGraphicPart.RottingWrinkleGraphic.MatAt(facing);
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

        private string EyeClosedTexPath(Side side)
        {
            return this.EyeTexPath("Closed", side);
        }

        private void InitializeGraphicsBeard()
        {
            string mainBeardDefTexPath = this.pawnFace.BeardDef.texPath + "_" + this.PawnCrownType + "_"
                                         + this.PawnHeadType;

            if (this.pawnFace.MoustacheDef != null)
            {
                string moustacheDefTexPath;

                if (this.pawnFace.MoustacheDef == MoustacheDefOf.Shaved)
                {
                    moustacheDefTexPath = this.pawnFace.MoustacheDef.texPath;
                }
                else
                {
                    moustacheDefTexPath = this.pawnFace.MoustacheDef.texPath + "_" + this.PawnCrownType;
                }

                this.faceGraphicPart.MoustacheGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    moustacheDefTexPath,
                    ShaderDatabase.Transparent,
                    Vector2.one,
                    this.pawnFace.BeardColor);
            }

            if (this.pawnFace.BeardDef == BeardDefOf.Beard_Shaved)
            {
                mainBeardDefTexPath = this.pawnFace.BeardDef.texPath;
            }

            this.faceGraphicPart.MainBeardGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                mainBeardDefTexPath,
                ShaderDatabase.Transparent,
                Vector2.one,
                this.pawnFace.BeardColor);
        }

        private void InitializeGraphicsEyes()
        {
            this.faceGraphicPart.DeadEyeGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                "Eyes/Eyes_Dead",
                ShaderDatabase.Transparent,
                Vector2.one,
                Color.white);

            this.SetEyePatches();

            this.faceGraphicPart.EyeLeftGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                      this.texPathEyeLeft,
                                      ShaderDatabase.Transparent,
                                      Vector2.one,
                                      this.Pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.faceGraphicPart.EyeLeftClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                            this.texPathEyeLeftClosed,
                                            ShaderDatabase.Transparent,
                                            Vector2.one,
                                            Color.black) as Graphic_Multi_NaturalEyes;

            this.faceGraphicPart.EyeRightGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                       this.texPathEyeRight,
                                       ShaderDatabase.Transparent,
                                       Vector2.one,
                                       this.Pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.faceGraphicPart.EyeRightClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                             this.texPathEyeRightClosed,
                                             ShaderDatabase.Transparent,
                                             Vector2.one,
                                             Color.black) as Graphic_Multi_NaturalEyes;
        }

        private void InitializeGraphicsMouth()
        {
            if (!this.HasNaturalMouth)
            {
                this.faceGraphicPart.MouthGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                        this.texPathMouth,
                                        ShaderDatabase.Transparent,
                                        Vector2.one,
                                        Color.white) as Graphic_Multi_NaturalHeadParts;
            }
            else
            {
                this.faceGraphicPart.MouthGraphic = FacialGraphics.MouthGraphic03;
            }
        }

        private void InitializeGraphicsWrinkles()
        {
            Color wrinkleColor = Color.Lerp(
                this.Pawn.story.SkinColor,
                this.Pawn.story.SkinColor * this.Pawn.story.SkinColor,
                Mathf.InverseLerp(50f, 100f, this.Pawn.ageTracker.AgeBiologicalYearsFloat));

            this.faceGraphicPart.WrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.pawnFace.WrinkleDef.texPath + "_" + this.PawnCrownType + "_" + this.PawnHeadType,
                ShaderDatabase.Transparent,
                Vector2.one,
                wrinkleColor);

            this.faceGraphicPart.RottingWrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.pawnFace.WrinkleDef.texPath + "_" + this.PawnCrownType + "_" + this.PawnHeadType,
                ShaderDatabase.Transparent,
                Vector2.one,
                wrinkleColor * FacialGraphics.SkinRottingMultiplyColor);
        }

        private void ResetBoolsAndPaths()
        {
            this.HasEyePatchLeft = false;
            this.HasEyePatchRight = false;
            this.isLeftEyeSolid = false;
            this.isRightEyeSolid = false;

            this.texPathEyeLeftPatch = null;
            this.texPathEyeRightPatch = null;

            this.texPathEyeRight = this.EyeTexPath(this.pawnFace.EyeDef.texPath, Side.Right);
            this.texPathEyeRightClosed = this.EyeClosedTexPath(Side.Right);

            this.texPathEyeLeft = this.EyeTexPath(this.pawnFace.EyeDef.texPath, Side.Left);
            this.texPathEyeLeftClosed = this.EyeClosedTexPath(Side.Left);

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
            this.texPathBrow = this.BrowTexPath(this.pawnFace.BrowDef);
        }

        private void SetEyePatches()
        {
            if (this.texPathEyeLeftPatch != null)
            {
                this.faceGraphicPart.EyeLeftPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                                               this.texPathEyeLeftPatch,
                                               ShaderDatabase.Transparent,
                                               Vector2.one,
                                               Color.white) as Graphic_Multi_AddedHeadParts;
                if (this.faceGraphicPart.EyeLeftPatchGraphic != null)
                {
                    this.HasEyePatchLeft = true;
                }
            }

            if (this.texPathEyeRightPatch != null)
            {
                this.faceGraphicPart.EyeRightPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                                                this.texPathEyeRightPatch,
                                                ShaderDatabase.Transparent,
                                                Vector2.one,
                                                Color.white) as Graphic_Multi_AddedHeadParts;
                if (this.faceGraphicPart.EyeRightPatchGraphic != null)
                {
                    this.HasEyePatchRight = true;
                }
            }
        }

        private void SetHeadOffsets()
        {
            switch (this.Pawn.gender)
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
            this.mood = this.Pawn.needs?.mood?.CurInstantLevel ?? 0.5f;

            int mouthTextureIndexOfMood = HumanMouthGraphics.GetMouthTextureIndexOfMood(this.mood);

            this.faceGraphicPart.MouthGraphic = HumanMouthGraphics.HumanMouthGraphic[mouthTextureIndexOfMood].graphic;
        }

        #endregion Private Methods

    }
}