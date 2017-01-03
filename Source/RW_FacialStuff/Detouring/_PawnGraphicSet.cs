using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_FacialStuff.Detouring
{

    // ReSharper disable once UnusedMember.Global
    public static class _PawnGraphicSet 
    {
        private static Dictionary<string, Pawn> HeadIndex = new Dictionary<string, Pawn>();
        public static readonly Color RottingColor = new Color(0.34f, 0.32f, 0.3f);

        //       [Detour(typeof(PawnGraphicSet), bindingFlags = (BindingFlags.Instance | BindingFlags.Public))]
        // ReSharper disable once UnusedMember.Global
        [Detour(typeof(PawnGraphicSet))]
        public static void ResolveAllGraphics(this PawnGraphicSet _this)
        {
#if RebuildHeads
            //This creates many empty textures. only neede for rebuilding
            ExportHeadBackToPNG();
#endif
            Pawn pawn = _this.pawn;
            _this.ClearCache();
            GraphicDatabaseHeadRecordsModded.BuildDatabaseIfNecessary();

            if (pawn.RaceProps.IsFlesh && (pawn.kindDef.race.ToString().Equals("Human")))
            {
                SaveablePawn pawnSave = MapComponent_FacialStuff.GetCache(pawn);

                if (!pawnSave.optimized)
                {
                    GraphicDatabaseHeadRecordsModded.DefineHeadParts(pawn);
                }

                //if (pawnSave.optimized)
                //    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, pawnSave.headGraphicIndex);

                _this.nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.bodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor);
                _this.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.bodyType, ShaderDatabase.CutoutSkin, RottingColor * pawn.story.SkinColor);
                _this.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);
                _this.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
                _this.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                _this.desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor * RottingColor);
                _this.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
                _this.ResolveApparelGraphics();
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

                    _this.headGraphic = GraphicDatabaseHeadRecordsModded.ModifiedVanillaHead(pawn, pawn.story.SkinColor, _this.hairGraphic);
                }
                else
                {
                    _this.headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor);
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
                _this.nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.bodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor);
                _this.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.bodyType, ShaderDatabase.CutoutSkin, RottingColor);
                _this.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);
                _this.headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor);
                _this.desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, RottingColor);
                _this.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
                _this.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                _this.ResolveApparelGraphics();



            }
            else
            {
                PawnKindLifeStage curKindLifeStage = _this.pawn.ageTracker.CurKindLifeStage;
                if (_this.pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
                {
                    _this.nakedGraphic = curKindLifeStage.bodyGraphicData.Graphic;
                }
                else
                {
                    _this.nakedGraphic = curKindLifeStage.femaleGraphicData.Graphic;
                }
                _this.rottingGraphic = _this.nakedGraphic.GetColoredVersion(ShaderDatabase.CutoutSkin, PawnGraphicSet.RottingColor, PawnGraphicSet.RottingColor);
                if (_this.pawn.RaceProps.packAnimal)
                {
                    _this.packGraphic = GraphicDatabase.Get<Graphic_Multi>(_this.nakedGraphic.path + "Pack", ShaderDatabase.Cutout, _this.nakedGraphic.drawSize, Color.white);
                }
                if (curKindLifeStage.dessicatedBodyGraphicData != null)
                {
                    _this.dessicatedGraphic = curKindLifeStage.dessicatedBodyGraphicData.GraphicColoredFor(_this.pawn);
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
