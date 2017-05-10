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



        [Detour(typeof(PawnGraphicSet), bindingFlags = (BindingFlags.Instance | BindingFlags.Public))]
        // ReSharper disable once UnusedMember.Global
        public static void ResolveAllGraphics(PawnGraphicSet _this)
        {
#if RebuildHeads
            //This creates many empty textures. only needed for rebuilding
            ExportHeadBackToPNG();
#endif
            _this.ClearCache();
            GraphicDatabaseHeadRecordsModded.BuildDatabaseIfNecessary();

            if (_this.pawn.RaceProps.IsFlesh && (_this.pawn.kindDef.race.ToString().Equals("Human")))
            {
                CompFace faceComp = _this.pawn.TryGetComp<CompFace>();

                // if (!faceComp.optimized)
                // {

                //   // Import MapComponent face definitions from old versions
                //   SaveablePawn pawnSave = MapComponent_FacialStuff.GetCache(_this.pawn);
                //   if (pawnSave != null)
                //   {
                //       faceComp.BeardDef = pawnSave.BeardDef;
                //       faceComp.EyeDef = pawnSave.EyeDef;
                //       faceComp.BrowDef = pawnSave.BrowDef;
                //       faceComp.MouthDef = pawnSave.MouthDef;
                //       faceComp.WrinkleDef = pawnSave.WrinkleDef;
                //
                //       faceComp.headGraphicIndex = pawnSave.headGraphicIndex;
                //       faceComp.type = pawnSave.type;
                //
                //       faceComp.SkinColorHex = pawnSave.SkinColorHex;
                //       faceComp.HairColorOrg = pawnSave.HairColorOrg;
                //       faceComp.optimized = pawnSave.optimized;
                //       faceComp.drawMouth = pawnSave.drawMouth;
                //   }
                //   else
                //   {
                if (!faceComp.optimized)
                    faceComp.DefineFace(_this.pawn);
                //   }
            //}


            //if (pawnSave.optimized)
            //    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, pawnSave.headGraphicIndex);

            _this.nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(_this.pawn.story.bodyType, ShaderDatabase.CutoutSkin, _this.pawn.story.SkinColor);
            _this.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(_this.pawn.story.bodyType, ShaderDatabase.CutoutSkin, PawnGraphicSet.RottingColor * _this.pawn.story.SkinColor);
            _this.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);
            _this.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
            _this.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(_this.pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, _this.pawn.story.hairColor);
            _this.desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(_this.pawn.story.HeadGraphicPath, _this.pawn.story.SkinColor * PawnGraphicSet.RottingColor);
            _this.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
            _this.ResolveApparelGraphics();
            PortraitsCache.Clear();

            if (!faceComp.sessionOptimized)
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
                        HeadIndex.Add(index, _this.pawn);

                        faceComp.headGraphicIndex = "Heads/Blank/" + pair.Key;
                        GraphicDatabaseHeadRecordsModded.headsModded.Add(new GraphicDatabaseHeadRecordsModded.HeadGraphicRecordModded(_this.pawn));
                        break;
                    }
                }

                //pawnSave.headGraphicIndex = "Heads/Blank/" + GraphicDatabaseHeadRecordsModded.headIndex.ToString("0000");
                //GraphicDatabaseHeadRecordsModded.headsModded.Add(new GraphicDatabaseHeadRecordsModded.HeadGraphicRecordModded(pawn));
                //GraphicDatabaseHeadRecordsModded.headIndex += 1;
            }

            if (_this.pawn.RaceProps.hasGenders)
            {

                _this.headGraphic = GraphicDatabaseHeadRecordsModded.ModifiedVanillaHead(_this.pawn, _this.pawn.story.SkinColor, _this.hairGraphic);
            }
            else
            {
                _this.headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(_this.pawn.story.HeadGraphicPath, _this.pawn.story.SkinColor);
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
            else if (_this.pawn.RaceProps.Humanlike)
            {
                _this.nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(_this.pawn.story.bodyType, ShaderDatabase.CutoutSkin, _this.pawn.story.SkinColor);
                _this.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(_this.pawn.story.bodyType, ShaderDatabase.CutoutSkin, PawnGraphicSet.RottingColor);
                _this.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);
                _this.headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(_this.pawn.story.HeadGraphicPath, _this.pawn.story.SkinColor);
                _this.desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(_this.pawn.story.HeadGraphicPath, PawnGraphicSet.RottingColor);
                _this.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
                _this.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(_this.pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, _this.pawn.story.hairColor);
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
