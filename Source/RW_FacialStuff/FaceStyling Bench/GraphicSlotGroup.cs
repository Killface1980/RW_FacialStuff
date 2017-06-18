namespace FaceStyling
{

    using Verse;

    public partial class Dialog_FaceStyling : Window
    {
        private enum GraphicSlotGroup
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
    }
}