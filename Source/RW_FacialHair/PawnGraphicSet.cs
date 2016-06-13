using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections;
using UnityEngine;
using Verse;

namespace RW_FacialHair
{
    // public class BeardyPawn:Pawn
    // {
    //     public Pawn_BeardTracker beardyPawn;
    // }



    public class PawnGraphicHairSet : PawnGraphicSet
    {
#pragma warning disable CS0824 // Konstruktor ist extern markiert
        public extern PawnGraphicHairSet();
#pragma warning restore CS0824 // Konstruktor ist extern markiert



        //     public Pawn _saveablePawn;
        //     public BeardDef _saveableBeard;
        //     private TacheDef _saveableTache;

        public Graphic beardGraphic;
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

        public new void ResolveAllGraphics()
        {
            ClearCache();
            if (pawn.RaceProps.Humanlike)
            {

                nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor);
                rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, RW_FacialHair.PawnGraphicHairSet.RottingColor);
                dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);

                headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor);

                if (pawn.gender != Gender.Female)
                {
                    if (pawn.ageTracker.AgeBiologicalYears > 18)
                    {

                        var pawnSave = MapComponent_FacialHair.Get.GetCache(pawn);

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




                        beardGraphic = GraphicDatabase.Get<Graphic_Multi>(_saveableBeard.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                        sideburnGraphic = GraphicDatabase.Get<Graphic_Multi>(_saveableSideburn.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                        tacheGraphic = GraphicDatabase.Get<Graphic_Multi>(_saveableTache.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);


                        Texture2D readHeadGraphicFront = null;

                        Texture2D readBeardGraphicFront = null;
                        Texture2D readSideburnGraphicFront = null;
                        Texture2D readTacheGraphicFront = null;

                        Texture2D readHeadGraphicSide = null;

                        Texture2D readBeardGraphicSide = null;
                        Texture2D readSideburnGraphicSide = null;
                        Texture2D readTacheGraphicSide = null;

                        Texture2D finalHeadFront = null;
                        Texture2D finalHeadSide = null;


                        MakeReadable(headGraphic.MatFront.mainTexture as Texture2D, ref readHeadGraphicFront);
                        MakeReadable(headGraphic.MatSide.mainTexture as Texture2D, ref readHeadGraphicSide);

                        MakeReadable(beardGraphic.MatFront.mainTexture as Texture2D, ref readBeardGraphicFront);
                        MakeReadable(beardGraphic.MatSide.mainTexture as Texture2D, ref readBeardGraphicSide);

                        MakeReadable(sideburnGraphic.MatFront.mainTexture as Texture2D, ref readSideburnGraphicFront);
                        MakeReadable(sideburnGraphic.MatSide.mainTexture as Texture2D, ref readSideburnGraphicSide);

                        MakeReadable(tacheGraphic.MatFront.mainTexture as Texture2D, ref readTacheGraphicFront);
                        MakeReadable(tacheGraphic.MatSide.mainTexture as Texture2D, ref readTacheGraphicSide);


                        //    if (UnityEngine.Random.Range(0f, 3f) >  100 / pawn.ageTracker.AgeBiologicalYears)
                        //    { }

                        MakeBeard(readSideburnGraphicFront, readBeardGraphicFront, readTacheGraphicFront, ref finalHeadFront);

                        MakeBeard(readBeardGraphicSide, readSideburnGraphicSide, readTacheGraphicSide, ref finalHeadSide);                        //           }

                        AddFacialHair(readHeadGraphicFront, finalHeadFront, ref finalHeadFront);
                        AddFacialHair(readHeadGraphicSide, finalHeadSide, ref finalHeadSide);
                        //       }

                        headGraphic.MatFront.mainTexture = finalHeadFront;
                        headGraphic.MatSide.mainTexture = finalHeadSide;
                    }
                }



                desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, RW_FacialHair.PawnGraphicHairSet.RottingColor);
                skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
                hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                ResolveApparelGraphics();

                //beardGraphic
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
                rottingGraphic = nakedGraphic.GetColoredVersion(ShaderDatabase.CutoutSkin, RW_FacialHair.PawnGraphicHairSet.RottingColor, RW_FacialHair.PawnGraphicHairSet.RottingColor);
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
                    if (layer2.a > 0)
                        final_color = Color.Lerp(layer1, layer2, layer2.a / 1.0f);
                    if (layer2.a == 1)
                        final_color = layer2;

                    if (layer3.a > 0)
                        final_color = Color.Lerp(layer1, layer3, layer3.a / 1.0f);
                    if (layer3.a == 1)
                        final_color = layer3;


                    //         Color final_color = Color.Lerp(headColor, new Color(beardColorFace.r * 0.35f, beardColorFace.g * 0.35f, beardColorFace.b * 0.35f), beardColor.a / 1.0f);
                    //        Color final_color = Color.Lerp(beardColor, tacheColor, tacheColor.a / 1.0f);

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

                    beardColor.r = beardColor.r * beardColorFace.r * UnityEngine.Random.Range(1f, 2.5f) / skin.r * whiteness;
                    beardColor.g = beardColor.g * beardColorFace.g * UnityEngine.Random.Range(1f, 2.5f) / skin.g * whiteness;
                    beardColor.b = beardColor.b * beardColorFace.b * UnityEngine.Random.Range(1f, 2.5f) / skin.b * whiteness;

                    Color final_color = headColor;
                    if (beardColor.a > 0)
                        final_color = Color.Lerp(headColor, beardColor, beardColor.a / 1.0f);
                    if (beardColor.a == 1)
                        final_color = beardColor;


                    //      Color final_color = Color.Lerp(headColor, beardColor, beardColor.a / 1.0f);

                    head.SetPixel(x, y, final_color);
                }
            }

            head.Apply();
            finalhead = head;
            return finalhead;
        }

        //   public override void ExposeData()
        //   {
        //       Scribe_References.LookReference(ref _saveablePawn, "Pawn");
        //       Scribe_Defs.LookDef(ref _saveableBeard, "BeardDef");
        //       Scribe_Defs.LookDef(ref _saveableTache, "TacheDef");
        //   }
        //  public void ExposeData()
        //  {
        //      Scribe_References.LookReference(ref pawn, "Pawn");
        //      Scribe_Values.LookValue(ref sideburnGraphic, "SideBurn");
        //      Scribe_Values.LookValue(ref tacheGraphic, "Tache");
        //      Scribe_Values.LookValue(ref beardGraphic, "Beard");
        //  }
    }
}
