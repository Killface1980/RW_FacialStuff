using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CommunityCoreLibrary;
using CommunityCoreLibrary.ColorPicker;
using RimWorld;
using RW_FacialStuff.Defs;
using UnityEngine;
using Verse;
using Object = UnityEngine.Object;

namespace RW_FacialStuff
{
    public class GraphicDatabaseHeadRecordsModded : GraphicDatabaseHeadRecords
    {
        public static string GetModTexturePath()
        {
            //       if (File.Exists("Mods/RW_FacialStuff/Textures/Things/Pawn/Humanlike/Heads/Female/Female_Average_Normal_back.png"))
            //           return "Mods/RW_FacialStuff/Textures/";

            return "Mods/RW_FacialStuff/Textures/";
            //   return "726338068/Textures/";

        }


        public static List<HeadGraphicRecord> heads = new List<HeadGraphicRecord>();
        public static List<HeadGraphicRecordModded> headsModded = new List<HeadGraphicRecordModded>();

        private static HeadGraphicRecord skull;
        private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        protected Pawn pawn;

        public static int headIndex = 0;


        public static void DefineHeadParts(Pawn pawn)
        {
            SaveablePawn pawnSave = MapComponent_FacialStuff.GetCache(pawn);

            if (pawn.story.HeadGraphicPath.Contains("Normal"))
                pawnSave.type = "Normal";

            if (pawn.story.HeadGraphicPath.Contains("Pointy"))
                pawnSave.type = "Pointy";

            if (pawn.story.HeadGraphicPath.Contains("Wide"))
                pawnSave.type = "Wide";


            pawnSave.EyeDef = PawnFaceMaker.RandomEyeDefFor(pawn, pawn.Faction.def);

            pawnSave.WrinkleDef = PawnFaceMaker.AssignWrinkleDefFor(pawn, pawn.Faction.def);


            pawnSave.SkinColorHex = ColorHelper.RGBtoHex(pawn.story.SkinColor);

            if (pawn.gender == Gender.Female)
            {
                pawnSave.LipDef = PawnFaceMaker.RandomLipDefFor(pawn, pawn.Faction.def);
            }


            if (pawn.gender == Gender.Male)
            {

                pawnSave.BeardDef = PawnFaceMaker.RandomBeardDefFor(pawn, pawn.Faction.def);

                pawnSave.HairColorHex = ColorHelper.RGBtoHex(pawn.story.hairColor);
            }
            pawnSave.optimized = true;
        }

        public static void ModifyVanillaHead(Pawn pawn, Graphic hairGraphic, ref Graphic headGraphic)
        {
            Graphic headGraphicVanilla = GetHeadNamed(pawn.story.HeadGraphicPath, Color.white);

            var pawnSave = MapComponent_FacialStuff.GetCache(pawn);

            Texture2D headGraphicFront = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Texture2D headGraphicSide = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Texture2D headGraphicBack = new Texture2D(128, 128, TextureFormat.RGBA32, false);

            Texture2D finalHeadFront = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Texture2D finalHeadSide = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Texture2D finalHeadBack = new Texture2D(128, 128, TextureFormat.RGBA32, false);

            Texture2D beardFront = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Texture2D beardSide = new Texture2D(128, 128, TextureFormat.RGBA32, false);

            Texture2D eyesHeadFront = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Texture2D eyesHeadSide = new Texture2D(128, 128, TextureFormat.RGBA32, false);

            Texture2D wrinklesHeadFront = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Texture2D wrinklesHeadSide = new Texture2D(128, 128, TextureFormat.RGBA32, false);

            Texture2D temptexturefront = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Texture2D temptextureside = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            // Texture2D temptextureback = new Texture2D(128, 128);

            Texture2D newhairfront = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Texture2D newhairside = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Texture2D newhairback = new Texture2D(128, 128, TextureFormat.RGBA32, false);

            MakeReadable(headGraphicVanilla.MatFront.mainTexture as Texture2D, ref headGraphicFront);
            MakeReadable(headGraphicVanilla.MatSide.mainTexture as Texture2D, ref headGraphicSide);
            MakeReadable(headGraphicVanilla.MatBack.mainTexture as Texture2D, ref headGraphicBack);

            MakeReadable(hairGraphic.MatFront.mainTexture as Texture2D, ref newhairfront);
            MakeReadable(hairGraphic.MatSide.mainTexture as Texture2D, ref newhairside);
            MakeReadable(hairGraphic.MatBack.mainTexture as Texture2D, ref newhairback);

            Graphic eyeGraphic;
            Graphic wrinkleGraphic = null;

            if (pawn.story.crownType == CrownType.Narrow)
            {
                eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.EyeDef.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.black);

                if (pawnSave.type == "Normal")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowNormal, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (pawnSave.type == "Pointy")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowPointy, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (pawnSave.type == "Wide")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowWide, ShaderDatabase.Cutout, Vector2.one, Color.black);
            }
            else
            {
                eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.EyeDef.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.black);

                if (pawnSave.type == "Normal")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (pawnSave.type == "Pointy")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (pawnSave.type == "Wide")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.black);
            }

            Texture2D readEyeGraphicFront = eyeGraphic.MatFront.mainTexture as Texture2D;
            Texture2D readEyeGraphicSide = eyeGraphic.MatSide.mainTexture as Texture2D;

            Texture2D readWrinkleGraphicFront = wrinkleGraphic.MatFront.mainTexture as Texture2D;
            Texture2D readWrinkleGraphicSide = wrinkleGraphic.MatSide.mainTexture as Texture2D;


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

                Texture2D readBeardGraphicFront = beardGraphic.MatFront.mainTexture as Texture2D;
                Texture2D readBeardGraphicSide = beardGraphic.MatSide.mainTexture as Texture2D;

                MergeTwoGraphics(headGraphicFront, pawn.story.SkinColor, readEyeGraphicFront, ref eyesHeadFront);
                MergeTwoGraphics(headGraphicSide, pawn.story.SkinColor, readEyeGraphicSide, ref eyesHeadSide);

                if (pawn.ageTracker.AgeBiologicalYearsFloat >= 40)
                {
                    MakeOld(pawn, eyesHeadFront, readWrinkleGraphicFront, ref wrinklesHeadFront);
                    MakeOld(pawn, eyesHeadSide, readWrinkleGraphicSide, ref wrinklesHeadSide);

                    AddFacialHair(pawn, wrinklesHeadFront, readBeardGraphicFront, ref temptexturefront);
                    AddFacialHair(pawn, wrinklesHeadSide, readBeardGraphicSide, ref temptextureside);
                }
                else
                {
                    AddFacialHair(pawn, eyesHeadFront, readBeardGraphicFront, ref temptexturefront);
                    AddFacialHair(pawn, eyesHeadSide, readBeardGraphicSide, ref temptextureside);
                }

            }


            if (pawn.gender == Gender.Female)
            {
                Graphic lipGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawn.story.crownType == CrownType.Narrow ? pawnSave.LipDef.texPathNarrow : pawnSave.LipDef.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.white);

                Texture2D readLipGraphicFront = lipGraphic.MatFront.mainTexture as Texture2D;
                Texture2D readLipGraphicSide = lipGraphic.MatSide.mainTexture as Texture2D;


                if (pawn.ageTracker.AgeBiologicalYearsFloat >= 40)
                {
                    MakeOld(pawn, headGraphicFront, readWrinkleGraphicFront, ref wrinklesHeadFront);
                    MakeOld(pawn, headGraphicSide, readWrinkleGraphicSide, ref wrinklesHeadSide);

                    MergeThreeGraphics(wrinklesHeadFront, pawn.story.SkinColor, readEyeGraphicFront, readLipGraphicFront, ref temptexturefront);
                    MergeThreeGraphics(wrinklesHeadSide, pawn.story.SkinColor, readEyeGraphicSide, readLipGraphicSide, ref temptextureside);
                }
                else
                {
                    MergeThreeGraphics(headGraphicFront, pawn.story.SkinColor, readEyeGraphicFront, readLipGraphicFront, ref temptexturefront);
                    MergeThreeGraphics(headGraphicSide, pawn.story.SkinColor, readEyeGraphicSide, readLipGraphicSide, ref temptextureside);
                }
            }


            // if (pawn.story.crownType == CrownType.Narrow)
            // {
            //
            //     TextureScale.Point(newhairfront, 112, 128);
            //     TextureScale.Point(newhairside, 112, 128);
            //     TextureScale.Point(newhairback, 112, 128);
            //
            //     TextureScale.ResizeCanvas(newhairfront, 128, 128);
            //     TextureScale.ResizeCanvas(newhairside, 128, 128);
            //     TextureScale.ResizeCanvas(newhairback, 128, 128);
            //
            // }

            MergeHeadWithHair(temptexturefront, newhairfront, pawn.story.hairColor, ref finalHeadFront);
            MergeHeadWithHair(temptextureside, newhairside, pawn.story.hairColor, ref finalHeadSide);
            MergeHeadWithHair(headGraphicBack, newhairback, pawn.story.hairColor, ref finalHeadBack);

            finalHeadFront.Compress(true);
            finalHeadSide.Compress(true);
            finalHeadBack.Compress(true);


            headGraphic.MatFront.mainTexture = finalHeadFront;
            headGraphic.MatSide.mainTexture = finalHeadSide;
            headGraphic.MatBack.mainTexture = finalHeadBack;

            Object.DestroyImmediate(headGraphicFront, true);
            Object.DestroyImmediate(headGraphicSide, true);
            Object.DestroyImmediate(headGraphicBack, true);

            Object.DestroyImmediate(beardFront, true);
            Object.DestroyImmediate(beardSide, true);

            Object.DestroyImmediate(eyesHeadFront, true);
            Object.DestroyImmediate(eyesHeadSide, true);

            Object.DestroyImmediate(wrinklesHeadFront, true);
            Object.DestroyImmediate(wrinklesHeadSide, true);

            Object.DestroyImmediate(temptexturefront, true);
            Object.DestroyImmediate(temptextureside, true);

            Object.DestroyImmediate(newhairfront, true);
            Object.DestroyImmediate(newhairside, true);
            Object.DestroyImmediate(newhairback, true);

            //Object.DestroyImmediate(temptextureback, true);



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



        /*
        private static void MergeThreeGraphics(Texture2D layer_1, Texture2D layer_2, Texture2D layer_3, ref Texture2D texture_final)
        {
            int startX = 0;
            int startY = 0;

            for (int x = startX; x < layer_1.width; x++)
            {

                for (int y = startY; y < layer_1.height; y++)
                {
                    Color layer1Color = layer_1.GetPixel(x, y);
                    Color layer2Color = layer_2.GetPixel(x - startX, y - startY);
                    Color layer3Color = layer_3.GetPixel(x - startX, y - startY);

                    Color mixcolor = Color.Lerp(layer1Color, layer2Color, layer2Color.a / 1f);
                    Color final_color = Color.Lerp(mixcolor, layer3Color, layer3Color.a / 1f);
                    //      if (layer1Color.a == 1 || layer2Color.a == 1 || layer3Color.a == 1)
                    final_color.a = layer1Color.a + layer2Color.a + layer3Color.a;
                    //    if (final_color.a >= 0.6)
                    //        final_color.a = 1;
                    //    else
                    //        final_color.a = 0;
                    texture_final.SetPixel(x, y, final_color);
                }
            }
            texture_final.Apply();
        }
*/
        private static void MergeThreeGraphics(Texture2D layer_1, Color layer1BlendColor, Texture2D layer_2, Texture2D layer_3, ref Texture2D texture_final)
        {
            int startX = 0;
            int startY = 0;

            //float num1 = Rand.Value;
            //float num2 = Rand.Value;
            //float num3 = Rand.Value;
            //
            //Color eyeColorRandom = new Color(num1, num2, num3);


            for (int x = startX; x < layer_1.width; x++)
            {

                for (int y = startY; y < layer_1.height; y++)
                {
                    Color layer1Color = layer_1.GetPixel(x, y);
                    layer1Color *= layer1BlendColor;
                    Color layer2Color = layer_2.GetPixel(x - startX, y - startY);
                    Color layer3Color = layer_3.GetPixel(x - startX, y - startY);

                    //    layer2Color *= eyeColorRandom;

                    Color mixcolor = Color.Lerp(layer1Color, layer2Color, layer2Color.a / 1f);
                    Color final_color = Color.Lerp(mixcolor, layer3Color, layer3Color.a / 1f);
                    if (layer1Color.a == 1)
                        final_color.a = 1;
                    texture_final.SetPixel(x, y, final_color);
                }
            }
            texture_final.Apply();
        }


        private static void AddFacialHair(Pawn pawn, Texture2D head, Texture2D beard, ref Texture2D finalhead)
        {
            int startX = 0;
            int startY = head.height - beard.height;

            for (int x = startX; x < head.width; x++)
            {

                for (int y = startY; y < head.height; y++)
                {
                    Color headColor = head.GetPixel(x, y);
                    Color beardColor = beard.GetPixel(x - startX, y - startY);

                    beardColor *= pawn.story.hairColor;



                    //beardColor.r = Math.Min(beardColor.r, pawn.story.hairColor.r) - 0.1f;
                    //beardColor.g = Math.Min(beardColor.g, pawn.story.hairColor.g) - 0.1f;
                    //beardColor.b = Math.Min(beardColor.b, pawn.story.hairColor.b) - 0.1f;


                    //if (BeardColorNamed.Equals("Grey"))
                    //{
                    //    final_color = Color.Lerp(headColor, beardColor * ColorGrey, beardColor.a / 1f);
                    //}
                    //else if (BeardColorNamed.Contains("Dark"))
                    //{
                    //    final_color = Color.Lerp(headColor, beardColor * Dark01, beardColor.a / 1f);
                    //
                    //}
                    //else
                    Color final_color = Color.Lerp(headColor, beardColor, beardColor.a / 1f);

                    finalhead.SetPixel(x, y, final_color);
                }
            }

            finalhead.Apply();
        }

        private static void MergeTwoGraphics(Texture2D bottom_layer, Color bottomColor, Texture2D top_layer, ref Texture2D finalTexture)
        {
            int startX = 0;
            int startY = bottom_layer.height - top_layer.height;


            for (int x = startX; x < bottom_layer.width; x++)
            {

                for (int y = startY; y < bottom_layer.height; y++)
                {
                    Color headColor = bottom_layer.GetPixel(x, y);
                    Color eyeColor = top_layer.GetPixel(x - startX, y - startY);
                    headColor *= bottomColor;
                    //      eyeColor *= eyeColorRandom;

                    Color final_color = Color.Lerp(headColor, eyeColor, eyeColor.a / 1f);

                    if (headColor.a == 1)
                        final_color.a = 1;

                    finalTexture.SetPixel(x, y, final_color);
                }
            }

            finalTexture.Apply();
        }

        public static void MergeHeadWithHair(Texture2D bottom_layer, Texture2D top_layer, Color topColor, ref Texture2D finalTexture)
        {
            int startX = 0;
            int startY = bottom_layer.height - top_layer.height;


            for (int x = startX; x < bottom_layer.width; x++)
            {

                for (int y = startY; y < bottom_layer.height; y++)
                {

                    Color headColor = bottom_layer.GetPixel(x, y);
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

        private static void MakeOld(Pawn pawn, Texture2D head, Texture2D wrinkles, ref Texture2D finalhead)
        {

            int startX = 0;
            int startY = head.height - wrinkles.height;

            for (int x = startX; x < head.width; x++)
            {

                for (int y = startY; y < head.height; y++)
                {
                    Color headColor = head.GetPixel(x, y);
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

        public static void BuildDatabaseIfNecessary()
        {
            heads.Clear();
            if (heads.Count > 0 && skull != null)
            {
                return;
            }

            // now adding only vanilla cleared heads to the main db

            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Female/Female_Average_Normal2"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Female/Female_Average_Pointy2"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Female/Female_Average_Wide2"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Female/Female_Narrow_Normal2"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Female/Female_Narrow_Pointy2"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Female/Female_Narrow_Wide2"));

            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Male/Male_Average_Normal2"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Male/Male_Average_Pointy2"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Male/Male_Average_Wide2"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Male/Male_Narrow_Normal2"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Male/Male_Narrow_Pointy2"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Male/Male_Narrow_Wide2"));

            skull = new HeadGraphicRecord(SkullPath);


            //   string[] headsFolderPaths = HeadsFolderPaths;
            //   for (int i = 0; i < headsFolderPaths.Length; i++)
            //   {
            //       string text = headsFolderPaths[i];
            //       foreach (string current in GraphicDatabaseUtility.GraphicNamesInFolder(text))
            //       {
            //           heads.Add(new HeadGraphicRecord(text + "/" + current));
            //       }
            //   }
        }


        public class HeadGraphicRecord
        {
            public Gender gender;

            public CrownType crownType;

            public string graphicPath;

            public List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

            public HeadGraphicRecord(string graphicPath)
            {
                this.graphicPath = graphicPath;
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
                Graphic_Multi graphic_Multi_Head = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPath, ShaderDatabase.Cutout, Vector2.one, color);
                //   Graphic_Multi graphic_Multi_Head = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPath, ShaderDatabase.CutoutSkin, Vector2.one, color);
                graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphic_Multi_Head));
                return graphic_Multi_Head;
            }
        }

        public class HeadGraphicRecordModded
        {
            public Pawn pawn = null;


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

            public Graphic_Multi GetGraphicBlank()
            {
                Graphic_Multi graphic_Multi_Head = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPathModded, ShaderDatabase.Cutout, Vector2.one, Color.white);

                return graphic_Multi_Head;
            }

        }

        public static Graphic_Multi GetModdedHeadNamed(Pawn pawn)
        {
            var pawnSave = MapComponent_FacialStuff.GetCache(pawn);

            for (int i = 0; i < headsModded.Count; i++)
            {
                HeadGraphicRecordModded headGraphicRecordModded = headsModded[i];

                if (headGraphicRecordModded.graphicPathModded == pawnSave.headGraphicIndex)
                {
                    return headGraphicRecordModded.GetGraphicBlank();
                }
            }

            Log.Message("Tried to get pawn head at path " + pawnSave.headGraphicIndex + " that was not found. Defaulting...");

            return heads.First().GetGraphic(pawn.story.SkinColor);
        }

    }
}


