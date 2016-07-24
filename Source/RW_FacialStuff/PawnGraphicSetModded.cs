using System.Reflection;
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

        public void ResolveAllGraphicsModded()
        {
            ClearCache();
            GraphicDatabaseHeadRecordsModded.BuildDatabaseIfNecessary();

            if (pawn.RaceProps.Humanlike)
            {
                nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor);
                rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, RottingColor);
                dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);

                var pawnSave = MapComponent_FacialStuff.GetCache(pawn);

                if (!pawnSave.optimized)
                {
                    GraphicDatabaseHeadRecordsModded.DefineHeadParts(pawn);
                }

                //if (pawnSave.optimized)
                //    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, pawnSave.headGraphicPathModded);


                desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, RottingColor); skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
                hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                ResolveApparelGraphics();
                PortraitsCache.Clear();

                headGraphic = GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(pawn);

                GraphicDatabaseHeadRecordsModded.ModifyVanillaHead(pawn, hairGraphic, ref headGraphic);


     //           typeof(Pawn_StoryTracker).GetField("skinColor", BindingFlags.Instance | BindingFlags.Public).SetValue(pawn.story, Color.cyan);
                
                //overwrites the crown type so that manually merged hair looks good again.
                //      pawn.story.crownType = CrownType.Average;

                /*
                for (int j = 0; j < apparelGraphics.Count; j++)
                {
                    if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                    {
                        // INTERESTING                
                        pawn.Drawer.renderer.graphics.headGraphic = hairGraphic;
                    }
                }
                */

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

        // Verse.PawnGraphicSet
        public GraphicMeshSet HairMeshSetModded
        {
            get
            {
                //       return MeshPool.humanlikeHairSetAverage;

                if (pawn.story.crownType == CrownType.Average)
                {
                    return MeshPool.humanlikeHairSetAverage;
                }
                if (pawn.story.crownType == CrownType.Narrow)
                {
                    return MeshPool.humanlikeHairSetNarrow;
                }
                Log.Error("Unknown crown type: " + pawn.story.crownType);
                return MeshPool.humanlikeHairSetAverage;
            }
        }


    }
}
