// ReSharper disable StyleCop.SA1401

namespace FacialStuff
{
    using System.Collections.Generic;

    using FacialStuff.Defs;
    using FacialStuff.Detouring;
    using FacialStuff.Genetics;
    using FacialStuff.Wiggler;

    using RimWorld;
    using RimWorld.Planet;

    using RW_FacialStuff;

    using UnityEngine;

    using Verse;

    public class CompFace : ThingComp
    {

        #region Fields

        public Color BeardColor = Color.clear;

        public BeardDef BeardDef = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");

        public BrowDef BrowDef;

        public CrownType crownType;

        public bool drawMouth = true;

        public EyeDef EyeDef;

        public PawnEyeWiggler eyeWiggler;

        public FullHead fullHeadType;

        public Color HairColorOrg;

        public bool HasLeftEyePatch;

        public bool HasNaturalMouth = true;

        public bool HasRightEyePatch;

        public bool HasSameBeardColor;

        public HeadType headType;

        public bool IgnoreRenderer;

        public bool IsDNAoptimized = false;

        public bool IsOptimized;

        public float melanin1 = 0f;

        public float melanin2 = 0f;

        public Graphic_Multi_NaturalHeadParts MouthGraphic;

        public Pawn pawn;

        public int rotationInt;

        public WrinkleDef WrinkleDef;

        private CellRect _viewRect;

        private Graphic beardGraphic;

        private Graphic browGraphic;

        private Graphic deadEyeGraphic;

        private Graphic_Multi_NaturalEyes eyeLeftClosedGraphic;

        private Graphic_Multi_NaturalEyes eyeLeftGraphic;

        private Graphic_Multi_AddedHeadParts eyeLeftPatchGraphic;

        private Graphic_Multi_NaturalEyes eyeRightClosedGraphic;

        private Graphic_Multi_NaturalEyes eyeRightGraphic;

        private Graphic_Multi_AddedHeadParts eyeRightPatchGraphic;

        private float headTypeX;

        private float headTypeY;

        // private float blinkRate;
        // public PawnHeadWiggler headWiggler;
        // todo: make proper dead eyes
        private bool IsLeftEyeSolid;
        private bool IsOld;
        private bool IsRightEyeSolid;
        private float mood = 0.5f;
        private string texPathMouth;
        private Graphic rottingWrinkleGraphic;
        private string SkinColorHex;
        private string texPathBrow;
        private string texPathEyeLeft;
        private string texPathEyeLeftClosed;
        private string texPathEyeLeftPatch;
        private string texPathEyeRight;
        private string texPathEyeRightClosed;
        private string texPathEyeRightPatch;
        private Graphic wrinkleGraphic;

        #endregion Fields

        #region Properties

        public GraphicMeshSet MouthMeshSet => MeshPoolFs.HumanlikeMouthSet[(int)this.fullHeadType];

        #endregion Properties

        #region Methods

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
                            headTypeX = FS_Settings.MaleAverageNormalOffsetX;
                            headTypeY = FS_Settings.MaleAverageNormalOffsetY;
                        }
                        else
                        {
                            headTypeX = FS_Settings.FemaleAverageNormalOffsetX;
                            headTypeY = FS_Settings.FemaleAverageNormalOffsetY;
                        }
                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            headTypeX = FS_Settings.MaleAveragePointyOffsetX;
                            headTypeY = FS_Settings.MaleAveragePointyOffsetY;
                        }
                        else
                        {
                            headTypeX = FS_Settings.FemaleAveragePointyOffsetX;
                            headTypeY = FS_Settings.FemaleAveragePointyOffsetY;
                        }
                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            headTypeX = FS_Settings.MaleAverageWideOffsetX;
                            headTypeY = FS_Settings.MaleAverageWideOffsetY;
                        }
                        else
                        {
                            headTypeX = FS_Settings.FemaleAverageWideOffsetX;
                            headTypeY = FS_Settings.FemaleAverageWideOffsetY;
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
                            headTypeX = FS_Settings.MaleNarrowNormalOffsetX;
                            headTypeY = FS_Settings.MaleNarrowNormalOffsetY;
                        }
                        else
                        {
                            headTypeX = FS_Settings.FemaleNarrowNormalOffsetX;
                            headTypeY = FS_Settings.FemaleNarrowNormalOffsetY;
                        }
                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            headTypeX = FS_Settings.MaleNarrowPointyOffsetX;
                            headTypeY = FS_Settings.MaleNarrowPointyOffsetY;
                        }
                        else
                        {
                            headTypeX = FS_Settings.FemaleNarrowPointyOffsetX;
                            headTypeY = FS_Settings.FemaleNarrowPointyOffsetY;
                        }
                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            headTypeX = FS_Settings.MaleNarrowWideOffsetX;
                            headTypeY = FS_Settings.MaleNarrowWideOffsetY;
                        }
                        else
                        {
                            headTypeX = FS_Settings.FemaleNarrowWideOffsetX;
                            headTypeY = FS_Settings.FemaleNarrowWideOffsetY;
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
            if (!this.HasNaturalMouth)
            {
                return null;
            }

            Material material = null;
            if (this.pawn.gender == Gender.Male)
            {
                material = this.beardGraphic.MatAt(facing);

                if (material != null)
                {
                    material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
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
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public string BrowTexPath(BrowDef browDef)
        {
            return "Brows/" + this.pawn.gender + "/Brow_" + this.pawn.gender + "_" + browDef.texPath;
        }

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
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        /// <summary>
        /// Sets the hair melanin for pawns. Checks for relatives by blood to be consistent within the family.
        /// </summary>
        public void DefineDNA()
        {
            HairMelanin.Genetics(this.pawn, this, out this.melanin1, out this.melanin2);
            this.IsDNAoptimized = true;
        }

        /// <summary>
        /// Basic face definition. Nothing works without this.
        /// </summary>
        public void DefineFace()
        {
            this.pawn = this.parent as Pawn;

            // Log.Message("FS " + this.pawn);
            if (this.pawn == null)
            {
                // Log.Message("Pawn is null, returning. ");
                return;
            }

            // Log.Message("Choosing eyes... ");
            FactionDef faction = this.pawn.Faction?.def;

            if (faction == null)
            {
                faction = FactionDefOf.PlayerColony;
            }

            this.EyeDef = PawnFaceChooser.RandomEyeDefFor(this.pawn, faction);

            // Log.Message(EyeDef.defName);
            this.BrowDef = PawnFaceChooser.RandomBrowDefFor(this.pawn, faction);

            // Log.Message(BrowDef.defName);
            this.WrinkleDef = PawnFaceChooser.AssignWrinkleDefFor(this.pawn);

            // Log.Message(WrinkleDef.defName);

            // this.MouthDef = PawnFaceChooser.RandomMouthDefFor(this.pawn, this.pawn.Faction.def);
            this.BeardDef = PawnFaceChooser.RandomBeardDefFor(this.pawn, faction);

            // Log.Message(BeardDef.defName);
            this.HairColorOrg = this.pawn.story.hairColor;

            this.HasSameBeardColor = Rand.Value > 0.2f;
            if (this.HasSameBeardColor)
            {
                this.BeardColor = PawnHairColors_PostFix.DarkerBeardColor(this.pawn.story.hairColor);
            }
            else
            {
                this.BeardColor = HairMelanin.RandomBeardColor();
            }

            this.IsOptimized = true;
        }

        public Material EyeLeftMatAt(Rot4 facing, bool portrait)
        {
            if (this.HasLeftEyePatch || !this.HasLeftEyePatch && this.IsLeftEyeSolid)
            {
                return null;
            }

            bool flag = true;
            Material material = this.eyeLeftGraphic.MatAt(facing);

            if (!portrait)
            {
                if (FS_Settings.MakeThemBlink)
                {
                    if (this.eyeWiggler.leftCanBlink)
                    {
                        if (this.eyeWiggler.asleep)
                        {
                            flag = false;
                            material = this.eyeLeftClosedGraphic.MatAt(facing);
                        }

                        if (flag)
                        {
                            if (Find.TickManager.TicksGame >= this.eyeWiggler.nextBlink + this.eyeWiggler.jitterLeft)
                            {
                                material = this.eyeLeftClosedGraphic.MatAt(facing);
                            }
                        }
                    }
                }
            }

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public Material EyeLeftPatchMatAt(Rot4 facing)
        {
            Material material = this.eyeLeftPatchGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public Material EyeRightMatAt(Rot4 facing, bool portrait)
        {
            if (this.HasRightEyePatch || (!this.HasRightEyePatch && this.IsRightEyeSolid))
            {
                return null;
            }

            bool flag = true;
            Material material = this.eyeRightGraphic.MatAt(facing);

            if (!portrait)
            {
                if (FS_Settings.MakeThemBlink)
                {
                    if (this.eyeWiggler.rightCanBlink)
                    {
                        if (this.eyeWiggler.asleep)
                        {
                            flag = false;
                            material = this.eyeRightClosedGraphic.MatAt(facing);
                        }

                        if (flag)
                        {
                            if (Find.TickManager.TicksGame >= this.eyeWiggler.nextBlink + this.eyeWiggler.jitterRight)
                            {
                                material = this.eyeRightClosedGraphic.MatAt(facing);
                            }
                        }
                    }
                }
            }

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public Material EyeRightPatchMatAt(Rot4 facing)
        {
            Material material = this.eyeRightPatchGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        private string EyeClosedTexPath(enums.Side side)
        {

            return this.EyeTexPath("Closed", side);
        }

        public string EyeTexPath(string eyeDefPath, enums.Side side)
        {


            string path = "Eyes/Eye_" + eyeDefPath + "_" + this.pawn.gender + "_" + side;

            return path;

            // "Eyes/Eye_" + this.pawn.gender + this.crownType + "_" + this.EyeDef.texPath + "_Right";
            // = "Eyes/Eye_" + this.pawn.gender + this.crownType + "_Closed_Right";
        }

        /// <summary>
        /// Initializes Facial stuff graphics.
        /// </summary>
        /// <returns>True if all went well.</returns>
        public bool InitializeGraphics()
        {
            if (this.pawn == null)
            {
                return false;
            }

            Color wrinkleColor = Color.Lerp(
                this.pawn.story.SkinColor,
                this.pawn.story.SkinColor * this.pawn.story.SkinColor,
                Mathf.InverseLerp(50f, 100f, this.pawn.ageTracker.AgeBiologicalYearsFloat));

            this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.WrinkleDef.texPath + "_" + this.crownType + "_" + this.headType,
                ShaderDatabase.Transparent,
                Vector2.one,
                wrinkleColor);

            this.rottingWrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.WrinkleDef.texPath + "_" + this.crownType + "_" + this.headType,
                ShaderDatabase.Transparent,
                Vector2.one,
                wrinkleColor * Headhelper.skinRottingMultiplyColor);

            string path = this.BeardDef.texPath + "_" + this.crownType + "_" + this.headType;

            if (this.BeardDef == DefDatabase<BeardDef>.GetNamed("Beard_Shaved"))
            {
                path = this.BeardDef.texPath;
            }

            this.beardGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                path,
                ShaderDatabase.Transparent,
                Vector2.one,
                this.BeardColor);

            this.browGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.texPathBrow,
                ShaderDatabase.Transparent,
                Vector2.one,
                Color.black);

            if (!this.HasNaturalMouth)
            {
                this.MouthGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                        this.texPathMouth,
                                        ShaderDatabase.Transparent,
                                        Vector2.one,
                                        Color.white) as Graphic_Multi_NaturalHeadParts;
            }
            else
            {
                this.MouthGraphic = FacialGraphics.MouthGraphic03;
            }

            this.deadEyeGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                "Eyes/Eyes_Dead",
                ShaderDatabase.Transparent,
                Vector2.one,
                Color.white);
            if (this.texPathEyeLeftPatch != null)
            {
                this.eyeLeftPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                                               this.texPathEyeLeftPatch,
                                               ShaderDatabase.Transparent,
                                               Vector2.one,
                                               Color.white) as Graphic_Multi_AddedHeadParts;
                if (this.eyeLeftPatchGraphic != null)
                {
                    this.HasLeftEyePatch = true;
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
                    this.HasRightEyePatch = true;
                }
            }

            this.eyeLeftGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                      this.texPathEyeLeft,
                                      ShaderDatabase.Transparent,
                                      Vector2.one,
                                      this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;
            this.eyeLeftClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                            this.texPathEyeLeftClosed,
                                            ShaderDatabase.Transparent,
                                            Vector2.one,
                                            Color.black) as Graphic_Multi_NaturalEyes;

            this.eyeRightGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                       this.texPathEyeRight,
                                       ShaderDatabase.Transparent,
                                       Vector2.one,
                                       this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;
            this.eyeRightClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                             this.texPathEyeRightClosed,
                                             ShaderDatabase.Transparent,
                                             Vector2.one,
                                             Color.black) as Graphic_Multi_NaturalEyes;

            return true;
        }

        //// todo: make mouths dynamic, check textures?
        public Material MouthMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            Material material = null;
            if (this.HasNaturalMouth)
            {
                if (!FS_Settings.UseMouth || !this.drawMouth)
                {
                    return null;
                }
            }

            bool flag = this.pawn.gender == Gender.Female;

            if (flag || !flag && this.BeardDef.drawMouth || !this.BeardDef.drawMouth && !this.HasNaturalMouth)
            {
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    material = this.MouthGraphic.MatAt(facing);
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    material = this.MouthGraphic.MatAt(facing);
                }

                if (material != null)
                {
                    material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
                }
            }

            return material;
        }

        // todo: eyes closed when anaesthetic
        // eyes closed time = > consciousness?
        // tiredness affect blink rate
        // Method used to animate the eye movement
        public override void PostDraw()
        {
            base.PostDraw();
            if (!this.pawn.Spawned)
            {
                return;
            }

            if (WorldRendererUtility.WorldRenderedNow)
            {
                return;
            }

            this._viewRect = Find.CameraDriver.CurrentViewRect;
            this._viewRect = this._viewRect.ExpandedBy(5);

            if (!this._viewRect.Contains(this.pawn.Position))
            {
                return;
            }

            if (Find.TickManager.TicksGame > this.eyeWiggler.nextBlinkEnd)
            {
                if (this.HasNaturalMouth)
                {
                    this.SetMouthAccordingToMoodLevel();
                }
            }

            // todo: head wiggler? move eyes to eyewiggler
            // this.headWiggler.WigglerTick();
            this.eyeWiggler.WigglerTick();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref this.pawn, "Pawn");

            Scribe_Defs.Look(ref this.EyeDef, "EyeDef");
            Scribe_Defs.Look(ref this.BrowDef, "BrowDef");

            // Scribe_Defs.Look(ref this.MouthDef, "MouthDef");
            Scribe_Defs.Look(ref this.WrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref this.BeardDef, "BeardDef");
            Scribe_Values.Look(ref this.IsOptimized, "optimized");
            Scribe_Values.Look(ref this.drawMouth, "drawMouth");

            Scribe_Values.Look(ref this.headType, "headType");
            Scribe_Values.Look(ref this.crownType, "crownType");
            Scribe_Values.Look(ref this.SkinColorHex, "SkinColorHex");
            Scribe_Values.Look(ref this.HairColorOrg, "HairColorOrg");
            Scribe_Values.Look(ref this.BeardColor, "BeardColor");
            Scribe_Values.Look(ref this.HasSameBeardColor, "sameBeardColor");

            Scribe_Values.Look(ref this.melanin1, "melanin1");
            Scribe_Values.Look(ref this.melanin2, "melanin2");
            Scribe_Values.Look(ref this.IsDNAoptimized, "DNAoptimized");
        }

        /// <summary>
        /// Basic pawn initialization.
        /// </summary>
        /// <returns>Success</returns>
        public bool SetHeadType()
        {
            if (this.pawn == null)
            {
                return false;
            }

            this.eyeWiggler = new PawnEyeWiggler(this.pawn);

            // this.headWiggler = new PawnHeadWiggler(this.pawn);
            this.IsOld = this.pawn.ageTracker.AgeBiologicalYearsFloat >= 50f;

            this.SetHeadAndCrownType();

            this.ResetBoolsAndPaths();

            if (FS_Settings.ShowExtraParts)
            {
                this.CheckForAddedOrMissingParts();
            }

            this.SetHeadOffsets();

            return true;
        }

        public Material WrinkleMatAt(Rot4 facing, RotDrawMode bodyCondition)
        {
            Material material = null;
            if (this.IsOld && FS_Settings.UseWrinkles)
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
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        private void CheckForAddedOrMissingParts()
        {
            List<BodyPartRecord> body = this.pawn.RaceProps.body.AllParts;
            foreach (Hediff hediff in this.pawn.health.hediffSet.hediffs)
            {
                BodyPartRecord leftEye = body.Find(x => x.def == BodyPartDefOf.LeftEye);
                BodyPartRecord rightEye = body.Find(x => x.def == BodyPartDefOf.RightEye);
                BodyPartRecord jaw = body.Find(x => x.def == BodyPartDefOf.Jaw);
                AddedBodyPartProps addedPartProps = hediff.def.addedPartProps;

                if (addedPartProps != null)
                {
                    if (hediff.Part == leftEye)
                    {
                        this.texPathEyeLeftPatch = "AddedParts/" + hediff.def.defName + "_Left" + "_" + this.crownType;
                        this.IsLeftEyeSolid = addedPartProps.isSolid;
                    }

                    if (hediff.Part == rightEye)
                    {
                        this.texPathEyeRightPatch = "AddedParts/" + hediff.def.defName + "_Right" + "_"
                                                    + this.crownType;
                        this.IsRightEyeSolid = addedPartProps.isSolid;
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
                        this.texPathEyeLeft =
                            this.EyeTexPath(
                                "Missing",
                                enums.Side.Left); // "Eyes/" + "ShotOut" + "_Left" + this.crownType;
                        this.eyeWiggler.leftCanBlink = false;
                    }

                    if (hediff.Part == rightEye)
                    {
                        this.texPathEyeRight = this.EyeTexPath("Missing", enums.Side.Right);
                        this.eyeWiggler.rightCanBlink = false;
                    }
                }
            }
        }

        private void ResetBoolsAndPaths()
        {
            this.HasLeftEyePatch = false;
            this.HasRightEyePatch = false;
            this.IsLeftEyeSolid = false;
            this.IsRightEyeSolid = false;

            this.texPathEyeLeftPatch = null;
            this.texPathEyeRightPatch = null;

            this.texPathEyeRight = this.EyeTexPath(this.EyeDef.texPath, enums.Side.Right);
            this.texPathEyeRightClosed = this.EyeClosedTexPath(enums.Side.Right);

            this.texPathEyeLeft = this.EyeTexPath(this.EyeDef.texPath, enums.Side.Left);
            this.texPathEyeLeftClosed = this.EyeClosedTexPath(enums.Side.Left);

            this.eyeWiggler.leftCanBlink = true;
            this.eyeWiggler.rightCanBlink = true;

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

        private void SetHeadAndCrownType()
        {
            if (this.pawn.story.HeadGraphicPath.Contains("Normal"))
            {
                this.headType = HeadType.Normal;
            }

            if (this.pawn.story.HeadGraphicPath.Contains("Pointy"))
            {
                this.headType = HeadType.Pointy;
            }

            if (this.pawn.story.HeadGraphicPath.Contains("Wide"))
            {
                this.headType = HeadType.Wide;
            }

            if (this.pawn.story.crownType == CrownType.Narrow)
            {
                this.crownType = CrownType.Narrow;
            }
            else
            {
                this.crownType = CrownType.Average;
            }
        }

        private void SetHeadOffsets()
        {
            if (this.pawn.gender == Gender.Male)
            {
                if (this.crownType == CrownType.Average)
                {
                    switch (this.headType)
                    {
                        case HeadType.Normal:
                            this.fullHeadType = FullHead.MaleAverageNormal;
                            this.headTypeX = FS_Settings.MaleAverageNormalOffsetX;
                            this.headTypeY = FS_Settings.MaleAverageNormalOffsetY;
                            break;

                        case HeadType.Pointy:
                            this.fullHeadType = FullHead.MaleAveragePointy;
                            this.headTypeX = FS_Settings.MaleAveragePointyOffsetX;
                            this.headTypeY = FS_Settings.MaleAveragePointyOffsetY;
                            break;

                        case HeadType.Wide:
                            this.fullHeadType = FullHead.MaleAverageWide;
                            this.headTypeX = FS_Settings.MaleAverageWideOffsetX;
                            this.headTypeY = FS_Settings.MaleAverageWideOffsetY;
                            break;
                    }
                }

                if (this.crownType == CrownType.Narrow)
                {
                    switch (this.headType)
                    {
                        case HeadType.Normal:
                            this.fullHeadType = FullHead.MaleNarrowNormal;
                            this.headTypeX = FS_Settings.MaleNarrowNormalOffsetX;
                            this.headTypeY = FS_Settings.MaleNarrowNormalOffsetY;
                            break;

                        case HeadType.Pointy:
                            this.fullHeadType = FullHead.MaleNarrowPointy;
                            this.headTypeX = FS_Settings.MaleNarrowPointyOffsetX;
                            this.headTypeY = FS_Settings.MaleNarrowPointyOffsetY;
                            break;

                        case HeadType.Wide:
                            this.fullHeadType = FullHead.MaleNarrowWide;
                            this.headTypeX = FS_Settings.MaleNarrowWideOffsetX;
                            this.headTypeY = FS_Settings.MaleNarrowWideOffsetY;
                            break;
                    }
                }
            }
            else if (this.pawn.gender == Gender.Female)
            {
                if (this.crownType == CrownType.Average)
                {
                    switch (this.headType)
                    {
                        case HeadType.Normal:
                            this.fullHeadType = FullHead.FemaleAverageNormal;
                            this.headTypeX = FS_Settings.FemaleAverageNormalOffsetX;
                            this.headTypeY = FS_Settings.FemaleAverageNormalOffsetY;
                            break;

                        case HeadType.Pointy:
                            this.fullHeadType = FullHead.FemaleAveragePointy;
                            this.headTypeX = FS_Settings.FemaleAveragePointyOffsetX;
                            this.headTypeY = FS_Settings.FemaleAveragePointyOffsetY;
                            break;

                        case HeadType.Wide:
                            this.fullHeadType = FullHead.FemaleAverageWide;
                            this.headTypeX = FS_Settings.FemaleAverageWideOffsetX;
                            this.headTypeY = FS_Settings.FemaleAverageWideOffsetY;
                            break;
                    }
                }

                if (this.crownType == CrownType.Narrow)
                {
                    switch (this.headType)
                    {
                        case HeadType.Normal:
                            this.fullHeadType = FullHead.FemaleNarrowNormal;
                            this.headTypeX = FS_Settings.FemaleNarrowNormalOffsetX;
                            this.headTypeY = FS_Settings.FemaleNarrowNormalOffsetY;
                            break;

                        case HeadType.Pointy:
                            this.fullHeadType = FullHead.FemaleNarrowPointy;
                            this.headTypeX = FS_Settings.FemaleNarrowPointyOffsetX;
                            this.headTypeY = FS_Settings.FemaleNarrowPointyOffsetY;
                            break;

                        case HeadType.Wide:
                            this.fullHeadType = FullHead.FemaleNarrowWide;
                            this.headTypeX = FS_Settings.FemaleNarrowWideOffsetX;
                            this.headTypeY = FS_Settings.FemaleNarrowWideOffsetY;
                            break;
                    }
                }
            }
            else
            {
                this.fullHeadType = FullHead.MaleAverageNormal;
            }
        }

        private void SetMouthAccordingToMoodLevel()
        {
            this.mood = this.pawn.needs?.mood?.CurInstantLevel ?? 0.5f;

            if (this.mood > 0.85f)
            {
                this.MouthGraphic = FacialGraphics.MouthGraphic01;
            }
            else if (this.mood > 0.7f)
            {
                this.MouthGraphic = FacialGraphics.MouthGraphic02;
            }
            else if (this.mood > 0.55f)
            {
                this.MouthGraphic = FacialGraphics.MouthGraphic03;
            }
            else if (this.mood > 0.4f)
            {
                this.MouthGraphic = FacialGraphics.MouthGraphic04;
            }
            else if (this.mood > 0.25f)
            {
                this.MouthGraphic = FacialGraphics.MouthGraphic05;
            }
            else
            {
                this.MouthGraphic = FacialGraphics.MouthGraphic06;
            }
        }

        #endregion Methods

    }
}