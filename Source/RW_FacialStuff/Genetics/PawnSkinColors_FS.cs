namespace FacialStuff.Detouring
{
    using global::Harmony;

    using UnityEngine;

    using Verse;

    public static class PawnSkinColors_FS
    {
        #region Fields

        public static readonly SkinColorData[] _SkinColors =
            {
                new SkinColorData(
                    0f,
                    0f,
                    new Color32(
                        255,
                        233,
                        217,
                        255)), // s 0.15, b 1.0
                new SkinColorData(
                    0.15f,
                    0.1f,
                    new Color32(
                        242,
                        196,
                        193,
                        255)), // s 0.2, b 0.95
                new SkinColorData(
                    0.25f,
                    0.25f,
                    new Color32(
                        243,
                        210,
                        181,
                        255)), // s 0.25, b 0.95
                new SkinColorData(
                    0.4f,
                    0.35f,
                    new Color32(
                        242,
                        190,
                        145,
                        255)), // s 0.4, b 0.95
                new SkinColorData(
                    0.55f,
                    0.45f,
                    new Color32(
                        255,
                        231,
                        179,
                        255)), // s 0.3, b 1.0
                new SkinColorData(
                    0.6f,
                    0.6f,
                    new Color32(
                        218,
                        136,
                        87,
                        255)), // s 0.6, b 0.85
                new SkinColorData(
                    0.7f,
                    0.7f,
                    new Color32(
                        165,
                        93,
                        41,
                        255)), // s 0.75, b 0.65
                new SkinColorData(
                    0.8f,
                    0.8f,
                    new Color32(127, 71, 51, 255)), // s 0.6, b 0.5
                new SkinColorData(
                    0.9f,
                    0.9f,
                    new Color32(
                        101,
                        56,
                        41,
                        255)), // s 0.6, b 0.95
                new SkinColorData(
                    1f,
                    1f,
                    new Color32(76, 36, 23, 255)) // s 0.7, b 0.3

                // Vanilla

                // new SkinColorData(0f, 0f, new Color(0.3882353f, 0.274509817f, 0.141176477f)),
                // new SkinColorData(0.1f, 0.05f, new Color(0.509803951f, 0.356862754f, 0.1882353f)),
                // new SkinColorData(0.25f, 0.2f, new Color(0.894117653f, 0.619607866f, 0.3529412f)),
                // new SkinColorData(0.5f, 0.285f, new Color(1f, 0.9372549f, 0.7411765f)),
                // new SkinColorData(0.75f, 0.785f, new Color(1f, 0.9372549f, 0.8352941f)),
                // new SkinColorData(1f, 1f, new Color(0.9490196f, 0.929411769f, 0.8784314f))
            };

        #endregion Fields

        #region Nested type: SkinColorData

        #region Structs

        public struct SkinColorData
        {
            #region Fields

            public Color color;

            public float melanin;

            public float selector;

            #endregion Fields

            #region Constructors

            public SkinColorData(float melanin, float selector, Color color)
            {
                this.melanin = melanin;
                this.selector = selector;
                this.color = color;
            }

            #endregion Constructors
        }

        #endregion Structs

        #endregion Nested type: SkinColorData

        #region Methods
        public static bool GetMelaninCommonalityFactor_Prefix(ref float __result, float melanin)
        {
            int skinDataLeftIndexByWhiteness = GetSkinDataIndexOfMelanin(melanin);
            if (skinDataLeftIndexByWhiteness == _SkinColors.Length - 1)
            {
                __result = GetSkinCommonalityFactor(skinDataLeftIndexByWhiteness);
                return false;
            }

            float t = Mathf.InverseLerp(
                _SkinColors[skinDataLeftIndexByWhiteness].melanin,
                _SkinColors[skinDataLeftIndexByWhiteness + 1].melanin,
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
            if (leftIndexForValue == _SkinColors.Length - 1)
            {
                return 0f;
            }

            int num = leftIndexForValue + 1;
            return Mathf.InverseLerp(_SkinColors[leftIndexForValue].melanin, _SkinColors[num].melanin, value);
        }

        public static Color GetSkinColor(float melanin)
        {
            int skinDataIndexOfMelanin = GetSkinDataIndexOfMelanin(melanin);
            if (skinDataIndexOfMelanin == _SkinColors.Length - 1)
            {
                return _SkinColors[skinDataIndexOfMelanin].color;
            }

            float t = Mathf.InverseLerp(
                _SkinColors[skinDataIndexOfMelanin].melanin,
                _SkinColors[skinDataIndexOfMelanin + 1].melanin,
                melanin);
            return Color.Lerp(
                _SkinColors[skinDataIndexOfMelanin].color,
                _SkinColors[skinDataIndexOfMelanin + 1].color,
                t);
        }

        public static bool GetSkinColor_Prefix(ref Color __result, float melanin)
        {
            int skinDataLeftIndexByMelanin = GetSkinDataIndexOfMelanin(melanin);
            if (skinDataLeftIndexByMelanin == _SkinColors.Length - 1)
            {
                __result = _SkinColors[skinDataLeftIndexByMelanin].color;
                return false;
            }

            float t = Mathf.InverseLerp(
                _SkinColors[skinDataLeftIndexByMelanin].melanin,
                _SkinColors[skinDataLeftIndexByMelanin + 1].melanin,
                melanin);
            __result = Color.Lerp(
                _SkinColors[skinDataLeftIndexByMelanin].color,
                _SkinColors[skinDataLeftIndexByMelanin + 1].color,
                t);
            return false;
        }

        public static int GetSkinDataIndexOfMelanin(float melanin)
        {
            int result = 0;
            for (int i = 0; i < _SkinColors.Length; i++)
            {
                if (melanin < _SkinColors[i].melanin)
                {
                    break;
                }

                result = i;
            }

            return result;
        }

        public static bool GetSkinDataIndexOfMelanin_Prefix(ref int __result, float melanin)
        {
            int result = 0;
            for (int i = 0; i < _SkinColors.Length; i++)
            {
                if (melanin < _SkinColors[i].melanin)
                {
                    break;
                }

                result = i;
            }

            __result = result;
            return false;
        }

        public static float GetValueFromRelativeLerp(int leftIndex, float lerp)
        {
            if (leftIndex >= _SkinColors.Length - 1)
            {
                return 1f;
            }

            if (leftIndex < 0)
            {
                return 0f;
            }

            int num = leftIndex + 1;
            return Mathf.Lerp(_SkinColors[leftIndex].melanin, _SkinColors[num].melanin, lerp);
        }

        public static bool IsDarkSkin_Prefix(ref bool __result, Color color)
        {
            Color skinColor = GetSkinColor(0.5f);
            __result = color.r + color.g + color.b <= skinColor.r + skinColor.g + skinColor.b + 0.01f;
            return false;
        }

        public static bool RandomMelanin_Prefix(ref float __result)
        {
            float value = Rand.Value;
            int num = 0;
            for (int i = 0; i < _SkinColors.Length; i++)
            {
                if (value < _SkinColors[i].selector)
                {
                    break;
                }

                num = i;
            }

            if (num == _SkinColors.Length - 1)
            {
                __result = _SkinColors[num].melanin;
                return false;
            }

            float t = Mathf.InverseLerp(_SkinColors[num].selector, _SkinColors[num + 1].selector, value);
            __result = Mathf.Lerp(_SkinColors[num].melanin, _SkinColors[num + 1].melanin, t);
            return false;
        }

        private static float GetSkinCommonalityFactor(int skinDataIndex)
        {
            float num = 0f;
            for (int i = 0; i < _SkinColors.Length; i++)
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
                num += _SkinColors[skinDataIndex].selector;
            }
            else if (_SkinColors.Length > 1)
            {
                num += (_SkinColors[skinDataIndex].selector - _SkinColors[skinDataIndex - 1].selector) / 2f;
            }

            if (skinDataIndex == _SkinColors.Length - 1)
            {
                num += 1f - _SkinColors[skinDataIndex].selector;
            }
            else if (_SkinColors.Length > 1)
            {
                num += (_SkinColors[skinDataIndex + 1].selector - _SkinColors[skinDataIndex].selector) / 2f;
            }

            return num;
        }

        #endregion Methods
    }
}