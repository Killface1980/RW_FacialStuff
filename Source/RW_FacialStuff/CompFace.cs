using static RW_FacialStuff.GraphicDatabaseHeadRecordsModded;
using static RW_FacialStuff.Headhelper;

namespace RW_FacialStuff
{
    using System.IO;

    using RW_FacialStuff.Defs;

    using UnityEngine;

    using Verse;

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

        public Graphic_Multi HeadGraphic;

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

        private Texture2D temptexturefront;

        private Texture2D temptextureside;

        private Texture2D temptextureback;

        private Texture2D finalHeadFront;
        private Texture2D finalHeadSide;
        private Texture2D finalHeadBack;
        private Texture2D disHeadFront;
        private Texture2D disHeadSide;
        private Texture2D disHeadBack;

        private Graphic_Multi headGraphicVanilla;
        private Graphic_Multi dissicatedHeadGraphicVanilla;

        private Texture2D maskTexFrontBack;

        private Texture2D maskTexSide;

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

        public bool GenerateHeadGraphics(Graphic hairGraphic)
        {
            this.isOld = this.pawn.ageTracker.AgeBiologicalYearsFloat >= 50f;

            this.temptexturefront = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            this.temptextureside = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            this.temptextureback = new Texture2D(1, 1, TextureFormat.ARGB32, false);

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
            this.MergeFaceParts(
                this.browGraphic,
                this.pawn.story.hairColor * darken,
                ref canvasHeadFront,
                ref canvasHeadSide);

            this.MergeFaceParts(
                this.eyeGraphic,
                Color.black,
                ref canvasHeadFront,
                ref canvasHeadSide);

            if (this.pawn.gender == Gender.Male)
            {
                if (FS_Settings.UseWrinkles && this.isOld)
                {
                    this.temptexturefront = MakeReadable(this.wrinkleGraphic.MatFront.mainTexture as Texture2D);
                    this.temptextureside = MakeReadable(this.wrinkleGraphic.MatSide.mainTexture as Texture2D);

                    this.MakeOld(this.temptexturefront, ref canvasHeadFront);
                    this.MakeOld(this.temptextureside, ref canvasHeadSide);
                }

                if (FS_Settings.UseMouth && (this.BeardDef.drawMouth && this.drawMouth))
                {
                    this.AddMouth(this.mouthGraphic, ref canvasHeadFront, ref canvasHeadSide);
                }

                if (!this.BeardDef.defName.Equals("Shaved"))
                {
                    bool flag = this.BeardDef.defName.Equals("Beard_Stubble");
                    this.MergeFaceParts(
                        this.beardGraphic,
                        Color.white,
                        ref canvasHeadFront,
                        ref canvasHeadSide,
                        isBeard: true, stubble: flag);
                }
            }

            if (this.pawn.gender == Gender.Female)
            {
                if (FS_Settings.UseWrinkles && this.isOld)
                {
                    this.temptexturefront = MakeReadable(this.wrinkleGraphic.MatFront.mainTexture as Texture2D);
                    this.temptextureside = MakeReadable(this.wrinkleGraphic.MatSide.mainTexture as Texture2D);

                    this.MakeOld(this.temptexturefront, ref canvasHeadFront);
                    this.MakeOld(this.temptextureside, ref canvasHeadSide);
                }

                if (FS_Settings.UseMouth && this.drawMouth)
                {
                    this.AddMouth(this.mouthGraphic, ref canvasHeadFront, ref canvasHeadSide);
                }
            }

            this.headGraphicVanilla = GetModdedHeadNamed(this.pawn, true, Color.white);
            this.dissicatedHeadGraphicVanilla = GetModdedHeadNamed(this.pawn, true, Color.white);

            this.finalHeadFront = MakeReadable(this.headGraphicVanilla.MatFront.mainTexture as Texture2D);
            this.finalHeadSide = MakeReadable(this.headGraphicVanilla.MatSide.mainTexture as Texture2D);
            this.finalHeadBack = MakeReadable(this.headGraphicVanilla.MatBack.mainTexture as Texture2D);

            this.disHeadFront = MakeReadable(this.dissicatedHeadGraphicVanilla.MatFront.mainTexture as Texture2D);
            this.disHeadSide = MakeReadable(this.dissicatedHeadGraphicVanilla.MatSide.mainTexture as Texture2D);
            this.disHeadBack = MakeReadable(this.dissicatedHeadGraphicVanilla.MatBack.mainTexture as Texture2D);


            this.temptexturefront = MakeReadable(hairGraphic.MatFront.mainTexture as Texture2D);
            this.temptextureside = MakeReadable(hairGraphic.MatSide.mainTexture as Texture2D);
            this.temptextureback = MakeReadable(hairGraphic.MatBack.mainTexture as Texture2D);

            switch (this.pawn.story.crownType)
            {
                case CrownType.Narrow:
                    ScaleTexture(this.temptexturefront,
                        out this.temptexturefront,
                        112,
                        128);
                    ScaleTexture(this.temptextureside,
                        out this.temptextureside,
                        112,
                        128);
                    ScaleTexture(this.temptextureback,
                        out this.temptextureback,
                        112,
                        128);

                    this.maskTexFrontBack = MaskTextures.MaskTex_Narrow_FrontBack;
                    this.maskTexSide = MaskTextures.MaskTex_Narrow_Side;
                    break;

                default:
                    this.maskTexFrontBack = MaskTextures.MaskTex_Average_FrontBack;
                    this.maskTexSide = MaskTextures.MaskTex_Average_Side;
                    break;
            }

            this.MergeHeadWithHair(
                this.temptexturefront,
                canvasHeadFront,
                this.maskTexFrontBack,
                ref this.finalHeadFront,
                ref this.disHeadFront);

            this.MergeHeadWithHair(
                this.temptextureside,
                canvasHeadSide,
                this.maskTexSide,
                ref this.finalHeadSide,
                ref this.disHeadSide);

            this.MergeHeadWithHair(
                this.temptextureback,
                canvasHeadBack,
                this.maskTexFrontBack,
                ref this.finalHeadBack,
                ref this.disHeadBack);


            if (false)
            {
                byte[] bytes = canvasHeadFront.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/MergedHeads/" + this.pawn.Name + "_01front.png", bytes);
                byte[] bytes2 = canvasHeadSide.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/MergedHeads/" + this.pawn.Name + "_02side.png", bytes2);
                byte[] bytes3 = canvasHeadBack.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/MergedHeads/" + this.pawn.Name + "_03back.png", bytes3);
            }

            this.finalHeadFront.Compress(true);
            this.finalHeadSide.Compress(true);
            this.finalHeadBack.Compress(true);

            this.disHeadFront.Compress(true);
            this.disHeadSide.Compress(true);
            this.disHeadBack.Compress(true);

            this.finalHeadFront.mipMapBias = 0.5f;
            this.finalHeadSide.mipMapBias = 0.5f;
            this.finalHeadBack.mipMapBias = 0.5f;

            this.disHeadFront.mipMapBias = 0.5f;
            this.disHeadSide.mipMapBias = 0.5f;
            this.disHeadBack.mipMapBias = 0.5f;

            this.finalHeadFront.Apply(false, true);
            this.finalHeadSide.Apply(false, true);
            this.finalHeadBack.Apply(false, true);

            this.disHeadFront.Apply(false, true);
            this.disHeadSide.Apply(false, true);
            this.disHeadBack.Apply(false, true);

            this.HeadGraphic = GetModdedHeadNamed(this.pawn, false, Color.white);
            this.DissicatedHeadGraphic = GetModdedHeadNamed(this.pawn, false, skinRottingMultiplyColor);

            this.HeadGraphic.MatFront.mainTexture = this.finalHeadFront;
            this.HeadGraphic.MatSide.mainTexture = this.finalHeadSide;
            this.HeadGraphic.MatBack.mainTexture = this.finalHeadBack;

            this.DissicatedHeadGraphic.MatFront.mainTexture = this.disHeadFront;
            this.DissicatedHeadGraphic.MatSide.mainTexture = this.disHeadSide;
            this.DissicatedHeadGraphic.MatBack.mainTexture = this.disHeadBack;

            UnityEngine.Object.DestroyImmediate(this.temptexturefront, true);
            UnityEngine.Object.DestroyImmediate(this.temptextureside, true);
            UnityEngine.Object.DestroyImmediate(this.temptextureback, true);

            UnityEngine.Object.DestroyImmediate(canvasHeadFront, true);
            UnityEngine.Object.DestroyImmediate(canvasHeadSide, true);
            UnityEngine.Object.DestroyImmediate(canvasHeadBack, true);


            this.sessionOptimized = true;
            return true;

            // moddedHeadGraphics.Add(new KeyValuePair<string, Graphic_Multi>(pawn + color.ToString(), headGraphic));
        }

        public void InitializeGraphics()
        {

            if (this.pawn == null)
            {
                return;
            }

            //// Save RAM 
            // if (this.finalHeadFront != null)
            // {
            // Object.DestroyImmediate(finalHeadFront, true);
            // Object.DestroyImmediate(finalHeadSide, true);
            // Object.DestroyImmediate(finalHeadBack, true);
            // Object.DestroyImmediate(disHeadFront, true);
            // Object.DestroyImmediate(disHeadSide, true);
            // Object.DestroyImmediate(disHeadBack, true);
            // }

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
                this.beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.BeardDef.texPathAverageNormal,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.white);
                this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.WrinkleDef.texPathAverageNormal,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.black);
            }

            if (this.type == "Pointy")
            {
                this.beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.BeardDef.texPathAveragePointy,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.white);
                this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.WrinkleDef.texPathAveragePointy,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.black);
            }

            if (this.type == "Wide")
            {
                this.beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.BeardDef.texPathAverageWide,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.white);
                this.wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    this.WrinkleDef.texPathAverageWide,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.black);
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
                Color.white);
            this.mouthGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                this.MouthDef.texPath,
                ShaderDatabase.Cutout,
                Vector2.one,
                this.pawn.story.SkinColor);

            if (this.pawn.gender == Gender.Female && this.BeardDef == null)
            {
                this.BeardDef = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");
            }

        }

        public void AddMouth(Graphic currentGraphic, ref Texture2D canvasHeadFront, ref Texture2D canvasHeadSide)
        {
            if (this.pawn.story.crownType == CrownType.Narrow)
            {
                ScaleTexture(currentGraphic.MatFront.mainTexture as Texture2D, out this.temptexturefront, 102, 128);
                ScaleTexture(currentGraphic.MatSide.mainTexture as Texture2D, out this.temptextureside, 102, 128);
            }
            else
            {
                this.temptexturefront = MakeReadable(currentGraphic.MatFront.mainTexture as Texture2D);
                this.temptextureside = MakeReadable(currentGraphic.MatSide.mainTexture as Texture2D);
            }

            this.MergeMouth(this.temptexturefront, this.pawn.story.SkinColor, ref canvasHeadFront);
            this.MergeMouth(this.temptextureside, this.pawn.story.SkinColor, ref canvasHeadSide);

        }

        public void MergeFaceParts(Graphic currentGraphic, Color color, ref Texture2D canvasHeadFront, ref Texture2D canvasHeadSide, bool isBeard = false, bool stubble = false)
        {

            if (this.pawn.story.crownType == CrownType.Narrow)
            {
                ScaleTexture(currentGraphic.MatFront.mainTexture as Texture2D, out this.temptexturefront, 102, 128);
                ScaleTexture(currentGraphic.MatSide.mainTexture as Texture2D, out this.temptextureside, 102, 128);
            }
            else
            {
                this.temptexturefront = MakeReadable(currentGraphic.MatFront.mainTexture as Texture2D);
                this.temptextureside = MakeReadable(currentGraphic.MatSide.mainTexture as Texture2D);
            }

            if (isBeard)
            {
                this.AddFacialHair(this.temptexturefront, ref canvasHeadFront, stubble);
                this.AddFacialHair(this.temptextureside, ref canvasHeadSide, stubble);
            }
            else
            {
                this.MergeTwoGraphics(this.temptexturefront, color, ref canvasHeadFront);
                this.MergeTwoGraphics(this.temptextureside, color, ref canvasHeadSide);
            }
        }

        public  void AddFacialHair(Texture2D beardTex, ref Texture2D finalTexture, bool stubble = false)
        {
            Texture2D tempBeardTex = MakeReadable(beardTex);
            Color color = new Color(0.6f, 0.6f, 0.6f);

            // offset neede if beards are stretched => narrow
            int offset = (finalTexture.width - tempBeardTex.width) / 2;
            int startX = 0;
            int startY = finalTexture.height - tempBeardTex.height;

            for (int x = startX; x < finalTexture.width; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {
                    Color headColor = finalTexture.GetPixel(x, y);

                    Color beardColor = tempBeardTex.GetPixel(x - startX - offset, y - startY);
                    if (stubble)
                    {
                        beardColor *= color;
                    }

                    beardColor *= this.pawn.story.hairColor;

                    Color final_color = Color.Lerp(headColor, beardColor, beardColor.a / 1f);

                    final_color.a = headColor.a + beardColor.a;

                    finalTexture.SetPixel(x, y, final_color);
                }
            }

            UnityEngine.Object.DestroyImmediate(tempBeardTex);

            finalTexture.Apply();
        }

        private void MergeTwoGraphics(Texture2D topLayerTex, Color multiplyColor, ref Texture2D finalTexture)
        {
            // offset neede if beards are stretched => narrow
            int offset = (finalTexture.width - topLayerTex.width) / 2;

            for (int x = 0; x < 128; x++)
            {

                for (int y = 0; y < 128; y++)
                {
                    Color topColor;

                    topColor = topLayerTex.GetPixel(x - offset, y);
                    Color headColor = finalTexture.GetPixel(x, y);

                    // eyeColor = topLayerTex.GetPixel(x, y);
                    topColor *= multiplyColor;

                    // eyeColor *= eyeColorRandom;
                    Color finalColor = Color.Lerp(headColor, topColor, topColor.a / 1f);

                    finalColor.a = headColor.a + topColor.a;

                    finalTexture.SetPixel(x, y, finalColor);
                }
            }

            finalTexture.Apply();
        }

        private void MergeHeadWithHair(Texture2D hairTex, Texture2D canvasTex, Texture2D maskTex, ref Texture2D finalTex1, ref Texture2D finalTex2)
        {
            Color pawnSkinColor = this.pawn.story.SkinColor;
            Color pawnHairColor = this.pawn.story.hairColor;

            Texture2D tempMaskTex = MakeReadable(maskTex);

            int offset = (finalTex1.width - hairTex.width) / 2;

            int startX = 0;
            int startY = finalTex1.height - hairTex.height;

            for (int x = startX; x < canvasTex.width + offset; x++)
            {
                for (int y = startY; y < finalTex1.height; y++)
                {
                     var loc_x = x - startX - offset;
                     var loc_y = y - startY;
                    Color maskColor = tempMaskTex.GetPixel(x, y);
                    Color headBase = finalTex1.GetPixel(x, y);
                    Color canvas = canvasTex.GetPixel(x, y);

                    Color hairColor = hairTex.GetPixel(loc_x, loc_y);

                    // Set up face
                    Color final_color1 = headBase * pawnSkinColor;
                    Color final_color2 = headBase * pawnSkinColor;

                    // Merge cansvas with face
                    final_color1 = Color.Lerp(final_color1, canvas, canvas.a);
                    final_color2 = Color.Lerp(final_color2, canvas, canvas.a);

                    float alpha1 = headBase.a + canvas.a;
                    final_color1.a = alpha1;
                    final_color2.a = alpha1;

                    // Cut out hair
                    hairColor *= pawnHairColor;
                    hairColor *= maskColor;

                    float alpha2 = final_color1.a + hairColor.a;

                    // Merge head with hair
                    final_color1 = Color.Lerp(final_color1, hairColor, hairColor.a);
                    final_color2 = Color.Lerp(final_color2, hairColor, hairColor.a);

                    final_color1.a = alpha2;
                    final_color2.a = alpha2;

                    finalTex1.SetPixel(x, y, final_color1);
                    finalTex2.SetPixel(x, y, final_color2);
                }
            }

            finalTex1.Apply();
            finalTex2.Apply();
            UnityEngine.Object.DestroyImmediate(tempMaskTex);
        }

        public void MergeMouth(Texture2D mouthTex, Color skinColor, ref Texture2D canvas)
        {
            // offset neede if beards are stretched => narrow
            int offset = (canvas.width - mouthTex.width) / 2;

            for (int x = 0; x < 128; x++)
            {

                for (int y = 0; y < 128; y++)
                {
                    Color mouthColor;

                    mouthColor = mouthTex.GetPixel(x - offset, y);
                    Color canvasColor = canvas.GetPixel(x, y);

                    Color finalColor = canvasColor;
                    if (mouthColor.a > 0f)
                    {
                        finalColor = mouthColor * skinColor;
                    }

                    canvas.SetPixel(x, y, finalColor);
                }
            }

            canvas.Apply();
        }

        public void MakeOld(Texture2D wrinkleTex, ref Texture2D canvas)
        {
            Pawn pawn = this.parent as Pawn;

            Texture2D tempWrinkleTex = MakeReadable(wrinkleTex);
            int startX = 0;
            int startY = 0;
            var col = pawn.story.SkinColor * pawn.story.SkinColor;

            for (int x = startX; x < canvas.width; x++)
            {
                for (int y = startY; y < canvas.height; y++)
                {
                    Color wrinkleColor = tempWrinkleTex.GetPixel(x, y);
                    Color canvasColor = canvas.GetPixel(x, y);

                    wrinkleColor *= col;
                    wrinkleColor.a *= Mathf.InverseLerp(50f, 100f, pawn.ageTracker.AgeBiologicalYearsFloat) * 0.8f;

                    Color final_color = Color.clear;
                    if (canvasColor.a > 0f)
                    {
                        final_color = canvasColor;
                    }
                    else if (wrinkleColor.a > 0f)
                    {
                        final_color = wrinkleColor;
                    }

                    canvas.SetPixel(x, y, final_color);
                }
            }

            UnityEngine.Object.DestroyImmediate(tempWrinkleTex);

            canvas.Apply();
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
