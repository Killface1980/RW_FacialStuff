namespace FacialStuff
{
    using System.Collections.Generic;

    using FacialStuff.Defs;
    using FacialStuff.Detouring;
    using FacialStuff.Wiggler;

    using RimWorld;
    using RimWorld.Planet;

    using RW_FacialStuff;

    using UnityEngine;

    using Verse;

    public class CompFace : ThingComp
    {

        public override void CompTick()
        {
        }

        public PawnHeadWiggler headWiggler;

        public PawnEyeWiggler eyeWiggler;

        public Color BeardColor = Color.clear;

        public BeardDef BeardDef = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");

        public BrowDef BrowDef;

        public CrownType crownType;

        public bool drawMouth = true;

        // todo: make dead eyes
        public EyeDef EyeDef;

        public FullHead fullHeadType;

        public Color HairColorOrg;

        public bool HasLeftEyePatch;

        public bool HasRightEyePatch;

        public HeadType headType;

        public bool ignoreRenderer;

        public Graphic_Multi_NaturalHeadParts MouthGraphic;

        public bool naturalMouth = true;

        public bool optimized;

        public Pawn pawn;

        public int rotationInt;

        public bool sameBeardColor;

        public WrinkleDef WrinkleDef;



        private CellRect _viewRect;


        private Graphic beardGraphic;

        private Graphic browGraphic;

        private string browTexPath;

        private Graphic deadEyeGraphic;







        private float headTypeX;

        private float headTypeY;

        private bool isOld;

        // private float blinkRate;

        private Graphic_Multi_NaturalEyes leftEyeClosedGraphic;

        private string leftEyeClosedTexPath;

        private Graphic_Multi_NaturalEyes leftEyeGraphic;

        private Graphic_Multi_AddedHeadParts leftEyePatchGraphic;

        private string leftEyePatchTexPath;

        private string leftEyeTexPath;

        private bool LeftIsSolid;

        private float mood = 0.5f;

        private string mouthTexPath;


        private Graphic_Multi_NaturalEyes rightEyeClosedGraphic;

        private string rightEyeClosedTexPath;

        private Graphic_Multi_NaturalEyes rightEyeGraphic;

        private Graphic_Multi_AddedHeadParts rightEyePatchGraphic;

        private string rightEyePatchTexPath;

        private string rightEyeTexPath;

        private bool RightIsSolid;

        private string SkinColorHex;

        private Graphic wrinkleGraphic;

        private Graphic rottingWrinkleGraphic;

        // Verse.PawnGraphicSet
        public GraphicMeshSet MouthMeshSet => MeshPoolFs.HumanlikeMouthSet[(int)this.fullHeadType];

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
            if (!this.naturalMouth)
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
            Material material = null;
            material = this.browGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public string BrowTexPath(BrowDef browDef)
        {
            return "Brows/" + this.pawn.gender + "/Brow_" + this.pawn.gender + "_"
                   + browDef.texPath;
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
            var faction = this.pawn.Faction?.def;

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

            this.sameBeardColor = Rand.Value > 0.2f;
            if (this.sameBeardColor)
            {
                this.BeardColor = PawnHairColors_PostFix.DarkerBeardColor(this.pawn.story.hairColor);
            }
            else
            {
                this.BeardColor = PawnHairColors_PostFix.RandomBeardColor();
            }

            this.optimized = true;
        }

        public string EyeTexPath(string eyeDefPath, enums.Side side, bool closed = false)
        {
            if (closed)
            {
                eyeDefPath = "Closed";
            }

            string path = "Eyes/Eye_" + eyeDefPath + "_" + this.pawn.gender + "_" + side;

            return path;

            // "Eyes/Eye_" + this.pawn.gender + this.crownType + "_" + this.EyeDef.texPath + "_Right";
            // = "Eyes/Eye_" + this.pawn.gender + this.crownType + "_Closed_Right";
        }

        public bool InitializeGraphics()
        {
            if (this.pawn == null)
            {
                return false;
            }

            if (this.BeardColor == Color.clear)
            {
                this.sameBeardColor = Rand.Value > 0.2f;

                if (this.sameBeardColor)
                {
                    this.BeardColor = PawnHairColors_PostFix.DarkerBeardColor(this.pawn.story.hairColor);
                }
                else
                {
                    this.BeardColor = PawnHairColors_PostFix.RandomBeardColor();
                }
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
                this.browTexPath,
                ShaderDatabase.Transparent,
                Vector2.one,
                Color.black);

            if (!this.naturalMouth)
            {
                this.MouthGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                        this.mouthTexPath,
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
            if (this.leftEyePatchTexPath != null)
            {
                this.leftEyePatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                                               this.leftEyePatchTexPath,
                                               ShaderDatabase.Transparent,
                                               Vector2.one,
                                               Color.white) as Graphic_Multi_AddedHeadParts;
                if (this.leftEyePatchGraphic != null)
                {
                    this.HasLeftEyePatch = true;
                }
            }

            if (this.rightEyePatchTexPath != null)
            {
                this.rightEyePatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                                                this.rightEyePatchTexPath,
                                                ShaderDatabase.Transparent,
                                                Vector2.one,
                                                Color.white) as Graphic_Multi_AddedHeadParts;
                if (this.rightEyePatchGraphic != null)
                {
                    this.HasRightEyePatch = true;
                }
            }

            this.leftEyeGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                      this.leftEyeTexPath,
                                      ShaderDatabase.Transparent,
                                      Vector2.one,
                                      this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;
            this.leftEyeClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                            this.leftEyeClosedTexPath,
                                            ShaderDatabase.Transparent,
                                            Vector2.one,
                                            Color.black) as Graphic_Multi_NaturalEyes;

            this.rightEyeGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                       this.rightEyeTexPath,
                                       ShaderDatabase.Transparent,
                                       Vector2.one,
                                       this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;
            this.rightEyeClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                             this.rightEyeClosedTexPath,
                                             ShaderDatabase.Transparent,
                                             Vector2.one,
                                             Color.black) as Graphic_Multi_NaturalEyes;

            return true;
        }

        public Material LeftEyeMatAt(Rot4 facing, bool portrait)
        {
            Material material = null;

            if (this.HasLeftEyePatch || !this.HasLeftEyePatch && this.LeftIsSolid)
            {
                return null;
            }

            bool flag = true;
            material = this.leftEyeGraphic.MatAt(facing);

            if (!portrait)
            {
                if (this.eyeWiggler.leftCanBlink)
                {
                    if (this.eyeWiggler.asleep)
                    {
                        flag = false;
                        material = this.leftEyeClosedGraphic.MatAt(facing);
                    }

                    if (flag)
                    {
                        if (Find.TickManager.TicksGame >= this.eyeWiggler.nextBlink + this.eyeWiggler.jitterLeft)
                        {
                            material = this.leftEyeClosedGraphic.MatAt(facing);
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

        public Material LeftEyePatchMatAt(Rot4 facing)
        {
            Material material = this.leftEyePatchGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public Material RightEyeMatAt(Rot4 facing, bool portrait)
        {
            if (this.HasRightEyePatch || !this.HasRightEyePatch && this.RightIsSolid)
            {
                return null;
            }

            Material material = null;
            bool flag = true;
            material = this.rightEyeGraphic.MatAt(facing);

            if (!portrait)
            {
                if (this.eyeWiggler.rightCanBlink)
                {
                    if (this.eyeWiggler.asleep)
                    {
                        flag = false;
                        material = this.rightEyeClosedGraphic.MatAt(facing);
                    }

                    if (flag)
                    {
                        if (Find.TickManager.TicksGame >= this.eyeWiggler.nextBlink + this.eyeWiggler.jitterRight)
                        {
                            material = this.rightEyeClosedGraphic.MatAt(facing);
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

        public Material RightEyePatchMatAt(Rot4 facing)
        {
            Material material = this.rightEyePatchGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        //// todo: make mouths dynamic, check textures?

        public Material MouthMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            Material material = null;
            if (this.naturalMouth)
            {
                if (!FS_Settings.UseMouth || !this.drawMouth)
                {
                    return null;
                }
            }

            bool flag = this.pawn.gender == Gender.Female;

            if (flag || !flag && this.BeardDef.drawMouth || !this.BeardDef.drawMouth && !this.naturalMouth)
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

        public Material WrinkleMatAt(Rot4 facing, RotDrawMode bodyCondition)
        {
            Material material = null;
            if (this.isOld && FS_Settings.UseWrinkles)
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
                if (this.naturalMouth)
                {
                    this.SetMouthAccordingToMoodLevel();
                }
            }

            // todo: head wiggler? move eyes to eyewiggler
            // this.headWiggler.WigglerTick();
            this.eyeWiggler.WigglerTick();

        }

        private void SetMouthAccordingToMoodLevel()
        {
            this.mood = pawn.needs?.mood?.CurInstantLevel ?? 0.5f;

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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref this.pawn, "Pawn");

            Scribe_Defs.Look(ref this.EyeDef, "EyeDef");
            Scribe_Defs.Look(ref this.BrowDef, "BrowDef");

            // Scribe_Defs.Look(ref this.MouthDef, "MouthDef");
            Scribe_Defs.Look(ref this.WrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref this.BeardDef, "BeardDef");
            Scribe_Values.Look(ref this.optimized, "optimized");
            Scribe_Values.Look(ref this.drawMouth, "drawMouth");

            Scribe_Values.Look(ref this.headType, "headType");
            Scribe_Values.Look(ref this.crownType, "crownType");
            Scribe_Values.Look(ref this.SkinColorHex, "SkinColorHex");
            Scribe_Values.Look(ref this.HairColorOrg, "HairColorOrg");
            Scribe_Values.Look(ref this.BeardColor, "BeardColor");
            Scribe_Values.Look(ref this.sameBeardColor, "sameBeardColor");
        }

        public bool SetHeadType()
        {
            this.pawn = this.parent as Pawn;

            if (this.pawn == null)
            {
                return false;
            }

            this.eyeWiggler = new PawnEyeWiggler(this.pawn);
            //// this.headWiggler = new PawnHeadWiggler(this.pawn);

            this.isOld = this.pawn.ageTracker.AgeBiologicalYearsFloat >= 50f;

            this.SetHeadAndCrownType();

            this.ResetBoolsAndPaths();

            this.CheckForAddedOrMissingParts();

            this.SetHeadOffsets();

            return true;
        }

        private void ResetBoolsAndPaths()
        {
            this.HasLeftEyePatch = false;
            this.HasRightEyePatch = false;
            this.LeftIsSolid = false;
            this.RightIsSolid = false;

            this.leftEyePatchTexPath = null;
            this.rightEyePatchTexPath = null;

            this.rightEyeTexPath = this.EyeTexPath(this.EyeDef.texPath, enums.Side.Right);
            this.rightEyeClosedTexPath = this.EyeTexPath(this.EyeDef.texPath, enums.Side.Right, true);

            this.leftEyeTexPath = this.EyeTexPath(this.EyeDef.texPath, enums.Side.Left);
            this.leftEyeClosedTexPath = this.EyeTexPath(this.EyeDef.texPath, enums.Side.Left, true);

            this.eyeWiggler.leftCanBlink = true;
            this.eyeWiggler.rightCanBlink = true;

#if develop
            {
                naturalMouth = false;
                this.mouthTexPath = "Mouth/Mouth_BionicJaw";

            }
#else
            this.naturalMouth = true;
#endif
            this.browTexPath = this.BrowTexPath(this.BrowDef);
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
                        this.leftEyePatchTexPath = "AddedParts/" + hediff.def.defName + "_Left" + "_" + this.crownType;
                        this.LeftIsSolid = addedPartProps.isSolid;
                    }

                    if (hediff.Part == rightEye)
                    {
                        this.rightEyePatchTexPath = "AddedParts/" + hediff.def.defName + "_Right" + "_" + this.crownType;
                        this.RightIsSolid = addedPartProps.isSolid;
                    }

                    if (hediff.Part == jaw)
                    {
                        this.mouthTexPath = "Mouth/Mouth_" + hediff.def.defName;
                        this.naturalMouth = false;
                    }
                }

                if (hediff.def == HediffDefOf.MissingBodyPart)
                {
                    if (hediff.Part == leftEye)
                    {
                        this.leftEyeTexPath =
                            this.EyeTexPath("Missing", enums.Side.Left); // "Eyes/" + "ShotOut" + "_Left" + this.crownType;
                        this.eyeWiggler.leftCanBlink = false;
                    }

                    if (hediff.Part == rightEye)
                    {
                        this.rightEyeTexPath = this.EyeTexPath("Missing", enums.Side.Right);
                        this.eyeWiggler.rightCanBlink = false;
                    }
                }
            }
        }
    }
}