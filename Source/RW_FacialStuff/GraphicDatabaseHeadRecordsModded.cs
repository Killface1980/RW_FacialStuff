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
        public static string ModTexturePath = "Mods/RW_FacialStuff/Textures/";
        private static string pawnAgeFileName;

        public static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

        public static List<Texture2D> textureList = new List<Texture2D>();

        private static EyeDef _saveableEye;
        private static WrinkleDef _saveableWrinkle;
        private static LipDef _saveableLip;
        private static BeardDef _saveableBeard;
        private static SideburnDef _saveableSideburn;
        private static TacheDef _saveableTache;

        private static Color ColorGrey = new Color(0.5f, 0.5f, 0.5f);

        private static Color Skin01 = new Color(0.3882353f, 0.274509817f, 0.141176477f);
        private static Color Skin02 = new Color(0.509803951f, 0.356862754f, 0.1882353f);
        private static Color Skin03 = new Color(0.894117653f, 0.619607866f, 0.3529412f);
        private static Color Skin04 = new Color(1f, 0.9372549f, 0.7411765f);
        private static Color Skin05 = new Color(1f, 0.9372549f, 0.8352941f);
        private static Color Skin06 = new Color(0.9490196f, 0.929411769f, 0.8784314f);


        public static List<HeadGraphicRecord> heads = new List<HeadGraphicRecord>();
        public static List<HeadGraphicRecordModded> headsModded = new List<HeadGraphicRecordModded>();

        private static HeadGraphicRecord skull = null;
        private static HeadGraphicRecordModded skullModded = null;
        private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        protected Pawn pawn;
        static string type;
        static string graphicPathNew;


        // The color is taken from the hair color, the mod creates a new head for each hair color (males only); needs to be simplified, colors merged
        //
        // Needed because the hair color can't be overlayed separately, so for now many new heads

        private static string HairColorNamed;
        private static string SkinColorNamed;
        /*
                public static Texture2D LoadTexture(string texturePath)
                {
                    Texture2D texture;
                    //           Debug.LogWarning("RW_Facial TextPath: " + texturePath);
                    //      if (textureCache.TryGetValue(texturePath, out texture)) return texture;

                    texture = new Texture2D(1, 1);
                    texture.LoadImage(File.ReadAllBytes(modpath + texturePath + ".png"));
                    texture.anisoLevel = 8;
                    texture.name = Path.GetFileName(modpath + texturePath + ".png");

                    //        textureCache.Add(texturePath, texture);

                    //         Debug.LogWarning("RW_Facial added to cache: " + texture.name);

                    return texture;
                }

        */
        private static void SetFileNameStuff(Pawn pawn, Color skinColor, Color haircolor, float pawnAgeFloat, string graphicPath)
        {
            SaveablePawn pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);

            if (pawnAgeFloat < 40f)
                pawnAgeFileName = "Young";

            if (pawnAgeFloat >= 40f && pawnAgeFloat < 47f)
                pawnAgeFileName = "40plus";

            if (pawnAgeFloat >= 47f && pawnAgeFloat < 54f)
                pawnAgeFileName = "47plus";

            if (pawnAgeFloat >= 54f && pawnAgeFloat < 61f)
                pawnAgeFileName = "54plus";

            if (pawnAgeFloat >= 61f && pawnAgeFloat < 68f)
                pawnAgeFileName = "61plus";

            if (pawnAgeFloat >= 68f && pawnAgeFloat < 75f)
                pawnAgeFileName = "68plus";

            if (pawnAgeFloat >= 75f)
                pawnAgeFileName = "75plus";

            //file name definition 2

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
                _saveableEye = PawnFaceMaker.RandomEyeDefFor(pawn, pawn.Faction.def);
                pawnSave.EyeDef = _saveableEye;
            }

            if (pawnSave.WrinkleDef != null)
            {
                _saveableWrinkle = pawnSave.WrinkleDef;
            }
            else
            {
                _saveableWrinkle = PawnFaceMaker.RandomWrinkleDefFor(pawn, pawn.Faction.def);
                pawnSave.WrinkleDef = _saveableWrinkle;
            }

            if (pawn.gender == Gender.Female)
            {
                if (pawnSave.LipDef != null)
                {
                    _saveableLip = pawnSave.LipDef;
                }
                else
                {
                    _saveableLip = PawnFaceMaker.RandomLipDefFor(pawn, pawn.Faction.def);
                    pawnSave.LipDef = _saveableLip;
                }

                graphicPathNew = "Things/Pawn/Humanlike/Heads/" + pawn.gender + "/" + pawn.gender + "_" + pawn.story.crownType + "_" + type + "-" + pawnAgeFileName + "-" + pawnSave.EyeDef.label + "-" + SkinColorNamed;
            }


            if (pawn.gender == Gender.Male)
            {
                if (pawnSave.BeardDef != null)
                {
                    _saveableBeard = pawnSave.BeardDef;
                }
                else
                {
                    _saveableBeard = PawnFaceMaker.RandomBeardDefFor(pawn, pawn.Faction.def);
                    pawnSave.BeardDef = _saveableBeard;
                }

                if (pawnSave.SideburnDef != null)
                {
                    _saveableSideburn = pawnSave.SideburnDef;
                }
                else
                {
                    _saveableSideburn = PawnFaceMaker.RandomSideburnDefFor(pawn, pawn.Faction.def);
                    pawnSave.SideburnDef = _saveableSideburn;
                }

                if (pawnSave.TacheDef != null)
                {
                    _saveableTache = pawnSave.TacheDef;
                }
                else
                {
                    _saveableTache = PawnFaceMaker.RandomTacheDefFor(pawn, pawn.Faction.def);
                    pawnSave.TacheDef = _saveableTache;
                }

                GetHairColorNamed(haircolor);
                GetSkinColorNamed(skinColor);

                if (_saveableBeard.label.Equals("BA-Shaved") && _saveableSideburn.label.Equals("SA-Shaved") && _saveableTache.label.Equals("MA-Shaved"))
                    graphicPathNew = "Things/Pawn/Humanlike/Heads/" + pawn.gender + "/" + pawn.gender + "_" + pawn.story.crownType + "_" + type + "-" + pawnAgeFileName + "-" + pawnSave.EyeDef.label + "-" + SkinColorNamed + "-Shaved";
                else
                    graphicPathNew = "Things/Pawn/Humanlike/Heads/" + pawn.gender + "/" + pawn.gender + "_" + pawn.story.crownType + "_" + type + "-" + pawnAgeFileName + "-" + pawnSave.EyeDef.label + "-" + pawnSave.TacheDef.label + "-" + pawnSave.SideburnDef.label + "-" + pawnSave.BeardDef.label + "-" + SkinColorNamed + "-" + HairColorNamed;
            }
        }

        public static void DecorateHead(ref string graphicPath, Pawn pawn, Color skinColor, Color haircolor)
        {

            SaveablePawn pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);

            float pawnAgeFloat = pawn.ageTracker.AgeBiologicalYearsFloat;

            pawnSave.GraphicPathOriginal = graphicPath;

            //file name definition 1

            SetFileNameStuff(pawn, skinColor, haircolor, pawnAgeFloat, graphicPath);

            if (File.Exists(ModTexturePath + graphicPathNew + "_front.png"))
            {
                graphicPath = graphicPathNew;
                return;
            }

            Graphic headGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(graphicPath, ShaderDatabase.Cutout, Vector2.one, Color.white);

            Texture2D headGraphicBack = headGraphic.MatBack.mainTexture as Texture2D;
            Texture2D headGraphicFront = headGraphic.MatFront.mainTexture as Texture2D;
            Texture2D headGraphicSide = headGraphic.MatSide.mainTexture as Texture2D;

            Texture2D finalHeadFront = new Texture2D(headGraphicFront.width, headGraphicFront.height);
            Texture2D finalHeadSide = new Texture2D(headGraphicSide.width, headGraphicSide.height);

            Texture2D beardFront = new Texture2D(headGraphicFront.width, headGraphicFront.height);
            Texture2D beardSide = new Texture2D(headGraphicSide.width, headGraphicSide.height);

            // Deactivated for now, no complex texture
            //      Texture2D beardFrontComplex = new Texture2D(headGraphicFront.width, headGraphicFront.height);
            //      Texture2D beardSideComplex = new Texture2D(headGraphicSide.width, headGraphicSide.height);

            Texture2D eyesHeadFront = new Texture2D(headGraphicFront.width, headGraphicFront.height);
            Texture2D eyesHeadSide = new Texture2D(headGraphicSide.width, headGraphicSide.height);

            Texture2D wrinklesHeadFront = new Texture2D(headGraphicFront.width, headGraphicFront.height);
            Texture2D wrinklesHeadSide = new Texture2D(headGraphicSide.width, headGraphicSide.height);


            Graphic eyeGraphic = null;
            Graphic wrinkleGraphic = null;

            if (pawn.story.crownType == CrownType.Narrow)
            {
                eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableEye.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.black);

                if (type == "Normal")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableWrinkle.texPathNarrowNormal, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (type == "Pointy")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableWrinkle.texPathNarrowPointy, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (type == "Wide")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableWrinkle.texPathNarrowWide, ShaderDatabase.Cutout, Vector2.one, Color.black);
            }
            else
            {
                eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableEye.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.black);

                if (type == "Normal")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableWrinkle.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (type == "Pointy")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableWrinkle.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (type == "Wide")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableWrinkle.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.black);
            }

            Texture2D readEyeGraphicFront = eyeGraphic.MatFront.mainTexture as Texture2D;
            Texture2D readEyeGraphicSide = eyeGraphic.MatSide.mainTexture as Texture2D;

            Texture2D readWrinkleGraphicFront = wrinkleGraphic.MatFront.mainTexture as Texture2D;
            Texture2D readWrinkleGraphicSide = wrinkleGraphic.MatSide.mainTexture as Texture2D;

            if (pawn.gender == Gender.Male)
            {
                Graphic beardGraphic = null;
                Graphic sideburnGraphic = null;
                Graphic tacheGraphic = null;

                if (pawn.story.crownType == CrownType.Narrow)
                {
                    if (type == "Normal")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableBeard.texPathNarrowNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableTache.texPathNarrowNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                    if (type == "Pointy")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableBeard.texPathNarrowPointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableTache.texPathNarrowPointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                    if (type == "Wide")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableBeard.texPathNarrowWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableTache.texPathNarrowWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }

                    sideburnGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableSideburn.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }
                else
                {
                    if (type == "Normal")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableBeard.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableTache.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                    if (type == "Pointy")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableBeard.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableTache.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                    if (type == "Wide")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableBeard.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableTache.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }

                    sideburnGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableSideburn.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }



                Texture2D readBeardGraphicFront = beardGraphic.MatFront.mainTexture as Texture2D;
                Texture2D readBeardGraphicSide = beardGraphic.MatSide.mainTexture as Texture2D;


                Texture2D readTacheGraphicFront = tacheGraphic.MatFront.mainTexture as Texture2D; ;
                Texture2D readTacheGraphicSide = tacheGraphic.MatSide.mainTexture as Texture2D;

                Texture2D readSideburnGraphicFront = sideburnGraphic.MatFront.mainTexture as Texture2D;
                Texture2D readSideburnGraphicSide = sideburnGraphic.MatSide.mainTexture as Texture2D;

                MergeThreeGraphics(readBeardGraphicFront, readSideburnGraphicFront, readTacheGraphicFront, ref beardFront);
                MergeThreeGraphics(readBeardGraphicSide, readSideburnGraphicSide, readTacheGraphicSide, ref beardSide);



                //       MakeBeard(readSideburnGraphicFront, readBeardGraphicFront, readTacheGraphicFront, ref beardFront, ref beardFrontComplex);
                //       MakeBeard(readBeardGraphicSide, readSideburnGraphicSide, readTacheGraphicSide, ref beardSide, ref beardSideComplex);


                MergeTwoGraphics(headGraphicFront, skinColor, readEyeGraphicFront, ref eyesHeadFront);
                MergeTwoGraphics(headGraphicSide, skinColor, readEyeGraphicSide, ref eyesHeadSide);

                if (pawnAgeFloat >= 40)
                {
                    MakeOld(pawnAgeFloat, eyesHeadFront, readWrinkleGraphicFront, ref wrinklesHeadFront);
                    MakeOld(pawnAgeFloat, eyesHeadSide, readWrinkleGraphicSide, ref wrinklesHeadSide);

                    AddFacialHair(pawn, wrinklesHeadFront, beardFront, ref finalHeadFront);
                    AddFacialHair(pawn, wrinklesHeadSide, beardSide, ref finalHeadSide);
                }
                else
                {
                    AddFacialHair(pawn, eyesHeadFront, beardFront, ref finalHeadFront);
                    AddFacialHair(pawn, eyesHeadSide, beardSide, ref finalHeadSide);
                }

            }


            if (pawn.gender == Gender.Female)
            {
                Graphic lipGraphic = null;

                if (pawn.story.crownType == CrownType.Narrow)
                {
                    lipGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableLip.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }
                else
                {
                    lipGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableLip.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }

                Texture2D readLipGraphicFront = lipGraphic.MatFront.mainTexture as Texture2D;
                Texture2D readLipGraphicSide = lipGraphic.MatSide.mainTexture as Texture2D;


                if (pawnAgeFloat >= 40)
                {
                    MakeOld(pawnAgeFloat, headGraphicFront, readWrinkleGraphicFront, ref wrinklesHeadFront);
                    MakeOld(pawnAgeFloat, headGraphicSide, readWrinkleGraphicSide, ref wrinklesHeadSide);

                    MergeThreeGraphics(wrinklesHeadFront, skinColor, readEyeGraphicFront, readLipGraphicFront, ref finalHeadFront);
                    MergeThreeGraphics(wrinklesHeadSide, skinColor, readEyeGraphicSide, readLipGraphicSide, ref finalHeadSide);
                }
                else
                {
                    MergeThreeGraphics(headGraphicFront, skinColor, readEyeGraphicFront, readLipGraphicFront, ref finalHeadFront);
                    MergeThreeGraphics(headGraphicSide, skinColor, readEyeGraphicSide, readLipGraphicSide, ref finalHeadSide);
                }


                //    AddEyes(pawn, headGraphicFront, readEyeGraphicFront, ref finalHeadFront);
                //    AddEyes(pawn, headGraphicSide, readEyeGraphicSide, ref finalHeadSide);

                //    AddEyes(pawn, finalHeadFront, readLipGraphicFront, ref finalHeadFront);
                //    AddEyes(pawn, finalHeadSide, readLipGraphicSide, ref finalHeadSide);

            }

            graphicPath = graphicPathNew;
            Texture2D finalHeadBack = new Texture2D(1, 1);
            MakeReadable(headGraphicBack, ref finalHeadBack);



            if (headGraphicBack != null)
                ExportHeadBackToPNG(pawn, finalHeadBack, skinColor, "back", graphicPath);
            if (finalHeadFront != null)
                ExportToPNG(pawn, finalHeadFront, "front", graphicPath);
            if (finalHeadSide != null)
                ExportToPNG(pawn, finalHeadSide, "side", graphicPath);

            //      if (headGraphicBack != null)
            //          ExportToPNG(pawn, headGraphicBack, "backm", graphicPath);
            //      if (finalHeadFront != null)
            //          ExportToPNG(pawn, beardFrontComplex, "frontm", graphicPath);
            //      if (finalHeadSide != null)
            //          ExportToPNG(pawn, beardSideComplex, "sidem", graphicPath);

            headsModded.Add(new HeadGraphicRecordModded(graphicPath));

            //   UnityEngine.Object.DestroyImmediate(readHeadGraphicBack, true);
            //   UnityEngine.Object.DestroyImmediate(readHeadGraphicFront, true);
            //   UnityEngine.Object.DestroyImmediate(readHeadGraphicSide, true);

            UnityEngine.Object.DestroyImmediate(finalHeadFront, true);
            UnityEngine.Object.DestroyImmediate(finalHeadSide, true);

            UnityEngine.Object.DestroyImmediate(beardFront, true);
            UnityEngine.Object.DestroyImmediate(beardSide, true);

            //    UnityEngine.Object.DestroyImmediate(beardFrontComplex, true);
            //    UnityEngine.Object.DestroyImmediate(beardSideComplex, true);


            UnityEngine.Object.DestroyImmediate(eyesHeadFront, true);
            UnityEngine.Object.DestroyImmediate(eyesHeadSide, true);

            UnityEngine.Object.DestroyImmediate(wrinklesHeadFront, true);
            UnityEngine.Object.DestroyImmediate(wrinklesHeadSide, true);


        }

        public static void RebuildHead(ref string graphicPath, Pawn pawn, Color skinColor, Color haircolor)
        {

            //this is a copy of DecorateHead, to do: merge these two instances to only one code block

            var pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);

            graphicPath = pawnSave.GraphicPathOriginal;

            float pawnAgeFloat = pawn.ageTracker.AgeBiologicalYearsFloat;

            SetFileNameStuff(pawn, skinColor, haircolor, pawnAgeFloat, graphicPath);

            Graphic headGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(graphicPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.SkinColor);

            Texture2D headGraphicBack = headGraphic.MatBack.mainTexture as Texture2D;
            Texture2D headGraphicFront = headGraphic.MatFront.mainTexture as Texture2D;
            Texture2D headGraphicSide = headGraphic.MatSide.mainTexture as Texture2D;

            Texture2D finalHeadFront = new Texture2D(headGraphicFront.width, headGraphicFront.height);
            Texture2D finalHeadSide = new Texture2D(headGraphicSide.width, headGraphicSide.height);

            Texture2D beardFront = new Texture2D(headGraphicFront.width, headGraphicFront.height);
            Texture2D beardSide = new Texture2D(headGraphicSide.width, headGraphicSide.height);

            // Deactivated for now, no complex texture
            //      Texture2D beardFrontComplex = new Texture2D(headGraphicFront.width, headGraphicFront.height);
            //      Texture2D beardSideComplex = new Texture2D(headGraphicSide.width, headGraphicSide.height);

            Texture2D eyesHeadFront = new Texture2D(headGraphicFront.width, headGraphicFront.height);
            Texture2D eyesHeadSide = new Texture2D(headGraphicSide.width, headGraphicSide.height);

            Texture2D wrinklesHeadFront = new Texture2D(headGraphicFront.width, headGraphicFront.height);
            Texture2D wrinklesHeadSide = new Texture2D(headGraphicSide.width, headGraphicSide.height);

            Texture2D lipsHeadFront = new Texture2D(headGraphicFront.width, headGraphicFront.height);
            Texture2D lipsHeadSide = new Texture2D(headGraphicSide.width, headGraphicSide.height);


            Graphic eyeGraphic = null;
            Graphic wrinkleGraphic = null;

            if (pawn.story.crownType == CrownType.Narrow)
            {
                eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableEye.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.black);

                if (type == "Normal")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableWrinkle.texPathNarrowNormal, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (type == "Pointy")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableWrinkle.texPathNarrowPointy, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (type == "Wide")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableWrinkle.texPathNarrowWide, ShaderDatabase.Cutout, Vector2.one, Color.black);
            }
            else
            {
                eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableEye.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.black);

                if (type == "Normal")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableWrinkle.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (type == "Pointy")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableWrinkle.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.black);
                if (type == "Wide")
                    wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableWrinkle.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.black);
            }


            Texture2D readEyeGraphicFront = eyeGraphic.MatFront.mainTexture as Texture2D;
            Texture2D readEyeGraphicSide = eyeGraphic.MatSide.mainTexture as Texture2D;

            Texture2D readWrinkleGraphicFront = wrinkleGraphic.MatFront.mainTexture as Texture2D;
            Texture2D readWrinkleGraphicSide = wrinkleGraphic.MatSide.mainTexture as Texture2D;


            if (pawn.gender == Gender.Male)
            {
                Graphic beardGraphic = null;
                Graphic sideburnGraphic = null;
                Graphic tacheGraphic = null;

                if (pawn.story.crownType == CrownType.Narrow)
                {
                    if (type == "Normal")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableBeard.texPathNarrowNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableTache.texPathNarrowNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                    if (type == "Pointy")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableBeard.texPathNarrowPointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableTache.texPathNarrowPointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                    if (type == "Wide")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableBeard.texPathNarrowWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableTache.texPathNarrowWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }

                    sideburnGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableSideburn.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }
                else
                {
                    if (type == "Normal")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableBeard.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableTache.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                    if (type == "Pointy")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableBeard.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableTache.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                    if (type == "Wide")
                    {
                        beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableBeard.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableTache.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }

                    sideburnGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableSideburn.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }



                Texture2D readBeardGraphicFront = beardGraphic.MatFront.mainTexture as Texture2D;
                Texture2D readBeardGraphicSide = beardGraphic.MatSide.mainTexture as Texture2D;


                Texture2D readTacheGraphicFront = tacheGraphic.MatFront.mainTexture as Texture2D; ;
                Texture2D readTacheGraphicSide = tacheGraphic.MatSide.mainTexture as Texture2D;

                Texture2D readSideburnGraphicFront = sideburnGraphic.MatFront.mainTexture as Texture2D;
                Texture2D readSideburnGraphicSide = sideburnGraphic.MatSide.mainTexture as Texture2D;

                MergeThreeGraphics(readBeardGraphicFront, readSideburnGraphicFront, readTacheGraphicFront, ref beardFront);
                MergeThreeGraphics(readBeardGraphicSide, readSideburnGraphicSide, readTacheGraphicSide, ref beardSide);



                //       MakeBeard(readSideburnGraphicFront, readBeardGraphicFront, readTacheGraphicFront, ref beardFront, ref beardFrontComplex);
                //       MakeBeard(readBeardGraphicSide, readSideburnGraphicSide, readTacheGraphicSide, ref beardSide, ref beardSideComplex);


                MergeTwoGraphics(headGraphicFront, skinColor, readEyeGraphicFront, ref eyesHeadFront);
                MergeTwoGraphics(headGraphicSide, skinColor, readEyeGraphicSide, ref eyesHeadSide);

                if (pawnAgeFloat >= 40)
                {
                    MakeOld(pawnAgeFloat, eyesHeadFront, readWrinkleGraphicFront, ref wrinklesHeadFront);
                    MakeOld(pawnAgeFloat, eyesHeadSide, readWrinkleGraphicSide, ref wrinklesHeadSide);

                    AddFacialHair(pawn, wrinklesHeadFront, beardFront, ref finalHeadFront);
                    AddFacialHair(pawn, wrinklesHeadSide, beardSide, ref finalHeadSide);
                }
                else
                {
                    AddFacialHair(pawn, eyesHeadFront, beardFront, ref finalHeadFront);
                    AddFacialHair(pawn, eyesHeadSide, beardSide, ref finalHeadSide);
                }

            }


            if (pawn.gender == Gender.Female)
            {
                Graphic lipGraphic = null;

                if (pawn.story.crownType == CrownType.Narrow)
                {
                    lipGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableLip.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }
                else
                {
                    lipGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(_saveableLip.texPathAverage, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }

                Texture2D readLipGraphicFront = lipGraphic.MatFront.mainTexture as Texture2D;
                Texture2D readLipGraphicSide = lipGraphic.MatSide.mainTexture as Texture2D;


                if (pawnAgeFloat >= 40)
                {
                    MakeOld(pawnAgeFloat, headGraphicFront, readWrinkleGraphicFront, ref wrinklesHeadFront);
                    MakeOld(pawnAgeFloat, headGraphicSide, readWrinkleGraphicSide, ref wrinklesHeadSide);

                    MergeThreeGraphics(wrinklesHeadFront, skinColor, readEyeGraphicFront, readLipGraphicFront, ref finalHeadFront);
                    MergeThreeGraphics(wrinklesHeadSide, skinColor, readEyeGraphicSide, readLipGraphicSide, ref finalHeadSide);
                }
                else
                {
                    MergeThreeGraphics(headGraphicFront, skinColor, readEyeGraphicFront, readLipGraphicFront, ref finalHeadFront);
                    MergeThreeGraphics(headGraphicSide, skinColor, readEyeGraphicSide, readLipGraphicSide, ref finalHeadSide);
                }


                //    AddEyes(pawn, headGraphicFront, readEyeGraphicFront, ref finalHeadFront);
                //    AddEyes(pawn, headGraphicSide, readEyeGraphicSide, ref finalHeadSide);

                //    AddEyes(pawn, finalHeadFront, readLipGraphicFront, ref finalHeadFront);
                //    AddEyes(pawn, finalHeadSide, readLipGraphicSide, ref finalHeadSide);

            }

            graphicPath = graphicPathNew;
            Texture2D finalHeadBack = new Texture2D(1, 1);
            MakeReadable(headGraphicBack, ref finalHeadBack);

            if (headGraphicBack != null)
                ExportHeadBackToPNG(pawn, finalHeadBack, skinColor, "back", graphicPath);
            if (finalHeadFront != null)
                ExportToPNG(pawn, finalHeadFront, "front", graphicPath);
            if (finalHeadSide != null)
                ExportToPNG(pawn, finalHeadSide, "side", graphicPath);

            //      if (headGraphicBack != null)
            //          ExportToPNG(pawn, headGraphicBack, "backm", graphicPath);
            //      if (finalHeadFront != null)
            //          ExportToPNG(pawn, beardFrontComplex, "frontm", graphicPath);
            //      if (finalHeadSide != null)
            //          ExportToPNG(pawn, beardSideComplex, "sidem", graphicPath);

            headsModded.Add(new HeadGraphicRecordModded(graphicPath));

            //   UnityEngine.Object.DestroyImmediate(readHeadGraphicBack, true);
            //   UnityEngine.Object.DestroyImmediate(readHeadGraphicFront, true);
            //   UnityEngine.Object.DestroyImmediate(readHeadGraphicSide, true);

            UnityEngine.Object.DestroyImmediate(finalHeadFront, true);
            UnityEngine.Object.DestroyImmediate(finalHeadSide, true);

            UnityEngine.Object.DestroyImmediate(beardFront, true);
            UnityEngine.Object.DestroyImmediate(beardSide, true);

            //    UnityEngine.Object.DestroyImmediate(beardFrontComplex, true);
            //    UnityEngine.Object.DestroyImmediate(beardSideComplex, true);


            UnityEngine.Object.DestroyImmediate(eyesHeadFront, true);
            UnityEngine.Object.DestroyImmediate(eyesHeadSide, true);

            UnityEngine.Object.DestroyImmediate(wrinklesHeadFront, true);
            UnityEngine.Object.DestroyImmediate(wrinklesHeadSide, true);


        }


        private static Texture2D MakeReadable(Texture2D texture, ref Texture2D myTexture2D)
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

        private static void GetHairColorNamed(Color hairColor)
        {
            float hairRed = hairColor.r;
            float hairGreen = hairColor.g;
            float hairBlue = hairColor.b;
            // Random values with 2 % chance

            if (hairRed == hairBlue && hairBlue == hairGreen)
            {
                // Old, grey hair
                if (hairRed >= 0.65f && hairRed <= 0.85f)
                {
                    HairColorNamed = "Grey";
                    return;
                }

            }
            //Dark
            if (hairColor == PawnHairColors._ColorMidnightBlack)
            {
                HairColorNamed = "Dark01";
                return;
            }

            //if (hairColor == PawnHairColors.Dark02)
            //{
            //    HairColorNamed = "Dark02";
            //    return;
            //}

            if (hairColor == PawnHairColors._ColorMediumDarkBrown)
            {
                HairColorNamed = "Dark03";
                return;
            }

            //if (hairColor == PawnHairColors.Dark04)
            //{
            //    HairColorNamed = "Dark04";
            //    return;
            //}

            //Reg Named
            if (hairColor == PawnHairColors._ColorTerraCotta)
            {
                HairColorNamed = "Color01";
                return;
            }

            //if (hairColor == PawnHairColors.Color02)
            //{
            //    HairColorNamed = "Color02";
            //    return;
            //}

            if (hairColor == PawnHairColors._ColorYellowBlonde)
            {
                HairColorNamed = "Color03";
                return;
            }
            //if (hairColor == PawnHairColors.Color04)
            //{
            //    HairColorNamed = "Color04";
            //    return;
            //}

            HairColorNamed = hairColor.ToString();
        }

        private static void GetSkinColorNamed(Color skinColor)
        {

            if (skinColor == Skin01)
            {
                SkinColorNamed = "Skin01";
                return;
            }

            if (skinColor == Skin02)
            {
                SkinColorNamed = "Skin02";
                return;
            }
            if (skinColor == Skin03)
            {
                SkinColorNamed = "Skin03";
                return;
            }
            if (skinColor == Skin04)
            {
                SkinColorNamed = "Skin04";
                return;
            }
            if (skinColor == Skin05)
            {
                SkinColorNamed = "Skin05";
                return;
            }
            if (skinColor == Skin06)
            {
                SkinColorNamed = "Skin06";
                return;
            }


            SkinColorNamed = skinColor.ToString();
        }

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

        /*
                public static void MakeBeard(Texture2D beard_layer_1, Texture2D beard_layer_2, Texture2D beard_layer_3, ref Texture2D beard_final, ref Texture2D beardComplex_final)
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


                            Color mixcolor = Color.Lerp(layer1, layer2, layer2.a / 1f);
                            Color final_color = Color.Lerp(mixcolor, layer3, layer3.a / 1f);

                            Color finalColorComplex;
                            if (final_color.a == 1)
                                finalColorComplex = Color.Lerp(Color.black, final_color, 1f);
                            else finalColorComplex = Color.black;

                            beard_final.SetPixel(x, y, final_color);
                            beardComplex_final.SetPixel(x, y, final_color);
                        }
                    }

                    beard_final.Apply();
                    beardComplex_final.Apply();

                }
        */


        public static void AddFacialHair(Pawn pawn, Texture2D head, Texture2D beard, ref Texture2D finalhead)
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
                    if (final_color.a > 0.65f)
                        final_color.a = 1;
                    else
                        final_color.a = 0;
                    finalhead.SetPixel(x, y, final_color);
                }
            }

            finalhead.Apply();
        }

        public static void MergeTwoGraphics(Texture2D bottom_layer, Texture2D top_layer, ref Texture2D finalTexture)
        {
            int startX = 0;
            int startY = bottom_layer.height - top_layer.height;

            for (int x = startX; x < bottom_layer.width; x++)
            {

                for (int y = startY; y < bottom_layer.height; y++)
                {
                    Color headColor = bottom_layer.GetPixel(x, y);
                    Color eyeColor = top_layer.GetPixel(x - startX, y - startY);

                    Color final_color = Color.Lerp(headColor, eyeColor, eyeColor.a / 1f);

                    if (headColor.a == 1)
                        final_color.a = 1;

                    finalTexture.SetPixel(x, y, final_color);
                }
            }

            finalTexture.Apply();
        }

        public static void MergeTwoGraphics(Texture2D bottom_layer, Color bottomColor, Texture2D top_layer, ref Texture2D finalTexture)
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


        public static void MakeOld(float pawnAge, Texture2D head, Texture2D wrinkles, ref Texture2D finalhead)
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

                    if (pawnAgeFileName == "40plus")
                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 1f) * 0.15f);
                    if (pawnAgeFileName == "47plus")
                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 1f) * 0.3f);
                    if (pawnAgeFileName == "54plus")
                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 1f) * 0.45f);
                    if (pawnAgeFileName == "61plus")
                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 1f) * 0.6f);
                    if (pawnAgeFileName == "68plus")
                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 1f) * 0.8f);
                    if (pawnAgeFileName == "75plus")
                        final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 1f) * 1f);

                    if (headColor.a == 1)
                        final_color.a = 1;

                    finalhead.SetPixel(x, y, final_color);
                }
            }

            finalhead.Apply();
        }

        private static void ExportToPNG(Pawn pawn, Texture2D inputTexture, string definition, string graphicpath)
        {
            byte[] bytes = inputTexture.EncodeToPNG();
            //         if (pawn.gender == Gender.Female)
            File.WriteAllBytes(ModTexturePath + graphicpath + "_" + definition + ".png", bytes);
            //     else
            //         File.WriteAllBytes(modpath + "Things/Pawn/Humanlike/Heads/Male/" + pawn.gender + "_" + pawn.story.crownType + "_" + pawn + "_" + definition + ".png", bytes);
            //     var pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);
        }

        private static void ExportHeadBackToPNG(Pawn pawn, Texture2D inputTexture, Color skinColor, string definition, string graphicpath)
        {

            Texture2D finalTexture = new Texture2D(inputTexture.width, inputTexture.height);

            int startX = 0;
            int startY = 0;

            for (int x = startX; x < inputTexture.width; x++)
            {

                for (int y = startY; y < inputTexture.height; y++)
                {
                    Color headColor = inputTexture.GetPixel(x, y);
                    headColor *= skinColor;
                    finalTexture.SetPixel(x, y, headColor);
                }
            }

            finalTexture.Apply();

            byte[] bytes = finalTexture.EncodeToPNG();
            File.WriteAllBytes(ModTexturePath + graphicpath + "_" + definition + ".png", bytes);
            UnityEngine.Object.DestroyImmediate(finalTexture);
        }

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
                    crownType = CrownType.Undefined;
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

            public Graphic_Multi_Head GetGraphic(Color skinColor)
            {
                //     for (int i = 0; i < graphics.Count; i++)
                //     {
                //  if (color.IndistinguishableFrom(graphics[i].Key))
                //  {
                //      return graphics[i].Value;
                //  }
                //     }
                Graphic_Multi_Head graphic_Multi_Head = (Graphic_Multi_Head)GraphicDatabase.Get<Graphic_Multi_Head>(graphicPath, ShaderDatabase.Cutout, Vector2.one, Color.white);
                //            graphics.Add(new KeyValuePair<Color, Graphic_Multi_Head>(color, graphic_Multi_Head));
                return graphic_Multi_Head;
            }

            /*
                        public Graphic_Multi_Head GetGraphic(Color skinColor, Color hairColor)
                        {
                            //     for (int i = 0; i < graphics.Count; i++)
                            //     {
                            //  if (color.IndistinguishableFrom(graphics[i].Key))
                            //  {
                            //      return graphics[i].Value;
                            //  }
                            //     }
                            Graphic_Multi_Head graphic_Multi_Head = (Graphic_Multi_Head)GraphicDatabase.Get<Graphic_Multi_Head>(graphicPath, ShaderDatabase.CutoutComplex, Vector2.one, skinColor, hairColor);
                            //            graphics.Add(new KeyValuePair<Color, Graphic_Multi_Head>(color, graphic_Multi_Head));
                            return graphic_Multi_Head;
                        }
              */
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
                if (!File.Exists(ModTexturePath + graphicPath + "_front.png"))
                    RebuildHead(ref graphicPath, pawn, skinColor, hairColor);

                for (int i = 0; i < headsModded.Count; i++)
                {
                    HeadGraphicRecordModded headGraphicRecord = headsModded[i];

                    if (headGraphicRecord.graphicPath == graphicPath)
                    {
                        return headGraphicRecord.GetGraphic(Color.white);
                    }
                }



            }

            for (int i = 0; i < heads.Count; i++)
            {
                HeadGraphicRecord headGraphicRecord = heads[i];

                if (headGraphicRecord.graphicPath == graphicPath)
                {
                    return headGraphicRecord.GetGraphic(Color.white);
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

    }



}


