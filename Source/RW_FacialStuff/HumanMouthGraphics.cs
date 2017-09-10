namespace FacialStuff
{
    using FacialStuff.Defs;
    using FacialStuff.Graphics_FS;

    using JetBrains.Annotations;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public class HumanMouthGraphics
    {
        #region Public Fields

        public MouthGraphicData[] HumanMouthGraphic;

        #endregion Public Fields

        #region Public Constructors

        public HumanMouthGraphics([NotNull] Pawn pawn)
        {
            Color color = Color.white;
            Graphic_Multi_NaturalHeadParts mouthGraphic01 =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Mood01.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic02 =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Mood02.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts MouthGraphic03 =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Mood03.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic04 =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Mood04.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic05 =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Mood05.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic06 =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Mood06.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            color = Color.Lerp(Color.white, new Color(0.96f, 0.89f, 0.75f), Rand.Value);

            Graphic_Multi_NaturalHeadParts mouthGraphicGrin =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Grin.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            if (pawn.mindState?.mentalBreaker != null)
            {
                float minor = pawn.mindState.mentalBreaker.BreakThresholdMinor;
                float major = pawn.mindState.mentalBreaker.BreakThresholdMajor;
                float extreme = pawn.mindState.mentalBreaker.BreakThresholdExtreme;

                float forth = (1f - minor) / 4;

                this.HumanMouthGraphic = new[]
                                             {
                                                 new MouthGraphicData(0f, mouthGraphic06),
                                                 new MouthGraphicData(extreme, mouthGraphic05),
                                                 new MouthGraphicData(major, mouthGraphic04),
                                                 new MouthGraphicData(minor, MouthGraphic03),
                                                 new MouthGraphicData(minor + forth, mouthGraphic02),
                                                 new MouthGraphicData(minor + 2 * forth, mouthGraphic01),
                                                 new MouthGraphicData(minor + 3 * forth, mouthGraphicGrin),
                                             };
            }
            else
            {
                this.HumanMouthGraphic = new[]
                                             {
                                                 new MouthGraphicData(0f, mouthGraphic06),
                                                 new MouthGraphicData(0.25f, mouthGraphic05),
                                                 new MouthGraphicData(0.4f, mouthGraphic04),
                                                 new MouthGraphicData(0.55f, MouthGraphic03),
                                                 new MouthGraphicData(0.7f, mouthGraphic02),
                                                 new MouthGraphicData(0.8f, mouthGraphic01),
                                                 new MouthGraphicData(0.9f, mouthGraphicGrin)
                };
            }
        }

        #endregion Public Constructors

        #region Public Methods

        public int GetMouthTextureIndexOfMood(float mood)
        {
            int result = 0;
            for (int i = 0; i < this.HumanMouthGraphic.Length; i++)
            {
                if (mood < this.HumanMouthGraphic[i].Mood)
                {
                    break;
                }

                result = i;
            }

            return result;
        }

        #endregion Public Methods

        #region Public Structs

        public struct MouthGraphicData
        {
            #region Public Fields

            public readonly Graphic_Multi_NaturalHeadParts Graphic;

            public readonly float Mood;

            #endregion Public Fields

            #region Public Constructors

            public MouthGraphicData(float mood, Graphic_Multi_NaturalHeadParts graphic)
            {
                this.Mood = mood;
                this.Graphic = graphic;
            }

            #endregion Public Constructors
        }

        #endregion Public Structs
    }
}