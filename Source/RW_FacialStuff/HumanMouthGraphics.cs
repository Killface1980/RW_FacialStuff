namespace FacialStuff
{
    using FacialStuff.Defs;
    using FacialStuff.Graphics_FS;

    using UnityEngine;

    using Verse;

    using Debug = System.Diagnostics.Debug;

    [StaticConstructorOnStartup]
    public static class HumanMouthGraphics
    {

        [JetBrains.Annotations.NotNull]
        public static readonly Graphic_Multi_NaturalHeadParts MouthGraphic03;

        [JetBrains.Annotations.NotNull]
        public static readonly MouthGraphicData[] HumanMouthGraphic;

        static HumanMouthGraphics()
        {
            Graphic_Multi_NaturalHeadParts mouthGraphic01 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                MouthDefOf.Mouth_Mood01.texPath,
                                                                ShaderDatabase.Transparent,
                                                                Vector2.one,
                                                                Color.black) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic02 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                MouthDefOf.Mouth_Mood02.texPath,
                                                                ShaderDatabase.Transparent,
                                                                Vector2.one,
                                                                Color.black) as Graphic_Multi_NaturalHeadParts;

            MouthGraphic03 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                    MouthDefOf.Mouth_Mood03.texPath,
                                                    ShaderDatabase.Transparent,
                                                    Vector2.one,
                                                    Color.black) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic04 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                MouthDefOf.Mouth_Mood04.texPath,
                                                                ShaderDatabase.Transparent,
                                                                Vector2.one,
                                                                Color.black) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic05 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                MouthDefOf.Mouth_Mood05.texPath,
                                                                ShaderDatabase.Transparent,
                                                                Vector2.one,
                                                                Color.black) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic06 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                MouthDefOf.Mouth_Mood06.texPath,
                                                                ShaderDatabase.Transparent,
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

        #region Nested type: MouthGraphicData

        public struct MouthGraphicData
        {
            #region Fields

            [JetBrains.Annotations.NotNull]
            public readonly Graphic_Multi_NaturalHeadParts Graphic;

            public readonly float Mood;

            #endregion Fields

            #region Constructors

            public MouthGraphicData(float mood, [JetBrains.Annotations.NotNull] Graphic_Multi_NaturalHeadParts graphic)
            {
                this.Mood = mood;
                this.Graphic = graphic;
            }

            #endregion Constructors
        }

        #endregion Nested type: MouthGraphicData
    }
}