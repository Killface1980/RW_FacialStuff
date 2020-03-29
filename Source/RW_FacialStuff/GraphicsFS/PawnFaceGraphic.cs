// ReSharper disable StyleCop.SA1401

using System;
using FacialStuff.Animator;
using FacialStuff.DefOfs;
using FacialStuff.Defs;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
    public class PawnFaceGraphic
    {
        const string STR_south      = "_south";
        const string STR_ROMV_Fangs = "ROMV_Fangs";

        public Graphic BrowGraphic;

        public Graphic DeadEyeGraphic;

        public Graphic_Multi_NaturalEyes EyeLeftClosedGraphic;

        public Graphic_Multi_NaturalEyes EyeLeftGraphic;
        public Graphic_Multi_NaturalEars EarLeftGraphic;

        public Graphic_Multi_AddedHeadParts EyeLeftPatchGraphic;

        public Graphic_Multi_NaturalEyes EyeRightClosedGraphic;

        public Graphic_Multi_NaturalEars EarLeftPatchGraphic;


        public Graphic_Multi_NaturalEyes EyeRightGraphic;
        public Graphic_Multi_NaturalEars EarRightGraphic;

        public Graphic_Multi_AddedHeadParts EyeRightPatchGraphic;
        public Graphic_Multi_AddedHeadParts EarRightPatchGraphic;

        public Graphic_Multi_NaturalHeadParts JawGraphic;

        public Graphic MainBeardGraphic;

        public Graphic MoustacheGraphic;

        public HumanMouthGraphics Mouthgraphic;

        public Graphic_Multi_NaturalHeadParts MouthGraphic;

        public Graphic RottingWrinkleGraphic;

        public string TexPathBrow;

        [NotNull] public string TexPathEyeLeftClosed;

        public string TexPathEyeRightClosed;

        public Graphic WrinkleGraphic;

        [NotNull] private readonly CompFace _compFace;

        [NotNull] private readonly Pawn _pawn;

        private          float    _mood = 0.5f;
        private readonly PawnFace pawnFace;
        public Graphic_Multi_NaturalEyes EyeLeftMissingGraphic;
        public Graphic_Multi_NaturalEyes EyeRightMissingGraphic;

        public Graphic_Multi_NaturalEars EarLeftMissingGraphic;
        public Graphic_Multi_NaturalEars EarRightMissingGraphic;

        public PawnFaceGraphic(CompFace compFace)
        {
            this._compFace = compFace;
            this._pawn     = compFace.Pawn;

            this.pawnFace = this._compFace.PawnFace;
            if (this.pawnFace != null)
            {
                if (this._compFace.Props.hasBeard)
                {
                    this.InitializeGraphicsBeard();
                }

                if (this._compFace.Props.hasWrinkles)
                {
                    this.InitializeGraphicsWrinkles();
                }

                this.MakeEyes();
                this.MakeEars();
            }

            if (this._compFace.Props.hasMouth)
            {
                this.Mouthgraphic = new HumanMouthGraphics(this._pawn);
                this.InitializeGraphicsMouth();
            }
        }

        public void MakeEyes()
        {
            if (!this._compFace.Props.hasEyes)
            {
                return;
            }

            this._compFace.TexPathEyeRight = this._compFace.EyeTexPath(Side.Right);
            this._compFace.TexPathEyeLeft = this._compFace.EyeTexPath(Side.Left);
            this._compFace.TexPathEyeRightMissing = this._compFace.EyeTexPath(Side.Right, EyeDefOf.Missing);
            this._compFace.TexPathEyeLeftMissing = this._compFace.EyeTexPath(Side.Left, EyeDefOf.Missing);
            this.TexPathEyeLeftClosed  = this._compFace.EyeTexPath(Side.Left,  EyeDefOf.Closed);
            this.TexPathEyeRightClosed = this._compFace.EyeTexPath(Side.Right, EyeDefOf.Closed);
            this.TexPathBrow           = this._compFace.BrowTexPath(this.pawnFace.BrowDef);


            this.InitializeGraphicsEyes();
            this.InitializeGraphicsBrows();
        }
        public void MakeEars()
        {
            if (!this._compFace.Props.hasEars)
            {
                return;
            }

            this._compFace.TexPathEarRight = this._compFace.EarTexPath(Side.Right);
            this._compFace.TexPathEarLeft = this._compFace.EarTexPath(Side.Left);
            this._compFace.TexPathEarRightMissing = this._compFace.EarTexPath(Side.Right, EarDefOf.Missing);
            this._compFace.TexPathEarLeftMissing = this._compFace.EarTexPath(Side.Left, EarDefOf.Missing);


            this.InitializeGraphicsEars();
        }

        public void SetMouthAccordingToMoodLevel()
        {
            if (this._pawn == null)
            {
                return;
            }

            if (!Controller.settings.UseMouth || this._compFace.BodyStat.Jaw != PartStatus.Natural)
            {
                return;
            }

            if (this._pawn.Fleeing() || this._pawn.IsBurning())
            {
                this.MouthGraphic = this.Mouthgraphic.MouthGraphicCrying;
                return;
            }

            if (this._pawn.health.InPainShock && !this._compFace.IsAsleep)
            {
                PawnEyeWiggler eyeWiggler = this._compFace.EyeWiggler;
                if (eyeWiggler == null || eyeWiggler.EyeRightBlinkNow && eyeWiggler.EyeLeftBlinkNow)
                {
                    this.MouthGraphic = this.Mouthgraphic.MouthGraphicCrying;
                    return;
                }
            }

            if (this._pawn.needs?.mood?.thoughts != null)
            {
                this._mood = this._pawn.needs.mood.CurInstantLevel;
            }

            int indexOfMood = this.Mouthgraphic.GetMouthTextureIndexOfMood(this._mood);

            this.MouthGraphic = this.Mouthgraphic.HumanMouthGraphic[indexOfMood].Graphic;
        }

        private void InitializeGraphicsBeard()
        {
            if (this.pawnFace == null)
            {
                return;
            }

            string mainBeardDefTexPath = this._compFace.GetBeardPath();

            string moustacheDefTexPath = this._compFace.GetMoustachePath();

            Color beardColor = this.pawnFace.BeardColor;
            Color tacheColor = this.pawnFace.BeardColor;

            if (this.pawnFace.MoustacheDef == MoustacheDefOf.Shaved)
            {
                // no error, only use the beard def shaved as texture
                tacheColor = Color.white;
            }

            if (this.pawnFace.BeardDef == BeardDefOf.Beard_Shaved)
            {
                beardColor = Color.white;
            }

            if (Controller.settings.SameBeardColor)
            {
                beardColor = tacheColor = this._pawn.story.hairColor;
            }

            this.MoustacheGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                                        moustacheDefTexPath,
                                                                                        ShaderDatabase.Cutout,
                                                                                        Vector2.one,
                                                                                        tacheColor);

            this.MainBeardGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                                        mainBeardDefTexPath,
                                                                                        ShaderDatabase.Cutout,
                                                                                        Vector2.one,
                                                                                        beardColor);
        }

        private void InitializeGraphicsBrows()
        {
            Color color = this._pawn.story.hairColor;
            this.BrowGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(this.TexPathBrow,
                                                                                   ShaderDatabase.CutoutSkin,
                                                                                   Vector2.one,
                                                                                   color);
        }

        private void InitializeGraphicsEyePatches()
        {
            if (!this._compFace.TexPathEyeLeftPatch.NullOrEmpty())
            {
                bool leftTexExists = this.EyePatchLeftTexExists();
                if (leftTexExists)
                {
                    this.EyeLeftPatchGraphic =
                    GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(this._compFace.TexPathEyeLeftPatch,
                                                                      ShaderDatabase.Transparent,
                                                                      Vector2.one,
                                                                      Color.white) as Graphic_Multi_AddedHeadParts;
                    this._compFace.BodyStat.EyeLeft = PartStatus.Artificial;
                }
                else
                {
                    Log.Message(
                                "Facial Stuff: No texture for added part: " + this._compFace.TexPathEyeLeftPatch
                                                                            + " - Graphic_Multi_AddedHeadParts");
                }
            }

            if (!this._compFace.TexPathEyeRightPatch.NullOrEmpty())
            {
                bool rightTexExists = this.EyePatchRightTexExists();
                if (rightTexExists)
                {
                    this.EyeRightPatchGraphic =
                    GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(this._compFace.TexPathEyeRightPatch,
                                                                      ShaderDatabase.Transparent,
                                                                      Vector2.one,
                                                                      Color.white) as Graphic_Multi_AddedHeadParts;
                    this._compFace.BodyStat.EyeRight = PartStatus.Artificial;
                }
                else
                {
                    Log.Message("Facial Stuff: No texture for added part: " + this._compFace.TexPathEyeRightPatch
                                                                            + " - Graphic_Multi_AddedHeadParts");
                }
            }
        }

        public bool EyePatchRightTexExists()
        {
            return !ContentFinder<Texture2D>.Get(this._compFace.TexPathEyeRightPatch + STR_south, false)
                                            .NullOrBad();
        }

        public bool EyePatchLeftTexExists()
        {
            return !ContentFinder<Texture2D>.Get(this._compFace.TexPathEyeLeftPatch + STR_south, false)
                                            .NullOrBad();
        }
        public bool EarPatchRightTexExists()
        {
            return !ContentFinder<Texture2D>.Get(this._compFace.TexPathEarRightPatch + STR_south, false)
                                            .NullOrBad();
        }

        public bool EarPatchLeftTexExists()
        {
            return !ContentFinder<Texture2D>.Get(this._compFace.TexPathEarLeftPatch + STR_south, false)
                                            .NullOrBad();
        }

        private void InitializeGraphicsEyes()
        {
            this.InitializeGraphicsEyePatches();

            Color eyeColor = Color.white;

            this.EyeLeftGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(this._compFace.TexPathEyeLeft,
                                                                                 ShaderDatabase.CutoutComplex,
                                                                                 Vector2.one,
                                                                                 eyeColor) as Graphic_Multi_NaturalEyes;

            this.EyeRightGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(this._compFace.TexPathEyeRight,
                                                                                  ShaderDatabase.CutoutComplex,
                                                                                  Vector2.one,
                                                                                  eyeColor) as Graphic_Multi_NaturalEyes;

            this.EyeLeftMissingGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(this._compFace.TexPathEyeLeftMissing,
                                                                                 ShaderDatabase.CutoutComplex,
                                                                                 Vector2.one,
                                                                                 eyeColor) as Graphic_Multi_NaturalEyes;

            this.EyeRightMissingGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(this._compFace.TexPathEyeRightMissing,
                                                                                  ShaderDatabase.CutoutComplex,
                                                                                  Vector2.one,
                                                                                  eyeColor) as Graphic_Multi_NaturalEyes;

            this.EyeLeftClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(this.TexPathEyeLeftClosed,
                                                                                       ShaderDatabase.Cutout,
                                                                                       Vector2.one,
                                                                                       eyeColor) as Graphic_Multi_NaturalEyes;

            this.EyeRightClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(this.TexPathEyeRightClosed,
                                                                                        ShaderDatabase.Cutout,
                                                                                        Vector2.one,
                                                                                        eyeColor) as Graphic_Multi_NaturalEyes;

            this.DeadEyeGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                                      StringsFS.PathHumanlike + "Eyes/Eyes_Dead",
                                                                                      ShaderDatabase.Cutout,
                                                                                      Vector2.one,
                                                                                      Color.black);
        }
        private void InitializeGraphicsEars()
        {
            // this.InitializeGraphicsEyePatches();

            Color earColor =_pawn.story.SkinColor;

            this.EarLeftGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEars>(this._compFace.TexPathEarLeft,
                                                                                 ShaderDatabase.CutoutComplex,
                                                                                 Vector2.one,
                                                                                 earColor) as Graphic_Multi_NaturalEars;

            this.EarRightGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEars>(this._compFace.TexPathEarRight,
                                                                                  ShaderDatabase.CutoutComplex,
                                                                                  Vector2.one,
                                                                                  earColor) as Graphic_Multi_NaturalEars;
            if (false)
            {
                this.EarLeftMissingGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEars>(this._compFace.TexPathEarLeftMissing,
                    ShaderDatabase.CutoutComplex,
                    Vector2.one,
                    earColor) as Graphic_Multi_NaturalEars;

                this.EarRightMissingGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEars>(this._compFace.TexPathEarRightMissing,
                    ShaderDatabase.CutoutComplex,
                    Vector2.one,
                    earColor) as Graphic_Multi_NaturalEars;
            }
        }

        private void InitializeGraphicsMouth()
        {
            if (!this._compFace.TexPathJawAddedPart.NullOrEmpty())
            {
                bool flag = ContentFinder<Texture2D>.Get(this._compFace.TexPathJawAddedPart + STR_south, false) != null;
                if (flag)
                {
                    this.JawGraphic =
                    GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(this._compFace.TexPathJawAddedPart,
                                                                        ShaderDatabase.CutoutSkin,
                                                                        Vector2.one,
                                                                        Color.white) as Graphic_Multi_NaturalHeadParts;
                    this._compFace.BodyStat.Jaw = PartStatus.Artificial;
                    string addedPart = this._compFace.TexPathJawAddedPart;
                    if (addedPart != null && addedPart.Contains(STR_ROMV_Fangs))
                    {
                        this._compFace.BodyStat.Jaw = PartStatus.DisplayOverBeard;
                    }

                    // all done, return
                    return;
                }

                // texture for added/extra part not found, log and default
                Log.Message(
                            "Facial Stuff: No texture for added part: " + this._compFace.TexPathJawAddedPart
                                                                        + " - Graphic_Multi_NaturalHeadParts. This is not an error, just an info.");

            }

            this.MouthGraphic = this.Mouthgraphic.HumanMouthGraphic[this._pawn.Dead || this._pawn.Downed ? 2 : 3]
                                    .Graphic;
        }

        private void InitializeGraphicsWrinkles()
        {
            Color wrinkleColor = this._pawn.story.SkinColor * new Color(0.1f, 0.1f, 0.1f);

            {
                wrinkleColor.a = this.pawnFace.WrinkleIntensity;

                WrinkleDef pawnFaceWrinkleDef = this.pawnFace.WrinkleDef;

                this.WrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                                          pawnFaceWrinkleDef.texPath +
                                                                                          "_"                        +
                                                                                          this
                                                                                         ._compFace
                                                                                         .PawnCrownType + "_" +
                                                                                          this._compFace.PawnHeadType,
                                                                                          ShaderDatabase.Transparent,
                                                                                          Vector2.one,
                                                                                          wrinkleColor);

                this.RottingWrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                                                 pawnFaceWrinkleDef
                                                                                                .texPath + "_" +
                                                                                                 this._compFace
                                                                                                     .PawnCrownType +
                                                                                                 "_"                +
                                                                                                 this
                                                                                                ._compFace
                                                                                                .PawnHeadType,
                                                                                                 ShaderDatabase
                                                                                                .Transparent,
                                                                                                 Vector2.one,
                                                                                                 wrinkleColor *
                                                                                                 FaceTextures
                                                                                                .SkinRottingMultiplyColor);
            }
        }
    }
}