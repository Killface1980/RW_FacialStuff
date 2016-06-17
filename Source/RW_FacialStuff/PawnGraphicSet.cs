using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{

    public class PawnGraphicHairSet : PawnGraphicSet
    {
#pragma warning disable CS0824 // Konstruktor ist extern markiert
        public extern PawnGraphicHairSet();
#pragma warning restore CS0824 // Konstruktor ist extern markiert

        public static Dictionary<string, Graphic> textureCache = new Dictionary<string, Graphic>();


        public static Graphic Cache(Pawn pawn, string texturePath, Color skincolor)
        {
            Graphic texture = null;
            //
            //   if (textureCache.TryGetValue(pawn.Label + "_" + texturePath, out texture)) return texture;
            //   else
            //   {
            texture = GraphicDatabase.Get<Graphic_Multi>(texturePath, ShaderDatabase.Cutout, Vector2.one, skincolor);
            //                textureCache.Add(pawn.Label + "_" + texturePath, texture);


            return texture;
            //          }
        }


        public new void ResolveAllGraphics()
        {
            ClearCache();
            if (pawn.RaceProps.Humanlike)
            {

                nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor);
                rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, RW_FacialStuff.PawnGraphicHairSet.RottingColor);
                dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);

                var pawnSave = MapComponent_FacialStuff.Get.GetCache(pawn);

              if (!pawnSave.optimized)
                  GraphicDatabaseModdedHeadRecords.AddCustomizedHead(pawn, pawn.story.SkinColor, pawn.story.hairColor, pawn.story.HeadGraphicPath);


                headGraphic = GraphicDatabaseModdedHeadRecords.GetHeadNamed(pawn, pawn.story.HeadGraphicPath, pawn.story.SkinColor, pawn.story.hairColor);

                desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, RottingColor);
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
                rottingGraphic = nakedGraphic.GetColoredVersion(ShaderDatabase.CutoutSkin, RottingColor, RottingColor);
                if (curKindLifeStage.dessicatedBodyGraphicData != null)
                {
                    dessicatedGraphic = curKindLifeStage.dessicatedBodyGraphicData.GraphicColoredFor(pawn);
                }
            }
        }



    }
}
