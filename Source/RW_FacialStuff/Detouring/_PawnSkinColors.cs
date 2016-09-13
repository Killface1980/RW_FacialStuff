using System.Reflection;
using UnityEngine;
using Verse;

namespace RW_FacialStuff.Detouring
{
    public static class _PawnSkinColors
    {
        private struct SkinColorData
        {
            public float whiteness;

            public float selector;

            public Color color;

            public SkinColorData(float whiteness, float selector, Color color)
            {
                this.whiteness = whiteness;
                this.selector = selector;
                this.color = color;
            }
        }

        private static readonly SkinColorData[] _SkinColors = {
            new SkinColorData(0f, 0f, new Color32(77,39,28, 255)),
            new SkinColorData(0.1f, 0.075f, new Color32(127,66,45,255)),
            new SkinColorData(0.25f, 0.15f, new Color32(165,93,41,255)),
            new SkinColorData(0.5f, 0.25f, new Color32(186,119,80,255)),
            new SkinColorData(0.6f, 0.35f, new Color32(250,223,170,255)),
            new SkinColorData(0.65f, 0.45f, new Color32(245,202,184,255)),
            new SkinColorData(0.75f, 0.55f, new Color32(230,158,124,255)),
            new SkinColorData(0.85f, 0.65f, new Color32(231,184,143,255)),
            new SkinColorData(0.95f, 0.85f, new Color32(245,210,178,255)),
            new SkinColorData(1f, 1f, new Color32(255,230,212,255))
  
                // Vanilla
//          new SkinColorData(0f, 0f, new Color(0.3882353f, 0.274509817f, 0.141176477f)),
//          new SkinColorData(0.1f, 0.05f, new Color(0.509803951f, 0.356862754f, 0.1882353f)),
//          new SkinColorData(0.25f, 0.2f, new Color(0.894117653f, 0.619607866f, 0.3529412f)),
//          new SkinColorData(0.5f, 0.285f, new Color(1f, 0.9372549f, 0.7411765f)),
//          new SkinColorData(0.75f, 0.785f, new Color(1f, 0.9372549f, 0.8352941f)),
//          new SkinColorData(1f, 1f, new Color(0.9490196f, 0.929411769f, 0.8784314f))
        };

        [Detour(typeof(RimWorld.PawnSkinColors), bindingFlags = (BindingFlags.Static | BindingFlags.Public))]
        public static bool IsDarkSkin(Color color)
        {
            Color skinColor = GetSkinColor(0.5f);
            return color.r + color.g + color.b <= skinColor.r + skinColor.g + skinColor.b + 0.01f;
        }

        [Detour(typeof(RimWorld.PawnSkinColors), bindingFlags = (BindingFlags.Static | BindingFlags.Public))]
        public static Color GetSkinColor(float skinWhiteness)
        {
            int skinDataLeftIndexByWhiteness = GetSkinDataLeftIndexByWhiteness(skinWhiteness);
            if (skinDataLeftIndexByWhiteness == _SkinColors.Length - 1)
            {
                return _SkinColors[skinDataLeftIndexByWhiteness].color;
            }
            float t = Mathf.InverseLerp(_SkinColors[skinDataLeftIndexByWhiteness].whiteness, _SkinColors[skinDataLeftIndexByWhiteness + 1].whiteness, skinWhiteness);
            return Color.Lerp(_SkinColors[skinDataLeftIndexByWhiteness].color, _SkinColors[skinDataLeftIndexByWhiteness + 1].color, t);
        }

        /*
        [Detour(typeof(EdB.PrepareCarefully.PawnColorUtils), bindingFlags = (BindingFlags.Static | BindingFlags.Public))]
        public static int GetColorLeftIndex(Color color)
        {
            int num = _SkinColors.Length - 1;
            for (int i = 0; i < _SkinColors.Length - 1; i++)
            {
                Color color2 = _SkinColors[i].color;
                Color color3 = _SkinColors[i + 1].color;
                if (color.r >= color2.r && color.r <= color3.r && color.g >= color2.g && color.g <= color3.g && color.b >= color2.b && color.b <= color3.b)
                {
                    num = i;
                    break;
                }
            }
            if (num == _SkinColors.Length - 1)
            {
                num = _SkinColors.Length - 2;
            }
            return num;
        }

        [Detour(typeof(EdB.PrepareCarefully.PawnColorUtils), bindingFlags = (BindingFlags.Static | BindingFlags.Public))]
        public static float GetSkinValue(Color color)
        {
            int colorLeftIndex = GetColorLeftIndex(color);
            if (colorLeftIndex == _SkinColors.Length - 1)
            {
                return 1f;
            }
            int num = colorLeftIndex + 1;
            float t = (color.b - _SkinColors[colorLeftIndex].color.b) / (_SkinColors[num].color.b - _SkinColors[colorLeftIndex].color.b);
            return Mathf.Lerp(_SkinColors[colorLeftIndex].whiteness, _SkinColors[colorLeftIndex+1].whiteness, t);
        }
*/

        private static int GetSkinDataLeftIndexByWhiteness(float skinWhiteness)
        {
            int result = 0;
            for (int i = 0; i < _SkinColors.Length; i++)
            {
                if (skinWhiteness < _SkinColors[i].whiteness)
                {
                    break;
                }
                result = i;
            }
            return result;
        }

        [Detour(typeof(RimWorld.PawnSkinColors), bindingFlags = (BindingFlags.Static | BindingFlags.Public))]
        public static float RandomSkinWhiteness()
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
                return _SkinColors[num].whiteness;
            }
            float t = Mathf.InverseLerp(_SkinColors[num].selector, _SkinColors[num + 1].selector, value);
            return Mathf.Lerp(_SkinColors[num].whiteness, _SkinColors[num + 1].whiteness, t);
        }
    }
}
