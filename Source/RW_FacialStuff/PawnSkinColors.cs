using UnityEngine;

namespace RW_FacialStuff
{
    public static class PawnSkinColorsModded
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

        private static readonly SkinColorData[] SkinColors = {
            new SkinColorData(0f, 0.05f, new Color32(89,45,32, 255)),
            new SkinColorData(0.1f, 0.1f, new Color32(127,66,45,255)),
            new SkinColorData(0.2f, 0.15f, new Color32(165,93,41,255)),
            new SkinColorData(0.3f, 0.2f, new Color32(186,119,80,255)),
            new SkinColorData(0.4f, 0.3f, new Color32(230,158,124,255)),
            new SkinColorData(0.5f, 0.4f, new Color32(231,184,143,255)),
            new SkinColorData(0.66f, 0.75f, new Color32(237,194,154,255)),
            new SkinColorData(0.83f, 0.85f, new Color32(245,210,178,255)),
            new SkinColorData(1f, 1f, new Color32(255,230,212,255))

//          new SkinColorData(0f, 0f, new Color(0.3882353f, 0.274509817f, 0.141176477f)),
//          new SkinColorData(0.1f, 0.05f, new Color(0.509803951f, 0.356862754f, 0.1882353f)),
//          new SkinColorData(0.25f, 0.2f, new Color(0.894117653f, 0.619607866f, 0.3529412f)),
//          new SkinColorData(0.5f, 0.285f, new Color(1f, 0.9372549f, 0.7411765f)),
//          new SkinColorData(0.75f, 0.785f, new Color(1f, 0.9372549f, 0.8352941f)),
//          new SkinColorData(1f, 1f, new Color(0.9490196f, 0.929411769f, 0.8784314f))
        };

        public static Color GetSkinColor(float skinWhiteness)
        {
            int skinDataLeftIndexByWhiteness = GetSkinDataLeftIndexByWhiteness(skinWhiteness);
            if (skinDataLeftIndexByWhiteness == SkinColors.Length - 1)
            {
                return SkinColors[skinDataLeftIndexByWhiteness].color;
            }
            float t = Mathf.InverseLerp(SkinColors[skinDataLeftIndexByWhiteness].whiteness, SkinColors[skinDataLeftIndexByWhiteness + 1].whiteness, skinWhiteness);
            return Color.Lerp(SkinColors[skinDataLeftIndexByWhiteness].color, SkinColors[skinDataLeftIndexByWhiteness + 1].color, t);
        }

        private static int GetSkinDataLeftIndexByWhiteness(float skinWhiteness)
        {
            int result = 0;
            for (int i = 0; i < SkinColors.Length; i++)
            {
                if (skinWhiteness < SkinColors[i].whiteness)
                {
                    break;
                }
                result = i;
            }
            return result;
        }
    }
}
