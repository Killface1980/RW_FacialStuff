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

        public List<Graphic> graphics = new List<Graphic>(8);

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

   //   // EdB.PrepareCarefully.CustomPawn
   //   public string HeadGraphicPath
   //   {
   //       get
   //       {
   //           return this.pawn.story.HeadGraphicPath + "/" + pawn;
   //       }
   //       set
   //       {
   //           typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.pawn.story, value + "/"+ pawn);
   //           this.ResetHead();
   //       }
   //   }
   //
   //   // EdB.PrepareCarefully.CustomPawn
   //   protected void ResetHead()
   //   {
   //       typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.pawn.story, this.FilterHeadPathForGender(this.pawn.story.HeadGraphicPath));
   //       this.graphics[5] = GraphicDatabaseHeadRecords.GetHeadNamed(this.pawn.story.HeadGraphicPath, this.pawn.story.SkinColor);
   //   }
   //
   //   // EdB.PrepareCarefully.CustomPawn
   //   protected string FilterHeadPathForGender(string path)
   //   {
   //       if (this.pawn.gender == Gender.Male)
   //       {
   //           return path.Replace("Female", "Male");
   //       }
   //       return path.Replace("Male", "Female");
   //   }


        public new void ResolveAllGraphics()
        {
            ClearCache();
            if (pawn.RaceProps.Humanlike)
            {

                Graphic baseHeadGraphic = null;

                nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor);
                rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, RW_FacialStuff.PawnGraphicHairSet.RottingColor);
                dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);


         //       baseHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor);
                headGraphic = GraphicDatabaseFacedHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor);

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



    }
}
