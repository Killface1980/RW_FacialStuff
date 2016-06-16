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

        public static Graphic GetBeardedHead(Graphic headGraphic, Pawn pawn, Color skinColor, Color haircolor)
        {
            Graphic beardGraphic;
            Graphic eyeGraphic;
            Graphic sideburnGraphic;
            Graphic tacheGraphic;


            if (pawn.gender == Gender.Male)
            {
                if (pawn.ageTracker.AgeBiologicalYears > 18)
                {

                    if (File.Exists(modpath + "/_cachedHeads/" + pawn.Label + "_side.png")
                        && File.Exists(modpath + "/_cachedHeads/" + pawn + "_front.png"))
                    {
                        headGraphic.MatFront.mainTexture = LoadTexture(modpath + "/_cachedHeads/" + pawn.Label + "_front.png");
                        headGraphic.MatFront.mainTexture = LoadTexture(modpath + "/_cachedHeads/" + pawn.Label + "_side.png");
                    }
                    else
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


                        Texture2D readHeadGraphicFront = null;
                        Texture2D readHeadGraphicSide = null;

                        Texture2D eyesHeadFront = null;
                        Texture2D eyesHeadSide = null;

                        Texture2D beardFront = null;
                        Texture2D beardSide = null;

                        Texture2D finalHeadFront = null;
                        Texture2D finalHeadSide = null;


                        MakeReadable(headGraphic.MatFront.mainTexture as Texture2D, ref readHeadGraphicFront);
                        MakeReadable(headGraphic.MatSide.mainTexture as Texture2D, ref readHeadGraphicSide);

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

                        headGraphic.MatFront.mainTexture = finalHeadFront;
                        headGraphic.MatSide.mainTexture = finalHeadSide;

                        AddToHeadCache(pawn, finalHeadFront, "front");
                        AddToHeadCache(pawn, finalHeadSide, "side");

                    }
                }

            }


            if (pawn.gender == Gender.Female)
            {
                if (pawn.ageTracker.AgeBiologicalYears > 16)
                {
                    //       if (File.Exists(modpath + "/_cachedHeads/" + pawn + "_" + pawn.ageTracker.BirthYear + "-" + pawn.ageTracker.BirthDayOfYear + "_side.png")
                    //           && File.Exists(modpath + "/_cachedHeads/" + pawn + "_" + pawn.ageTracker.BirthYear + "-" + pawn.ageTracker.BirthDayOfYear + "_front.png"))
                    //       {
                    //           headGraphic.MatFront.mainTexture = LoadTexture(modpath + "/_cachedHeads/" + pawn + "_" + pawn.ageTracker.BirthYear + "-" + pawn.ageTracker.BirthDayOfYear + "_front.png");
                    //           headGraphic.MatFront.mainTexture = LoadTexture(modpath + "/_cachedHeads/" + pawn + "_" + pawn.ageTracker.BirthYear + "-" + pawn.ageTracker.BirthDayOfYear + "_side.png");
                    //       }
                    if (true)
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


                        MakeReadable(headGraphic.MatFront.mainTexture as Texture2D, ref readHeadGraphicFront);
                        MakeReadable(headGraphic.MatSide.mainTexture as Texture2D, ref readHeadGraphicSide);

                        AddEyes(pawn, readHeadGraphicFront, readEyeGraphicFront, ref finalHeadFront);
                        AddEyes(pawn, readHeadGraphicSide, readEyeGraphicSide, ref finalHeadSide);

                        headGraphic.MatFront.mainTexture = finalHeadFront;
                        headGraphic.MatSide.mainTexture = finalHeadSide;

                        //    AddToHeadCache(finalHeadFront, "_front");
                        //    AddToHeadCache(finalHeadSide, "_side");

                    }



                }
            }

            return headGraphic;


        }

        public static Texture2D MakeReadable(Texture2D texture, ref Texture2D myTexture2D)
        {

            //       if (textureCache.TryGetValue(texture.name, out myTexture2D)) return myTexture2D;


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

            //       textureCache.Add(texture.name, myTexture2D);

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
            File.WriteAllBytes(modpath + "/_cachedHeads/" + pawn.Label + "_" + definition + ".png", bytes);
        }


        private static List<HeadGraphicRecord> heads = new List<HeadGraphicRecord>();
        private static HeadGraphicRecord skull = null;
        private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";

        private static readonly string[] HeadsFolderPaths = new string[]
        {
            "Things/Pawn/Humanlike/Heads/Male",
            "Things/Pawn/Humanlike/Heads/Female"
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

        private class HeadGraphicRecord
        {
            public Gender gender;

            public CrownType crownType;

            public string graphicPath;

            private List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

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


        public static Graphic_Multi GetHeadNamed(string graphicPath, Color skinColor)
        {
            BuildDatabaseIfNecessary();
            for (int i = 0; i < heads.Count; i++)
            {
                HeadGraphicRecord headGraphicRecord = heads[i];

                if (headGraphicRecord.graphicPath == graphicPath)
                {
                    return headGraphicRecord.GetGraphic(skinColor);
                }
            }
            Log.Message("Tried to get pawn head at path " + graphicPath + " that was not found. Defaulting...");
            return heads.First<HeadGraphicRecord>().GetGraphic(skinColor);
        }

    }
}
