using System.IO;
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
            //This creates many empty textures. only neede for rebuilding
           // ExportHeadBackToPNG();


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
                //    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, pawnSave.headGraphicIndex);


                desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, RottingColor); skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
                hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                ResolveApparelGraphics();
                PortraitsCache.Clear();

                pawnSave.headGraphicIndex = "Things/Pawn/Humanlike/Heads/Blank/" + GraphicDatabaseHeadRecordsModded.headIndex.ToString("0000");
                GraphicDatabaseHeadRecordsModded.headsModded.Add(new GraphicDatabaseHeadRecordsModded.HeadGraphicRecordModded(pawn));
                GraphicDatabaseHeadRecordsModded.headIndex += 1;
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

        private static void ExportHeadBackToPNG()
        {

            Texture2D finalTexture = new Texture2D(128, 128);

            int startX = 0;
            int startY = 0;

            for (int x = startX; x < finalTexture.width; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {
                    finalTexture.SetPixel(x, y, Color.clear);
                }
            }

            finalTexture.Apply();

            for (int i = 0; i < 512; i++)
            {
                byte[] bytes = finalTexture.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/Textures/Things/Pawn/Humanlike/Heads/Blank/" + i.ToString("0000") + "_front.png", bytes);
                File.WriteAllBytes("Mods/RW_FacialStuff/Textures/Things/Pawn/Humanlike/Heads/Blank/" + i.ToString("0000") + "_side.png", bytes);
                File.WriteAllBytes("Mods/RW_FacialStuff/Textures/Things/Pawn/Humanlike/Heads/Blank/" + i.ToString("0000") + "_back.png", bytes);

            }

            Object.DestroyImmediate(finalTexture);
        }

    }
}
