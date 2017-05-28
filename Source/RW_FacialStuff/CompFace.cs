using System.Collections.Generic;
using System.IO;
using System.Linq;
using RW_FacialStuff.Defs;
using UnityEngine;
using Verse;
using static RW_FacialStuff.GraphicDatabaseHeadRecordsModded;
using static RW_FacialStuff.Headhelper;

namespace RW_FacialStuff
{
    public class CompFace : ThingComp
    {


        public BeardDef BeardDef;
        public EyeDef EyeDef;
        public BrowDef BrowDef;
        public MouthDef MouthDef;
        public WrinkleDef WrinkleDef;
        public string headGraphicIndex;
        public string type;

        public string SkinColorHex;
        public Color HairColorOrg;

        public bool optimized;
        public bool sessionOptimized;
        public bool drawMouth = true;

        private Graphic _eyeGraphic;
        private Graphic _browGraphic;
        private Graphic _beardGraphic;
        private Graphic _wrinkleGraphic;
        private Graphic _lipGraphic;

        private Texture2D _temptexturefront;
        private Texture2D _temptextureside;
        private Texture2D _temptextureback;

        public Graphic_Multi HeadGraphic;
        public Graphic_Multi DissicatedHeadGraphic;

        public void GetModdedHeadNamed(bool useVanilla, Color color, out Graphic_Multi head)
        {
            Pawn pawn = parent as Pawn;

            if (useVanilla)
            {
                foreach (HeadGraphicRecordVanillaCustom headGraphicRecordVanillaCustom in headsVanillaCustom)
                {
                    if (headGraphicRecordVanillaCustom.graphicPathVanillaCustom == pawn.story.HeadGraphicPath.Remove(0, 22))
                    {
                        head = headGraphicRecordVanillaCustom.GetGraphic(color);
                        return;
                    }
                }
            }

            foreach (HeadGraphicRecordModded headGraphicRecordModded in headsModded)
            {
                if (headGraphicRecordModded.graphicPathModded == headGraphicIndex)
                {
                    head = headGraphicRecordModded.GetGraphicBlank(color);
                    return;
                }
            }

            Log.Message("Tried to get pawn head at path " + headGraphicIndex + " that was not found. Defaulting...");

            head = headsVanillaCustom.First().GetGraphic(color);
        }


        public void DefineFace()
        {
            Pawn pawn = parent as Pawn;

            if (pawn == null)
                return;

            if (pawn.story.HeadGraphicPath.Contains("Normal"))
                type = "Normal";


            if (pawn.story.HeadGraphicPath.Contains("Pointy"))
                type = "Pointy";

            if (pawn.story.HeadGraphicPath.Contains("Wide"))
                type = "Wide";


            EyeDef = PawnFaceChooser.RandomEyeDefFor(pawn, pawn.Faction.def);

            BrowDef = PawnFaceChooser.RandomBrowDefFor(pawn, pawn.Faction.def);

            WrinkleDef = PawnFaceChooser.AssignWrinkleDefFor(pawn, pawn.Faction.def);

            MouthDef = PawnFaceChooser.RandomMouthDefFor(pawn, pawn.Faction.def);


            BeardDef = PawnFaceChooser.RandomBeardDefFor(pawn, pawn.Faction.def);


            HairColorOrg = pawn.story.hairColor;


            optimized = true;
        }

        public void SetGraphics()
        {
            Pawn pawn = parent as Pawn;
            if (pawn == null)
                return;


            // Create the blank canvas texture
            if (texture_ == null)
            {
                texture_ = new Texture2D(128, 128);

                // Reset all pixels color to transparent
                Color32 resetColor = new Color32(255, 255, 255, 0);
                Color32[] resetColorArray = texture_.GetPixels32();

                for (int i = 0; i < resetColorArray.Length; i++)
                {
                    resetColorArray[i] = resetColor;
                }

                texture_.SetPixels32(resetColorArray);
                texture_.Apply();
            }



            _eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(EyeDef.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);
            _browGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(BrowDef.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);
            _lipGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(MouthDef.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);

            if (pawn.gender == Gender.Female && BeardDef == null)
                BeardDef = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");

            if (type == "Normal")
            {
                _beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(BeardDef.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
                _wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(WrinkleDef.texPathAverageNormal, ShaderDatabase.Cutout, Vector2.one, Color.black);
            }
            if (type == "Pointy")
            {
                _beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(BeardDef.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
                _wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(WrinkleDef.texPathAveragePointy, ShaderDatabase.Cutout, Vector2.one, Color.black);
            }
            if (type == "Wide")
            {
                _beardGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(BeardDef.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
                _wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(WrinkleDef.texPathAverageWide, ShaderDatabase.Cutout, Vector2.one, Color.black);
            }


        }

        public bool GenerateHeadGraphics(Color baseColor, Graphic hairGraphic)
        {
            Pawn pawn = parent as Pawn;



            _temptexturefront = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            _temptextureside = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            _temptextureback = new Texture2D(1, 1, TextureFormat.ARGB32, false);


            // grab the blank texture instead of Vanilla
            bool oldAge = pawn.ageTracker.AgeBiologicalYearsFloat >= 50;


            Texture2D canvasHeadFront = new Texture2D(128, 128);
            Texture2D canvasHeadSide = new Texture2D(128, 128);
            Texture2D canvasHeadBack = new Texture2D(128, 128);

            Graphics.CopyTexture(texture_, canvasHeadFront);
            Graphics.CopyTexture(texture_, canvasHeadSide);
            Graphics.CopyTexture(texture_, canvasHeadBack);



            Log.Message("FacialStuff: Textures initialized");

            //  if (pawn.story.crownType == CrownType.Narrow)
            //  {
            //      eyeGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.EyeDef.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.white);
            //      browGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.BrowDef.texPathNarrow, ShaderDatabase.Cutout, Vector2.one, Color.white);
            //
            //      if (oldAge)
            //      {
            //          if (pawnSave.type == "Normal")
            //              wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowNormal, ShaderDatabase.Cutout, Vector2.one, Color.white);
            //          if (pawnSave.type == "Pointy")
            //              wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowPointy, ShaderDatabase.Cutout, Vector2.one, Color.white);
            //          if (pawnSave.type == "Wide")
            //              wrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(pawnSave.WrinkleDef.texPathNarrowWide, ShaderDatabase.Cutout, Vector2.one, Color.white);
            //      }
            //  }
            //  else
            //  {

            //   }



            // if (_textures.Contains(eyeGraphic.MatFront.mainTexture as Texture2D))
            // {
            //     _textures[1]
            // }
            // _textures.Add(eyeGraphic.MatFront.mainTexture as Texture2D);

            // Brows

            if (pawn.story.crownType == CrownType.Narrow)
            {
                ScaleTexture(_browGraphic.MatFront.mainTexture as Texture2D, ref _temptexturefront, 102, 128);
                ScaleTexture(_browGraphic.MatSide.mainTexture as Texture2D, ref _temptextureside, 102, 128);
            }
            else
            {
                _temptexturefront = MakeReadable(_browGraphic.MatFront.mainTexture as Texture2D);
                _temptextureside = MakeReadable(_browGraphic.MatSide.mainTexture as Texture2D);
            }
            MergeTwoGraphics(ref canvasHeadFront, _temptexturefront, pawn.story.hairColor * new Color(0.3f, 0.3f, 0.3f));
            MergeTwoGraphics(ref canvasHeadSide, _temptextureside, pawn.story.hairColor * new Color(0.3f, 0.3f, 0.3f));

            Log.Message("FacialStuff: Brows merged");

            // Eyes

            if (pawn.story.crownType == CrownType.Narrow)
            {
                ScaleTexture(_eyeGraphic.MatFront.mainTexture as Texture2D, ref _temptexturefront, 102, 128);
                ScaleTexture(_eyeGraphic.MatSide.mainTexture as Texture2D, ref _temptextureside, 102, 128);
            }
            else
            {
                _temptexturefront = MakeReadable(_eyeGraphic.MatFront.mainTexture as Texture2D);
                _temptextureside = MakeReadable(_eyeGraphic.MatSide.mainTexture as Texture2D);
            }
            MergeTwoGraphics(ref canvasHeadFront, _temptexturefront, Color.black);
            MergeTwoGraphics(ref canvasHeadSide, _temptextureside, Color.black);

            #region Male
            if (pawn.gender == Gender.Male)
            {

                if (oldAge)
                {
                    _temptexturefront = MakeReadable(_wrinkleGraphic.MatFront.mainTexture as Texture2D);
                    _temptextureside = MakeReadable(_wrinkleGraphic.MatSide.mainTexture as Texture2D);

                    MakeOld(pawn, ref canvasHeadFront, _temptexturefront);
                    MakeOld(pawn, ref canvasHeadSide, _temptextureside);
                }


                if (FS_Settings.UseMouth)
                {
                    if (BeardDef.drawMouth && drawMouth)
                    {

                        Graphic mouthGraphic = GraphicDatabase.Get<Graphic_Multi_HeadParts>(MouthDef.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);
                        if (pawn.story.crownType == CrownType.Narrow)
                        {
                            ScaleTexture(mouthGraphic.MatFront.mainTexture as Texture2D, ref _temptexturefront, 102, 128);
                            ScaleTexture(mouthGraphic.MatSide.mainTexture as Texture2D, ref _temptextureside, 102, 128);
                        }
                        else
                        {
                            _temptexturefront = MakeReadable(mouthGraphic.MatFront.mainTexture as Texture2D);
                            _temptextureside = MakeReadable(mouthGraphic.MatSide.mainTexture as Texture2D);
                        }
                        MergeTwoGraphics(ref canvasHeadFront, _temptexturefront, Color.black);
                        MergeTwoGraphics(ref canvasHeadSide, _temptextureside, Color.black);
                    }
                }


                if (pawn.story.crownType == CrownType.Narrow)
                {
                    ScaleTexture(_beardGraphic.MatFront.mainTexture as Texture2D, ref _temptexturefront, 102, 128);
                    ScaleTexture(_beardGraphic.MatSide.mainTexture as Texture2D, ref _temptextureside, 102, 128);
                }
                else
                {
                    _temptexturefront = MakeReadable(_beardGraphic.MatFront.mainTexture as Texture2D);
                    _temptextureside = MakeReadable(_beardGraphic.MatSide.mainTexture as Texture2D);
                }

                AddFacialHair(pawn, ref canvasHeadFront, _temptexturefront);
                AddFacialHair(pawn, ref canvasHeadSide, _temptextureside);

            }

            #endregion

            #region Female
            if (pawn.gender == Gender.Female)
            {

                if (oldAge)
                {
                    _temptexturefront = MakeReadable(_wrinkleGraphic.MatFront.mainTexture as Texture2D);
                    _temptextureside = MakeReadable(_wrinkleGraphic.MatSide.mainTexture as Texture2D);

                    MakeOld(pawn, ref canvasHeadFront, _temptexturefront);
                    MakeOld(pawn, ref canvasHeadSide, _temptextureside);

                }

                if (FS_Settings.UseMouth)
                {
                    if (drawMouth)
                    {
                        if (pawn.story.crownType == CrownType.Narrow)
                        {
                            ScaleTexture(_lipGraphic.MatFront.mainTexture as Texture2D, ref _temptexturefront, 102, 128);
                            ScaleTexture(_lipGraphic.MatSide.mainTexture as Texture2D, ref _temptextureside, 102, 128);
                        }
                        else
                        {
                            _temptexturefront = MakeReadable(_lipGraphic.MatFront.mainTexture as Texture2D);
                            _temptextureside = MakeReadable(_lipGraphic.MatSide.mainTexture as Texture2D);
                        }
                        MergeTwoGraphics(ref canvasHeadFront, _temptexturefront, Color.black);
                        MergeTwoGraphics(ref canvasHeadSide, _temptextureside, Color.black);
                    }
                }
            }
            #endregion


            if (pawn.story.crownType == CrownType.Narrow)
            {
                ScaleTexture(MakeReadable(hairGraphic.MatFront.mainTexture as Texture2D), ref _temptexturefront, 112, 128);
                ScaleTexture(MakeReadable(hairGraphic.MatSide.mainTexture as Texture2D), ref _temptextureside, 112, 128);
                ScaleTexture(MakeReadable(hairGraphic.MatBack.mainTexture as Texture2D), ref _temptextureback, 112, 128);
            }
            else
            {
                _temptexturefront = MakeReadable(hairGraphic.MatFront.mainTexture as Texture2D);
                _temptextureside = MakeReadable(hairGraphic.MatSide.mainTexture as Texture2D);
                _temptextureback = MakeReadable(hairGraphic.MatBack.mainTexture as Texture2D);
            }

            //    MergeColor(ref canvasHeadBack, pawn.story.SkinColor);
            if (pawn.story.crownType == CrownType.Narrow)
            {
                MergeTwoLayers(ref canvasHeadFront, _temptexturefront, MaskTextures.MaskTex_Narrow_FrontBack, pawn.story.hairColor);
                MergeTwoLayers(ref canvasHeadSide, _temptextureside, MaskTextures.MaskTex_Narrow_Side, pawn.story.hairColor);
                MergeTwoLayers(ref canvasHeadBack, _temptextureback, MaskTextures.MaskTex_Narrow_FrontBack, pawn.story.hairColor);
            }
            else
            {
                MergeTwoLayers(ref canvasHeadFront, _temptexturefront, MaskTextures.MaskTex_Average_FrontBack, pawn.story.hairColor);
                MergeTwoLayers(ref canvasHeadSide, _temptextureside, MaskTextures.MaskTex_Average_Side, pawn.story.hairColor);
                MergeTwoLayers(ref canvasHeadBack, _temptextureback, MaskTextures.MaskTex_Average_FrontBack, pawn.story.hairColor);
            }

            Graphic_Multi headGraphicVanilla;
            Graphic_Multi dissicatedHeadGraphicVanilla;
            GetModdedHeadNamed(true, pawn.story.SkinColor, out headGraphicVanilla);
            GetModdedHeadNamed(true, pawn.story.SkinColor * skinRottingMultiplyColor, out dissicatedHeadGraphicVanilla);

            Texture2D finalHeadFront = MakeReadable(headGraphicVanilla.MatFront.mainTexture as Texture2D);
            Texture2D finalHeadSide = MakeReadable(headGraphicVanilla.MatSide.mainTexture as Texture2D);
            Texture2D finalHeadBack = MakeReadable(headGraphicVanilla.MatBack.mainTexture as Texture2D);

            Texture2D disHeadFront = MakeReadable(dissicatedHeadGraphicVanilla.MatFront.mainTexture as Texture2D);
            Texture2D disHeadSide = MakeReadable(dissicatedHeadGraphicVanilla.MatSide.mainTexture as Texture2D);
            Texture2D disHeadBack = MakeReadable(dissicatedHeadGraphicVanilla.MatBack.mainTexture as Texture2D);


            //   PaintHeadWithColor(finalHeadFront, baseColor);
            //   PaintHeadWithColor(finalHeadSide, baseColor);
            //   PaintHeadWithColor(finalHeadBack, baseColor);
            //
            //   PaintHeadWithColor(disHeadFront, baseColor * PawnGraphicSet.RottingColor);
            //   PaintHeadWithColor(disHeadSide, baseColor * PawnGraphicSet.RottingColor);
            //   PaintHeadWithColor(disHeadBack, baseColor * PawnGraphicSet.RottingColor);


            if (pawn.story.crownType == CrownType.Narrow)
            {
                MergeTwoLayers(ref finalHeadFront, canvasHeadFront, MaskTextures.MaskTex_Narrow_FrontBack, pawn.story.hairColor);
                MergeTwoLayers(ref finalHeadSide, canvasHeadSide, MaskTextures.MaskTex_Narrow_Side, pawn.story.hairColor);
                MergeTwoLayers(ref finalHeadBack, canvasHeadBack, MaskTextures.MaskTex_Narrow_FrontBack, pawn.story.hairColor);

                MergeTwoLayers(ref disHeadFront, canvasHeadFront, MaskTextures.MaskTex_Narrow_FrontBack, pawn.story.hairColor);
                MergeTwoLayers(ref disHeadSide, canvasHeadSide, MaskTextures.MaskTex_Narrow_Side, pawn.story.hairColor);
                MergeTwoLayers(ref disHeadBack, canvasHeadBack, MaskTextures.MaskTex_Narrow_FrontBack, pawn.story.hairColor);
            }
            else
            {
                MergeTwoLayers(ref finalHeadFront, canvasHeadFront, MaskTextures.MaskTex_Average_FrontBack, pawn.story.hairColor);
                MergeTwoLayers(ref finalHeadSide, canvasHeadSide, MaskTextures.MaskTex_Average_Side, pawn.story.hairColor);
                MergeTwoLayers(ref finalHeadBack, canvasHeadBack, MaskTextures.MaskTex_Average_FrontBack, pawn.story.hairColor);

                MergeTwoLayers(ref disHeadFront, canvasHeadFront, MaskTextures.MaskTex_Average_FrontBack, pawn.story.hairColor);
                MergeTwoLayers(ref disHeadSide, canvasHeadSide, MaskTextures.MaskTex_Average_Side, pawn.story.hairColor);
                MergeTwoLayers(ref disHeadBack, canvasHeadBack, MaskTextures.MaskTex_Average_FrontBack, pawn.story.hairColor);
            }

            if (false)
            {
                byte[] bytes = canvasHeadFront.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/MergedHeads/" + pawn.Name + "_01front.png", bytes);
                byte[] bytes2 = canvasHeadSide.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/MergedHeads/" + pawn.Name + "_02side.png", bytes2);
                byte[] bytes3 = canvasHeadBack.EncodeToPNG();
                File.WriteAllBytes("Mods/RW_FacialStuff/MergedHeads/" + pawn.Name + "_03back.png", bytes3);
            }

            finalHeadFront.Compress(true);
            finalHeadSide.Compress(true);
            finalHeadBack.Compress(true);

            disHeadFront.Compress(true);
            disHeadSide.Compress(true);
            disHeadBack.Compress(true);

            finalHeadFront.mipMapBias = 0.5f;
            finalHeadSide.mipMapBias = 0.5f;
            finalHeadBack.mipMapBias = 0.5f;

            disHeadFront.mipMapBias = 0.5f;
            disHeadSide.mipMapBias = 0.5f;
            disHeadBack.mipMapBias = 0.5f;

            finalHeadFront.Apply(false, true);
            finalHeadSide.Apply(false, true);
            finalHeadBack.Apply(false, true);

            disHeadFront.Apply(false, true);
            disHeadSide.Apply(false, true);
            disHeadBack.Apply(false, true);

            GetModdedHeadNamed(false, pawn.story.SkinColor, out HeadGraphic);
            GetModdedHeadNamed(false, pawn.story.SkinColor * skinRottingMultiplyColor, out DissicatedHeadGraphic);

            HeadGraphic.MatFront.mainTexture = finalHeadFront;
            HeadGraphic.MatSide.mainTexture = finalHeadSide;
            HeadGraphic.MatBack.mainTexture = finalHeadBack;

            DissicatedHeadGraphic.MatFront.mainTexture = disHeadFront;
            DissicatedHeadGraphic.MatSide.mainTexture = disHeadSide;
            DissicatedHeadGraphic.MatBack.mainTexture = disHeadBack;

            //Object.DestroyImmediate(finalHeadFront, true);
            //Object.DestroyImmediate(finalHeadSide, true);
            //Object.DestroyImmediate(finalHeadBack, true);
            //
            //Object.DestroyImmediate(disHeadFront, true);
            //Object.DestroyImmediate(disHeadSide, true);
            //Object.DestroyImmediate(disHeadBack, true);



            sessionOptimized = true;
            return sessionOptimized;

            //    moddedHeadGraphics.Add(new KeyValuePair<string, Graphic_Multi>(pawn + color.ToString(), headGraphic));

        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref EyeDef, "EyeDef");
            Scribe_Defs.Look(ref BrowDef, "BrowDef");
            Scribe_Defs.Look(ref MouthDef, "MouthDef");
            Scribe_Defs.Look(ref WrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref BeardDef, "BeardDef");
            Scribe_Values.Look(ref optimized, "optimized");
            Scribe_Values.Look(ref drawMouth, "drawMouth");

            Scribe_Values.Look(ref headGraphicIndex, "headGraphicIndex");
            Scribe_Values.Look(ref type, "type");
            Scribe_Values.Look(ref SkinColorHex, "SkinColorHex");
            Scribe_Values.Look(ref HairColorOrg, "HairColorOrg");
        }
    }
}
