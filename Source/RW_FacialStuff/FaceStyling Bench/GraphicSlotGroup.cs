namespace FacialStuff
{
    public class enums
    {
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
            /// The brows.
            /// </summary>
            Brows,

            /// <summary>
            /// The mouth.
            /// </summary>
            Mouth,

            /// <summary>
            /// The beard.
            /// </summary>
            Beard,

            Hair,

            Overhead,

            NumberOfTypes
        }

        public enum Side
        {
            Left = 0,

            Right = 1,

        }
    }
}
