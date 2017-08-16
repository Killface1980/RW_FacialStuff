namespace FacialStuff
{
    public static class Enums
    {
        #region GraphicSlotGroup enum

        public enum GraphicSlotGroup
        {
            Invalid = -1,

            Body,

            OnSkinOnLegs,

            OnSkin,

            Middle,

            Shell,

            Head,

            RightEye,

            LeftEye,

            /// <summary>
            ///     The brows.
            /// </summary>
            Brows,

            /// <summary>
            ///     The mouth.
            /// </summary>
            Mouth,

            /// <summary>
            ///     The beard.
            /// </summary>
            Beard,

            Moustache,

            Hair,

            Overhead,

            NumberOfTypes
        }

        #endregion GraphicSlotGroup enum

        #region Side enum

        public enum Side
        {
            Left = 0,

            Right = 1
        }

        #endregion Side enum
    }
}