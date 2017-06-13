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


        public static void ScaleTexture(Texture2D sourceTex, out Texture2D destTex, int targetWidth, int targetHeight)
        {

            float warpFactorX = 1f;
            float warpFactorY = 1f;
            Color[] destPix;

            Texture2D scaleTex = MakeReadable(sourceTex);

            destTex = new Texture2D(targetWidth, targetHeight, TextureFormat.ARGB32, false);
            destPix = new Color[destTex.width * destTex.height];
            int y = 0;
            while (y < destTex.height)
            {
                int x = 0;
                while (x < destTex.width)
                {
                    float xFrac = x * 1.0F / (destTex.width - 1);
                    float yFrac = y * 1.0F / (destTex.height - 1);
                    float warpXFrac = Mathf.Pow(xFrac, warpFactorX);
                    float warpYFrac = Mathf.Pow(yFrac, warpFactorY);
                    destPix[y * destTex.width + x] = scaleTex.GetPixelBilinear(warpXFrac, warpYFrac);
                    x++;
                }
                y++;
            }
            destTex.SetPixels(destPix);
            destTex.Apply();
            Object.Destroy(scaleTex);

            // try
            // {
            //     ScaledTexDict.Add(xx, destTex);
            // }
            // catch (ArgumentNullException argumentNullException)
            // {
            // }
        }


        public bool InitializeGraphics()
        {

            if (this.pawn == null)
            {
                return false;
            }

            this.isOld = this.pawn.ageTracker.AgeBiologicalYearsFloat >= 50f;

            // Create the blank canvas texture
            if (BlankTex == null)
            {
                BlankTex = new Texture2D(128, 128);

                // Reset all pixels color to transparent
                Color32 resetColor = Color.clear;
                Color32[] resetColorArray = BlankTex.GetPixels32();

                for (int i = 0; i < resetColorArray.Length; i++)
                {
                    resetColorArray[i] = resetColor;
                }

                BlankTex.SetPixels32(resetColorArray);
                BlankTex.Apply();
            }
            Color wrinkleColor = Color.gray * this.pawn.story.SkinColor;
            wrinkleColor = Color.Lerp(pawn.story.SkinColor, Color.black, Mathf.InverseLerp(50f, 100f, pawn.ageTracker.AgeBiologicalYearsFloat));


            if (this.type == "Normal")
            {
                this.beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.BeardDef.texPathAverageNormal,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    this.pawn.story.hairColor);
                this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.WrinkleDef.texPathAverageNormal,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    wrinkleColor);
            }

            if (this.type == "Pointy")
            {
                this.beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.BeardDef.texPathAveragePointy,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    this.pawn.story.hairColor);
                this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.WrinkleDef.texPathAveragePointy,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    wrinkleColor);
            }

            if (this.type == "Wide")
            {
                this.beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.BeardDef.texPathAverageWide,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    this.pawn.story.hairColor);
                this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.WrinkleDef.texPathAverageWide,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    wrinkleColor);
            }

            this.eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.EyeDef.texPath,
                ShaderDatabase.Cutout,
                Vector2.one,
                Color.white);
            this.browGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.BrowDef.texPath,
                ShaderDatabase.Cutout,
                Vector2.one,
                this.pawn.story.hairColor * new Color(0.3f, 0.3f, 0.3f));

            this.mouthGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.MouthDef.texPath,
                ShaderDatabase.Cutout,
                Vector2.one,
                this.pawn.story.SkinColor);

            if (this.pawn.gender == Gender.Female && this.BeardDef == null)
            {
                this.BeardDef = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");
            }
            return true;

        }



        // Verse.PawnGraphicSet
        public Material BeardMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false)
        {
            Material material = null;
            if (this.pawn.gender == Gender.Male)
            {
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    if (stump)
                    {
                        material = null;
                    }
                    else
                    {
                        material = this.beardGraphic.MatAt(facing, null);
                    }
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    if (stump)
                    {
                        material = null;
                    }
                    else
                    {
                        material = this.beardGraphic.MatAt(facing, null);
                    }
                }

                if (material != null)
                {
                    material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
                }
            }
            return material;
        }

        public Material HairCutMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false)
        {
            if (!FS_Settings.MergeHair)
            {
                return null;
            }

            Material material = null;
            if (bodyCondition == RotDrawMode.Fresh)
            {
                if (stump)
                {
                    material = null;
                }
                else
                {
                    material = this.HairCutGraphic.MatAt(facing, null);
                }
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                if (stump)
                {
                    material = null;
                }
                else
                {
                    material = this.HairCutGraphic.MatAt(facing, null);
                }
            }

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }
            return material;
        }

        public Material EyeMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false)
        {
            Material material = null;
            if (bodyCondition == RotDrawMode.Fresh)
            {
                if (stump)
                {
                    material = null;
                }
                else
                {
                    material = this.eyeGraphic.MatAt(facing, null);
                }
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                if (stump)
                {
                    material = null;
                }
                else
                {
                    // dead staring eyes maybe?
                    material = this.eyeGraphic.MatAt(facing, null);
                }
            }

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }
            return material;
        }

        public Material BrowMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false)
        {
            Material material = null;
            if (bodyCondition == RotDrawMode.Fresh)
            {
                if (stump)
                {
                    material = null;
                }
                else
                {
                    material = this.browGraphic.MatAt(facing, null);
                }
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                if (stump)
                {
                    material = null;
                }
                else
                {
                    // dead staring eyes maybe?
                    material = this.browGraphic.MatAt(facing, null);
                }
            }

            if (material != null)
            {
                material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }
            return material;
        }

        public Material MouthMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false)
        {
            Material material = null;
            bool flag = this.pawn.gender == Gender.Female;
            if (FS_Settings.UseMouth && (flag || !flag && this.BeardDef.drawMouth) && this.drawMouth)
            {

                if (bodyCondition == RotDrawMode.Fresh)
                {
                    if (stump)
                    {
                        material = null;
                    }
                    else
                    {
                        material = this.mouthGraphic.MatAt(facing, null);
                    }
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    if (stump)
                    {
                        material = null;
                    }
                    else
                    {
                        // dead staring eyes maybe?
                        material = this.mouthGraphic.MatAt(facing, null);
                    }
                }

                if (material != null)
                {
                    material = this.pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
                }
            }

            return material;
        }

        public Material WrinkleMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false)
        {

            Material material = null;
            if (this.isOld && FS_Settings.UseWrinkles)
            {
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    if (stump)
                    {
                        material = null;
                    }
                    else
                    {
                        material = this.wrinkleGraphic.MatAt(facing, null);
                    }
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    if (stump)
                    {
                        material = null;
                    }
                    else
                    {
                        // dead staring eyes maybe?
                        material = this.wrinkleGraphic.MatAt(facing, null);
                    }
                }
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
