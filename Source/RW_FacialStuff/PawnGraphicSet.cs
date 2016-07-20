using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{

    public class PawnGraphicSetModded : PawnGraphicSet
    {
#pragma warning disable CS0824 // Konstruktor ist extern markiert
        public extern PawnGraphicSetModded();
#pragma warning restore CS0824 // Konstruktor ist extern markiert

        //   public static Graphic Cache(Pawn pawn, string texturePath, Color skincolor)
        //   {
        //       Graphic texture = null;
        //       //
        //       //   if (textureCache.TryGetValue(pawn.Label + "_" + texturePath, out texture)) return texture;
        //       //   else
        //       //   {
        //       texture = GraphicDatabase.Get<Graphic_Multi>(texturePath, ShaderDatabase.Cutout, Vector2.one, skincolor);
        //       //                textureCache.Add(pawn.Label + "_" + texturePath, texture);
        //
        //
        //       return texture;
        //       //          }
        //   }

        public PawnGraphicSet graphics;

        public void ResolveAllGraphicsModded()
        {
            ClearCache();
            if (pawn.RaceProps.Humanlike)
            {

                nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor);
                rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, RottingColor * pawn.story.SkinColor);
                dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);

                var pawnSave = MapComponent_FacialStuff.GetCache(pawn);
              
              if (!pawnSave.optimized)
                  GraphicDatabaseHeadRecordsModded.AddCustomizedHead(pawn, pawn.story.SkinColor, pawn.story.hairColor, pawn.story.HeadGraphicPath);
                headGraphic = GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(pawn, pawn.story.HeadGraphicPath, pawn.story.SkinColor, pawn.story.hairColor);
                desiccatedHeadGraphic = GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(pawn, pawn.story.HeadGraphicPath, RottingColor);
                skullGraphic = GraphicDatabaseHeadRecords.GetSkull();

// INTERESTING                pawn.Drawer.renderer.graphics.headGraphic = skullGraphic;

                hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                ResolveApparelGraphics();
                PortraitsCache.Clear();

            //  List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
            //  for (int j = 0; j < apparelGraphics.Count; j++)
            //  {
            //      if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
            //      {
            //          if (apparelGraphics[j].sourceApparel.def.apparel.wornGraphicPath.Contains("Hat"))
            //              apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Accessory;
            //      }
            //  }
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
