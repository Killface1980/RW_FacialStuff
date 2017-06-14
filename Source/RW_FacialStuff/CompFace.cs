using static RW_FacialStuff.GraphicDatabaseHeadRecordsModded;
using static RW_FacialStuff.Headhelper;

namespace RW_FacialStuff
{
    using System;
    using System.IO;

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

        public BeardDef BeardDef;

        public BrowDef BrowDef;

        public Graphic_Multi DissicatedHeadGraphic;

        public bool drawMouth = true;

        public EyeDef EyeDef;

        public Color HairColorOrg;

        public Graphic HairCutGraphic;

        public string headGraphicIndex;

        public MouthDef MouthDef;

        public bool optimized;

        public bool sessionOptimized;

        public string SkinColorHex;

        public string type = null;

        public WrinkleDef WrinkleDef;

        private Graphic beardGraphic;

        private Graphic browGraphic;

        private Graphic eyeGraphic;

        private Graphic mouthGraphic;

        private Graphic wrinkleGraphic;



        private Texture2D finalHeadFront;
        private Texture2D finalHeadSide;
        private Texture2D finalHeadBack;


        private Graphic_Multi headGraphicVanilla;
        private Graphic_Multi dissicatedHeadGraphicVanilla;



        public bool isOld;

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
                newType = "Normal";
            }

            if (this.pawn.story.HeadGraphicPath.Contains("Pointy"))
            {
                newType = "Pointy";
            }

            if (this.pawn.story.HeadGraphicPath.Contains("Wide"))
            {
                newType = "Wide";
            }

            if (this.type == null || this.type != newType)
            {
                this.type = newType;
            }

            return true;
        }

        public bool InitializeGraphics()
        {
            if (this.pawn == null)
            {
                return false;
            }
            if (this.pawn.gender == Gender.Female && this.BeardDef == null)
            {
                this.BeardDef = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");
            }

            string suffix = "_Average";

            switch (this.pawn.story.crownType)
            {
                case CrownType.Narrow:
                    suffix = "_Narrow";
                    break;
            }


            this.isOld = this.pawn.ageTracker.AgeBiologicalYearsFloat >= 50f;


            var wrinkleColor = Color.Lerp(pawn.story.SkinColor, this.pawn.story.SkinColor * Color.gray, Mathf.InverseLerp(50f, 100f, pawn.ageTracker.AgeBiologicalYearsFloat));


            if (this.type == "Normal")
            {
                this.beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.BeardDef.texPathAverageNormal + suffix,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    this.pawn.story.hairColor);
                this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.WrinkleDef.texPathAverageNormal + suffix,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    wrinkleColor);
            }

            if (this.type == "Pointy")
            {
                this.beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.BeardDef.texPathAveragePointy + suffix,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    this.pawn.story.hairColor);
                this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.WrinkleDef.texPathAveragePointy + suffix,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    wrinkleColor);
            }

            if (this.type == "Wide")
            {
                this.beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.BeardDef.texPathAverageWide + suffix,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    this.pawn.story.hairColor);
                this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.WrinkleDef.texPathAverageWide + suffix,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    wrinkleColor);
            }

            this.eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.EyeDef.texPath + suffix,
                ShaderDatabase.CutoutSkin,
                Vector2.one,
                Color.white);

            Color darkenColor = new Color(0.2f, 0.2f, 0.2f);

            this.browGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.BrowDef.texPath + suffix,
                ShaderDatabase.CutoutSkin,
                Vector2.one,
                this.pawn.story.hairColor * darkenColor);

            this.mouthGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.MouthDef.texPath + suffix,
                ShaderDatabase.CutoutSkin,
                Vector2.one,
                this.pawn.story.SkinColor * darkenColor);


            return true;

        }



        // Verse.PawnGraphicSet
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

        public Material EyeMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            Material material = null;
            if (bodyCondition == RotDrawMode.Fresh)
            {
                material = this.eyeGraphic.MatAt(facing, null);
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                material = this.eyeGraphic.MatAt(facing, null);
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
            bool flag = this.pawn.gender == Gender.Female;
            if (FS_Settings.UseMouth && (flag || !flag && this.BeardDef.drawMouth) && this.drawMouth)
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
                Log.Error("Unknown crown type: " + this.pawn.story.crownType);
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
            Scribe_Values.Look(ref this.type, "type");
            Scribe_Values.Look(ref this.SkinColorHex, "SkinColorHex");
            Scribe_Values.Look(ref this.HairColorOrg, "HairColorOrg");
        }
    }
}
