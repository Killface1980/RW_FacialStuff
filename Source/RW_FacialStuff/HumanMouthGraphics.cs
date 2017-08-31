namespace FacialStuff
{
    using FacialStuff.Defs;
    using FacialStuff.Graphics_FS;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public static class HumanMouthGraphics
    {
        #region Public Fields

        public static readonly MouthGraphicData[] HumanMouthGraphic;
        public static readonly Graphic_Multi_NaturalHeadParts MouthGraphic03;

        #endregion Public Fields

        #region Public Constructors

        static HumanMouthGraphics()
        {
            Graphic_Multi_NaturalHeadParts mouthGraphic01 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                MouthDefOf.Mouth_Mood01.texPath,
                                                                ShaderDatabase.CutoutSkin,
                                                                Vector2.one,
                                                                Color.black) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic02 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                MouthDefOf.Mouth_Mood02.texPath,
                                                                ShaderDatabase.CutoutSkin,
                                                                Vector2.one,
                                                                Color.black) as Graphic_Multi_NaturalHeadParts;

            MouthGraphic03 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                    MouthDefOf.Mouth_Mood03.texPath,
                                                    ShaderDatabase.CutoutSkin,
                                                    Vector2.one,
                                                    Color.black) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic04 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                MouthDefOf.Mouth_Mood04.texPath,
                                                                ShaderDatabase.CutoutSkin,
                                                                Vector2.one,
                                                                Color.black) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic05 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                MouthDefOf.Mouth_Mood05.texPath,
                                                                ShaderDatabase.CutoutSkin,
                                                                Vector2.one,
                                                                Color.black) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic06 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                MouthDefOf.Mouth_Mood06.texPath,
                                                                ShaderDatabase.CutoutSkin,
                                                                Vector2.one,
                                                                Color.black) as Graphic_Multi_NaturalHeadParts;

            HumanMouthGraphic = new[]
            {
                new MouthGraphicData(0f, mouthGraphic06),
                new MouthGraphicData(0.25f, mouthGraphic05),
                new MouthGraphicData(0.4f, mouthGraphic04),
                new MouthGraphicData(0.55f, MouthGraphic03),
                new MouthGraphicData(0.7f, mouthGraphic02),
                new MouthGraphicData(0.85f, mouthGraphic01)
            };
        }

        #endregion Public Constructors

        #region Public Methods

        public static int GetMouthTextureIndexOfMood(float mood)
        {
            int result = 0;
            for (int i = 0; i < HumanMouthGraphic.Length; i++)
            {
                if (mood < HumanMouthGraphic[i].Mood)
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