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
    }
}