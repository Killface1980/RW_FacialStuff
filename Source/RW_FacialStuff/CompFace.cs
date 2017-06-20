namespace FacialStuff
{
    using System.Collections.Generic;

    using FaceStyling;

    using FacialStuff.Defs;
    using FacialStuff.Detouring;

    using RimWorld;
    using RimWorld.Planet;

    using RW_FacialStuff;

    using UnityEngine;

    using Verse;

    public class CompFace : ThingComp
    {
        private static readonly SimpleCurve EyeMotionFullCurve = new SimpleCurve
                                                                     {
                                                                         { new CurvePoint(0f, 0f), true },
                                                                         {
                                                                             new CurvePoint(0.05f, -1f),
                                                                             true
                                                                         },

                                                                         // { new CurvePoint(0.25f, -1f), true },
                                                                         // { new CurvePoint(0.3f, -1f), true },
                                                                         {
                                                                             // { new CurvePoint(0.5f, 0f), true },
                                                                             // { new CurvePoint(0.7f, 1f), true },
                                                                             // { new CurvePoint(0.75f, 1f), true },
                                                                             new CurvePoint(0.65f, 1f), true
                                                                         },
                                                                         { new CurvePoint(0.85f, 0f), true }
                                                                     };

        private static readonly SimpleCurve EyeMotionHalfCurve = new SimpleCurve
                                                                     {
                                                                         { new CurvePoint(0f, 0f), true },
                                                                         { new CurvePoint(0.1f, 1f), true },
                                                                         {
                                                                             // { new CurvePoint(0.5f, 1f), true },
                                                                             new CurvePoint(0.65f, 1f), true
                                                                         },
                                                                         { new CurvePoint(0.75f, 0f), true }
                                                                     };




        #region Development
        public bool ignoreRenderer;
        #endregion

        #region Fields


        public Vector3 eyemoveR = new Vector3(0, 0, 0);
        public Vector3 eyemoveL = new Vector3(0, 0, 0);
        public Pawn pawn;

        #region Defs

        public BeardDef BeardDef = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");

        public BrowDef BrowDef;

        // todo: make dead eyes
        public EyeDef EyeDef;

        public MouthDef MouthDef = null;

        public WrinkleDef WrinkleDef;

        #endregion

        #region Graphics

        public Color HairColorOrg;
        public Color BeardColor = Color.clear;

        private Graphic beardGraphic;

        private Graphic browGraphic;

        private Graphic deadEyeGraphic;

        private Graphic_Multi_NaturalEyes leftEyeClosedGraphic;
        private Graphic_Multi_NaturalEyes leftEyeGraphic = null;
        private Graphic_Multi_AddedHeadParts leftEyePatchGraphic = null;
        private Graphic mouthGraphic;
        private Graphic_Multi_NaturalEyes rightEyeClosedGraphic;
        private Graphic_Multi_NaturalEyes rightEyeGraphic = null;
        private Graphic_Multi_AddedHeadParts rightEyePatchGraphic = null;

        private Graphic wrinkleGraphic;

        #endregion

        #region Materials

        public Material BeardMatAt(Rot4 facing)
        {
            Material material = null;
            if (this.pawn.gender == Gender.Male)
            {
                material = this.beardGraphic.MatAt(facing, null);

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
            material = this.browGraphic.MatAt(facing, null);

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public Material DeadEyeMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            Material material = null;
            if (bodyCondition == RotDrawMode.Fresh)
            {
                material = this.deadEyeGraphic.MatAt(facing, null);
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                material = this.deadEyeGraphic.MatAt(facing, null);
            }

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }



        public Material LeftEyeMatAt(Rot4 facing, bool portrait)
        {
            Material material = null;

            bool flag = true;
            material = this.leftEyeGraphic.MatAt(facing, null);
            if (portrait)
            {

                if (material != null)
                {
                    material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
                }

                return material;
            }
            if (this.LeftCanBlink)
            {
                if (asleep)
                {
                    flag = false;
                    material = this.leftEyeClosedGraphic.MatAt(facing, null);
                }

                if (flag)
                {
                    if (Find.TickManager.TicksGame >= this.nextBlink + this.jitterLeft)
                    {
                        material = this.leftEyeClosedGraphic.MatAt(facing, null);
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
            Material material = this.leftEyePatchGraphic.MatAt(facing, null);

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public Material RightEyeMatAt(Rot4 facing, bool portrait)
        {
            Material material = null;
            bool flag = true;
            material = this.rightEyeGraphic.MatAt(facing, null);

            if (portrait)
            {
                if (material != null)
                {
                    material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
                }

                return material;
            }

            if (this.RightCanBlink)
            {
                if (asleep)
                {
                    flag = false;
                    material = this.rightEyeClosedGraphic.MatAt(facing, null);
                }
                if (flag)
                {
                    if (Find.TickManager.TicksGame >= this.nextBlink + this.jitterRight)
                    {
                        material = this.rightEyeClosedGraphic.MatAt(facing, null);
                    }
                }
            }
            else
            {
            }

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public Material RightEyePatchMatAt(Rot4 facing)
        {
            Material material = this.rightEyePatchGraphic.MatAt(facing, null);

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        // todo: make mouths dynamic, check textures
        public Material MouthMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            Material material = null;
            if (FS_Settings.UseMouth && this.drawMouth)
            {
                bool flag = this.pawn.gender == Gender.Female;

                if (flag || !flag && this.BeardDef.drawMouth)
                {
                    if (bodyCondition == RotDrawMode.Fresh)
                    {
                        material = this.mouthGraphic.MatAt(facing, null);
                    }
                    else if (bodyCondition == RotDrawMode.Rotting)
                    {
                        material = this.mouthGraphic.MatAt(facing, null);
                    }

                    if (material != null)
                    {
                        material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
                    }
                }
            }

            return material;
        }

        // todo: check textures
        public Material WrinkleMatAt(Rot4 facing)
        {
            Material material = null;
            if (this.isOld && FS_Settings.UseWrinkles)
            {
                material = this.wrinkleGraphic.MatAt(facing, null);
            }

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        #endregion

        #region Strings

        public CrownType crownType;

        public HeadType headType;

        private string SkinColorHex;

        private string leftEyeClosedTexPath = null;

        private string leftEyePatchTexPath = null;

        private string leftEyeTexPath;

        private string rightEyeClosedTexPath;

        private string rightEyePatchTexPath = null;

        private string rightEyeTexPath;
        #endregion

        #region Bools



        public bool LeftIsSolid = false;

        public bool RightIsSolid = false;

        public bool drawMouth = true;

        public bool hasLeftEyePatch = false;

        public bool hasRightEyePatch = false;

        public bool isOld;

        public bool optimized;
        private bool halfAnimX;

        private bool halfAnimY;

        private bool moveX;
        private bool moveY;
        #endregion

        #region Floats & Ints




        private float factorX = 0.02f;

        private float factorY = 0.01f;

        private float flippedX = 0f;

        private float flippedY = 0f;


        // private float blinkRate;
        private int jitterLeft = 0;

        private int jitterRight = 0;

        private int lastBlinkended;


        private int nextBlink = -5000;

        private int nextBlinkEnd = -5000;

        public bool sameBeardColor;

        private bool asleep;

        private string browTexPath;

        #endregion


        #endregion

        #region Methods

        // Verse.PawnGraphicSet
        public GraphicMeshSet MouthMeshSet
        {
            get
            {
                switch (pawn.gender)
                {
                    case Gender.Male:
                        if (this.pawn.story.crownType == CrownType.Average)
                        {
                            return MeshPoolFs.humanlikeHeadSetAverage;
                        }
                        if (this.pawn.story.crownType == CrownType.Narrow)
                        {
                            return MeshPoolFs.humanlikeHeadSetNarrow;
                        }
                        break;

                    case Gender.Female:
                        if (this.pawn.story.crownType == CrownType.Average)
                        {
                            return MeshPoolFs.humanlikeHeadSetAverageFemale;
                        }
                        if (this.pawn.story.crownType == CrownType.Narrow)
                        {
                            return MeshPoolFs.humanlikeHeadSetNarrowFemale;
                        }
                        break;

                }
                Log.Error("Unknown crown type: " + this.pawn.story.crownType);
                return MeshPool.humanlikeHairSetAverage;
            }
        }


        public bool SetHeadType()
        {
            this.pawn = this.parent as Pawn;

            if (this.pawn == null)
            {
                return false;
            }


            if (this.pawn.story.HeadGraphicPath.Contains("Normal"))
            {
                headType = HeadType.Normal;
            }

            if (this.pawn.story.HeadGraphicPath.Contains("Pointy"))
            {
                headType = HeadType.Pointy;
            }

            if (this.pawn.story.HeadGraphicPath.Contains("Wide"))
            {
                headType = HeadType.Wide;
            }

            if (this.pawn.story.crownType == CrownType.Narrow)
            {
                this.crownType = CrownType.Narrow;
            }
            else
            {
                this.crownType = CrownType.Average;
            }

            this.hasLeftEyePatch = false;
            this.hasRightEyePatch = false;
            this.LeftIsSolid = false;
            this.RightIsSolid = false;

            this.leftEyePatchTexPath = null;
            this.rightEyePatchTexPath = null;

            this.rightEyeTexPath = EyeTexPath(this.EyeDef.texPath, enums.Side.Right);
            this.rightEyeClosedTexPath = EyeTexPath(this.EyeDef.texPath, enums.Side.Right, true);

            this.leftEyeTexPath = EyeTexPath(this.EyeDef.texPath, enums.Side.Left);
            this.leftEyeClosedTexPath = EyeTexPath(this.EyeDef.texPath, enums.Side.Left, true);

            this.LeftCanBlink = true;
            this.RightCanBlink = true;

            // "Eyes/Eye_" + this.pawn.gender + this.crownType + "_" + this.EyeDef.texPath + "_Right";

            // = "Eyes/Eye_" + this.pawn.gender + this.crownType + "_Closed_Right";


            // this.leftEyeTexPath = "Eyes/Eye_" + this.pawn.gender + this.crownType + "_" + this.EyeDef.texPath
            // + "_Left";
            // this.leftEyeClosedTexPath = "Eyes/Eye_" + this.pawn.gender + this.crownType + "_Closed_Left";
            this.browTexPath = this.BrowTexPath(BrowDef);

            foreach (Hediff hediff in this.pawn.health.hediffSet.hediffs)
            {
                List<BodyPartRecord> body = this.pawn.RaceProps.body.AllParts;

                BodyPartRecord leftEye = body.Find(x => x.def == BodyPartDefOf.LeftEye);
                BodyPartRecord rightEye = body.Find(x => x.def == BodyPartDefOf.RightEye);
                AddedBodyPartProps addedPartProps = hediff.def.addedPartProps;

                if (addedPartProps != null)
                {
                    // BodyPartRecord jaw = body.Find(x => x.def == BodyPartDefOf.Jaw);

                    // if (hediff.Part == jaw)
                    // {
                    // this.ExtraJaw_texPath = "AddedParts/" + hediff.def.defName +  this.crownType;
                    // }
                    // BodyPartRecord leftEye = this.pawn.RaceProps.body.GetPartAtIndex(27);
                    // BodyPartRecord rightEye = this.pawn.RaceProps.body.GetPartAtIndex(28);
                    if (hediff.Part == leftEye)
                    {
                        this.leftEyePatchTexPath = "AddedParts/" + hediff.def.defName + "_Left" + "_" + this.crownType;
                        this.LeftIsSolid = addedPartProps.isSolid;
                    }

                    if (hediff.Part == rightEye)
                    {
                        this.rightEyePatchTexPath = "AddedParts/" + hediff.def.defName + "_Right"
                                                    + "_" + this.crownType;
                        this.RightIsSolid = addedPartProps.isSolid;
                    }
                }

                if (hediff.def == HediffDefOf.MissingBodyPart)
                {
                    if (hediff.Part == leftEye)
                    {
                        this.leftEyeTexPath = EyeTexPath("Missing", enums.Side.Left);// "Eyes/" + "ShotOut" + "_Left" + this.crownType;
                        this.LeftCanBlink = false;
                    }

                    if (hediff.Part == rightEye)
                    {
                        this.rightEyeTexPath = EyeTexPath("Missing", enums.Side.Right); ;
                        this.RightCanBlink = false;
                    }
                }
            }

            if (this.pawn.gender == Gender.Male)
            {
                if (this.crownType == CrownType.Average)
                {
                    switch (this.headType)
                    {
                        case HeadType.Normal:
                            headTypeX = FS_Settings.MaleAverageNormalOffsetX;
                            headTypeY = FS_Settings.MaleAverageNormalOffsetY;
                            break;
                        case HeadType.Pointy:
                            headTypeX = FS_Settings.MaleAveragePointyOffsetX;
                            headTypeY = FS_Settings.MaleAveragePointyOffsetY;
                            break;
                        case HeadType.Wide:
                            headTypeX = FS_Settings.MaleAverageWideOffsetX;
                            headTypeY = FS_Settings.MaleAverageWideOffsetY;
                            break;
                    }
                }
                if (this.crownType == CrownType.Narrow)
                {
                    switch (this.headType)
                    {
                        case HeadType.Normal:
                            headTypeX = FS_Settings.MaleNarrowNormalOffsetX;
                            headTypeY = FS_Settings.MaleNarrowNormalOffsetY;
                            break;
                        case HeadType.Pointy:
                            headTypeX = FS_Settings.MaleNarrowPointyOffsetX;
                            headTypeY = FS_Settings.MaleNarrowPointyOffsetY;
                            break;
                        case HeadType.Wide:
                            headTypeX = FS_Settings.MaleNarrowWideOffsetX;
                            headTypeY = FS_Settings.MaleNarrowWideOffsetY;
                            break;
                    }
                }
            }

            if (this.pawn.gender == Gender.Female)
            {
                if (this.crownType == CrownType.Average)
                {
                    switch (this.headType)
                    {
                        case HeadType.Normal:
                            headTypeX = FS_Settings.FemaleAverageNormalOffsetX;
                            headTypeY = FS_Settings.FemaleAverageNormalOffsetY;
                            break;
                        case HeadType.Pointy:
                            headTypeX = FS_Settings.FemaleAveragePointyOffsetX;
                            headTypeY = FS_Settings.FemaleAveragePointyOffsetY;
                            break;
                        case HeadType.Wide:
                            headTypeX = FS_Settings.FemaleAverageWideOffsetX;
                            headTypeY = FS_Settings.FemaleAverageWideOffsetY;
                            break;
                    }
                }
                if (this.crownType == CrownType.Narrow)
                {
                    switch (this.headType)
                    {
                        case HeadType.Normal:
                            headTypeX = FS_Settings.FemaleNarrowNormalOffsetX;
                            headTypeY = FS_Settings.FemaleNarrowNormalOffsetY;
                            break;
                        case HeadType.Pointy:
                            headTypeX = FS_Settings.FemaleNarrowPointyOffsetX;
                            headTypeY = FS_Settings.FemaleNarrowPointyOffsetY;
                            break;
                        case HeadType.Wide:
                            headTypeX = FS_Settings.FemaleNarrowWideOffsetX;
                            headTypeY = FS_Settings.FemaleNarrowWideOffsetY;
                            break;
                    }
                }
            }

            return true;
        }

        public string EyeTexPath(string eyeDefPath, enums.Side side, bool closed = false)
        {
            string path = "Eyes/Eye_" + this.pawn.gender + "_" + this.crownType + "_";

            if (closed)
            {
                path += "Closed";
            }
            else
            {
                path += eyeDefPath;
            }

            path += "_" + side;

            return path;

            // "Eyes/Eye_" + this.pawn.gender + this.crownType + "_" + this.EyeDef.texPath + "_Right";
            // = "Eyes/Eye_" + this.pawn.gender + this.crownType + "_Closed_Right";
        }

        public string BrowTexPath(BrowDef browDef)
        {
            return "Brows/" + this.pawn.gender + "/Brow_" + this.pawn.gender + "_" + this.crownType + "_"
                   + browDef.texPath;
        }

        public void DefineFace()
        {
            this.pawn = this.parent as Pawn;

            if (this.pawn == null)
            {
                return;
            }

            this.EyeDef = PawnFaceChooser.RandomEyeDefFor(this.pawn, this.pawn.Faction.def);

            this.BrowDef = PawnFaceChooser.RandomBrowDefFor(this.pawn, this.pawn.Faction.def);

            this.WrinkleDef = PawnFaceChooser.AssignWrinkleDefFor(this.pawn, this.pawn.Faction.def);

            this.MouthDef = PawnFaceChooser.RandomMouthDefFor(this.pawn, this.pawn.Faction.def);

            this.BeardDef = PawnFaceChooser.RandomBeardDefFor(this.pawn, this.pawn.Faction.def);

            this.HairColorOrg = this.pawn.story.hairColor;

            this.sameBeardColor = Rand.Value > 0.2f;
            if (this.sameBeardColor)
                this.BeardColor = _PawnHairColors.DarkerBeardColor(this.pawn.story.hairColor);
            else
                this.BeardColor = _PawnHairColors.RandomBeardColor();

            this.optimized = true;

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
                    this.BeardColor = _PawnHairColors.DarkerBeardColor(this.pawn.story.hairColor);
                else
                    this.BeardColor = _PawnHairColors.RandomBeardColor();
            }

            if (this.MouthDef == null)
            {
                MouthDef = MouthDefOf.Mouth_Default;
            }


            this.isOld = this.pawn.ageTracker.AgeBiologicalYearsFloat >= 50f;

            Color wrinkleColor = Color.Lerp(
                this.pawn.story.SkinColor,
                this.pawn.story.SkinColor * this.pawn.story.SkinColor,
                Mathf.InverseLerp(50f, 100f, this.pawn.ageTracker.AgeBiologicalYearsFloat));

            this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.WrinkleDef.texPath + "_" + this.crownType + "_" + this.headType,
                ShaderDatabase.Transparent,
                Vector2.one,
                wrinkleColor);

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

            this.mouthGraphic = FacialGraphics.MouthGraphic3;

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
                    this.hasLeftEyePatch = true;
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
                    this.hasRightEyePatch = true;
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

        // todo: eyes closed when anaesthetic

        // eyes closed time = > consciousness?
        // tiredness affect blink rate
        // Method used to animate the eye movement

        private CellRect _viewRect = new CellRect();

        private bool LeftCanBlink;

        private bool RightCanBlink;

        private float mood = 0.5f;

        public object rotationInt;

        public float headTypeX;

        public float headTypeY;

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

            _viewRect = Find.CameraDriver.CurrentViewRect;
            _viewRect = _viewRect.ExpandedBy(5);

            if (!_viewRect.Contains(this.pawn.Position))
                return;

            int tickManagerTicksGame = Find.TickManager.TicksGame;
            float x = Mathf.InverseLerp(this.lastBlinkended, this.nextBlink, tickManagerTicksGame);
            float movePixel = 0f;
            float movePixelY = 0f;

            if (this.moveX || this.moveY)
            {
                if (this.moveX)
                {
                    if (this.halfAnimX)
                    {
                        movePixel = EyeMotionHalfCurve.Evaluate(x) * this.factorX;
                    }
                    else
                    {
                        movePixel = EyeMotionFullCurve.Evaluate(x) * this.factorX;
                    }
                }

                if (this.moveY)
                {
                    if (this.halfAnimY)
                    {
                        movePixelY = EyeMotionHalfCurve.Evaluate(x) * this.factorY;
                    }
                    else
                    {
                        movePixelY = EyeMotionFullCurve.Evaluate(x) * this.factorY;
                    }
                }

                if (this.RightCanBlink)
                    this.eyemoveR = new Vector3(movePixel * this.flippedX, 0, movePixelY * this.flippedY);
                if (this.LeftCanBlink)
                    this.eyemoveL = new Vector3(movePixel * this.flippedX, 0, movePixelY * this.flippedY);
            }

            // float moveX = (float)Math.Sin(Find.TickManager.TicksGame * 0.1f) * this.factorX;
            // float moveY = (float)Math.Cos(Find.TickManager.TicksGame * 0.1f) * this.factorY;
            if (tickManagerTicksGame > this.nextBlinkEnd)
            {
                float ticksTillNextBlink = Rand.Range(60f, 240f);
                float blinkDuration = Rand.Range(5f, 35f);

                //  Log.Message(
                //      "FS Blinker: " + this.pawn + " - ticksTillNextBlinkORG: " + ticksTillNextBlink.ToString("N0")
                //      + " - blinkDurationORG: " + blinkDuration.ToString("N0"));

                float dynamic = this.pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
                float factor = Mathf.Lerp(0.125f, 1f, dynamic);

                float dynamic2 = this.pawn.needs.rest.CurLevel;
                float factor2 = Mathf.Lerp(0.125f, 1f, dynamic2);

                ticksTillNextBlink *= factor * factor2;
                blinkDuration /= factor * factor * factor2;

                //    Log.Message(
                //        "FS Blinker: " + this.pawn + " - Consc: " + dynamic.ToStringPercent() + " - factorC: " + factor.ToString("N2") + " - Rest: "
                //        + dynamic2.ToStringPercent() + " - factorR: " + factor2.ToString("N2") + " - ticksTillNextBlink: " + ticksTillNextBlink.ToString("N0")
                //        + " - blinkDuration: " + blinkDuration.ToString("N0"));

                this.nextBlink = (int)(tickManagerTicksGame + ticksTillNextBlink);
                this.nextBlinkEnd = (int)(this.nextBlink + blinkDuration);

                if (this.pawn.CurJob != null && this.pawn.jobs.curDriver.asleep)
                {
                    this.asleep = true;
                    return;
                }

                this.asleep = false;

                // this.jitterLeft = 1f;
                // this.jitterRight = 1f;

                // blinkRate = Mathf.Lerp(2f, 0.25f, this.pawn.needs.rest.CurLevel);


                // range *= (int)blinkRate;
                // blinkDuration /= (int)this.blinkRate;
                if (Rand.Value > 0.9f)
                {
                    // early "nerous" blinking. I guss positive values have no effect ...
                    this.jitterLeft = (int)Rand.Range(-10f, 90f);
                    this.jitterRight = (int)Rand.Range(-10f, 90f);
                }
                else
                {
                    this.jitterLeft = 0;
                    this.jitterRight = 0;
                }

                // only animate eye movement if animation lasts at least 2.5 seconds
                if (ticksTillNextBlink > 80f)
                {
                    this.moveX = Rand.Value > 0.7f;
                    this.moveY = Rand.Value > 0.85f;
                    this.halfAnimX = Rand.Value > 0.3f;
                    this.halfAnimY = Rand.Value > 0.3f;
                    this.flippedX = Rand.Range(-1f, 1f);
                    this.flippedY = Rand.Range(-1f, 1f);
                }
                else
                {
                    this.moveX = this.moveY = false;
                }

                this.lastBlinkended = tickManagerTicksGame;
                if (this.pawn.needs != null)
                {
                    this.mood = this.pawn.needs.mood.CurLevel;

                }
                else this.mood = 0.5f;

                if (this.mood != null)
                {
                    if (mood > 0.9f) mouthGraphic = FacialGraphics.MouthGraphic1;
                    else if (mood > 0.8f) mouthGraphic = FacialGraphics.MouthGraphic2;
                    else if (mood > 0.7f) mouthGraphic = FacialGraphics.MouthGraphic3;
                    else if (mood > 0.55f) mouthGraphic = FacialGraphics.MouthGraphic4;
                    else if (mood > 0.45f) mouthGraphic = FacialGraphics.MouthGraphic5;
                    else if (mood > 0.35f) mouthGraphic = FacialGraphics.MouthGraphic6;
                    else mouthGraphic = FacialGraphics.MouthGraphic7;
                }
                else mouthGraphic = FacialGraphics.MouthGraphic3;

            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref this.pawn, "Pawn");

            Scribe_Defs.Look(ref this.EyeDef, "EyeDef");
            Scribe_Defs.Look(ref this.BrowDef, "BrowDef");
            //      Scribe_Defs.Look(ref this.MouthDef, "MouthDef");
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

        #endregion

    }
}