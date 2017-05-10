using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;
using Object = UnityEngine.Object;

namespace RW_FacialStuff
{
    public static class GraphicDatabaseHeadRecordsModded 
    {
        // use mouth?

        public static List<HeadGraphicRecordVanillaCustom> headsVanillaCustom = new List<HeadGraphicRecordVanillaCustom>();
        public static List<HeadGraphicRecordModded> headsModded = new List<HeadGraphicRecordModded>();

        private static HeadGraphicRecordVanillaCustom skull;
        private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";



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

                string[] array = fileNameWithoutExtension?.Split('_');

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
                foreach (KeyValuePair<Color, Graphic_Multi> graphic_multi in graphics)
                {
                    if (color.IndistinguishableFrom(graphic_multi.Key))
                    {
                        return graphic_multi.Value;
                    }
                }
                Graphic_Multi graphicMultiHead = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPathVanillaCustom, ShaderDatabase.Cutout, Vector2.one, color);
                graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphicMultiHead));
                return graphicMultiHead;
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
                CompFace faceComp = pawn.TryGetComp<CompFace>();

                this.pawn = pawn;
                graphicPath = pawn.story.HeadGraphicPath;
                graphicPathModded = faceComp.headGraphicIndex;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(graphicPath);
                string[] array = fileNameWithoutExtension?.Split('_');
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
            CompFace faceComp = pawn.TryGetComp<CompFace>();

            if (useVanilla)
            {
                foreach (HeadGraphicRecordVanillaCustom headGraphicRecordVanillaCustom in headsVanillaCustom)
                {
                    if (headGraphicRecordVanillaCustom.graphicPathVanillaCustom == pawn.story.HeadGraphicPath.Remove(0, 22))
                    {
                        return headGraphicRecordVanillaCustom.GetGraphic(color);
                    }
                }
            }

            foreach (HeadGraphicRecordModded headGraphicRecordModded in headsModded)
            {
                if (headGraphicRecordModded.graphicPathModded == faceComp.headGraphicIndex)
                {
                    return headGraphicRecordModded.GetGraphicBlank(color);
                }
            }

            Log.Message("Tried to get pawn head at path " + faceComp.headGraphicIndex + " that was not found. Defaulting...");

            return headsVanillaCustom.First().GetGraphic(color);
        }

        public static void BuildDatabaseIfNecessary()
        {
            //  headsVanillaCustom.Clear();
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



        public static List<KeyValuePair<string, Graphic_Multi>> moddedHeadGraphics = new List<KeyValuePair<string, Graphic_Multi>>();

        //   private static List<Texture2D> _textures;

        public static Graphic_Multi ModifiedVanillaHead(Pawn pawn, Color color, Graphic hairGraphic)
        {

            //for (int i = 0; i < moddedHeadGraphics.Count; i++)
            //{
            //    if (i.Equals(pawn + color.ToString()))
            //    {
            //        return moddedHeadGraphics[i].Value;
            //    }
            //}


            CompFace faceComp = pawn.TryGetComp<CompFace>();
            Graphic_Multi headGraphic = GetModdedHeadNamed(pawn, false, Color.white);

            // grab the blank texture instead of Vanilla
            Graphic headGraphicVanilla = GetModdedHeadNamed(pawn, true, Color.white);
            bool oldAge = pawn.ageTracker.AgeBiologicalYearsFloat >= 50;



            Texture2D finalHeadFront = MakeReadable(headGraphicVanilla.MatFront.mainTexture as Texture2D);
            Texture2D finalHeadSide = MakeReadable(headGraphicVanilla.MatSide.mainTexture as Texture2D);
            Texture2D finalHeadBack = MakeReadable(headGraphicVanilla.MatBack.mainTexture as Texture2D);

            PaintHead(finalHeadFront, color);
            PaintHead(finalHeadSide, color);
            PaintHead(finalHeadBack, color);

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
            Graphic eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(faceComp.EyeDef.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);
            Graphic browGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(faceComp.BrowDef.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);

            if (oldAge)
            {
                if (faceComp.type == "Normal")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(faceComp.WrinkleDef.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (faceComp.type == "Pointy")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(faceComp.WrinkleDef.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (faceComp.type == "Wide")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(faceComp.WrinkleDef.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.black);

            }
            //   }

            Texture2D temptexturefront = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            Texture2D temptextureside = new Texture2D(1, 1, TextureFormat.ARGB32, false);

            // if (_textures.Contains(eyeGraphic.MatFront.mainTexture as Texture2D))
            // {
            //     _textures[1]
            // }
            // _textures.Add(eyeGraphic.MatFront.mainTexture as Texture2D);

            // Brows

            if (pawn.story.crownType == CrownType.Narrow)
            {
                ScaleTexture(browGraphic.MatFront.mainTexture as Texture2D, ref temptexturefront, 102, 128);
                ScaleTexture(browGraphic.MatSide.mainTexture as Texture2D, ref temptextureside, 102, 128);
            }
            else
            {
                temptexturefront = MakeReadable(browGraphic.MatFront.mainTexture as Texture2D);
                temptextureside = MakeReadable(browGraphic.MatSide.mainTexture as Texture2D);
            }
            MergeTwoGraphics(ref finalHeadFront, temptexturefront, pawn.story.hairColor * new Color(0.3f, 0.3f, 0.3f));
            MergeTwoGraphics(ref finalHeadSide, temptextureside, pawn.story.hairColor * new Color(0.3f, 0.3f, 0.3f));

            // Eyes

            if (pawn.story.crownType == CrownType.Narrow)
            {
                ScaleTexture(eyeGraphic.MatFront.mainTexture as Texture2D, ref temptexturefront, 102, 128);
                ScaleTexture(eyeGraphic.MatSide.mainTexture as Texture2D, ref temptextureside, 102, 128);
            }
            else
            {
                temptexturefront = MakeReadable(eyeGraphic.MatFront.mainTexture as Texture2D);
                temptextureside = MakeReadable(eyeGraphic.MatSide.mainTexture as Texture2D);
            }
            MergeTwoGraphics(ref finalHeadFront, temptexturefront, Color.black);
            MergeTwoGraphics(ref finalHeadSide, temptextureside, Color.black);

            #region Male
            if (pawn.gender == Gender.Male)
            {
                Graphic beardGraphic = null;

                if (faceComp.type == "Normal")
                {
                    beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(faceComp.BeardDef.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }
                if (faceComp.type == "Pointy")
                {
                    beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(faceComp.BeardDef.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }
                if (faceComp.type == "Wide")
                {
                    beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(faceComp.BeardDef.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }

                //     }


                if (oldAge)
                {
                    temptexturefront = MakeReadable(wrinkleGraphic.MatFront.mainTexture as Texture2D);
                    temptextureside = MakeReadable(wrinkleGraphic.MatSide.mainTexture as Texture2D);

                    MakeOld(pawn, ref finalHeadFront, temptexturefront);
                    MakeOld(pawn, ref finalHeadSide, temptextureside);
                }


                if (faceComp.BeardDef.drawMouth && faceComp.drawMouth)
                {

                    Graphic mouthGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(faceComp.MouthDef.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    if (pawn.story.crownType == CrownType.Narrow)
                    {
                        ScaleTexture(mouthGraphic.MatFront.mainTexture as Texture2D, ref temptexturefront, 102, 128);
                        ScaleTexture(mouthGraphic.MatSide.mainTexture as Texture2D, ref temptextureside, 102, 128);
                    }
                    else
                    {
                        temptexturefront = MakeReadable(mouthGraphic.MatFront.mainTexture as Texture2D);
                        temptextureside = MakeReadable(mouthGraphic.MatSide.mainTexture as Texture2D);
                    }
                    MergeTwoGraphics(ref finalHeadFront, temptexturefront, Color.black);
                    MergeTwoGraphics(ref finalHeadSide, temptextureside, Color.black);
                }


                if (pawn.story.crownType == CrownType.Narrow)
                {
                    ScaleTexture(beardGraphic.MatFront.mainTexture as Texture2D, ref temptexturefront, 102, 128);
                    ScaleTexture(beardGraphic.MatSide.mainTexture as Texture2D, ref temptextureside, 102, 128);
                }
                else
                {
                    temptexturefront = MakeReadable(beardGraphic.MatFront.mainTexture as Texture2D);
                    temptextureside = MakeReadable(beardGraphic.MatSide.mainTexture as Texture2D);
                }

                AddFacialHair(pawn, ref finalHeadFront, temptexturefront);
                AddFacialHair(pawn, ref finalHeadSide, temptextureside);

            }

            #endregion

            #region Female
            if (pawn.gender == Gender.Female)
            {

                if (oldAge)
                {
                    temptexturefront = MakeReadable(wrinkleGraphic.MatFront.mainTexture as Texture2D);
                    temptextureside = MakeReadable(wrinkleGraphic.MatSide.mainTexture as Texture2D);

                    MakeOld(pawn, ref finalHeadFront, temptexturefront);
                    MakeOld(pawn, ref finalHeadSide, temptextureside);

                }
#if !NoCCL
                if (ModConfigMenu.useMouth)
#else
                if (faceComp.drawMouth)
#endif
                {
                    Graphic lipGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(faceComp.MouthDef.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    if (pawn.story.crownType == CrownType.Narrow)
                    {
                        ScaleTexture(lipGraphic.MatFront.mainTexture as Texture2D, ref temptexturefront, 102, 128);
                        ScaleTexture(lipGraphic.MatSide.mainTexture as Texture2D, ref temptextureside, 102, 128);
                    }
                    else
                    {
                        temptexturefront = MakeReadable(lipGraphic.MatFront.mainTexture as Texture2D);
                        temptextureside = MakeReadable(lipGraphic.MatSide.mainTexture as Texture2D);
                    }
                    MergeTwoGraphics(ref finalHeadFront, temptexturefront, Color.black);
                    MergeTwoGraphics(ref finalHeadSide, temptextureside, Color.black);
                }
            }
            #endregion

            //   temptexturefront = Object.Instantiate(hairGraphic.MatFront.mainTexture as Texture2D);
            //   temptextureside = Object.Instantiate(hairGraphic.MatSide.mainTexture as Texture2D);
            //   temptextureback = Object.Instantiate(hairGraphic.MatBack.mainTexture as Texture2D);
            Texture2D temptextureback = new Texture2D(1, 1, TextureFormat.ARGB32, false);

            if (pawn.story.crownType == CrownType.Narrow)
            {
                ScaleTexture(MakeReadable(hairGraphic.MatFront.mainTexture as Texture2D), ref temptexturefront, 112, 128);
                ScaleTexture(MakeReadable(hairGraphic.MatSide.mainTexture as Texture2D), ref temptextureside, 112, 128);
                ScaleTexture(MakeReadable(hairGraphic.MatBack.mainTexture as Texture2D), ref temptextureback, 112, 128);
            }
            else
            {
                temptexturefront = MakeReadable(hairGraphic.MatFront.mainTexture as Texture2D);
                temptextureside = MakeReadable(hairGraphic.MatSide.mainTexture as Texture2D);
                temptextureback = MakeReadable(hairGraphic.MatBack.mainTexture as Texture2D);
            }

            //    MergeColor(ref finalHeadBack, pawn.story.SkinColor);
            if (pawn.story.crownType == CrownType.Narrow)
            {
                MergeHeadWithHair(ref finalHeadFront, temptexturefront, MaskTextures.MaskTex_Narrow_FrontBack, pawn.story.hairColor);
                MergeHeadWithHair(ref finalHeadSide, temptextureside, MaskTextures.MaskTex_Narrow_Side, pawn.story.hairColor);
                MergeHeadWithHair(ref finalHeadBack, temptextureback, MaskTextures.MaskTex_Narrow_FrontBack, pawn.story.hairColor);
            }
            else
            {
                MergeHeadWithHair(ref finalHeadFront, temptexturefront, MaskTextures.MaskTex_Average_FrontBack, pawn.story.hairColor);
                MergeHeadWithHair(ref finalHeadSide, temptextureside, MaskTextures.MaskTex_Average_Side, pawn.story.hairColor);
                MergeHeadWithHair(ref finalHeadBack, temptextureback, MaskTextures.MaskTex_Average_FrontBack, pawn.story.hairColor);
            }

            if (false)
            {
                byte[] bytes = finalHeadFront.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/MergedHeads/" + pawn.Name + "_01front.png", bytes);
                byte[] bytes2 = finalHeadSide.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/MergedHeads/" + pawn.Name + "_02side.png", bytes2);
                byte[] bytes3 = finalHeadBack.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/MergedHeads/" + pawn.Name + "_03back.png", bytes3);
            }

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

            faceComp.sessionOptimized = true;

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
            Texture2D myTexture2D = new Texture2D(texture.width, texture.width, TextureFormat.ARGB32, false);

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

        private static void AddFacialHair(Pawn pawn, ref Texture2D finalTexture, Texture2D beardTex)
        {
            var tempBeardTex = MakeReadable(beardTex);
            // offset neede if beards are stretched => narrow
            int offset = (finalTexture.width - tempBeardTex.width) / 2;
            int startX = 0;
            int startY = finalTexture.height - tempBeardTex.height;

            for (int x = startX; x < finalTexture.width; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {
                    Color headColor = finalTexture.GetPixel(x, y);

                    Color beardColor;


                    beardColor = tempBeardTex.GetPixel(x - startX - offset, y - startY);


                    beardColor *= pawn.story.hairColor;

                    Color final_color = Color.Lerp(headColor, beardColor, beardColor.a / 1f);

                    final_color.a = headColor.a + beardColor.a;

                    finalTexture.SetPixel(x, y, final_color);
                }
            }

            finalTexture.Apply();
        }

        private static void MergeTwoGraphics(ref Texture2D finalTexture, Texture2D topLayerTex, Color multiplyColor)
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
                    //          eyeColor = topLayerTex.GetPixel(x, y);
                    topColor *= multiplyColor;
                    //      eyeColor *= eyeColorRandom;

                    Color final_color = Color.Lerp(headColor, topColor, topColor.a / 1f);

                    final_color.a = headColor.a + topColor.a;

                    finalTexture.SetPixel(x, y, final_color);
                }
            }


            finalTexture.Apply();
        }

        private static void MergeHeadWithHair(ref Texture2D finalTexture, Texture2D top_layer, Color topColor)
        {

            int offset = (finalTexture.width - top_layer.width) / 2;

            int startX = 0;
            int startY = finalTexture.height - top_layer.height;


            for (int x = startX; x < top_layer.width + offset; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {

                    Color headColor = finalTexture.GetPixel(x, y);
                    Color hairColor;

                    hairColor = top_layer.GetPixel(x - startX - offset, y - startY);

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

        private static void MergeHeadWithHair(ref Texture2D finalTexture, Texture2D top_layer, Texture2D maskTex, Color topColor)
        {
            var tempMaskTex = MakeReadable(maskTex);

            int offset = (finalTexture.width - top_layer.width) / 2;

            int startX = 0;
            int startY = finalTexture.height - top_layer.height;


            for (int x = startX; x < top_layer.width + offset; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {

                    Color headColor = finalTexture.GetPixel(x, y);
                    Color maskColor = tempMaskTex.GetPixel(x, y);

                    Color hairColor = top_layer.GetPixel(x - startX - offset, y - startY);

                    hairColor *= maskColor;

                    hairColor *= topColor;

                    Color final_color = Color.Lerp(headColor, hairColor, hairColor.a);

                    if (headColor.a > 0 || hairColor.a > 0)
                        final_color.a = headColor.a + hairColor.a;

                    finalTexture.SetPixel(x, y, final_color);
                }
            }

            finalTexture.Apply();
        }


        private static void MakeOld(Pawn pawn, ref Texture2D finalhead, Texture2D wrinkleTex)
        {
            var tempWrinkleTex = MakeReadable(wrinkleTex);
            int startX = 0;
            int startY = finalhead.height - tempWrinkleTex.height;

            for (int x = startX; x < finalhead.width; x++)
            {
                for (int y = startY; y < finalhead.height; y++)
                {
                    Color headColor = finalhead.GetPixel(x, y);
                    Color wrinkleColor = tempWrinkleTex.GetPixel(x - startX, y - startY);

                    Color final_color = headColor;

                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 0.6f) * Mathf.InverseLerp(50, 200, pawn.ageTracker.AgeBiologicalYearsFloat));

                    final_color.a = headColor.a + wrinkleColor.a;

                    finalhead.SetPixel(x, y, final_color);
                }
            }

            finalhead.Apply();
        }

        private static Color[] destPix;

        private static void ScaleTexture(Texture2D sourceTex, ref Texture2D destTex, int targetWidth, int targetHeight)
        {
            float warpFactorX = 1f;
            float warpFactorY = 1f;

            var scaleTex = MakeReadable(sourceTex);

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

        }
    }
}


