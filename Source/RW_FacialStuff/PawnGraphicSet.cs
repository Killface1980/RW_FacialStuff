using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{

    public class PawnGraphicHairSet : PawnGraphicSet
    {
#pragma warning disable CS0824 // Konstruktor ist extern markiert
        public extern PawnGraphicHairSet();
#pragma warning restore CS0824 // Konstruktor ist extern markiert

        static string modpath = "Mods/RW_FacialStuff/Textures/";

        public Graphic beardGraphic;
        public Graphic eyeGraphic;
        public Graphic sideburnGraphic;
        public Graphic tacheGraphic;

        public Material BeardMatAt(Rot4 facing)
        {
            Material baseMat = beardGraphic.MatAt(facing, null);
            return flasher.GetDamagedMat(baseMat);
        }

        public Material TacheMatAt(Rot4 facing)
        {
            Material baseMat = tacheGraphic.MatAt(facing, null);
            return flasher.GetDamagedMat(baseMat);
        }

        private static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

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

        public new void ResolveAllGraphics()
        {
            ClearCache();
            if (pawn.RaceProps.Humanlike)
            {

                nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor);
                rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, RW_FacialStuff.PawnGraphicHairSet.RottingColor);
                dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);

                headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor);

                if (pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsPrisoner)
                {
                    if (pawn.gender == Gender.Male)
                    {
                        if (pawn.ageTracker.AgeBiologicalYears > 18)
                        {

                            //   if (File.Exists(modpath + "/_cachedHeads/" + pawn + "_" + pawn.ageTracker.BirthYear + "-" + pawn.ageTracker.BirthDayOfYear + "_side.png")
                            //       && File.Exists(modpath + "/_cachedHeads/" + pawn + "_" + pawn.ageTracker.BirthYear + "-" + pawn.ageTracker.BirthDayOfYear + "_front.png"))
                            //   {
                            //       headGraphic.MatFront.mainTexture = LoadTexture(modpath + "/_cachedHeads/" + pawn + "_" + pawn.ageTracker.BirthYear + "-" + pawn.ageTracker.BirthDayOfYear + "_front.png");
                            //       headGraphic.MatFront.mainTexture = LoadTexture(modpath + "/_cachedHeads/" + pawn + "_" + pawn.ageTracker.BirthYear + "-" + pawn.ageTracker.BirthDayOfYear + "_side.png");
                            //   }
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

                                BeardDef _saveableBeard;

                                if (pawnSave.BeardDef != null)
                                {
                                    _saveableBeard = pawnSave.BeardDef;
                                }
                                else
                                {
                                    _saveableBeard = PawnBeardChooser.RandomBeardDefFor(pawn, pawn.Faction.def);
                                    pawnSave.BeardDef = _saveableBeard;
                                }

                                SideburnDef _saveableSideburn;

                                if (pawnSave.SideburnDef != null)
                                {
                                    _saveableSideburn = pawnSave.SideburnDef;
                                }
                                else
                                {
                                    _saveableSideburn = PawnBeardChooser.RandomSideburnDefFor(pawn, pawn.Faction.def);
                                    pawnSave.SideburnDef = _saveableSideburn;
                                }

                                TacheDef _saveableTache;

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

                                MakeBeard(readSideburnGraphicFront, readBeardGraphicFront, readTacheGraphicFront, ref finalHeadFront);
                                MakeBeard(readBeardGraphicSide, readSideburnGraphicSide, readTacheGraphicSide, ref finalHeadSide);                        //           }

                                AddEyes(readHeadGraphicFront, readEyeGraphicFront, ref eyesHeadFront);
                                AddEyes(readHeadGraphicSide, readEyeGraphicSide, ref eyesHeadSide);

                                AddFacialHair(eyesHeadFront, finalHeadFront, ref finalHeadFront);
                                AddFacialHair(eyesHeadSide, finalHeadSide, ref finalHeadSide);

                                headGraphic.MatFront.mainTexture = finalHeadFront;
                                headGraphic.MatSide.mainTexture = finalHeadSide;

                                //         AddToHeadCache(finalHeadFront, "_front");
                                //         AddToHeadCache(finalHeadSide, "_side");

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

                                AddEyes(readHeadGraphicFront, readEyeGraphicFront, ref finalHeadFront);
                                AddEyes(readHeadGraphicSide, readEyeGraphicSide, ref finalHeadSide);

                                headGraphic.MatFront.mainTexture = finalHeadFront;
                                headGraphic.MatSide.mainTexture = finalHeadSide;

                                //    AddToHeadCache(finalHeadFront, "_front");
                                //    AddToHeadCache(finalHeadSide, "_side");

                            }
                        }

                    } 
                }



                desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, RW_FacialStuff.PawnGraphicHairSet.RottingColor);
                skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
                hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                ResolveApparelGraphics();
            }
            else
            {
                PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
                if (pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
                {
                    nakedGraphic = curKindLifeStage.bodyGraphicData.Graphic;
                }
                else
                {
                    nakedGraphic = curKindLifeStage.femaleGraphicData.Graphic;
                }
                rottingGraphic = nakedGraphic.GetColoredVersion(ShaderDatabase.CutoutSkin, RW_FacialStuff.PawnGraphicHairSet.RottingColor, RW_FacialStuff.PawnGraphicHairSet.RottingColor);
                if (curKindLifeStage.dessicatedBodyGraphicData != null)
                {
                    dessicatedGraphic = curKindLifeStage.dessicatedBodyGraphicData.GraphicColoredFor(pawn);
                }
            }
        }

        public Texture2D MakeReadable(Texture2D texture, ref Texture2D myTexture2D)
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

        public Texture2D MakeBeard(Texture2D beard_layer_1, Texture2D beard_layer_2, Texture2D beard_layer_3, ref Texture2D beard_final)
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

        public Texture2D AddFacialHair(Texture2D head, Texture2D beard, ref Texture2D finalhead)
        {
            int startX = 0;
            int startY = head.height - beard.height;

            for (int x = startX; x < head.width; x++)
            {

                for (int y = startY; y < head.height; y++)
                {
                    Color headColor = head.GetPixel(x, y);
                    Color beardColor = beard.GetPixel(x - startX, y - startY);

                    Color beardColorFace = pawn.story.hairColor;
                    Color skin = pawn.story.SkinColor;
                    float whiteness = pawn.story.skinWhiteness;

                    beardColor.r = beardColor.r * beardColorFace.r * UnityEngine.Random.Range(1.2f, 2.2f) / skin.r * whiteness;
                    beardColor.g = beardColor.g * beardColorFace.g * UnityEngine.Random.Range(1.2f, 2.2f) / skin.g * whiteness;
                    beardColor.b = beardColor.b * beardColorFace.b * UnityEngine.Random.Range(1.2f, 2.2f) / skin.b * whiteness;


                    Color final_color = headColor;
                    final_color = Color.Lerp(headColor, beardColor, beardColor.a/1f);

                    head.SetPixel(x, y, final_color);
                }
            }

            head.Apply();
            finalhead = head;
            return finalhead;
        }

        public Texture2D AddEyes(Texture2D head, Texture2D eyes, ref Texture2D finalhead)
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

        public void AddToHeadCache(Texture2D inputTexture, string definition)
        {
            byte[] bytes = inputTexture.EncodeToPNG();
            File.WriteAllBytes(modpath + "/_cachedHeads/" + pawn + "_" + pawn.ageTracker.BirthYear + "-" + pawn.ageTracker.BirthDayOfYear + definition + ".png", bytes);
        }

    }
}
