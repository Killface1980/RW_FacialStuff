namespace FacialStuff.Graphics
{
    using FacialStuff.Defs;
    using FacialStuff.Enums;

    using JetBrains.Annotations;

    using UnityEngine;

    using Verse;

    public class PawnGraphic
    {

        #region Public Fields

        public Graphic BrowGraphic;

        public Graphic DeadEyeGraphic;

        public Graphic_Multi_NaturalEyes EyeLeftClosedGraphic;

        public Graphic_Multi_NaturalEyes EyeLeftGraphic;

        [CanBeNull]
        public Graphic_Multi_AddedHeadParts EyeLeftPatchGraphic;

        public Graphic_Multi_NaturalEyes EyeRightClosedGraphic;

        public Graphic_Multi_NaturalEyes EyeRightGraphic;

        [CanBeNull]
        public Graphic_Multi_AddedHeadParts EyeRightPatchGraphic;

        [CanBeNull]
        public Graphic HandGraphic;

        [CanBeNull]
        public Graphic_Multi_NaturalHeadParts JawGraphic;

        public Graphic MainBeardGraphic;

        [CanBeNull]
        public Graphic MoustacheGraphic;

        public HumanMouthGraphics mouthgraphic;
        public Graphic_Multi_NaturalHeadParts MouthGraphic;

        public Graphic RottingWrinkleGraphic;

        [CanBeNull]
        public string texPathBrow;

        [CanBeNull]
        public string texPathEyeLeft;

        [CanBeNull]
        public string texPathEyeLeftClosed;

        [CanBeNull]
        public string texPathEyeLeftPatch;

        [CanBeNull]
        public string texPathEyeRight;

        [CanBeNull]
        public string texPathEyeRightClosed;

        [CanBeNull]
        public string texPathEyeRightPatch;

        [CanBeNull]
        public string texPathJawAddedPart;

        public Graphic WrinkleGraphic;

        #endregion Public Fields

        #region Private Fields

        [NotNull]
        private readonly CompFace compFace;

        private float mood = 0.5f;

        private readonly Pawn pawn;

        public PawnGraphic(CompFace compFace)
        {
            this.compFace = compFace;
            this.pawn = compFace.Pawn;


            if (this.compFace.Props.hasWrinkles)
            {
                this.InitializeGraphicsWrinkles();
            }

            if (this.compFace.Props.hasBeard)
            {
                this.InitializeGraphicsBeard();
            }

            if (this.compFace.Props.hasEyes)
            {

                EyeDef pawnFaceEyeDef = this.compFace.PawnFace.EyeDef;
                this.texPathEyeRight = this.compFace.EyeTexPath(pawnFaceEyeDef.texPath, Side.Right);
                this.texPathEyeLeft = this.compFace.EyeTexPath(pawnFaceEyeDef.texPath, Side.Left);
                this.texPathEyeLeftClosed = this.compFace.EyeClosedTexPath(Side.Left);
                this.texPathEyeRightClosed = this.compFace.EyeClosedTexPath(Side.Right);

                this.InitializeGraphicsEyes();

                this.texPathBrow = this.compFace.BrowTexPath(this.compFace.PawnFace.BrowDef);
                this.InitializeGraphicsBrows();
            }
            if (this.compFace.Props.hasMouth)
            {
                this.mouthgraphic = new HumanMouthGraphics(this.pawn);
                this.InitializeGraphicsMouth();
            }

            if (this.compFace.Props.hasHands)
            {
                this.InitializeGraphicsHand();
            }
        }

        #endregion Private Fields

        #region Public Methods


        public void SetMouthAccordingToMoodLevel()
        {
            if (this.pawn == null)
            {
                return;
            }

            if (!Controller.settings.UseMouth || !this.compFace.hasNaturalJaw)
            {
                return;
            }

            if (this.pawn.health.InPainShock && !this.compFace.EyeWiggler.IsAsleep)
            {
                if (this.compFace.EyeWiggler.EyeRightBlinkNow && this.compFace.EyeWiggler.EyeLeftBlinkNow)
                {
                    this.MouthGraphic = this.mouthgraphic.mouthGraphicCrying;
                    return;
                }
            }

            if (this.pawn.needs?.mood?.thoughts != null)
            {
                this.mood = this.pawn.needs.mood.CurInstantLevel;
            }

            int mouthTextureIndexOfMood = this.mouthgraphic.GetMouthTextureIndexOfMood(this.mood);

            this.MouthGraphic = this.mouthgraphic.HumanMouthGraphic[mouthTextureIndexOfMood].Graphic;
        }

        #endregion Public Methods

        #region Private Methods

        private void InitializeGraphicsBeard()
        {
            PawnFace pawnFace = this.compFace.PawnFace;
            if (pawnFace != null)
            {
                string mainBeardDefTexPath = compFace.GetBeardPath(pawnFace.BeardDef);

                string moustacheDefTexPath = compFace.GetMoustachePath(pawnFace.MoustacheDef);

                Color beardColor = pawnFace.BeardColor;
                Color tacheColor = pawnFace.BeardColor;

                if (pawnFace.MoustacheDef == MoustacheDefOf.Shaved)
                {
                    // no error, only use the beard def shaved as texture
                    tacheColor = Color.white;
                }

                if (pawnFace.BeardDef == BeardDefOf.Beard_Shaved)
                {
                    beardColor = Color.white;
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
        }

        private void InitializeGraphicsBrows()
        {
            Color color = this.pawn.story.hairColor;
            this.BrowGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                this.texPathBrow,
                ShaderDatabase.CutoutSkin,
                Vector2.one,
                color);
        }

        private void InitializeGraphicsEyePatches()
        {
            if (!this.texPathEyeLeftPatch.NullOrEmpty())
            {
                bool flag = !ContentFinder<Texture2D>.Get(this.texPathEyeLeftPatch + "_front", false).NullOrBad();
                if (flag)
                {
                    this.EyeLeftPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                                                                   this.texPathEyeLeftPatch,
                                                                   ShaderDatabase.Transparent,
                                                                   Vector2.one,
                                                                   Color.white) as Graphic_Multi_AddedHeadParts;
                    this.compFace.HasEyePatchLeft = true;
                }
                else
                {
                    this.compFace.HasEyePatchLeft = false;
                    Log.Message(
                        "Facial Stuff: No texture for added part: " + this.texPathEyeLeftPatch
                        + " - Graphic_Multi_AddedHeadParts");
                }
            }
            else
            {
                this.compFace.HasEyePatchLeft = false;
            }

            if (!this.texPathEyeRightPatch.NullOrEmpty())
            {
                bool flag2 = !ContentFinder<Texture2D>.Get(this.texPathEyeRightPatch + "_front", false).NullOrBad();
                if (flag2)
                {
                    this.EyeRightPatchGraphic =
                        GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                            this.texPathEyeRightPatch,
                            ShaderDatabase.Transparent,
                            Vector2.one,
                            Color.white) as Graphic_Multi_AddedHeadParts;
                    this.compFace.HasEyePatchRight = true;
                }
                else
                {
                    Log.Message(
                        "Facial Stuff: No texture for added part: " + this.texPathEyeRightPatch
                        + " - Graphic_Multi_AddedHeadParts");
                    this.compFace.HasEyePatchRight = false;
                }
            }
            else
            {
                this.compFace.HasEyePatchRight = false;
            }
        }

        private void InitializeGraphicsEyes()
        {
            this.InitializeGraphicsEyePatches();

            this.EyeLeftGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                      this.texPathEyeLeft,
                                      ShaderDatabase.Cutout,
                                      Vector2.one,
                                      this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.EyeRightGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                       this.texPathEyeRight,
                                       ShaderDatabase.Cutout,
                                       Vector2.one,
                                       this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.EyeLeftClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                            this.texPathEyeLeftClosed,
                                            ShaderDatabase.Cutout,
                                            Vector2.one,
                                            this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.EyeRightClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                             this.texPathEyeRightClosed,
                                             ShaderDatabase.Cutout,
                                             Vector2.one,
                                             this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.DeadEyeGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                "Eyes/Eyes_Dead",
                ShaderDatabase.Cutout,
                Vector2.one,
                this.pawn.story.SkinColor);
        }

        private void InitializeGraphicsHand()
        {
            if (!this.compFace.Props.hasHands)
            {
                return;
            }
            string texNameHand = "Hands/" + this.compFace.Props.handType + "_Hand";

            this.HandGraphic = GraphicDatabase.Get<Graphic_Single>(
                texNameHand,
                ShaderDatabase.CutoutSkin,
                new Vector2(1f, 1f),
                this.pawn.story.SkinColor,
                this.pawn.story.SkinColor);
        }
        private void InitializeGraphicsMouth()
        {
            if (!this.texPathJawAddedPart.NullOrEmpty())
            {
                bool flag = ContentFinder<Texture2D>.Get(this.texPathJawAddedPart + "_front", false) != null;
                if (flag)
                {
                    this.JawGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                          this.texPathJawAddedPart,
                                                          ShaderDatabase.CutoutSkin,
                                                          Vector2.one,
                                                          Color.white) as Graphic_Multi_NaturalHeadParts;
                    this.compFace.hasNaturalJaw = false;

                    // all done, return
                    return;
                }

                // texture for added/extra part not found, log and default
                Log.Message(
                    "Facial Stuff: No texture for added part: " + this.texPathJawAddedPart
                    + " - Graphic_Multi_NaturalHeadParts. This is not an error, just an info.");
            }

            this.compFace.hasNaturalJaw = true;
            this.MouthGraphic = this.mouthgraphic
                .HumanMouthGraphic[this.pawn.Dead || this.pawn.Downed ? 2 : 3].Graphic;
        }

        private void InitializeGraphicsWrinkles()
        {
            Color wrinkleColor = this.pawn.story.SkinColor * new Color(0.1f, 0.1f, 0.1f);

            PawnFace pawnFace = this.compFace.PawnFace;
            {
                wrinkleColor.a = pawnFace.wrinkleIntensity;

                WrinkleDef pawnFaceWrinkleDef = pawnFace.WrinkleDef;

                this.WrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    pawnFaceWrinkleDef.texPath + "_" + this.compFace.PawnCrownType + "_" + this.compFace.PawnHeadType,
                    ShaderDatabase.Transparent,
                    Vector2.one,
                    wrinkleColor);

                this.RottingWrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    pawnFaceWrinkleDef.texPath + "_" + this.compFace.PawnCrownType + "_" + this.compFace.PawnHeadType,
                    ShaderDatabase.Transparent,
                    Vector2.one,
                    wrinkleColor * FaceTextures.SkinRottingMultiplyColor);
            }
        }

        #endregion Private Methods
    }
}