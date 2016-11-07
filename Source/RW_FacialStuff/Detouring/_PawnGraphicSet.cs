using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_FacialStuff.Detouring
{

    // ReSharper disable once UnusedMember.Global
    public class _PawnGraphicSet : PawnGraphicSet
    {
        private static Dictionary<string, Pawn> HeadIndex = new Dictionary<string, Pawn>();

        public _PawnGraphicSet(Pawn pawn) : base(pawn)
        {
            this.pawn = pawn;
            flasher = new DamageFlasher(pawn);
        }

        [Detour(typeof(PawnGraphicSet), bindingFlags = (BindingFlags.Instance | BindingFlags.Public))]
        // ReSharper disable once UnusedMember.Global
        public new void ResolveAllGraphics()
        {
#if RebuildHeads
            //This creates many empty textures. only neede for rebuilding
            ExportHeadBackToPNG();
#endif
            ClearCache();
            GraphicDatabaseHeadRecordsModded.BuildDatabaseIfNecessary();

            if (pawn.RaceProps.IsFlesh && (pawn.kindDef.race.ToString().Equals("Human")))
            {
                SaveablePawn pawnSave = MapComponent_FacialStuff.GetCache(pawn);

                if (!pawnSave.optimized)
                {
                    //  pawn.story.skinWhiteness = Rand.Value;

                    if (pawn.story.traits.DegreeOfTrait(TraitDef.Named("TemperaturePreference")) == 2)
                    {
                        if (pawn.story.skinWhiteness < 0.85f)
                        {
                            pawn.story.skinWhiteness += 0.15f;
                        }
                    }
                    else if (pawn.story.traits.DegreeOfTrait(TraitDef.Named("TemperaturePreference")) == 1)
                    {
                        if (pawn.story.skinWhiteness < 0.75f)
                        {
                            pawn.story.skinWhiteness += 0.15f;
                        }
                    }
                    else if (pawn.story.traits.DegreeOfTrait(TraitDef.Named("TemperaturePreference")) == 0)
                    {
                        //if (pawn.story.skinWhiteness < 0.15f || pawn.story.skinWhiteness > 0.8f)
                        //{
                        //    pawn.story.skinWhiteness = Random.Range(0.2f, 0.66f);
                        //}
                    }
                    else if (pawn.story.traits.DegreeOfTrait(TraitDef.Named("TemperaturePreference")) == -1)
                    {
                        if (pawn.story.skinWhiteness > 0.5f)
                        {
                            pawn.story.skinWhiteness -= 0.15f;
                        }
                    }
                    else if (pawn.story.traits.DegreeOfTrait(TraitDef.Named("TemperaturePreference")) == -2)
                    {
                        if (pawn.story.skinWhiteness > 0.25f)
                        {
                            pawn.story.skinWhiteness -= 0.15f;
                        }
                    }
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
                    // Build the empty head index once to be used for the blank heads
                    if (HeadIndex.Count == 0)
                        for (int i = 0; i < 1024; i++)
                        {
                            HeadIndex.Add(i.ToString("0000"), null);
                        }
                    // Get the first free index and go on
                    foreach (KeyValuePair<string, Pawn> pair in HeadIndex)
                    {
                        if (pair.Value == null)
                        {
                            string index = pair.Key;
                            HeadIndex.Remove(pair.Key);
                            HeadIndex.Add(index, pawn);

                            pawnSave.headGraphicIndex = "Heads/Blank/" + pair.Key;
                            GraphicDatabaseHeadRecordsModded.headsModded.Add(new GraphicDatabaseHeadRecordsModded.HeadGraphicRecordModded(pawn));
                            break;
                        }
                    }

                    //pawnSave.headGraphicIndex = "Heads/Blank/" + GraphicDatabaseHeadRecordsModded.headIndex.ToString("0000");
                    //GraphicDatabaseHeadRecordsModded.headsModded.Add(new GraphicDatabaseHeadRecordsModded.HeadGraphicRecordModded(pawn));
                    //GraphicDatabaseHeadRecordsModded.headIndex += 1;
                }

                if (pawn.RaceProps.hasGenders)
                {

                    headGraphic = GraphicDatabaseHeadRecordsModded.ModifiedVanillaHead(pawn, pawn.story.SkinColor, hairGraphic);
                }
                else
                {
                    headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor);
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
            else if (pawn.RaceProps.Humanlike)
            {
                nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor);
                rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, RottingColor);
                dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);
                headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor);
                desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, RottingColor);
                skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
                hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                ResolveApparelGraphics();


                SaveablePawn pawnSave = MapComponent_FacialStuff.GetCache(pawn);
                if (!pawnSave.sessionOptimized)
                {
                    // Build the empty head index once to be used for the blank heads
                    if (HeadIndex.Count == 0)
                        for (int i = 0; i < 1024; i++)
                        {
                            HeadIndex.Add(i.ToString("0000"), null);
                        }
                    // Get the first free index and go on
                    foreach (KeyValuePair<string, Pawn> pair in HeadIndex)
                    {
                        if (pair.Value == null)
                        {
                            string index = pair.Key;
                            HeadIndex.Remove(pair.Key);
                            HeadIndex.Add(index, pawn);

                            pawnSave.headGraphicIndex = "Heads/Blank/" + pair.Key;
                            GraphicDatabaseHeadRecordsModded.headsModded.Add(new GraphicDatabaseHeadRecordsModded.HeadGraphicRecordModded(pawn));
                            break;
                        }
                    }

                    //pawnSave.headGraphicIndex = "Heads/Blank/" + GraphicDatabaseHeadRecordsModded.headIndex.ToString("0000");
                    //GraphicDatabaseHeadRecordsModded.headsModded.Add(new GraphicDatabaseHeadRecordsModded.HeadGraphicRecordModded(pawn));
                    //GraphicDatabaseHeadRecordsModded.headIndex += 1;
                }

                if (pawn.RaceProps.hasGenders && hairGraphic != null)
                {
                    headGraphic = GraphicDatabaseHeadRecordsModded.ModifiedAlienHead(pawn, pawn.story.SkinColor, hairGraphic);
                }
                else
                {
                    headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor);
                }

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

            for (int i = 0; i < 1024; i++)
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
