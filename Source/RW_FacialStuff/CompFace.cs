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

        public bool sessionOptimized;

        public string SkinColorHex;

        public string headTypeSuffix = "_Normal";

        public WrinkleDef WrinkleDef;

        private Graphic beardGraphic;

        private Graphic browGraphic;

        private Graphic eyeGraphic;

        private Graphic mouthGraphic;

        private Graphic wrinkleGraphic;

        //     private Graphic bionicGraphic;


        public bool isOld;

        public bool hasBionic;

        private string BionicEye_texPath;

        private int BionicEye_place;

        private Graphic eyesClosedGraphic;

        private int nextBlink;

        private int nextBlinkEnd;

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
            if (this.MouthDef == null) this.MouthDef = DefDatabase<MouthDef>.GetNamed("Mouth_Female_Default");

            // bool flag1 = false;
            // bool flag2 = false;
            // this.hasBionic = false;
            // foreach (Hediff hediff in this.pawn.health.hediffSet.hediffs)
            // {
            //     AddedBodyPartProps addedPartProps = hediff.def.addedPartProps;
            //     if (addedPartProps != null && addedPartProps.isBionic)
            //     {
            //         if (hediff.Part.def == BodyPartDefOf.LeftEye) flag1 = true;
            //
            //         if (hediff.Part.def == BodyPartDefOf.RightEye) flag2 = true;
            //         this.hasBionic = true;
            //     }
            //
            // }
            // if (flag1 || flag2)
            // {
            //
            //     if (flag1 && flag2)
            //     {
            //         this.BionicEye_texPath = "AddedParts/BionicEye_Both";
            //         this.BionicEye_place = 2;
            //     }
            //     else if (flag1)
            //     {
            //         this.BionicEye_texPath = "AddedParts/BionicEye_Left";
            //         this.BionicEye_place = 0;
            //     }
            //     else
            //     {
            //         this.BionicEye_texPath = "AddedParts/BionicEye_Right";
            //         this.BionicEye_place = 1;
            //     }
            // }

            return true;
        }

        public bool InitializeGraphics()
        {
            if (this.pawn == null)
            {
                return false;
            }



            this.isOld = this.pawn.ageTracker.AgeBiologicalYearsFloat >= 50f;


            var wrinkleColor = Color.Lerp(this.pawn.story.SkinColor, this.pawn.story.SkinColor * Color.gray, Mathf.InverseLerp(50f, 100f, this.pawn.ageTracker.AgeBiologicalYearsFloat));

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

            this.eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.EyeDef.texPath + this.crownTypeSuffix,
                ShaderDatabase.Transparent,
                Vector2.one,
                Color.white);

            this.eyesClosedGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                "Eyes/Eyes_Closed",
                ShaderDatabase.Transparent,
                Vector2.one,
                Color.white);

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


            // if (hasBionic)
            // {
            //     this.bionicGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(BionicEye_texPath, ShaderDatabase.Transparent, Vector2.one, Color.white);
            // }

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

        public Material EyeMatAt(Rot4 facing, bool portrait, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            Material material = null;
            if (bodyCondition == RotDrawMode.Fresh)
            {
                bool flag = true;
                if (portrait)
                {
                    material = this.eyeGraphic.MatAt(facing, null);
                    material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
                    return material;
                }
                //                if (this.pawn.GetPosture() == PawnPosture.LayingAny)

                if (this.pawn.CurJob != null && this.pawn.jobs.curDriver.asleep || this.pawn.Dead)
                {
                    flag = false;
                    material = this.eyesClosedGraphic.MatAt(facing, null);
                }
                if (flag)
                {
                    if (Find.TickManager.TicksGame >= this.nextBlink)
                    {
                        material = this.eyesClosedGraphic.MatAt(facing, null);
                        if (Find.TickManager.TicksGame > this.nextBlinkEnd)
                        {
                            nextBlink = Find.TickManager.TicksGame + (int)Rand.Range(30f, 240f);
                            this.nextBlinkEnd = this.nextBlink + (int)Rand.Range(5f, 20f);
                        }
                    }
                    else
                    {
                        material = this.eyeGraphic.MatAt(facing, null);
                    }
                }
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                material = this.eyesClosedGraphic.MatAt(facing, null);
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

        // public Material BionicMatAt(Rot4 facing)
        // {
        //     Material material = this.bionicGraphic.MatAt(facing, null);
        //
        //     if (this.BionicEye_place == 1)
        //     {
        //         if (facing == Rot4.South)
        //         {
        //             material = this.bionicGraphic.MatAt(facing, null);
        //         }
        //     }
        //     if (material != null)
        //     {
        //         material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
        //     }
        //
        //     return material;
        // }

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
