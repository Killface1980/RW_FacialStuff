using static RW_FacialStuff.GraphicDatabaseHeadRecordsModded;
using static RW_FacialStuff.Headhelper;

namespace RW_FacialStuff
{
    using System;
    using System.IO;

    using RimWorld;

    using RW_FacialStuff.Defs;

    using UnityEngine;

    using Verse;

    using Object = UnityEngine.Object;

    public class CompFace : ThingComp
    {
        public override void Initialize(CompProperties props)
        {
            this.props = props;
        }

        private Pawn pawn;

        public BeardDef BeardDef = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");

        public BrowDef BrowDef;

        public string crownTypeSuffix = "_Average";

        public bool drawMouth = true;

        public EyeDef EyeDef;

        public Color HairColorOrg;

        public Graphic HairCutGraphic;

        public string headGraphicIndex;

        public MouthDef MouthDef = DefDatabase<MouthDef>.GetNamed("Mouth_Female_Default");

        public bool optimized;

        public string SkinColorHex;

        public string headTypeSuffix = "_Normal";

        public WrinkleDef WrinkleDef;

        private Graphic beardGraphic;

        private Graphic browGraphic;

        private Graphic mouthGraphic;

        private Graphic wrinkleGraphic;

        private Graphic_Multi_NaturalEyes leftEyeGraphic = null;

        private Graphic_Multi_NaturalEyes rightEyeGraphic = null;

        private Graphic_Multi_EyeWear leftEyePatchGraphic = null;

        private Graphic_Multi_EyeWear rightEyePatchGraphic = null;

        public bool isOld;

        private string LeftEyePatch_texPath = null;

        private string RightEyePatch_texPath = null;

        private int nextBlink = -5000;

        private int nextBlinkEnd = -5000;


        public bool hasLeftEyePatch = false;

        public bool hasRightEyePatch = false;

        private Graphic_Multi_NaturalEyes leftEyeClosedGraphic;

        private Graphic_Multi_NaturalEyes rightEyeClosedGraphic;

        private string RightEye_texPath;

        private string RightEyeClosed_texPath;

        private string LeftEyeClosed_texPath;

        private string LeftEye_texPath;

        public bool leftIsSolid;

        public bool rightIsSolid;

        //     private float blinkRate;

        private int jitterLeft = 0;
        private int jitterRight = 0;

        private Graphic deadEyeGraphic;

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

            this.optimized = true;
        }

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

            // weird bug: no MouthDef defined for some visitors (male, 17)
            if (this.MouthDef == null)
            {
                this.MouthDef = DefDatabase<MouthDef>.GetNamed("Mouth_Female_Default");
            }

            this.hasLeftEyePatch = false;
            this.hasRightEyePatch = false;
            this.leftIsSolid = false;
            this.rightIsSolid = false;

            this.LeftEyePatch_texPath = null;
            this.RightEyePatch_texPath = null;

            this.RightEye_texPath = "Eyes/Eye_" + this.pawn.gender + this.crownTypeSuffix + "_" + this.EyeDef.texPath + "_Right";

            this.RightEyeClosed_texPath = "Eyes/Eye_" + this.pawn.gender + this.crownTypeSuffix + "_Closed_Right";

            this.LeftEye_texPath = "Eyes/Eye_" + this.pawn.gender + this.crownTypeSuffix + "_" + this.EyeDef.texPath + "_Left";

            this.LeftEyeClosed_texPath = "Eyes/Eye_" + this.pawn.gender + this.crownTypeSuffix + "_Closed_Left";

            foreach (Hediff hediff in this.pawn.health.hediffSet.hediffs)
            {
                AddedBodyPartProps addedPartProps = hediff.def.addedPartProps;
                if (addedPartProps != null)
                {
                    BodyPartRecord leftEye = this.pawn.RaceProps.body.GetPartAtIndex(27);
                    BodyPartRecord rightEye = this.pawn.RaceProps.body.GetPartAtIndex(28);

                    if (hediff.Part == leftEye)
                    {
                        this.LeftEyePatch_texPath = "AddedParts/" + hediff.def.defName + "_Left" + this.crownTypeSuffix;
                        this.leftIsSolid = addedPartProps.isSolid;
                    }

                    if (hediff.Part == rightEye)
                    {
                        this.RightEyePatch_texPath = "AddedParts/" + hediff.def.defName + "_Right" + this.crownTypeSuffix;
                        this.rightIsSolid = addedPartProps.isSolid;
                    }
                }

            }



            return true;
        }

        public bool InitializeGraphics()
        {
            if (this.pawn == null)
            {
                return false;
            }

            this.isOld = this.pawn.ageTracker.AgeBiologicalYearsFloat >= 50f;


            Color wrinkleColor = Color.Lerp(this.pawn.story.SkinColor, this.pawn.story.SkinColor * Color.gray, Mathf.InverseLerp(50f, 100f, this.pawn.ageTracker.AgeBiologicalYearsFloat));

            this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.WrinkleDef.texPath + this.crownTypeSuffix + this.headTypeSuffix,
                ShaderDatabase.Transparent,
                Vector2.one,
                wrinkleColor);

            string path = this.BeardDef.texPath + this.crownTypeSuffix + this.headTypeSuffix;

            if (this.BeardDef == DefDatabase<BeardDef>.GetNamed("Beard_Shaved"))
            {
                path = this.BeardDef.texPath;
            }

            this.beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    path,
                    ShaderDatabase.Transparent,
                    Vector2.one,
                    this.pawn.story.hairColor);

            this.browGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.BrowDef.texPath + this.crownTypeSuffix,
                ShaderDatabase.Transparent,
                Vector2.one,
                Color.black);

            this.mouthGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.MouthDef.texPath + this.crownTypeSuffix,
                ShaderDatabase.Transparent,
                Vector2.one,
                Color.black);


            this.deadEyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>("Eyes/Eyes_Dead", ShaderDatabase.Transparent, Vector2.one, Color.white);
           if (this.LeftEyePatch_texPath != null)
           {
               this.leftEyePatchGraphic = GraphicDatabase.Get<Graphic_Multi_EyeWear>(this.LeftEyePatch_texPath, ShaderDatabase.Transparent, Vector2.one, Color.white) as Graphic_Multi_EyeWear;
               if (this.leftEyePatchGraphic != null)
               {
                   this.hasLeftEyePatch = true;
               }
           }
           if (this.RightEyePatch_texPath != null)
           {
               this.rightEyePatchGraphic = GraphicDatabase.Get<Graphic_Multi_EyeWear>(this.RightEyePatch_texPath, ShaderDatabase.Transparent, Vector2.one, Color.white) as Graphic_Multi_EyeWear;
               if (this.rightEyePatchGraphic != null)
               {
                   this.hasRightEyePatch = true;
               }
           }

            this.leftEyeGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(this.LeftEye_texPath, ShaderDatabase.Transparent, Vector2.one, Color.white) as Graphic_Multi_NaturalEyes;
            this.leftEyeClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(this.LeftEyeClosed_texPath, ShaderDatabase.Transparent, Vector2.one, Color.white) as Graphic_Multi_NaturalEyes;



            this.rightEyeGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(this.RightEye_texPath, ShaderDatabase.Transparent, Vector2.one, Color.white) as Graphic_Multi_NaturalEyes;
            this.rightEyeClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(this.RightEyeClosed_texPath, ShaderDatabase.Transparent, Vector2.one, Color.white) as Graphic_Multi_NaturalEyes;


            return true;

        }


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

        public Material HairCutMatAt(Rot4 facing)
        {
            if (!FS_Settings.MergeHair)
            {
                return null;
            }

            Material material = this.HairCutGraphic.MatAt(facing, null);

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (!pawn.Spawned) return;

            if (Find.TickManager.TicksGame > this.nextBlinkEnd)
            {
                //     this.jitterLeft = 1f;
                //     this.jitterRight = 1f;

                //     blinkRate = Mathf.Lerp(2f, 0.25f, this.pawn.needs.rest.CurLevel);

                float range = Rand.Range(20f, 180f);
                float blinkDuration = Rand.Range(5f, 35f);
                //
                //  range *= (int)blinkRate;
                //  blinkDuration /= (int)this.blinkRate;

                nextBlink = (int)(Find.TickManager.TicksGame + range);
                this.nextBlinkEnd = (int)(this.nextBlink + blinkDuration);

                if (Rand.Value > 0.9f)
                {
                    this.jitterLeft = (int)Rand.Range(-10f, 90f);
                    this.jitterRight = (int)Rand.Range(-10f, 90f);
                }
                else
                {
                    this.jitterLeft = 0;
                    this.jitterRight = 0;
                }
            }
        }

        public Material LeftEyeMatAt(Rot4 facing, bool portrait)
        {
            Material material = null;

            bool flag = true;
            if (portrait)
            {
                material = this.leftEyeGraphic.MatAt(facing, null);

                if (material != null)
                    material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
                return material;
            }

            if (this.pawn.CurJob != null && this.pawn.jobs.curDriver.asleep)
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

        public Material RightEyeMatAt(Rot4 facing, bool portrait)
        {
            Material material = null;
            bool flag = true;
            if (portrait)
            {
                material = this.rightEyeGraphic.MatAt(facing, null);

                if (material != null)
                    material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
                return material;
            }

            if (this.pawn.CurJob != null && this.pawn.jobs.curDriver.asleep)
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

        public Material DeadEyeMatAt(Rot4 facing, bool portrait, RotDrawMode bodyCondition = RotDrawMode.Fresh)
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

        public Material LeftEyePatchMatAt(Rot4 facing)
        {

            Material material = this.leftEyePatchGraphic.MatAt(facing, null);

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
        #endregion


        // Verse.PawnGraphicSet
        public GraphicMeshSet HeadMeshSet
        {
            get
            {
                if (this.pawn.story.crownType == CrownType.Average)
                {
                    return MeshPoolFs.humanlikeHeadSetAverage;
                }

                if (this.pawn.story.crownType == CrownType.Narrow)
                {
                    return MeshPoolFs.humanlikeHeadSetNarrow;
                }

                Log.Error("Unknown crown headTypeSuffix: " + this.pawn.story.crownType);
                return MeshPool.humanlikeHeadSet;
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
        }
    }
}
