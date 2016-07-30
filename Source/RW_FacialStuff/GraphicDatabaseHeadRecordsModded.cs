using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityCoreLibrary;
using CommunityCoreLibrary.ColorPicker;
using RW_FacialStuff.Defs;
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

            pawnSave.BrowDef = PawnFaceChooser.RandomBrowDefFor(pawn, pawn.Faction.def);

            pawnSave.WrinkleDef = PawnFaceChooser.AssignWrinkleDefFor(pawn, pawn.Faction.def);

            pawnSave.SkinColorHex = ColorHelper.RGBtoHex(pawn.story.SkinColor);

            pawnSave.LipDef = PawnFaceChooser.RandomLipDefFor(pawn, pawn.Faction.def);

            if (pawn.gender == Gender.Male)
            {

                pawnSave.BeardDef = PawnFaceChooser.RandomBeardDefFor(pawn, pawn.Faction.def);

                pawnSave.HairColorHex = ColorHelper.RGBtoHex(pawn.story.hairColor);
            }


            pawnSave.optimized = true;
        }

        public static List<KeyValuePair<string, Graphic_Multi>> moddedHeadGraphics = new List<KeyValuePair<string, Graphic_Multi>>();


        public static Graphic_Multi ModifiedVanillaHead(Pawn pawn, Color color, Graphic hairGraphic)
        {

            //for (int i = 0; i < moddedHeadGraphics.Count; i++)
            //{
            //    if (i.Equals(pawn + color.ToString()))
            //    {
            //        return moddedHeadGraphics[i].Value;
            //    }
            //}


            var pawnSave = MapComponent_FacialStuff.GetCache(pawn);
            var headGraphic = GetModdedHeadNamed(pawn, false, Color.white);

            // grab the blank texture instead of Vanilla
            Graphic headGraphicVanilla = GetModdedHeadNamed(pawn, true, Color.white);
            bool oldAge = pawn.ageTracker.AgeBiologicalYearsFloat >= 40;



            var finalHeadFront = MakeReadable(headGraphicVanilla.MatFront.mainTexture as Texture2D);
            var finalHeadSide = MakeReadable(headGraphicVanilla.MatSide.mainTexture as Texture2D);
            var finalHeadBack = MakeReadable(headGraphicVanilla.MatBack.mainTexture as Texture2D);

            PaintHead(finalHeadFront, color);
            PaintHead(finalHeadSide, color);
            PaintHead(finalHeadBack, color);

            Graphic eyeGraphic;
            Graphic browGraphic;
            Graphic wrinkleGraphic = null;

            //  if (pawn.story.crownType == CrownType.Narrow)
            //  {
            //      eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.EyeDef.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.white);
            //      browGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.BrowDef.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.white);
            //
            //      if (oldAge)
            //      {
            //          if (pawnSave.type == "Normal")
            //              wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
            //          if (pawnSave.type == "Pointy")
            //              wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowPointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
            //          if (pawnSave.type == "Wide")
            //              wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
            //      }
            //  }
            //  else
            //  {
            eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.EyeDef.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.white);
            browGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.BrowDef.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.white);

            if (oldAge)
            {
                if (pawnSave.type == "Normal")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                if (pawnSave.type == "Pointy")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                if (pawnSave.type == "Wide")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.white);

            }
            //   }

            var temptexturefront = eyeGraphic.MatFront.mainTexture as Texture2D;
            var temptextureside = eyeGraphic.MatSide.mainTexture as Texture2D;

            if (pawn.story.crownType == CrownType.Narrow)
            {

                MergeTwoGraphics(ref finalHeadFront, temptexturefront, Color.black);
                MergeTwoGraphics(ref finalHeadSide, temptextureside, Color.black);

            }
            else
            {

                MergeTwoGraphics(ref finalHeadFront, temptexturefront, Color.black);
                MergeTwoGraphics(ref finalHeadSide, temptextureside, Color.black);
            }

            temptexturefront = browGraphic.MatFront.mainTexture as Texture2D;
            temptextureside = browGraphic.MatSide.mainTexture as Texture2D;
            if (pawn.story.crownType == CrownType.Narrow)
            {
                MergeTwoGraphics(ref finalHeadFront, temptexturefront, Color.black);
                MergeTwoGraphics(ref finalHeadSide, temptextureside, Color.black);

            }
            else
            {

                MergeTwoGraphics(ref finalHeadFront, temptexturefront, Color.black);
                MergeTwoGraphics(ref finalHeadSide, temptextureside, Color.black);
            }

            #region Male
            if (pawn.gender == Gender.Male)
            {
                Graphic beardGraphic = null;

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

                //     }


                if (oldAge)
                {
                    temptexturefront = wrinkleGraphic.MatFront.mainTexture as Texture2D;
                    temptextureside = wrinkleGraphic.MatSide.mainTexture as Texture2D;

                    MakeOld(pawn, ref finalHeadFront, temptexturefront);
                    MakeOld(pawn, ref finalHeadSide, temptextureside);
                }

                if (pawnSave.BeardDef.drawMouth)
                {

                    Graphic lipGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.LipDef.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    temptexturefront = lipGraphic.MatFront.mainTexture as Texture2D;
                    temptextureside = lipGraphic.MatSide.mainTexture as Texture2D;
                    if (pawn.story.crownType == CrownType.Narrow)
                    {

                        MergeTwoGraphics(ref finalHeadFront, temptexturefront, Color.white);
                        MergeTwoGraphics(ref finalHeadSide, temptextureside, Color.white);

                    }
                    else
                    {

                        MergeTwoGraphics(ref finalHeadFront, temptexturefront, Color.white);
                        MergeTwoGraphics(ref finalHeadSide, temptextureside, Color.white);
                    }
                }

                temptexturefront = beardGraphic.MatFront.mainTexture as Texture2D;
                temptextureside = beardGraphic.MatSide.mainTexture as Texture2D;

                if (pawn.story.crownType == CrownType.Narrow)
                {
                AddFacialHair(pawn, ref finalHeadFront, temptexturefront);
                AddFacialHair(pawn, ref finalHeadSide, temptextureside);
                }
                else
                {
                    
                AddFacialHair(pawn, ref finalHeadFront, temptexturefront);
                AddFacialHair(pawn, ref finalHeadSide, temptextureside);
                }


            }

            #endregion

            #region Female
            if (pawn.gender == Gender.Female)
            {
                Graphic lipGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.LipDef.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.white);

                if (oldAge)
                {
                    temptexturefront = wrinkleGraphic.MatFront.mainTexture as Texture2D;
                    temptextureside = wrinkleGraphic.MatSide.mainTexture as Texture2D;

                    MakeOld(pawn, ref finalHeadFront, temptexturefront);
                    MakeOld(pawn, ref finalHeadSide, temptextureside);

                }
                temptexturefront = (lipGraphic.MatFront.mainTexture as Texture2D);
                temptextureside = (lipGraphic.MatSide.mainTexture as Texture2D);
                if (pawn.story.crownType == CrownType.Narrow)
                {
                    MergeTwoGraphics(ref finalHeadFront, temptexturefront, Color.white);
                    MergeTwoGraphics(ref finalHeadSide, temptextureside, Color.white);
                }
                else
                {
                    MergeTwoGraphics(ref finalHeadFront, temptexturefront, Color.white);
                    MergeTwoGraphics(ref finalHeadSide, temptextureside, Color.white);

                }
            }
            #endregion
            temptexturefront = (MakeReadable(hairGraphic.MatFront.mainTexture as Texture2D));
            temptextureside = MakeReadable(hairGraphic.MatSide.mainTexture as Texture2D);
            var temptextureback = MakeReadable(hairGraphic.MatBack.mainTexture as Texture2D);

            //   temptexturefront = Object.Instantiate(hairGraphic.MatFront.mainTexture as Texture2D);
            //   temptextureside = Object.Instantiate(hairGraphic.MatSide.mainTexture as Texture2D);
            //   temptextureback = Object.Instantiate(hairGraphic.MatBack.mainTexture as Texture2D);

            if (pawn.story.crownType == CrownType.Narrow)
            {

                //       temptexturefront.Resize(64, 128);
                //       temptextureside.Resize(64, 128);
                //       temptextureback.Resize(64, 128);

            }

            //    MergeColor(ref finalHeadBack, pawn.story.SkinColor);

            MergeHeadWithHair(ref finalHeadFront, temptexturefront, pawn.story.hairColor);
            MergeHeadWithHair(ref finalHeadSide, temptextureside, pawn.story.hairColor);
            MergeHeadWithHair(ref finalHeadBack, temptextureback, pawn.story.hairColor);


            finalHeadFront.Compress(true);
            finalHeadSide.Compress(true);
            finalHeadBack.Compress(true);

            finalHeadFront.mipMapBias = 0.5f;
            finalHeadSide.mipMapBias = 0.5f;
            finalHeadBack.mipMapBias = 0.5f;

            finalHeadFront.Apply(false, true);
            finalHeadSide.Apply(false, true);
            finalHeadBack.Apply(false, true);


            headGraphic.MatFront.mainTexture = finalHeadFront;
            headGraphic.MatSide.mainTexture = finalHeadSide;
            headGraphic.MatBack.mainTexture = finalHeadBack;

            Object.DestroyImmediate(temptexturefront, true);
            Object.DestroyImmediate(temptextureside, true);
            Object.DestroyImmediate(temptextureback, true);


            pawnSave.sessionOptimized = true;

            //    moddedHeadGraphics.Add(new KeyValuePair<string, Graphic_Multi>(pawn + color.ToString(), headGraphic));

            return headGraphic;
        }
        private static void PaintHead(Texture2D finalHeadFront, Color color)
        {
            for (int x = 0; x < 128; x++)
            {

                for (int y = 0; y < 128; y++)
                {
                    Color headColor = finalHeadFront.GetPixel(x, y);
                    headColor *= color;

                    //      Color final_color = Color.Lerp(headColor, eyeColor, eyeColor.a / 1f);


                    finalHeadFront.SetPixel(x, y, headColor);
                }
            }

            finalHeadFront.Apply();
        }

        public static Texture2D MakeReadable(Texture2D texture)
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
            var myTexture2D = new Texture2D(texture.width, texture.width);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();

            // Reset the active RenderTexture
            //    RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);
            return myTexture2D;
            // "myTexture2D" now has the same pixels from "texture" and it's readable.
        }

        private static void AddFacialHair(Pawn pawn, ref Texture2D finalTexture, Texture2D beard)
        {
         
            int startX = 0;
            int startY = finalTexture.height - beard.height;

            for (int x = startX; x < finalTexture.width; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {
                    Color headColor = finalTexture.GetPixel(x, y);

                    Color beardColor;


                     beardColor = beard.GetPixel(x - startX, y - startY);
                        

                    beardColor *= pawn.story.hairColor;

                    Color final_color = Color.Lerp(headColor, beardColor, beardColor.a / 1f);

                    finalTexture.SetPixel(x, y, final_color);
                }
            }

            finalTexture.Apply();
        }

        private static void MergeTwoGraphics(ref Texture2D finalTexture, Texture2D topLayer, Color topColor)
        {
         
            for (int x = 0; x < 128; x++)
            {

                for (int y = 0; y < 128; y++)
                {
                    Color eyeColor;

                        eyeColor = topLayer.GetPixel(x, y);
                    Color headColor = finalTexture.GetPixel(x, y);
                    //          eyeColor = topLayer.GetPixel(x, y);
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

            int offset = ((finalTexture.width - top_layer.width) / 2);


            for (int x = startX; x < top_layer.width + offset; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {

                    Color headColor = finalTexture.GetPixel(x, y);
                    Color hairColor;

                                           hairColor = top_layer.GetPixel(x - startX + offset, y - startY);

                    if (y > 82)
                        hairColor.a = 0;
                    if (y > 79 && y < 82 && hairColor.a > 0)
                        hairColor = Color.black;

                    hairColor *= topColor;

                    Color final_color = Color.Lerp(headColor, hairColor, hairColor.a);

                    if (headColor.a > 0 || hairColor.a > 0)
                        final_color.a = headColor.a + hairColor.a;

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

        private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            Color[] rpixels = result.GetPixels(0);
            float incX = (1.0f / (float)targetWidth);
            float incY = (1.0f / (float)targetHeight);
            for (int px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
            }
            result.SetPixels(rpixels, 0);
            result.Apply();
            return result;
        }
    }
}


