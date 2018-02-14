// ReSharper disable InconsistentNaming

using UnityEngine;
using Verse;

namespace FacialStuff
{
    public static class Offsets
    {
        //// total max with repetitions: LayerSpacing = 0.46875f;

        public const float YOffsetPawns = 0.05f;

        private const float SubInterval = 0.00390625f;
        public const float YOffset_PrimaryEquipmentUnder = 0f;
        public const float YOffset_Behind = 0.00390625f;
        public const float YOffset_Body = 0.0078125f;
        public const float YOffsetInterval_Clothes = 0.00390625f;
        public const float YOffset_HandsFeet = 0.005f;
        public const float YOffset_Wounds = 0.01953125f;
        public const float YOffset_Shell = 0.0234375f;

        public const float YOffset_Head = 0.02734375f;

        public const float YOffset_Wrinkles = 0.0005f;
        public const float YOffset_Eyes = 0.001f;
        public const float YOffset_Brows = 0.0015f;
        public const float YOffset_UnnaturalEyes = 0.002f;

        public const float YOffset_Beard = 0.003f;
        public const float YOffset_Tache = 0.0035f;

        public const float YOffset_Mouth = 0.004f;

        public const float YOffset_LeftPart = 0.0001f;
        public const float YOffset_RightPart = 0.0002f;

        public const float YOffset_OnHead = 0.03125f;
        public const float YOffset_HairOnHead = 0.0005f; // FS
        public const float YOffset_GearOnHead = 0.001f; // FS

        public const float YOffset_PostHead = 0.03515625f;
        public const float YOffset_HandsFeetOver = 0.03634375f; // FS
        public const float YOffset_CarriedThing = 0.0390625f;
        public const float YOffset_PrimaryEquipmentOver = 0.0390625f;
        public const float YOffset_Status = 0.04296875f;

        // Verse.Listing_Standard
        public static float Slider(this Listing_Standard listing, float value, float leftValue, float rightValue, bool middleAlignment = false, string label = null, string leftAlignedLabel = null, string rightAlignedLabel = null, float roundTo = -1f)
        {
            Rect rect = listing.GetRect(22f);
            float result = Widgets.HorizontalSlider(rect, value, leftValue, rightValue, middleAlignment, label, leftAlignedLabel, rightAlignedLabel, roundTo);
            listing.Gap(listing.verticalSpacing);
            return result;
        }
    }
}