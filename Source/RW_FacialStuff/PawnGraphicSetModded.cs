using System.IO;
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

            if (pawn.kindDef.race.ToString().Equals("Human"))
            {
                var pawnSave = MapComponent_FacialStuff.GetCache(pawn);

                if (!pawnSave.optimized)
                {
                    //  pawn.story.skinWhiteness = Rand.Value;

                    pawn.story.hairDef = PawnFaceChooser.RandomHairDefFor(pawn, pawn.Faction.def);
                    switch (pawn.story.traits.DegreeOfTrait(TraitDef.Named("TemperaturePreference")))
                    {
                        case 2:
                            if (pawn.story.skinWhiteness < 0.9f)
                            {
                                pawn.story.skinWhiteness = Random.Range(0.9f, 1f);
                            }
                            break;
                        case 1:
                            if (pawn.story.skinWhiteness < 0.8f)
                            {
                                pawn.story.skinWhiteness = Random.Range(0.8f, 1f);
                            }
                            break;
                        case 0:
                            //if (pawn.story.skinWhiteness < 0.15f || pawn.story.skinWhiteness > 0.8f)
                            //{
                            //    pawn.story.skinWhiteness = Random.Range(0.2f, 0.66f);
                            //}
                            break;
                        case -1:
                            if (pawn.story.skinWhiteness > 0.7f)
                            {
                                pawn.story.skinWhiteness = Random.Range(0.35f, 0.7f);
                            }
                            break;
                        case -2:
                            if (pawn.story.skinWhiteness > 0.65f)
                            {
                                pawn.story.skinWhiteness = Random.Range(0.35f, 0.65f);
                            }
                            break;
                    }
                    pawn.story.hairColor = PawnHairColorsModded.RandomHairColorModded(pawn.story.SkinColor, pawn.ageTracker.AgeBiologicalYears);
                    GraphicDatabaseHeadRecordsModded.DefineHeadParts(pawn);

                }

                //if (pawnSave.optimized)
                //    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, pawnSave.headGraphicIndex);

                nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor);
                rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, RottingColor * pawn.story.SkinColor);
                dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);
                skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
                hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor * RottingColor);
                ResolveApparelGraphics();
                PortraitsCache.Clear();

                if (!pawnSave.sessionOptimized)
                {
                    pawnSave.headGraphicIndex = "Heads/Blank/" + GraphicDatabaseHeadRecordsModded.headIndex.ToString("0000");
                    GraphicDatabaseHeadRecordsModded.headsModded.Add(new GraphicDatabaseHeadRecordsModded.HeadGraphicRecordModded(pawn));
                    GraphicDatabaseHeadRecordsModded.headIndex += 1;
                }

                if (pawn.RaceProps.hasGenders)
                {

                    headGraphic = GraphicDatabaseHeadRecordsModded.ModifiedVanillaHead(pawn, pawn.story.SkinColor, hairGraphic);
                }
                else
                {
                    this.headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(this.pawn.story.HeadGraphicPath, this.pawn.story.SkinColor);
                }
                //           typeof(Pawn_StoryTracker).GetField("skinColor", BindingFlags.Instance | BindingFlags.Public).SetValue(pawn.story, Color.cyan);



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
                if (pawn.RaceProps.Humanlike)
            {
                nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(this.pawn.story.BodyType, ShaderDatabase.CutoutSkin, this.pawn.story.SkinColor);
                this.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(this.pawn.story.BodyType, ShaderDatabase.CutoutSkin, PawnGraphicSet.RottingColor);
                this.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);
                this.headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(this.pawn.story.HeadGraphicPath, this.pawn.story.SkinColor);
                this.desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(this.pawn.story.HeadGraphicPath, PawnGraphicSet.RottingColor);
                this.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
                this.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(this.pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, this.pawn.story.hairColor);
                this.ResolveApparelGraphics();
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
                File.WriteAllBytes("Mods/RW_FacialStuff/Textures/Heads/Blank/" + i.ToString("0000") + "_front.png", bytes);
                File.WriteAllBytes("Mods/RW_FacialStuff/Textures/Heads/Blank/" + i.ToString("0000") + "_side.png", bytes);
                File.WriteAllBytes("Mods/RW_FacialStuff/Textures/Heads/Blank/" + i.ToString("0000") + "_back.png", bytes);

            }

            Object.DestroyImmediate(finalTexture);
        }

    }
}
