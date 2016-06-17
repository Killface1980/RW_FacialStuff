using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    public class GraphicDatabaseFacedHeadRecords
    {
        static string modpath = "Mods/RW_FacialStuff/Textures/";

        public static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();


        public static Texture2D LoadTexture(string texturePath)
        {
            Texture2D texture = null;

            if (textureCache.TryGetValue(texturePath, out texture)) return texture;

            texture = new Texture2D(1, 1);
            texture.LoadImage(File.ReadAllBytes(modpath + texturePath + ".png"));
            texture.anisoLevel = 8;

            textureCache.Add(texturePath, texture);

            return texture;
        }

        public static Graphic DecorateHead(Graphic headGraph, Pawn pawn, Color skinColor, Color haircolor)
        {
            Graphic beardGraphic;
            Graphic eyeGraphic;
            Graphic sideburnGraphic;
            Graphic tacheGraphic;


            if (pawn.gender == Gender.Male)
            {
                if (pawn.ageTracker.AgeBiologicalYears > 18)
                {

                    var pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);

                    EyeDef _saveableEye = null;

                    if (pawnSave.EyeDef != null)
                    {
                        _saveableEye = pawnSave.EyeDef;
                    }
                    else
                    {
                        _saveableEye = PawnBeardChooser.RandomEyeDefFor(pawn, pawn.Faction.def);
                        pawnSave.EyeDef = _saveableEye;
                    }

                    BeardDef _saveableBeard = null;

                    if (pawnSave.BeardDef != null)
                    {
                        _saveableBeard = pawnSave.BeardDef;
                    }
                    else
                    {
                        _saveableBeard = PawnBeardChooser.RandomBeardDefFor(pawn, pawn.Faction.def);
                        pawnSave.BeardDef = _saveableBeard;
                    }

                    SideburnDef _saveableSideburn = null;

                    if (pawnSave.SideburnDef != null)
                    {
                        _saveableSideburn = pawnSave.SideburnDef;
                    }
                    else
                    {
                        _saveableSideburn = PawnBeardChooser.RandomSideburnDefFor(pawn, pawn.Faction.def);
                        pawnSave.SideburnDef = _saveableSideburn;
                    }

                    TacheDef _saveableTache = null;

                    if (pawnSave.TacheDef != null)
                    {
                        _saveableTache = pawnSave.TacheDef;
                    }
                    else
                    {
                        _saveableTache = PawnBeardChooser.RandomTacheDefFor(pawn, pawn.Faction.def);
                        pawnSave.TacheDef = _saveableTache;
                    }



                    eyeGraphic = GraphicDatabase.Get<Graphic_Multi>(_saveableEye.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);

                    beardGraphic = GraphicDatabase.Get<Graphic_Multi>(_saveableBeard.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                    sideburnGraphic = GraphicDatabase.Get<Graphic_Multi>(_saveableSideburn.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                    tacheGraphic = GraphicDatabase.Get<Graphic_Multi>(_saveableTache.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);


                    Texture2D readHeadGraphicBack = null;
                    Texture2D readHeadGraphicFront = null;
                    Texture2D readHeadGraphicSide = null;

                    Texture2D eyesHeadFront = null;
                    Texture2D eyesHeadSide = null;

                    Texture2D beardFront = null;
                    Texture2D beardSide = null;

                    Texture2D finalHeadFront = null;
                    Texture2D finalHeadSide = null;

                    readHeadGraphicBack = LoadTexture(headGraph.path + "_back");
                    readHeadGraphicFront = LoadTexture(headGraph.path + "_front");
                    readHeadGraphicSide = LoadTexture(headGraph.path + "_side");

                    Texture2D readEyeGraphicFront = null;
                    Texture2D readEyeGraphicSide = null;

                    Texture2D readBeardGraphicFront = null;
                    Texture2D readBeardGraphicSide = null;

                    Texture2D readTacheGraphicFront = null;
                    Texture2D readTacheGraphicSide = null;

                    if (pawn.story.crownType == CrownType.Narrow)
                    {
                        readEyeGraphicFront = LoadTexture(_saveableEye.texPath + "_Narrow_front");
                        readEyeGraphicSide = LoadTexture(_saveableEye.texPath + "_Narrow_side");

                        readBeardGraphicFront = LoadTexture(_saveableBeard.texPath + "_Narrow_front");
                        readBeardGraphicSide = LoadTexture(_saveableBeard.texPath + "_Narrow_side");

                        readTacheGraphicFront = LoadTexture(_saveableTache.texPath + "_Narrow_front");
                        readTacheGraphicSide = LoadTexture(_saveableTache.texPath + "_Narrow_side");

                    }
                    else
                    {
                        readEyeGraphicFront = LoadTexture(_saveableEye.texPath + "_front");
                        readEyeGraphicSide = LoadTexture(_saveableEye.texPath + "_side");

                        readBeardGraphicFront = LoadTexture(_saveableBeard.texPath + "_front");
                        readBeardGraphicSide = LoadTexture(_saveableBeard.texPath + "_side");

                        readTacheGraphicFront = LoadTexture(_saveableTache.texPath + "_front");
                        readTacheGraphicSide = LoadTexture(_saveableTache.texPath + "_side");
                    }


                    Texture2D readSideburnGraphicFront = LoadTexture(_saveableSideburn.texPath + "_front");
                    Texture2D readSideburnGraphicSide = LoadTexture(_saveableSideburn.texPath + "_side");

                    MakeBeard(readSideburnGraphicFront, readBeardGraphicFront, readTacheGraphicFront, ref beardFront);
                    MakeBeard(readBeardGraphicSide, readSideburnGraphicSide, readTacheGraphicSide, ref beardSide);                        //           }

                    AddEyes(pawn, readHeadGraphicFront, readEyeGraphicFront, ref eyesHeadFront);
                    AddEyes(pawn, readHeadGraphicSide, readEyeGraphicSide, ref eyesHeadSide);

                    AddFacialHair(pawn, eyesHeadFront, beardFront, ref finalHeadFront, haircolor);
                    AddFacialHair(pawn, eyesHeadSide, beardSide, ref finalHeadSide, haircolor);

                    headGraph.MatFront.mainTexture = finalHeadFront;
                    headGraph.MatSide.mainTexture = finalHeadSide;


                    AddToHeadCache(pawn, readHeadGraphicBack, "back");
                    AddToHeadCache(pawn, finalHeadFront, "front");
                    AddToHeadCache(pawn, finalHeadSide, "side");

                }


            }


            if (pawn.gender == Gender.Female)
            {
                if (pawn.ageTracker.AgeBiologicalYears > 16)
                {


                    var pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);

                    EyeDef _saveableEye;

                    if (pawnSave.EyeDef != null)
                    {
                        _saveableEye = pawnSave.EyeDef;
                    }
                    else
                    {
                        _saveableEye = PawnBeardChooser.RandomEyeDefFor(pawn, pawn.Faction.def);
                        pawnSave.EyeDef = _saveableEye;
                    }

                    eyeGraphic = GraphicDatabase.Get<Graphic_Multi>(_saveableEye.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);

                    Texture2D readHeadGraphicBack = null;
                    Texture2D readHeadGraphicFront = null;
                    Texture2D readHeadGraphicSide = null;

                    Texture2D finalHeadFront = null;
                    Texture2D finalHeadSide = null;

                    Texture2D readEyeGraphicFront = null;
                    Texture2D readEyeGraphicSide = null;

                    if (pawn.story.crownType == CrownType.Narrow)
                    {
                        readEyeGraphicFront = LoadTexture(_saveableEye.texPath + "_Narrow_front");
                        readEyeGraphicSide = LoadTexture(_saveableEye.texPath + "_Narrow_side");
                    }
                    else
                    {
                        readEyeGraphicFront = LoadTexture(_saveableEye.texPath + "_front");
                        readEyeGraphicSide = LoadTexture(_saveableEye.texPath + "_side");
                    }

                    readHeadGraphicBack = LoadTexture(headGraph.path + "_back");
                    readHeadGraphicFront = LoadTexture(headGraph.path + "_front");
                    readHeadGraphicSide = LoadTexture(headGraph.path + "_side");

                    AddEyes(pawn, readHeadGraphicFront, readEyeGraphicFront, ref finalHeadFront);
                    AddEyes(pawn, readHeadGraphicSide, readEyeGraphicSide, ref finalHeadSide);

                    headGraph.MatFront.mainTexture = finalHeadFront;
                    headGraph.MatSide.mainTexture = finalHeadSide;

                    AddToHeadCache(pawn, readHeadGraphicBack, "back");
                    AddToHeadCache(pawn, finalHeadFront, "front");
                    AddToHeadCache(pawn, finalHeadSide, "side");


                }
            }

            return headGraph;
        }

        public static Texture2D MakeReadable(Texture2D texture, ref Texture2D myTexture2D)
        {

            if (textureCache.TryGetValue(texture.name, out myTexture2D)) return myTexture2D;


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

            textureCache.Add(texture.name, myTexture2D);

            return myTexture2D;
            // "myTexture2D" now has the same pixels from "texture" and it's readable.
        }

        public static Texture2D MakeBeard(Texture2D beard_layer_1, Texture2D beard_layer_2, Texture2D beard_layer_3, ref Texture2D beard_final)
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

                    beard_layer_1.SetPixel(x, y, final_color);
                }
            }

            beard_layer_1.Apply();
            beard_final = beard_layer_1;
            return beard_final;
        }

        public static Texture2D AddFacialHair(Pawn pawn, Texture2D head, Texture2D beard, ref Texture2D finalhead, Color haircolor)
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


                    Color final_color = headColor;
                    final_color = Color.Lerp(headColor, beardColor, beardColor.a / 1f);

                    head.SetPixel(x, y, final_color);
                }
            }

            head.Apply();
            finalhead = head;
            return finalhead;
        }

        public static Texture2D AddEyes(Pawn pawn, Texture2D head, Texture2D eyes, ref Texture2D finalhead)
        {

            int startX = 0;
            int startY = head.height - eyes.height;

            for (int x = startX; x < head.width; x++)
            {

                for (int y = startY; y < head.height; y++)
                {
                    Color headColor = head.GetPixel(x, y);
                    Color eyeColor = eyes.GetPixel(x - startX, y - startY);

                    Color beardColorFace = pawn.story.hairColor;
                    Color skin = pawn.story.SkinColor;
                    float whiteness = pawn.story.skinWhiteness;



                    Color final_color = headColor;
                    final_color = Color.Lerp(headColor, eyeColor, eyeColor.a / 1f);

                    head.SetPixel(x, y, final_color);
                }
            }

            head.Apply();
            finalhead = head;
            return finalhead;
        }

        public static void AddToHeadCache(Pawn pawn, Texture2D inputTexture, string definition)
        {
            byte[] bytes = inputTexture.EncodeToPNG();
            if (pawn.gender == Gender.Female)
            File.WriteAllBytes(modpath + "_cachedHeads/Female/" + pawn.gender + "_" + pawn.story.crownType + "_" + pawn + "_" + definition + ".png", bytes);
            else
                File.WriteAllBytes(modpath + "_cachedHeads/Male/" + pawn.gender + "_" + pawn.story.crownType + "_" + pawn + "_" + definition + ".png", bytes);

        }


        public static List<HeadGraphicRecord> heads = new List<HeadGraphicRecord>();
        public static List<HeadGraphicRecordModded> headsFaced = new List<HeadGraphicRecordModded>();

        private static HeadGraphicRecord skull = null;
        private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";

        private static readonly string[] HeadsFolderPaths = new string[]
        {
          "Things/Pawn/Humanlike/Heads/Male",
          "Things/Pawn/Humanlike/Heads/Female"
        };

        private static readonly string[] ModdedHeadsFolderPath = new string[]
        {
            "_cachedHeads/Male",
           "_cachedHeads/Female"
        };

        private static void BuildDatabaseIfNecessary()
        {
            if (heads.Count > 0 && skull != null)
            {
                return;
            }
            string[] headsFolderPaths = HeadsFolderPaths;
            for (int i = 0; i < headsFolderPaths.Length; i++)
            {
                string text = headsFolderPaths[i];
                foreach (string current in GraphicDatabaseUtility.GraphicNamesInFolder(text))
                {
                    heads.Add(new HeadGraphicRecord(text + "/" + current));
                }
            }
            skull = new HeadGraphicRecord(SkullPath);
        }

        private static void BuildModdedDatabaseIfNecessary()
        {
       //   if (headsFaced.Count > 0 && skull != null)
       //   {
       //       return;
       //   }

            string[] headsModdedFolderPaths = ModdedHeadsFolderPath;
            for (int i = 0; i < headsModdedFolderPaths.Length; i++)
            {
                string text = headsModdedFolderPaths[i];
                foreach (string current in ModInitializer.GraphicNamesInFolder(text))
                {
                    headsFaced.Add(new HeadGraphicRecordModded(text + "/" + current));
                }
            }
            skull = new HeadGraphicRecord(SkullPath);
        }

        protected Pawn pawn;

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

            public Graphic_Multi GetGraphic(Color color)
            {
                for (int i = 0; i < this.graphics.Count; i++)
                {
                    if (color.IndistinguishableFrom(this.graphics[i].Key))
                    {
                        return this.graphics[i].Value;
                    }
                }
                Graphic_Multi graphic_Multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(this.graphicPath, ShaderDatabase.CutoutSkin, Vector2.one, color);
                this.graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphic_Multi));
                return graphic_Multi;
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

            private List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

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

            public Graphic_Multi GetGraphic(Color color)
            {
                for (int i = 0; i < this.graphics.Count; i++)
                {
                    if (color.IndistinguishableFrom(this.graphics[i].Key))
                    {
                        return this.graphics[i].Value;
                    }
                }
                Graphic_Multi graphic_Multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPath, ShaderDatabase.CutoutSkin, Vector2.one, color);
                this.graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphic_Multi));
                return graphic_Multi;
            }
        }

        public static Graphic_Multi GetHeadNamed(Pawn pawn, string graphicPath, Color skinColor, Color hairColor)
        {
            BuildDatabaseIfNecessary();
            BuildModdedDatabaseIfNecessary();
            for (int i = 0; i < headsFaced.Count; i++)
            {
                HeadGraphicRecordModded headGraphicRecord = headsFaced[i];

                if (headGraphicRecord.graphicPath == graphicPath)
                {

                    var pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);
                    if (pawnSave.optimized)
                    {

                    }

                    return headGraphicRecord.GetGraphic(skinColor);
                }

            }
            Log.Message("Tried to get pawn head at path " + graphicPath + " that was not found. Defaulting...");
            return heads.First().GetGraphic(skinColor);
        }


        public static Graphic_Multi GetHeadRandom(Gender gender, Color skinColor, CrownType crownType)
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

            for (int i = 0; i < heads.Count; i++)
            {
                HeadGraphicRecord headGraphicRecord = heads[i];

                if (headGraphicRecord.graphicPath == graphicPath)
                {
                    HeadGraphicRecordModded headgraph = new HeadGraphicRecordModded(graphicPath);
                    headgraph.crownType = headGraphicRecord.crownType;
                    headgraph.gender = headGraphicRecord.gender;

                    DecorateHead(headgraph.GetGraphic(skinColor), pawn, skinColor, hairColor);

              //    headgraph.pawn = pawn;
              //    headgraph.graphicPath = "_cachedHeads/" + pawn.gender + "_" + pawn.story.crownType + "_" + pawn;
              //    headsFaced.Add(headgraph);

                    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, "_cachedHeads/" + pawn.gender+"/" + pawn.gender + "_" + pawn.story.crownType + "_" + pawn);

                    var pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);
                    pawnSave.optimized = true;
                }

            }



            //            moddedhead.graphicPath = headGraphicRecord.graphicPath;



        }

    }
}
