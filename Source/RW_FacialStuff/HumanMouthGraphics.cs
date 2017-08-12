namespace FacialStuff
{
    using FacialStuff.Graphics_FS;

    public static class HumanMouthGraphics
    {
        public struct MouthGraphicData
        {
            #region Fields

            public Graphic_Multi_NaturalHeadParts graphic;
            public float mood;


            #endregion Fields

            #region Constructors

            public MouthGraphicData(float mood, Graphic_Multi_NaturalHeadParts graphic)
            {
                this.mood = mood;
                this.graphic = graphic;
            }

            #endregion Constructors
        }

        public static readonly MouthGraphicData[] HumanMouthGraphic =
            {
                new MouthGraphicData(
                    0f,
                    FacialGraphics.MouthGraphic06),
                new MouthGraphicData(
                    0.25f,
                    FacialGraphics.MouthGraphic05),
                new MouthGraphicData(
                    0.4f,
                    FacialGraphics.MouthGraphic04),
                new MouthGraphicData(
                    0.55f,
                    FacialGraphics.MouthGraphic03),
                new MouthGraphicData(
                    0.7f,
                    FacialGraphics.MouthGraphic02),
                new MouthGraphicData(
                    0.85f,
                    FacialGraphics.MouthGraphic01)
            };

        public static int GetMouthTextureIndexOfMood(float mood)
        {
            int result = 0;
            for (int i = 0; i < HumanMouthGraphic.Length; i++)
            {
                if (mood < HumanMouthGraphic[i].mood)
                {
                    break;
                }

                result = i;
            }

            return result;
        }
    }
}
