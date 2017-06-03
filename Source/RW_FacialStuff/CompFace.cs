using static RW_FacialStuff.GraphicDatabaseHeadRecordsModded;
using static RW_FacialStuff.Headhelper;

namespace RW_FacialStuff
{
    using System.IO;
    using System.Linq;

    using RW_FacialStuff.Defs;

    using UnityEngine;

    using Verse;

    public class CompFace : ThingComp
    {

        public BeardDef BeardDef;

        public BrowDef BrowDef;

        public Graphic_Multi DissicatedHeadGraphic;

        public bool drawMouth = true;

        public EyeDef EyeDef;

        public Color HairColorOrg;

        public Graphic_Multi HeadGraphic;

        public string headGraphicIndex;

        public MouthDef MouthDef;

        public bool optimized;

        public bool sessionOptimized;

        public string SkinColorHex;

        public string type = null;

        public WrinkleDef WrinkleDef;

        private Graphic _beardGraphic;

        private Graphic _browGraphic;

        private Graphic _eyeGraphic;

        private Graphic _mouthGraphic;

        private Texture2D _temptextureback;

        private Texture2D _temptexturefront;

        private Texture2D _temptextureside;

        private Texture2D finalHeadFront;
        private Texture2D finalHeadSide;
        private Texture2D finalHeadBack;
        private Texture2D disHeadFront;
        private Texture2D disHeadSide;
        private Texture2D disHeadBack;
        private Graphic _wrinkleGraphic;

        private Graphic_Multi headGraphicVanilla;
        private Graphic_Multi dissicatedHeadGraphicVanilla;

        private bool reInit;

        public void DefineFace()
        {

            Pawn pawn = this.parent as Pawn;

            if (pawn == null)
                return;

            this.EyeDef = PawnFaceChooser.RandomEyeDefFor(pawn, pawn.Faction.def);

            this.BrowDef = PawnFaceChooser.RandomBrowDefFor(pawn, pawn.Faction.def);

            this.WrinkleDef = PawnFaceChooser.AssignWrinkleDefFor(pawn, pawn.Faction.def);

            this.MouthDef = PawnFaceChooser.RandomMouthDefFor(pawn, pawn.Faction.def);

            this.BeardDef = PawnFaceChooser.RandomBeardDefFor(pawn, pawn.Faction.def);

            this.HairColorOrg = pawn.story.hairColor;

            this.optimized = true;
        }

        public void SetHeadType()
        {
            Pawn pawn = this.parent as Pawn;

            if (pawn == null)
                return;

            string newType = null;

            if (pawn.story.HeadGraphicPath.Contains("Normal"))
            {
                newType = "Normal";
            }

            if (pawn.story.HeadGraphicPath.Contains("Pointy"))
            {
                newType = "Pointy";
            }

            if (pawn.story.HeadGraphicPath.Contains("Wide"))
            {
                newType = "Wide";
            }

            if (this.type == null || this.type != newType)
            {
                this.type = newType;
            }
        }

        public bool GenerateHeadGraphics(Graphic hairGraphic)
        {
            Pawn pawn = this.parent as Pawn;
            bool oldAge = pawn.ageTracker.AgeBiologicalYearsFloat >= 50;
            Color hairColor = pawn.story.hairColor;

            this._temptexturefront = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            this._temptextureside = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            this._temptextureback = new Texture2D(1, 1, TextureFormat.ARGB32, false);

            Texture2D canvasHeadFront = new Texture2D(128, 128);
            Texture2D canvasHeadSide = new Texture2D(128, 128);
            Texture2D canvasHeadBack = new Texture2D(128, 128);

            Graphics.CopyTexture(BlankTex, canvasHeadFront);
            Graphics.CopyTexture(BlankTex, canvasHeadSide);
            Graphics.CopyTexture(BlankTex, canvasHeadBack);

            // if (pawn.story.crownType == CrownType.Narrow)
            // {
            // eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.EyeDef.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.white);
            // browGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.BrowDef.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.white);
            // if (oldAge)
            // {
            // if (pawnSave.type == "Normal")
            // wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
            // if (pawnSave.type == "Pointy")
            // wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowPointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
            // if (pawnSave.type == "Wide")
            // wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
            // }
            // }
            // else
            // {

            // }

            // if (_textures.Contains(eyeGraphic.MatFront.mainTexture as Texture2D))
            // {
            // _textures[1]
            // }
            // _textures.Add(eyeGraphic.MatFront.mainTexture as Texture2D);
            Color darken = new Color(0.3f, 0.3f, 0.3f, 1f);
            MergeFaceParts(
                pawn,
                this._browGraphic,
                hairColor * darken,
                ref canvasHeadFront,
                ref canvasHeadSide, ref _temptexturefront, ref _temptextureside);

            MergeFaceParts(pawn, this._eyeGraphic, Color.black, ref canvasHeadFront, ref canvasHeadSide, ref _temptexturefront, ref _temptextureside);

            if (pawn.gender == Gender.Male)
            {
                if (oldAge)
                {
                    _temptexturefront = MakeReadable(this._wrinkleGraphic.MatFront.mainTexture as Texture2D);
                    _temptextureside = MakeReadable(this._wrinkleGraphic.MatSide.mainTexture as Texture2D);

                    MakeOld(pawn, this._temptexturefront, ref canvasHeadFront);
                    MakeOld(pawn, this._temptextureside, ref canvasHeadSide);
                }

                if (FS_Settings.UseMouth && (this.BeardDef.drawMouth && this.drawMouth))
                {
                    MergeFaceParts(pawn, this._mouthGraphic, Color.black, ref canvasHeadFront, ref canvasHeadSide, ref _temptexturefront, ref _temptextureside);
                }

                if (!BeardDef.defName.Equals("Shaved"))
                {
                    MergeFaceParts(
                        pawn,
                        this._beardGraphic,
                        Color.white,
                        ref canvasHeadFront,
                        ref canvasHeadSide, ref _temptexturefront, ref _temptextureside,
                        true);
                }
            }

            if (pawn.gender == Gender.Female)
            {
                if (oldAge)
                {
                    _temptexturefront = MakeReadable(this._wrinkleGraphic.MatFront.mainTexture as Texture2D);
                    _temptextureside = MakeReadable(this._wrinkleGraphic.MatSide.mainTexture as Texture2D);

                    MakeOld(pawn, this._temptexturefront, ref canvasHeadFront);
                    MakeOld(pawn, this._temptextureside, ref canvasHeadSide);
                }

                if (FS_Settings.UseMouth && this.drawMouth)
                {
                    MergeFaceParts(pawn, this._mouthGraphic, Color.black, ref canvasHeadFront, ref canvasHeadSide, ref _temptexturefront, ref _temptextureside);
                }
            }

            _temptexturefront = MakeReadable(hairGraphic.MatFront.mainTexture as Texture2D);
            _temptextureside = MakeReadable(hairGraphic.MatSide.mainTexture as Texture2D);
            _temptextureback = MakeReadable(hairGraphic.MatBack.mainTexture as Texture2D);


            if (pawn.story.crownType == CrownType.Narrow)
            {
                ScaleTexture(_temptexturefront,
                    out this._temptexturefront,
                    112,
                    128);
                ScaleTexture(_temptextureside,
                    out this._temptextureside,
                    112,
                    128);
                ScaleTexture(_temptextureback,
                    out this._temptextureback,
                    112,
                    128);
            }


            // MergeColor(ref canvasHeadBack, pawn.story.SkinColor);
            if (pawn.story.crownType == CrownType.Narrow)
            {
                MergeHeadWithHair(hairColor, this._temptexturefront,
                    MaskTextures.MaskTex_Narrow_FrontBack,
                    ref canvasHeadFront);
                MergeHeadWithHair(hairColor, this._temptextureside,
                    MaskTextures.MaskTex_Narrow_Side,
                    ref canvasHeadSide);
                MergeHeadWithHair(hairColor, this._temptextureback,
                    MaskTextures.MaskTex_Narrow_FrontBack,
                    ref canvasHeadBack);
            }
            else
            {
                MergeHeadWithHair(hairColor, this._temptexturefront,
                    MaskTextures.MaskTex_Average_FrontBack,
                    ref canvasHeadFront);
                MergeHeadWithHair(hairColor, this._temptextureside,
                    MaskTextures.MaskTex_Average_Side,
                    ref canvasHeadSide);
                MergeHeadWithHair(hairColor, this._temptextureback,
                    MaskTextures.MaskTex_Average_FrontBack,
                    ref canvasHeadBack);
            }


            headGraphicVanilla = GetModdedHeadNamed(pawn, true, Color.white);
            dissicatedHeadGraphicVanilla = GetModdedHeadNamed(pawn, true, Color.white);

            finalHeadFront = MakeReadable(headGraphicVanilla.MatFront.mainTexture as Texture2D);
            finalHeadSide = MakeReadable(headGraphicVanilla.MatSide.mainTexture as Texture2D);
            finalHeadBack = MakeReadable(headGraphicVanilla.MatBack.mainTexture as Texture2D);

            disHeadFront = MakeReadable(dissicatedHeadGraphicVanilla.MatFront.mainTexture as Texture2D);
            disHeadSide = MakeReadable(dissicatedHeadGraphicVanilla.MatSide.mainTexture as Texture2D);
            disHeadBack = MakeReadable(dissicatedHeadGraphicVanilla.MatBack.mainTexture as Texture2D);

            PaintHeadWithColor(ref finalHeadFront, pawn.story.SkinColor);
            PaintHeadWithColor(ref finalHeadSide, pawn.story.SkinColor);
            PaintHeadWithColor(ref finalHeadBack, pawn.story.SkinColor);
            PaintHeadWithColor(ref disHeadFront, pawn.story.SkinColor);
            PaintHeadWithColor(ref disHeadSide, pawn.story.SkinColor);
            PaintHeadWithColor(ref disHeadBack, pawn.story.SkinColor);

            if (pawn.story.crownType == CrownType.Narrow)
            {
                MergeHeadWithHair(hairColor, canvasHeadFront, MaskTextures.MaskTex_Narrow_FrontBack, ref finalHeadFront);
                MergeHeadWithHair(hairColor, canvasHeadSide, MaskTextures.MaskTex_Narrow_Side, ref finalHeadSide);
                MergeHeadWithHair(hairColor, canvasHeadBack, MaskTextures.MaskTex_Narrow_FrontBack, ref finalHeadBack);

                MergeHeadWithHair(hairColor, canvasHeadFront, MaskTextures.MaskTex_Narrow_FrontBack, ref disHeadFront);
                MergeHeadWithHair(hairColor, canvasHeadSide, MaskTextures.MaskTex_Narrow_Side, ref disHeadSide);
                MergeHeadWithHair(hairColor, canvasHeadBack, MaskTextures.MaskTex_Narrow_FrontBack, ref disHeadBack);
            }
            else
            {
                MergeHeadWithHair(hairColor, canvasHeadFront, MaskTextures.MaskTex_Average_FrontBack, ref finalHeadFront);
                MergeHeadWithHair(hairColor, canvasHeadSide, MaskTextures.MaskTex_Average_Side, ref finalHeadSide);
                MergeHeadWithHair(hairColor, canvasHeadBack, MaskTextures.MaskTex_Average_FrontBack, ref finalHeadBack);

                MergeHeadWithHair(hairColor, canvasHeadFront, MaskTextures.MaskTex_Average_FrontBack, ref disHeadFront);
                MergeHeadWithHair(hairColor, canvasHeadSide, MaskTextures.MaskTex_Average_Side, ref disHeadSide);
                MergeHeadWithHair(hairColor, canvasHeadBack, MaskTextures.MaskTex_Average_FrontBack, ref disHeadBack);
            }

            if (false)
            {
                byte[] bytes = canvasHeadFront.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/MergedHeads/" + pawn.Name + "_01front.png", bytes);
                byte[] bytes2 = canvasHeadSide.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/MergedHeads/" + pawn.Name + "_02side.png", bytes2);
                byte[] bytes3 = canvasHeadBack.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/MergedHeads/" + pawn.Name + "_03back.png", bytes3);
            }

            finalHeadFront.Compress(true);
            finalHeadSide.Compress(true);
            finalHeadBack.Compress(true);

            disHeadFront.Compress(true);
            disHeadSide.Compress(true);
            disHeadBack.Compress(true);

            finalHeadFront.mipMapBias = 0.5f;
            finalHeadSide.mipMapBias = 0.5f;
            finalHeadBack.mipMapBias = 0.5f;

            disHeadFront.mipMapBias = 0.5f;
            disHeadSide.mipMapBias = 0.5f;
            disHeadBack.mipMapBias = 0.5f;

            finalHeadFront.Apply(false, true);
            finalHeadSide.Apply(false, true);
            finalHeadBack.Apply(false, true);

            disHeadFront.Apply(false, true);
            disHeadSide.Apply(false, true);
            disHeadBack.Apply(false, true);

            HeadGraphic = GetModdedHeadNamed(pawn, false, Color.white);
            DissicatedHeadGraphic = GetModdedHeadNamed(pawn, false, skinRottingMultiplyColor);

            this.HeadGraphic.MatFront.mainTexture = finalHeadFront;
            this.HeadGraphic.MatSide.mainTexture = finalHeadSide;
            this.HeadGraphic.MatBack.mainTexture = finalHeadBack;

            this.DissicatedHeadGraphic.MatFront.mainTexture = disHeadFront;
            this.DissicatedHeadGraphic.MatSide.mainTexture = disHeadSide;
            this.DissicatedHeadGraphic.MatBack.mainTexture = disHeadBack;

            Object.DestroyImmediate(this._temptexturefront, true);
            Object.DestroyImmediate(this._temptextureside, true);
            Object.DestroyImmediate(this._temptextureback, true);

            Object.DestroyImmediate(canvasHeadFront, true);
            Object.DestroyImmediate(canvasHeadSide, true);
            Object.DestroyImmediate(canvasHeadBack, true);


            this.sessionOptimized = true;
            return true;

            // moddedHeadGraphics.Add(new KeyValuePair<string, Graphic_Multi>(pawn + color.ToString(), headGraphic));
        }

        public void InitializeGraphics()
        {
            Pawn pawn = this.parent as Pawn;

            if (pawn == null)
            {
                return;
            }

            //// Save RAM 
            //if (this.finalHeadFront != null)
            //{
            //    Object.DestroyImmediate(finalHeadFront, true);
            //    Object.DestroyImmediate(finalHeadSide, true);
            //    Object.DestroyImmediate(finalHeadBack, true);
            //    Object.DestroyImmediate(disHeadFront, true);
            //    Object.DestroyImmediate(disHeadSide, true);
            //    Object.DestroyImmediate(disHeadBack, true);
            //
            //}

            // Create the blank canvas texture
            if (BlankTex == null)
            {
                BlankTex = new Texture2D(128, 128);

                // Reset all pixels color to transparent
                Color32 resetColor = new Color32(255, 255, 255, 0);
                Color32[] resetColorArray = BlankTex.GetPixels32();

                for (int i = 0; i < resetColorArray.Length; i++)
                {
                    resetColorArray[i] = resetColor;
                }

                BlankTex.SetPixels32(resetColorArray);
                BlankTex.Apply();
            }

            if (this.type == "Normal")
            {
                this._beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.BeardDef.texPathAverageNormal,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.white);
                this._wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.WrinkleDef.texPathAverageNormal,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.black);
            }

            if (this.type == "Pointy")
            {
                this._beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.BeardDef.texPathAveragePointy,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.white);
                this._wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.WrinkleDef.texPathAveragePointy,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.black);
            }

            if (this.type == "Wide")
            {
                this._beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.BeardDef.texPathAverageWide,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.white);
                this._wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.WrinkleDef.texPathAverageWide,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.black);
            }

            this._eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.EyeDef.texPath,
                ShaderDatabase.Cutout,
                Vector2.one,
                Color.white);
            this._browGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.BrowDef.texPath,
                ShaderDatabase.Cutout,
                Vector2.one,
                Color.white);
            this._mouthGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.MouthDef.texPath,
                ShaderDatabase.Cutout,
                Vector2.one,
                pawn.story.SkinColor);

            if (pawn.gender == Gender.Female && this.BeardDef == null)
            {
                this.BeardDef = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");
            }

        }

        public override void PostExposeData()
        {
            base.PostExposeData();
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