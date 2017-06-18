namespace RW_FacialStuff
{
    using System.Collections.Generic;

    using RimWorld;

    using RW_FacialStuff.Defs;
    using RW_FacialStuff.Detouring;

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
                                                                         // { new CurvePoint(0.5f, 0f), true },
                                                                         // { new CurvePoint(0.7f, 1f), true },
                                                                         {
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


        #region Fields

        public Vector3 eyemove = new Vector3(0, 0, 0);
        private Pawn pawn;

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
            if (portrait)
            {
                material = this.leftEyeGraphic.MatAt(facing, null);

                if (material != null)
                {
                    material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
                }

                return material;
            }

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
                else
                {
                    material = this.leftEyeGraphic.MatAt(facing, null);
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
            if (portrait)
            {
                material = this.rightEyeGraphic.MatAt(facing, null);

                if (material != null)
                {
                    material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
                }

                return material;
            }

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
                else
                {
                    material = this.rightEyeGraphic.MatAt(facing, null);
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
        public string crownTypeSuffix = "_Average";

        public string headTypeSuffix = "_Normal";

        private string SkinColorHex;

        private string headGraphicIndex;
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

        #endregion


        #endregion

        #region Methods

        public bool SetHeadType()
        {
            this.pawn = this.parent as Pawn;

            if (this.pawn == null)
            {
                return false;
            }

            string newType = null;

            if (this.pawn.story.HeadGraphicPath.Contains("Normal"))
            {
                newType = "_Normal";
            }

            if (this.pawn.story.HeadGraphicPath.Contains("Pointy"))
            {
                newType = "_Pointy";
            }

            if (this.pawn.story.HeadGraphicPath.Contains("Wide"))
            {
                newType = "_Wide";
            }

            if (this.headTypeSuffix == null || this.headTypeSuffix != newType)
            {
                this.headTypeSuffix = newType;
            }

            if (this.pawn.story.crownType == CrownType.Narrow)
            {
                this.crownTypeSuffix = "_Narrow";
            }
            else
            {
                this.crownTypeSuffix = "_Average";
            }

            this.hasLeftEyePatch = false;
            this.hasRightEyePatch = false;
            this.LeftIsSolid = false;
            this.RightIsSolid = false;

            this.leftEyePatchTexPath = null;
            this.rightEyePatchTexPath = null;

            this.rightEyeTexPath = "Eyes/Eye_" + this.pawn.gender + this.crownTypeSuffix + "_" + this.EyeDef.texPath
                                   + "_Right";

            this.rightEyeClosedTexPath = "Eyes/Eye_" + this.pawn.gender + this.crownTypeSuffix + "_Closed_Right";

            this.leftEyeTexPath = "Eyes/Eye_" + this.pawn.gender + this.crownTypeSuffix + "_" + this.EyeDef.texPath
                                  + "_Left";

            this.leftEyeClosedTexPath = "Eyes/Eye_" + this.pawn.gender + this.crownTypeSuffix + "_Closed_Left";

            foreach (Hediff hediff in this.pawn.health.hediffSet.hediffs)
            {
                AddedBodyPartProps addedPartProps = hediff.def.addedPartProps;
                if (addedPartProps != null)
                {
                    List<BodyPartRecord> body = this.pawn.RaceProps.body.AllParts;

                    BodyPartRecord leftEye = body.Find(x => x.def == BodyPartDefOf.LeftEye);
                    BodyPartRecord rightEye = body.Find(x => x.def == BodyPartDefOf.RightEye);

                    // BodyPartRecord jaw = body.Find(x => x.def == BodyPartDefOf.Jaw);

                    // if (hediff.Part == jaw)
                    // {
                    // this.ExtraJaw_texPath = "AddedParts/" + hediff.def.defName +  this.crownTypeSuffix;
                    // }
                    // BodyPartRecord leftEye = this.pawn.RaceProps.body.GetPartAtIndex(27);
                    // BodyPartRecord rightEye = this.pawn.RaceProps.body.GetPartAtIndex(28);
                    if (hediff.Part == leftEye)
                    {
                        this.leftEyePatchTexPath = "AddedParts/" + hediff.def.defName + "_Left" + this.crownTypeSuffix;
                        this.LeftIsSolid = addedPartProps.isSolid;
                    }

                    if (hediff.Part == rightEye)
                    {
                        this.rightEyePatchTexPath = "AddedParts/" + hediff.def.defName + "_Right"
                                                    + this.crownTypeSuffix;
                        this.RightIsSolid = addedPartProps.isSolid;
                    }
                }
            }

            return true;
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
                MouthDef = DefDatabase<MouthDef>.GetNamed("Mouth_Female_Default");
            }


            this.isOld = this.pawn.ageTracker.AgeBiologicalYearsFloat >= 50f;

            Color wrinkleColor = Color.Lerp(
                this.pawn.story.SkinColor,
                this.pawn.story.SkinColor * this.pawn.story.SkinColor,
                Mathf.InverseLerp(50f, 100f, this.pawn.ageTracker.AgeBiologicalYearsFloat));

            this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.WrinkleDef.texPath + this.crownTypeSuffix + this.headTypeSuffix,
                ShaderDatabase.Transparent,
                Vector2.one,
                wrinkleColor);

            string path = this.BeardDef.texPath + this.crownTypeSuffix + this.headTypeSuffix;

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
                this.BrowDef.texPath + this.crownTypeSuffix,
                ShaderDatabase.Transparent,
                Vector2.one,
                Color.black);

            this.mouthGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.MouthDef.texPath + this.crownTypeSuffix,
                ShaderDatabase.Transparent,
                Vector2.one,
                Color.black);

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
                                      Color.black) as Graphic_Multi_NaturalEyes;
            this.leftEyeClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                            this.leftEyeClosedTexPath,
                                            ShaderDatabase.Transparent,
                                            Vector2.one,
                                            Color.black) as Graphic_Multi_NaturalEyes;

            this.rightEyeGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                       this.rightEyeTexPath,
                                       ShaderDatabase.Transparent,
                                       Vector2.one,
                                       Color.black) as Graphic_Multi_NaturalEyes;
            this.rightEyeClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                             this.rightEyeClosedTexPath,
                                             ShaderDatabase.Transparent,
                                             Vector2.one,
                                             Color.black) as Graphic_Multi_NaturalEyes;

            return true;
        }

        // todo: eyes closed when anaesthetic

        // eyes closed time = > consciousness
        // Method used to animate the eye movement
        public override void PostDraw()
        {
            base.PostDraw();
            if (!this.pawn.Spawned)
            {
                return;
            }

            int tickManagerTicksGame = Find.TickManager.TicksGame;
            var x = Mathf.InverseLerp(this.lastBlinkended, this.nextBlink, tickManagerTicksGame);
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

                this.eyemove = new Vector3(movePixel * this.flippedX, 0, movePixelY * this.flippedY);
            }

            // float moveX = (float)Math.Sin(Find.TickManager.TicksGame * 0.1f) * this.factorX;
            // float moveY = (float)Math.Cos(Find.TickManager.TicksGame * 0.1f) * this.factorY;
            if (tickManagerTicksGame > this.nextBlinkEnd)
            {
                float range = Rand.Range(20f, 180f);
                float blinkDuration = Rand.Range(5f, 35f);

                this.nextBlink = (int)(tickManagerTicksGame + range);
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
                if (range > 80f)
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
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref this.pawn, "Pawn");

            Scribe_Defs.Look(ref this.EyeDef, "EyeDef");
            Scribe_Defs.Look(ref this.BrowDef, "BrowDef");
            Scribe_Defs.Look(ref this.MouthDef, "MouthDef");
            Scribe_Defs.Look(ref this.WrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref this.BeardDef, "BeardDef");
            Scribe_Values.Look(ref this.optimized, "optimized");
            Scribe_Values.Look(ref this.drawMouth, "drawMouth");

            Scribe_Values.Look(ref this.headGraphicIndex, "headGraphicIndex");
            Scribe_Values.Look(ref this.headTypeSuffix, "headTypeSuffix");
            Scribe_Values.Look(ref this.crownTypeSuffix, "crownTypeSuffix");
            Scribe_Values.Look(ref this.SkinColorHex, "SkinColorHex");
            Scribe_Values.Look(ref this.HairColorOrg, "HairColorOrg");
            Scribe_Values.Look(ref this.BeardColor, "BeardColor");
            Scribe_Values.Look(ref this.sameBeardColor, "sameBeardColor");



        }

        #endregion

    }
}