using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityCoreLibrary.ColorPicker;
using UnityEngine;
using Verse;
using Object = UnityEngine.Object;

namespace RW_FacialStuff
{
    public class GraphicDatabaseHeadRecordsModded : GraphicDatabaseHeadRecords
    {

        public static List<HeadGraphicRecordVanillaCustom> headsVanillaCustom = new List<HeadGraphicRecordVanillaCustom>();
        public static List<HeadGraphicRecordModded> headsModded = new List<HeadGraphicRecordModded>();

        private static HeadGraphicRecordVanillaCustom skull;
        private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        protected Pawn pawn;

        public static int headIndex = 0;

        public class HeadGraphicRecordVanillaCustom
        {
            public Gender gender;

            public CrownType crownType;

            public string graphicPathVanillaCustom;

            public List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

            public HeadGraphicRecordVanillaCustom(string graphicPath)
            {
                graphicPathVanillaCustom = graphicPath;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(graphicPath);

                string[] array = fileNameWithoutExtension.Split('_');

                try
                {
                    crownType = (CrownType)((byte)ParseHelper.FromString(array[array.Length - 2], typeof(CrownType)));
                    gender = (Gender)((byte)ParseHelper.FromString(array[array.Length - 3], typeof(Gender)));
                }
                catch (Exception ex)
                {
                    Log.Error("Parse error with head graphic at " + graphicPath + ": " + ex.Message);
                    crownType = CrownType.Undefined;
                    gender = Gender.None;
                }


            }

            public Graphic_Multi GetGraphic(Color color)
            {
                for (int i = 0; i < graphics.Count; i++)
                {
                    if (color.IndistinguishableFrom(graphics[i].Key))
                    {
                        return graphics[i].Value;
                    }
                }
                Graphic_Multi graphic_Multi_Head = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPathVanillaCustom, ShaderDatabase.Cutout, Vector2.one, color);
                graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphic_Multi_Head));
                return graphic_Multi_Head;
            }
        }

        public class HeadGraphicRecordModded
        {
            public Pawn pawn;

            public List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

            public bool unique = false;

            public Gender gender;

            public CrownType crownType;

            public string graphicPath;

            public string graphicPathModded;

            public HeadGraphicRecordModded(Pawn pawn)
            {
                var pawnSave = MapComponent_FacialStuff.GetCache(pawn);

                this.pawn = pawn;
                graphicPath = pawn.story.HeadGraphicPath;
                graphicPathModded = pawnSave.headGraphicIndex;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(graphicPath);
                string[] array = fileNameWithoutExtension.Split('_');
                try
                {
                    crownType = (CrownType)((byte)ParseHelper.FromString(array[array.Length - 2], typeof(CrownType)));
                    gender = (Gender)((byte)ParseHelper.FromString(array[array.Length - 3], typeof(Gender)));
                }
                catch (Exception ex)
                {
                    Log.Error("Parse error with head graphic at " + graphicPath + ": " + ex.Message);
                    crownType = CrownType.Undefined;
                    gender = Gender.None;
                }


            }

            public Graphic_Multi GetGraphicBlank(Color color)
            {
                for (int i = 0; i < graphics.Count; i++)
                {
                    if (color.IndistinguishableFrom(graphics[i].Key))
                    {
                        return graphics[i].Value;
                    }
                }

                Graphic_Multi graphic_Multi_Head = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPathModded, ShaderDatabase.Cutout, Vector2.one, color);
                graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphic_Multi_Head));

                return graphic_Multi_Head;
            }

        }

        public static Graphic_Multi GetModdedHeadNamed(Pawn pawn, bool useVanilla, Color color)
        {
            var pawnSave = MapComponent_FacialStuff.GetCache(pawn);

            if (useVanilla)
            {
                for (int i = 0; i < headsVanillaCustom.Count; i++)
                {
                    HeadGraphicRecordVanillaCustom headGraphicRecordVanillaCustom = headsVanillaCustom[i];

                    if (headGraphicRecordVanillaCustom.graphicPathVanillaCustom == pawn.story.HeadGraphicPath.Remove(0, 22))
                    {
                        return headGraphicRecordVanillaCustom.GetGraphic(color);
                    }
                }
            }

            for (int i = 0; i < headsModded.Count; i++)
            {
                HeadGraphicRecordModded headGraphicRecordModded = headsModded[i];

                if (headGraphicRecordModded.graphicPathModded == pawnSave.headGraphicIndex)
                {
                    return headGraphicRecordModded.GetGraphicBlank(color);
                }
            }

            Log.Message("Tried to get pawn head at path " + pawnSave.headGraphicIndex + " that was not found. Defaulting...");

            return headsVanillaCustom.First().GetGraphic(color);
        }

        public static void BuildDatabaseIfNecessary()
        {
            headsVanillaCustom.Clear();
            if (headsVanillaCustom.Count > 0 && skull != null)
            {
                return;
            }

            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Average_Normal"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Average_Pointy"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Average_Wide"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Narrow_Normal"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Narrow_Pointy"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Narrow_Wide"));

            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Average_Normal"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Average_Pointy"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Average_Wide"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Narrow_Normal"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Narrow_Pointy"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Narrow_Wide"));

            skull = new HeadGraphicRecordVanillaCustom(SkullPath);


            //   string[] headsFolderPaths = HeadsFolderPaths;
            //   for (int i = 0; i < headsFolderPaths.Length; i++)
            //   {
            //       string text = headsFolderPaths[i];
            //       foreach (string current in GraphicDatabaseUtility.GraphicNamesInFolder(text))
            //       {
            //           headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom(text + "/" + current));
            //       }
            //   }
        }

        public static void DefineHeadParts(Pawn pawn)
        {
            SaveablePawn pawnSave = MapComponent_FacialStuff.GetCache(pawn);

            if (pawn.story.HeadGraphicPath.Contains("Normal"))
                pawnSave.type = "Normal";

            if (pawn.story.HeadGraphicPath.Contains("Pointy"))
                pawnSave.type = "Pointy";

            if (pawn.story.HeadGraphicPath.Contains("Wide"))
                pawnSave.type = "Wide";


            pawnSave.EyeDef = PawnFaceChooser.RandomEyeDefFor(pawn, pawn.Faction.def);

            pawnSave.WrinkleDef = PawnFaceChooser.AssignWrinkleDefFor(pawn, pawn.Faction.def);

            pawnSave.SkinColorHex = ColorHelper.RGBtoHex(pawn.story.SkinColor);

            if (pawn.gender == Gender.Female)
            {
                pawnSave.LipDef = PawnFaceChooser.RandomLipDefFor(pawn, pawn.Faction.def);
            }


            if (pawn.gender == Gender.Male)
            {

                pawnSave.BeardDef = PawnFaceChooser.RandomBeardDefFor(pawn, pawn.Faction.def);

                pawnSave.HairColorHex = ColorHelper.RGBtoHex(pawn.story.hairColor);
            }


            pawnSave.optimized = true;
        }

        public static List<KeyValuePair<string,Graphic_Multi>> moddedHeadGraphics = new List<KeyValuePair<string, Graphic_Multi>>();


        public static Graphic_Multi ModifiedVanillaHead(Pawn pawn,Color color, Graphic hairGraphic)
        {

            for (int i = 0; i < moddedHeadGraphics.Count; i++)
            {
                if (i.Equals(pawn+color.ToString()))
                {
                    return moddedHeadGraphics[i].Value;
                }
            }


            var pawnSave = MapComponent_FacialStuff.GetCache(pawn);
            var headGraphic = GetModdedHeadNamed(pawn, false, Color.white);

            // grab the blank texture instead of Vanilla
            Graphic headGraphicVanilla = GetModdedHeadNamed(pawn, true, Color.white);
            bool oldAge = pawn.ageTracker.AgeBiologicalYearsFloat >= 40;

            Texture2D temptexturefront = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Texture2D temptextureside = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Texture2D temptextureback = new Texture2D(128, 128, TextureFormat.RGBA32, false);


            MakeReadable(headGraphicVanilla.MatFront.mainTexture as Texture2D, ref temptexturefront);
            MakeReadable(headGraphicVanilla.MatSide.mainTexture as Texture2D, ref temptextureside);
            MakeReadable(headGraphicVanilla.MatBack.mainTexture as Texture2D, ref temptextureback);

            var finalHeadFront = Object.Instantiate(temptexturefront);
            var finalHeadSide = Object.Instantiate(temptextureside);
            var finalHeadBack = Object.Instantiate(temptextureback);

            MergeTwoGraphics(ref finalHeadFront, temptexturefront, color);
            MergeTwoGraphics(ref finalHeadSide, temptextureside, color);
            MergeTwoGraphics(ref finalHeadBack, temptextureback, color);

            Graphic eyeGraphic;
            Graphic wrinkleGraphic = null;

            if (pawn.story.crownType == CrownType.Narrow)
            {
                eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.EyeDef.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.black);

                if (oldAge)
                {
                    if (pawnSave.type == "Normal")
                        wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowNormal, ShaderDatabase.Cutout, Vector2.one, Color.black);
                    if (pawnSave.type == "Pointy")
                        wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowPointy, ShaderDatabase.Cutout, Vector2.one, Color.black);
                    if (pawnSave.type == "Wide")
                        wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowWide, ShaderDatabase.Cutout, Vector2.one, Color.black);
                }
            }
            else
            {
                eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.EyeDef.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.black);

                if (oldAge)
                {
                    if (pawnSave.type == "Normal")
                        wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.black);
                    if (pawnSave.type == "Pointy")
                        wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.black);
                    if (pawnSave.type == "Wide")
                        wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.black);

                }
            }

            temptexturefront = eyeGraphic.MatFront.mainTexture as Texture2D;
            temptextureside = eyeGraphic.MatSide.mainTexture as Texture2D;

            MergeTwoGraphics(ref finalHeadFront, temptexturefront, pawn.story.SkinColor);
            MergeTwoGraphics(ref finalHeadSide, temptextureside, pawn.story.SkinColor);

            #region Male
            if (pawn.gender == Gender.Male)
            {
                Graphic beardGraphic = null;

                if (pawn.story.crownType == CrownType.Narrow)
                {
                    if (pawnSave.type == "Normal")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.BeardDef.texPathNarrowNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                    if (pawnSave.type == "Pointy")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.BeardDef.texPathNarrowPointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                    if (pawnSave.type == "Wide")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.BeardDef.texPathNarrowWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }

                }
                else
                {
                    if (pawnSave.type == "Normal")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.BeardDef.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                    if (pawnSave.type == "Pointy")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.BeardDef.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                    if (pawnSave.type == "Wide")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.BeardDef.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }

                }

                if (oldAge)
                {
                    temptexturefront = wrinkleGraphic.MatFront.mainTexture as Texture2D;
                    temptextureside = wrinkleGraphic.MatSide.mainTexture as Texture2D;

                    MakeOld(pawn, ref finalHeadFront, temptexturefront);
                    MakeOld(pawn, ref finalHeadSide, temptextureside);

                    temptexturefront = beardGraphic.MatFront.mainTexture as Texture2D;
                    temptextureside = beardGraphic.MatSide.mainTexture as Texture2D;

                    AddFacialHair(pawn, ref finalHeadFront, temptexturefront);
                    AddFacialHair(pawn, ref finalHeadFront, temptexturefront);
                }
                else
                {
                    temptexturefront = beardGraphic.MatFront.mainTexture as Texture2D;
                    temptextureside = beardGraphic.MatSide.mainTexture as Texture2D;

                    AddFacialHair(pawn, ref finalHeadFront, temptexturefront);
                    AddFacialHair(pawn, ref finalHeadFront, temptexturefront);
                }

            }

            #endregion

            #region Female
            if (pawn.gender == Gender.Female)
            {
                Graphic lipGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawn.story.crownType == CrownType.Narrow ? pawnSave.LipDef.texPathNarrow : pawnSave.LipDef.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.white);

                if (oldAge)
                {
                    temptexturefront = wrinkleGraphic.MatFront.mainTexture as Texture2D;
                    temptextureside = wrinkleGraphic.MatSide.mainTexture as Texture2D;

                    MakeOld(pawn, ref finalHeadFront, temptexturefront);
                    MakeOld(pawn, ref finalHeadSide, temptextureside);

                }
                temptexturefront = lipGraphic.MatFront.mainTexture as Texture2D;
                temptextureside = lipGraphic.MatSide.mainTexture as Texture2D;

                MergeTwoGraphics(ref finalHeadFront, temptexturefront, Color.white);
                MergeTwoGraphics(ref finalHeadSide, temptextureside, Color.white);

            }
            #endregion
            MakeReadable(hairGraphic.MatFront.mainTexture as Texture2D, ref temptexturefront);
            MakeReadable(hairGraphic.MatSide.mainTexture as Texture2D, ref temptextureside);
            MakeReadable(hairGraphic.MatBack.mainTexture as Texture2D, ref temptextureback);

            if (pawn.story.crownType == CrownType.Narrow)
            {

                TextureScale.Bilinear(temptexturefront, 112, 128);
                TextureScale.Bilinear(temptextureside, 112, 128);
                TextureScale.Bilinear(temptextureback, 112, 128);

                TextureScale.ResizeCanvas(temptexturefront, 128, 128);
                TextureScale.ResizeCanvas(temptextureside, 128, 128);
                TextureScale.ResizeCanvas(temptextureback, 128, 128);

            }

        //    MergeColor(ref finalHeadBack, pawn.story.SkinColor);

            MergeHeadWithHair(ref finalHeadFront, temptexturefront,  pawn.story.hairColor);
            MergeHeadWithHair(ref finalHeadSide, temptextureside,  pawn.story.hairColor);
            MergeHeadWithHair(ref finalHeadBack, temptextureback,  pawn.story.hairColor);
          
            finalHeadFront.Compress(true);
            finalHeadSide.Compress(true);
            finalHeadBack.Compress(true);

            finalHeadFront.Apply(false, true);
            finalHeadSide.Apply(false, true);
            finalHeadBack.Apply(false, true);

            headGraphic.MatFront.mainTexture = finalHeadFront;
            headGraphic.MatSide.mainTexture = finalHeadSide;
            headGraphic.MatBack.mainTexture = finalHeadBack;

            temptexturefront.Apply(false, true);
            temptextureside.Apply(false, true);
            temptextureback.Apply(false, true);

            Object.DestroyImmediate(temptexturefront, true);
            Object.DestroyImmediate(temptextureside, true);
            Object.DestroyImmediate(temptextureback, true);


            pawnSave.sessionOptimized = true;

            moddedHeadGraphics.Add(new KeyValuePair<string, Graphic_Multi>(pawn+color.ToString(), headGraphic));

            return headGraphic;
        }

        public static void MakeReadable(Texture2D texture, ref Texture2D myTexture2D)
        {

            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(
                                texture.width,
                                texture.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            myTexture2D = new Texture2D(texture.width, texture.width);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();

            // Reset the active RenderTexture
            //    RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            // "myTexture2D" now has the same pixels from "texture" and it's readable.
        }

        private static void AddFacialHair(Pawn pawn, ref Texture2D finalhead, Texture2D beard)
        {
            int startX = 0;
            int startY = finalhead.height - beard.height;

            for (int x = startX; x < finalhead.width; x++)
            {

                for (int y = startY; y < finalhead.height; y++)
                {
                    Color headColor = finalhead.GetPixel(x, y);
                    Color beardColor = beard.GetPixel(x - startX, y - startY);

                    beardColor *= pawn.story.hairColor;

                    Color final_color = Color.Lerp(headColor, beardColor, beardColor.a / 1f);

                    finalhead.SetPixel(x, y, final_color);
                }
            }

            finalhead.Apply();
        }

        private static void MergeTwoGraphics(ref Texture2D finalTexture, Texture2D topLayer, Color topColor)
        {

            for (int x = 0; x < 128; x++)
            {

                for (int y = 0; y < 128; y++)
                {
                    Color headColor = finalTexture.GetPixel(x, y);
                    Color eyeColor = topLayer.GetPixel(x, y);
                    eyeColor *= topColor;
                    //      eyeColor *= eyeColorRandom;

                    Color final_color = Color.Lerp(headColor, eyeColor, eyeColor.a / 1f);


                    finalTexture.SetPixel(x, y, final_color);
                }
            }

            finalTexture.Apply();
        }

        private static void MergeHeadWithHair(ref Texture2D finalTexture, Texture2D top_layer, Color topColor)
        {
            int startX = 0;
            int startY = finalTexture.height - top_layer.height;


            for (int x = startX; x < finalTexture.width; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {

                    Color headColor = finalTexture.GetPixel(x, y);
                    Color hairColor = top_layer.GetPixel(x - startX, y - startY);

                    if (y > 82)
                        hairColor.a = 0;
                    if (y > 79 && y < 82 && hairColor.a > 0)
                        hairColor = Color.black;

                    if (hairColor.a < 1f)
                        hairColor.a = 0;

                    hairColor *= topColor;

                    Color final_color = Color.Lerp(headColor, hairColor, hairColor.a / 1f);

                    if (headColor.a == 1)
                        final_color.a = 1;

                    finalTexture.SetPixel(x, y, final_color);
                }
            }

            finalTexture.Apply();
        }

        private static void MakeOld(Pawn pawn, ref Texture2D finalhead, Texture2D wrinkles)
        {

            int startX = 0;
            int startY = finalhead.height - wrinkles.height;

            for (int x = startX; x < finalhead.width; x++)
            {

                for (int y = startY; y < finalhead.height; y++)
                {
                    Color headColor = finalhead.GetPixel(x, y);
                    Color wrinkleColor = wrinkles.GetPixel(x - startX, y - startY);

                    Color final_color = headColor;

                    if (pawn.ageTracker.AgeBiologicalYearsFloat >= 40)
                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 1f) * 0.15f);
                    if (pawn.ageTracker.AgeBiologicalYearsFloat >= 47)
                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 1f) * 0.3f);
                    if (pawn.ageTracker.AgeBiologicalYearsFloat >= 54)
                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 1f) * 0.45f);
                    if (pawn.ageTracker.AgeBiologicalYearsFloat >= 61)
                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 1f) * 0.6f);
                    if (pawn.ageTracker.AgeBiologicalYearsFloat >= 68)
                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 1f) * 0.8f);
                    if (pawn.ageTracker.AgeBiologicalYearsFloat >= 76)
                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 1f) * 1f);

                    if (headColor.a == 1)
                        final_color.a = 1;

                    finalhead.SetPixel(x, y, final_color);
                }
            }

            finalhead.Apply();
        }

    }
}


