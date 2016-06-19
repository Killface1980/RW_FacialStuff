using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{

    public class GraphicDatabaseHeadRecordsModded : GraphicDatabaseHeadRecords
    {
        public static string modpath = "Mods/RW_FacialStuff/Textures/";

        public static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

        public static List<Texture2D> textureList = new List<Texture2D>();

        private static EyeDef _saveableEye;
        private static BeardDef _saveableBeard;
        private static SideburnDef _saveableSideburn;
        private static TacheDef _saveableTache;



        public static Texture2D LoadTexture(string texturePath)
        {
            Texture2D texture;
            //           Debug.LogWarning("RW_Facial TextPath: " + texturePath);
            //      if (textureCache.TryGetValue(texturePath, out texture)) return texture;

            texture = new Texture2D(1, 1);
            texture.LoadImage(File.ReadAllBytes(modpath + texturePath + ".png"));
            texture.anisoLevel = 8;
            texture.name = Path.GetFileName(modpath + texturePath + ".png");

            //      texture.Compress(true);


            //        textureCache.Add(texturePath, texture);

            //         Debug.LogWarning("RW_Facial added to cache: " + texture.name);

            return texture;
        }

        public static void DecorateHead(ref string graphicPath, Pawn pawn, Color skinColor, Color haircolor)
        {

            var pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);

            string graphicPathNew = null;

            string type = null;

            if (graphicPath.Contains("Normal"))
                type = "Normal";

            if (graphicPath.Contains("Pointy"))
                type = "Pointy";

            if (graphicPath.Contains("Wide"))
                type = "Wide";


            if (pawnSave.EyeDef != null)
            {
                _saveableEye = pawnSave.EyeDef;
            }
            else
            {
                _saveableEye = PawnFaceChooser.RandomEyeDefFor(pawn, pawn.Faction.def);
                pawnSave.EyeDef = _saveableEye;
            }

            if (pawn.gender == Gender.Female)
            {
                graphicPathNew = "Things/Pawn/Humanlike/Heads/" + pawn.gender + "/" + pawn.gender + "_" + pawn.story.crownType + "_" + type + "-" + pawnSave.EyeDef.label;
            }


            if (pawn.gender == Gender.Male)
            {
                if (pawnSave.BeardDef != null)
                {
                    _saveableBeard = pawnSave.BeardDef;
                }
                else
                {
                    _saveableBeard = PawnFaceChooser.RandomBeardDefFor(pawn, pawn.Faction.def);
                    pawnSave.BeardDef = _saveableBeard;
                }

                if (pawnSave.SideburnDef != null)
                {
                    _saveableSideburn = pawnSave.SideburnDef;
                }
                else
                {
                    _saveableSideburn = PawnFaceChooser.RandomSideburnDefFor(pawn, pawn.Faction.def);
                    pawnSave.SideburnDef = _saveableSideburn;
                }

                if (pawnSave.TacheDef != null)
                {
                    _saveableTache = pawnSave.TacheDef;
                }
                else
                {
                    _saveableTache = PawnFaceChooser.RandomTacheDefFor(pawn, pawn.Faction.def);
                    pawnSave.TacheDef = _saveableTache;
                }

                graphicPathNew = "Things/Pawn/Humanlike/Heads/" + pawn.gender + "/" + pawn.gender + "_" + pawn.story.crownType + "_" + type + "-" + pawnSave.EyeDef.label + "-" + pawnSave.TacheDef.label + "-" + pawnSave.SideburnDef.label + "-" + pawnSave.BeardDef.label + "-" + pawn.story.skinWhiteness + "-" + pawn.story.hairColor;
            }

            if (File.Exists(modpath + graphicPathNew + "_front.png"))
            {
                graphicPath = graphicPathNew;
                goto exit;
            }

            var headGraphic = GraphicDatabase.Get<Graphic_Multi_Head>(graphicPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.SkinColor);

            Texture2D readHeadGraphicBack = headGraphic.MatBack.mainTexture as Texture2D;
            Texture2D readHeadGraphicFront = headGraphic.MatFront.mainTexture as Texture2D;
            Texture2D readHeadGraphicSide = headGraphic.MatSide.mainTexture as Texture2D;

            //          Texture2D readHeadGraphicBack = LoadTexture(graphicPath + "_back");
            //          Texture2D readHeadGraphicFront = LoadTexture(graphicPath + "_front");
            //          Texture2D readHeadGraphicSide = LoadTexture(graphicPath + "_side");
            //Texture2D readEyeGraphicFront = LoadTexture(_saveableEye.texPath + "_front");
            //Texture2D readEyeGraphicSide = LoadTexture(_saveableEye.texPath + "_side");


            Texture2D finalHeadFront = new Texture2D(128, 128);
            Texture2D finalHeadSide = new Texture2D(128, 128);

            Texture2D beardFront = new Texture2D(128, 128);
            Texture2D beardSide = new Texture2D(128, 128);

            Texture2D eyesHeadFront = new Texture2D(128, 128); ;
            Texture2D eyesHeadSide = new Texture2D(128, 128); ;

            var eyeGraphic = GraphicDatabase.Get<Graphic_Multi_Head>(_saveableEye.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);

            Texture2D readEyeGraphicFront = eyeGraphic.MatFront.mainTexture as Texture2D;
            Texture2D readEyeGraphicSide = eyeGraphic.MatSide.mainTexture as Texture2D;


            if (pawn.gender == Gender.Male)
            {
                var beardGraphic = GraphicDatabase.Get<Graphic_Multi_Head>(_saveableBeard.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                var sideburnGraphic = GraphicDatabase.Get<Graphic_Multi_Head>(_saveableSideburn.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                var tacheGraphic = GraphicDatabase.Get<Graphic_Multi_Head>(_saveableTache.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);



                Texture2D readBeardGraphicFront = beardGraphic.MatFront.mainTexture as Texture2D;
                Texture2D readBeardGraphicSide = beardGraphic.MatSide.mainTexture as Texture2D;


                Texture2D readTacheGraphicFront = tacheGraphic.MatFront.mainTexture as Texture2D; ;
                Texture2D readTacheGraphicSide = tacheGraphic.MatSide.mainTexture as Texture2D;

                Texture2D readSideburnGraphicFront = sideburnGraphic.MatFront.mainTexture as Texture2D;
                Texture2D readSideburnGraphicSide = sideburnGraphic.MatSide.mainTexture as Texture2D;

                //readBeardGraphicFront = LoadTexture(_saveableBeard.texPath + "_front");
                //readBeardGraphicSide = LoadTexture(_saveableBeard.texPath + "_side");

                //Texture2D readTacheGraphicFront = LoadTexture(_saveableTache.texPath + "_front");
                //Texture2D readTacheGraphicSide = LoadTexture(_saveableTache.texPath + "_side");
                //
                //Texture2D readSideburnGraphicFront = LoadTexture(_saveableSideburn.texPath + "_front");
                //Texture2D readSideburnGraphicSide = LoadTexture(_saveableSideburn.texPath + "_side");


                MakeBeard(readSideburnGraphicFront, readBeardGraphicFront, readTacheGraphicFront, ref beardFront);
                MakeBeard(readBeardGraphicSide, readSideburnGraphicSide, readTacheGraphicSide, ref beardSide);                        //           }

                AddEyes(pawn, readHeadGraphicFront, readEyeGraphicFront, ref eyesHeadFront);
                AddEyes(pawn, readHeadGraphicSide, readEyeGraphicSide, ref eyesHeadSide);

                AddFacialHair(pawn, eyesHeadFront, beardFront, haircolor, ref finalHeadFront);
                AddFacialHair(pawn, eyesHeadSide, beardSide, haircolor, ref finalHeadSide);

            }


            if (pawn.gender == Gender.Female)
            {
                AddEyes(pawn, readHeadGraphicFront, readEyeGraphicFront, ref finalHeadFront);
                AddEyes(pawn, readHeadGraphicSide, readEyeGraphicSide, ref finalHeadSide);
            }

            graphicPath = graphicPathNew;

            if (readHeadGraphicBack != null)
                ExportToPNG(pawn, readHeadGraphicBack, "back", graphicPath);
            if (finalHeadFront != null)
                ExportToPNG(pawn, finalHeadFront, "front", graphicPath);
            if (finalHeadSide != null)
                ExportToPNG(pawn, finalHeadSide, "side", graphicPath);

            headsModded.Add(new HeadGraphicRecordModded(graphicPath));

            UnityEngine.Object.DestroyImmediate(readHeadGraphicBack, true);
            UnityEngine.Object.DestroyImmediate(finalHeadFront, true);
            UnityEngine.Object.DestroyImmediate(finalHeadSide, true);

            exit:;

        }


        public static Texture2D MakeReadable(Texture2D texture, ref Texture2D myTexture2D)
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

            return myTexture2D;
            // "myTexture2D" now has the same pixels from "texture" and it's readable.
        }

        public static void MakeBeard(Texture2D beard_layer_1, Texture2D beard_layer_2, Texture2D beard_layer_3, ref Texture2D beard_final)
        {
            int startX = 0;
            int startY = 0;

            for (int x = startX; x < beard_layer_1.width; x++)
            {

                for (int y = startY; y < beard_layer_1.height; y++)
                {
                    Color layer1 = beard_layer_1.GetPixel(x, y);
                    Color layer2 = beard_layer_2.GetPixel(x - startX, y - startY);
                    Color layer3 = beard_layer_3.GetPixel(x - startX, y - startY);

                    Color final_color = layer1;
                    Color mixcolor;


                    mixcolor = Color.Lerp(layer1, layer2, layer2.a / 1f);

                    final_color = Color.Lerp(mixcolor, layer3, layer3.a / 1f);

                    beard_final.SetPixel(x, y, final_color);
                }
            }

            beard_final.Apply();
        }

        public static void AddFacialHair(Pawn pawn, Texture2D head, Texture2D beard, Color haircolor, ref Texture2D finalhead)
        {
            int startX = 0;
            int startY = head.height - beard.height;

            for (int x = startX; x < head.width; x++)
            {

                for (int y = startY; y < head.height; y++)
                {
                    Color headColor = head.GetPixel(x, y);
                    Color beardColor = beard.GetPixel(x - startX, y - startY);

                    Color skin = pawn.story.SkinColor;
                    float whiteness = pawn.story.skinWhiteness;

                    beardColor.r = beardColor.r * haircolor.r * UnityEngine.Random.Range(1.2f, 2.2f) / skin.r * whiteness;
                    beardColor.g = beardColor.g * haircolor.g * UnityEngine.Random.Range(1.2f, 2.2f) / skin.g * whiteness;
                    beardColor.b = beardColor.b * haircolor.b * UnityEngine.Random.Range(1.2f, 2.2f) / skin.b * whiteness;

                    Color final_color = Color.Lerp(headColor, beardColor, beardColor.a / 1f);

                    finalhead.SetPixel(x, y, final_color);
                }
            }

            finalhead.Apply();
        }

        public static void AddEyes(Pawn pawn, Texture2D head, Texture2D eyes, ref Texture2D finalhead)
        {

            int startX = 0;
            int startY = head.height - eyes.height;

            for (int x = startX; x < head.width; x++)
            {

                for (int y = startY; y < head.height; y++)
                {
                    Color headColor = head.GetPixel(x, y);
                    Color eyeColor = eyes.GetPixel(x - startX, y - startY);

                    Color final_color = Color.Lerp(headColor, eyeColor, eyeColor.a / 1f);

                    finalhead.SetPixel(x, y, final_color);
                }
            }

            finalhead.Apply();
        }

        public static void ExportToPNG(Pawn pawn, Texture2D inputTexture, string definition, string graphicpath)
        {
            byte[] bytes = inputTexture.EncodeToPNG();
            //         if (pawn.gender == Gender.Female)
            File.WriteAllBytes(modpath + graphicpath + "_" + definition + ".png", bytes);
            //     else
            //         File.WriteAllBytes(modpath + "Things/Pawn/Humanlike/Heads/Male/" + pawn.gender + "_" + pawn.story.crownType + "_" + pawn + "_" + definition + ".png", bytes);
            //     var pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);
        }


        public static List<HeadGraphicRecord> heads = new List<HeadGraphicRecord>();
        public static List<HeadGraphicRecordModded> headsModded = new List<HeadGraphicRecordModded>();

        private static HeadGraphicRecord skull = null;
        private static HeadGraphicRecordModded skullModded = null;
        private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";


        private static readonly string[] HeadsFolderPaths = new string[]
        {
          "Things/Pawn/Humanlike/Heads/Male",
          "Things/Pawn/Humanlike/Heads/Female"
        };

        private static void BuildDatabaseIfNecessary()
        {
            heads.Clear();
            if (heads.Count > 0 && skull != null)
            {
                return;
            }

            // now adding only vanilla cleared heads to the main db

            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Female/Female_Average_Normal"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Female/Female_Average_Pointy"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Female/Female_Average_Wide"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Female/Female_Narrow_Normal"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Female/Female_Narrow_Pointy"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Female/Female_Narrow_Wide"));

            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Male/Male_Average_Normal"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Male/Male_Average_Pointy"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Male/Male_Average_Wide"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Male/Male_Narrow_Normal"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Male/Male_Narrow_Pointy"));
            heads.Add(new HeadGraphicRecord("Things/Pawn/Humanlike/Heads/Male/Male_Narrow_Wide"));

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

        private static void BuildModdedDatabaseIfNecessary()
        {
            if (headsModded.Count > 0 && skullModded != null)
            {
                return;
            }

            string[] headsModdedFolderPaths = HeadsFolderPaths;
            for (int i = 0; i < headsModdedFolderPaths.Length; i++)
            {
                string text = headsModdedFolderPaths[i];
                foreach (string current in GraphicDatabaseUtility.GraphicNamesInFolder(text))
                {
                    headsModded.Add(new HeadGraphicRecordModded(text + "/" + current));
                }
            }
            skullModded = new HeadGraphicRecordModded(SkullPath);
        }

        protected Pawn pawn;

        public class HeadGraphicRecord
        {
            public Gender gender;

            public CrownType crownType;

            public string graphicPath;

            public List<KeyValuePair<Color, Graphic_Multi_Head>> graphics = new List<KeyValuePair<Color, Graphic_Multi_Head>>();

            public HeadGraphicRecord(string graphicPath)
            {
                this.graphicPath = graphicPath;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(graphicPath);

                string[] array = fileNameWithoutExtension.Split(new char[]
                {
                            '_'
                });

                try
                {
                    this.crownType = (CrownType)((byte)ParseHelper.FromString(array[array.Length - 2], typeof(CrownType)));
                    this.gender = (Gender)((byte)ParseHelper.FromString(array[array.Length - 3], typeof(Gender)));
                }
                catch (Exception ex)
                {
                    Log.Error("Parse error with head graphic at " + graphicPath + ": " + ex.Message);
                    this.crownType = CrownType.Undefined;
                    this.gender = Gender.None;
                }


            }

            public Graphic_Multi_Head GetGraphic(Color color)
            {
                for (int i = 0; i < this.graphics.Count; i++)
                {
                    if (color.IndistinguishableFrom(graphics[i].Key))
                    {
                        return graphics[i].Value;
                    }
                }
                Graphic_Multi_Head graphic_Multi_Head = (Graphic_Multi_Head)GraphicDatabase.Get<Graphic_Multi_Head>(graphicPath, ShaderDatabase.CutoutSkin, Vector2.one, color);
                graphics.Add(new KeyValuePair<Color, Graphic_Multi_Head>(color, graphic_Multi_Head));
                return graphic_Multi_Head;
            }
        }

        public class HeadGraphicRecordModded
        {
            public Pawn pawn = null;


            public bool vanilla = false;

            public bool unique = false;

            public Gender gender;

            public CrownType crownType;

            public string graphicPath;

            //         private List<KeyValuePair<Color, Graphic_Multi_Head>> graphics = new List<KeyValuePair<Color, Graphic_Multi_Head>>();

            public HeadGraphicRecordModded(string graphicPath)
            {

                this.graphicPath = graphicPath;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(graphicPath);
                string[] array = fileNameWithoutExtension.Split(new char[]
                {
                    '_'
                });
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

            public Graphic_Multi_Head GetGraphic(Color color)
            {
                //     for (int i = 0; i < graphics.Count; i++)
                //     {
                //  if (color.IndistinguishableFrom(graphics[i].Key))
                //  {
                //      return graphics[i].Value;
                //  }
                //     }
                Graphic_Multi_Head graphic_Multi_Head = (Graphic_Multi_Head)GraphicDatabase.Get<Graphic_Multi_Head>(graphicPath, ShaderDatabase.CutoutSkin, Vector2.one, color);
                //            graphics.Add(new KeyValuePair<Color, Graphic_Multi_Head>(color, graphic_Multi_Head));
                return graphic_Multi_Head;
            }
        }

        public static Graphic_Multi_Head GetModdedHeadNamed(Pawn pawn, string graphicPath, Color skinColor, Color hairColor)
        {
            //          MethodInfo method = typeof(GraphicDatabaseHeadRecords).GetMethod("BuildDatabaseIfNecessary", BindingFlags.Static | BindingFlags.NonPublic);
            //          method.Invoke(null, null);

            var pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);

            BuildDatabaseIfNecessary();
            BuildModdedDatabaseIfNecessary();

            if (pawnSave.optimized)
            {
                for (int i = 0; i < headsModded.Count; i++)
                {
                    HeadGraphicRecordModded headGraphicRecord = headsModded[i];

                    if (headGraphicRecord.graphicPath == graphicPath)
                    {
                        //                  if (pawnSave.optimized)
                        //                  {
                        //                  }
                        return headGraphicRecord.GetGraphic(skinColor);
                    }
                }
            }

            for (int i = 0; i < heads.Count; i++)
            {
                HeadGraphicRecord headGraphicRecord = heads[i];

                if (headGraphicRecord.graphicPath == graphicPath)
                {
                    //                  if (pawnSave.optimized)
                    //                  {
                    //                  }
                    return headGraphicRecord.GetGraphic(skinColor);
                }
            }


            Log.Message("Tried to get pawn head at path " + graphicPath + " that was not found. Defaulting...");
            return heads.First().GetGraphic(skinColor);
        }

        public static Graphic_Multi_Head GetHeadRandomUnmodded(Gender gender, Color skinColor, CrownType crownType)
        {
            BuildDatabaseIfNecessary();
            Predicate<HeadGraphicRecord> predicate = (HeadGraphicRecord head) => head.crownType == crownType && head.gender == gender;
            int num = 0;
            HeadGraphicRecord headGraphicRecord;
            while (true)
            {
                headGraphicRecord = heads.RandomElement();
                if (predicate(headGraphicRecord))
                {
                    break;
                }
                num++;
                if (num > 40)
                {
                    goto Block_2;
                }
            }
            return headGraphicRecord.GetGraphic(skinColor);
            Block_2:
            foreach (HeadGraphicRecord current in heads.InRandomOrder(null))
            {
                if (predicate(current))
                {
                    return current.GetGraphic(skinColor);
                }
            }
            Log.Error("Failed to find head for gender=" + gender + ". Defaulting...");
            return heads.First().GetGraphic(skinColor);
        }

        public static void AddCustomizedHead(Pawn pawn, Color skinColor, Color hairColor, string graphicPath)
        {

            DecorateHead(ref graphicPath, pawn, skinColor, hairColor);

            typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, graphicPath);

            var pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);
            pawnSave.optimized = true;
        }

        // RimWorld.Pawn_StoryTracker


    }



}


