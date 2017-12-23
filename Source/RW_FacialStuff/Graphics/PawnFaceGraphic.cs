namespace FacialStuff.Graphics
{
    using FacialStuff.Animator;
    using FacialStuff.Defs;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class PawnFaceGraphic
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
        public string texPathEyeLeftClosed;



        [CanBeNull]
        public string texPathEyeRightClosed;




        public Graphic WrinkleGraphic;

        #endregion Public Fields

        #region Private Fields

        [NotNull]
        private readonly CompFace compFace;

        private float mood = 0.5f;

        private readonly Pawn pawn;


        public PawnFaceGraphic(CompFace compFace)
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
                this.compFace.texPathEyeRight = this.compFace.EyeTexPath(pawnFaceEyeDef.texPath, Side.Right);
                this.compFace.texPathEyeLeft = this.compFace.EyeTexPath(pawnFaceEyeDef.texPath, Side.Left);
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


        }

        #endregion Private Fields

        #region Public Methods


        public void SetMouthAccordingToMoodLevel()
        {
            if (this.pawn == null)
            {
                return;
            }

            if (!Controller.settings.UseMouth || this.compFace.bodyStat.jaw != PartStatus.Natural)
            {
                return;
            }

            if (this.pawn.Fleeing() || this.pawn.IsBurning())
            {
                this.MouthGraphic = this.mouthgraphic.mouthGraphicCrying;
                return;
            }

            if (this.pawn.health.InPainShock && !this.compFace.IsAsleep)
            {
                PawnEyeWiggler eyeWiggler = this.compFace.EyeWiggler;
                if (eyeWiggler == null || (eyeWiggler.EyeRightBlinkNow && eyeWiggler.EyeLeftBlinkNow))
                {
                    this.MouthGraphic = this.mouthgraphic.mouthGraphicCrying;
                    return;
                }
            }

            if (this.pawn.needs?.mood?.thoughts != null)
            {
                this.mood = this.pawn.needs.mood.CurInstantLevel;
            }

            int indexOfMood = this.mouthgraphic.GetMouthTextureIndexOfMood(this.mood);

            this.MouthGraphic = this.mouthgraphic.HumanMouthGraphic[indexOfMood].Graphic;
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
            if (!this.compFace.texPathEyeLeftPatch.NullOrEmpty())
            {
                bool flag = !ContentFinder<Texture2D>.Get(this.compFace.texPathEyeLeftPatch + "_front", false).NullOrBad();
                if (flag)
                {
                    this.EyeLeftPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                                                   this.compFace.texPathEyeLeftPatch,
                                                                   ShaderDatabase.Transparent,
                                                                   Vector2.one,
                                                                   Color.white) as Graphic_Multi_AddedHeadParts;
                    this.compFace.bodyStat.eyeLeft = PartStatus.Artificial;
                }
                else
                {
                    Log.Message(
                        "Facial Stuff: No texture for added part: " + this.compFace.texPathEyeLeftPatch
                        + " - Graphic_Multi_AddedHeadParts");
                }
            }

            if (!this.compFace.texPathEyeRightPatch.NullOrEmpty())
            {
                bool flag2 = !ContentFinder<Texture2D>.Get(this.compFace.texPathEyeRightPatch + "_front", false).NullOrBad();
                if (flag2)
                {
                    this.EyeRightPatchGraphic =
                        GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
                            this.compFace.texPathEyeRightPatch,
                            ShaderDatabase.Transparent,
                            Vector2.one,
                            Color.white) as Graphic_Multi_AddedHeadParts;
                    this.compFace.bodyStat.eyeRight = PartStatus.Artificial;
                }
                else
                {
                    Log.Message(
                        "Facial Stuff: No texture for added part: " + this.compFace.texPathEyeRightPatch
                        + " - Graphic_Multi_AddedHeadParts");
                }
            }
        }

        private void InitializeGraphicsEyes()
        {
            this.InitializeGraphicsEyePatches();

            this.EyeLeftGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                      this.compFace.texPathEyeLeft,
                                      ShaderDatabase.Cutout,
                                      Vector2.one,
                                      this.pawn.story.SkinColor) as Graphic_Multi_NaturalEyes;

            this.EyeRightGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                                       this.compFace.texPathEyeRight,
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


        private void InitializeGraphicsMouth()
        {
            if (!this.compFace.texPathJawAddedPart.NullOrEmpty())
            {
                bool flag = ContentFinder<Texture2D>.Get(this.compFace.texPathJawAddedPart + "_front", false) != null;
                if (flag)
                {
                    this.JawGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                          this.compFace.texPathJawAddedPart,
                                                          ShaderDatabase.CutoutSkin,
                                                          Vector2.one,
                                                          Color.white) as Graphic_Multi_NaturalHeadParts;
                    this.compFace.bodyStat.jaw = PartStatus.Artificial;

                    // all done, return
                    return;
                }

                // texture for added/extra part not found, log and default
                Log.Message(
                    "Facial Stuff: No texture for added part: " + this.compFace.texPathJawAddedPart
                    + " - Graphic_Multi_NaturalHeadParts. This is not an error, just an info.");
            }

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