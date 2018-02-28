using UnityEngine;
using Verse;

namespace FacialStuff.Genetics
{
    public static class PawnSkinColors_FS
    {
        public static readonly SkinColorData[] SkinColors =
        {
            // Preference of lighter skin colors in favor of more hair color variety
            new SkinColorData(
                              0f,
                              0f,
                              new Color32(255, 233, 217, 255)),
            new SkinColorData(
                              0.05f,
                              0.1f,
                              new Color32(242, 196, 193, 255)),
            new SkinColorData(
                              0.15f,
                              0.25f,
                              new Color32(243, 210, 181, 255)),
            new SkinColorData(
                              0.35f,
                              0.45f,
                              new Color32(235, 184, 141, 255)),

            // new Color32(242, 190, 145, 255)),
            new SkinColorData(
                              0.5f,
                              0.6f,
                              new Color32(255, 235, 170, 255)),

            // new Color32(255, 231, 179, 255)),
            new SkinColorData(
                              0.7f,
                              0.8f,
                              new Color32(218, 136, 87, 255)),
            new SkinColorData(
                              0.8f,
                              0.85f,
                              new Color32(165, 93, 41, 255)),
            new SkinColorData(
                              0.85f,
                              0.9f,
                              new Color32(127, 71, 51, 255)),
            new SkinColorData(
                              0.95f,
                              0.95f,
                              new Color32(101, 56, 41, 255)),
            new SkinColorData(
                              1f,
                              1f,
                              new Color32(76, 36, 23, 255))

            // Vanilla

            // new SkinColorData(0f, 0f, new Color(0.9490196f, 0.929411769f, 0.8784314f)),
            // new SkinColorData(0.25f, 0.215f, new Color(1f, 0.9372549f, 0.8352941f)),
            // new SkinColorData(0.5f, 0.715f, new Color(1f, 0.9372549f, 0.7411765f)),
            // new SkinColorData(0.75f, 0.8f, new Color(0.894117653f, 0.619607866f, 0.3529412f)),
            // new SkinColorData(0.9f, 0.95f, new Color(0.509803951f, 0.356862754f, 0.1882353f)),
            // new SkinColorData(1f, 1f, new Color(0.3882353f, 0.274509817f, 0.141176477f))
        };

        // ReSharper disable once RedundantAssignment
        public static bool GetMelaninCommonalityFactor_Prefix(ref float __result, float melanin)
        {
            int skinDataLeftIndexByWhiteness = GetSkinDataIndexOfMelanin(melanin);
            if (skinDataLeftIndexByWhiteness == SkinColors.Length - 1)
            {
                __result = GetSkinCommonalityFactor(skinDataLeftIndexByWhiteness);
                return false;
            }

            float t = Mathf.InverseLerp(
                                        SkinColors[skinDataLeftIndexByWhiteness].Melanin,
                                        SkinColors[skinDataLeftIndexByWhiteness + 1].Melanin,
                                        melanin);
            __result = Mathf.Lerp(
                                  GetSkinCommonalityFactor(skinDataLeftIndexByWhiteness),
                                  GetSkinCommonalityFactor(skinDataLeftIndexByWhiteness + 1),
                                  t);
            return false;
        }

        // FS bench
        public static float GetRelativeLerpValue(float value)
        {
            int leftIndexForValue = GetSkinDataIndexOfMelanin(value);
            if (leftIndexForValue == SkinColors.Length - 1)
            {
                return 0f;
            }

            int num = leftIndexForValue + 1;
            return Mathf.InverseLerp(SkinColors[leftIndexForValue].Melanin, SkinColors[num].Melanin, value);
        }

        public static Color GetSkinColor(float melanin)
        {
            int skinDataIndexOfMelanin = GetSkinDataIndexOfMelanin(melanin);
            if (skinDataIndexOfMelanin == SkinColors.Length - 1)
            {
                return SkinColors[skinDataIndexOfMelanin].Color;
            }

            float t = Mathf.InverseLerp(
                                        SkinColors[skinDataIndexOfMelanin].Melanin,
                                        SkinColors[skinDataIndexOfMelanin + 1].Melanin,
                                        melanin);
            return Color.Lerp(
                              SkinColors[skinDataIndexOfMelanin].Color,
                              SkinColors[skinDataIndexOfMelanin + 1].Color,
                              t);
        }

        public static bool GetSkinColor_Prefix(ref Color __result, float melanin)
        {
            __result = GetSkinColor(melanin);
            return false;
        }

        public static int GetSkinDataIndexOfMelanin(float melanin)
        {
            int __result = 0;
            for (int i = 0; i < SkinColors.Length; i++)
            {
                if (melanin < SkinColors[i].Melanin)
                {
                    break;
                }

                __result = i;
            }

            return __result;
        }

        // ReSharper disable once InconsistentNaming
        public static bool GetSkinDataIndexOfMelanin_Prefix(ref int __result, float melanin)
        {
            __result = GetSkinDataIndexOfMelanin(melanin);
            return false;
        }

        public static float GetValueFromRelativeLerp(int leftIndex, float lerp)
        {
            if (leftIndex >= SkinColors.Length - 1)
            {
                return 1f;
            }

            if (leftIndex < 0)
            {
                return 0f;
            }

            int num = leftIndex + 1;
            return Mathf.Lerp(SkinColors[leftIndex].Melanin, SkinColors[num].Melanin, lerp);
        }

        public static bool IsDarkSkin_Prefix(ref bool __result, Color color)
        {
            Color skinColor = GetSkinColor(0.5f);
            __result        = color.r + color.g + color.b <= skinColor.r + skinColor.g + skinColor.b + 0.01f;
            return false;
        }

        public static bool RandomMelanin_Prefix(ref float __result)
        {
            float value = Rand.Value;
            int   num   = 0;
            for (int i = 0; i < SkinColors.Length; i++)
            {
                if (value < SkinColors[i].Selector)
                {
                    break;
                }

                num = i;
            }

            if (num == SkinColors.Length - 1)
            {
                __result = SkinColors[num].Melanin;
                return false;
            }

            float t  = Mathf.InverseLerp(SkinColors[num].Selector, SkinColors[num + 1].Selector, value);
            __result = Mathf.Lerp(SkinColors[num].Melanin, SkinColors[num         + 1].Melanin, t);
            return false;
        }

        private static float GetSkinCommonalityFactor(int skinDataIndex)
        {
            float num = 0f;
            for (int i = 0; i < SkinColors.Length; i++)
            {
                num = Mathf.Max(num, GetTotalAreaWhereClosestToSelector(i));
            }

            return GetTotalAreaWhereClosestToSelector(skinDataIndex) / num;
        }

        private static float GetTotalAreaWhereClosestToSelector(int skinDataIndex)
        {
            float num = 0f;
            if (skinDataIndex == 0)
            {
                num += SkinColors[skinDataIndex].Selector;
            }
            else if (SkinColors.Length > 1)
            {
                num += (SkinColors[skinDataIndex].Selector - SkinColors[skinDataIndex - 1].Selector) / 2f;
            }

            if (skinDataIndex == SkinColors.Length - 1)
            {
                num += 1f - SkinColors[skinDataIndex].Selector;
            }
            else if (SkinColors.Length > 1)
            {
                num += (SkinColors[skinDataIndex + 1].Selector - SkinColors[skinDataIndex].Selector) / 2f;
            }

            return num;
        }

        public struct SkinColorData
        {
            #region Public Fields

            public Color Color;

            public float Melanin;

            public float Selector;

            #endregion Public Fields

            #region Public Constructors

            public SkinColorData(float melanin, float selector, Color color)
            {
                this.Melanin  = melanin;
                this.Selector = selector;
                this.Color    = color;
            }

            #endregion Public Constructors
        }
    }
}