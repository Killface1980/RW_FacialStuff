// ReSharper disable All
namespace FacialStuff.FaceEditor.ColorPicker
{
    using System.Globalization;

    using UnityEngine;

    public static class ColorHelper
    {
        /// <summary>
        ///     From http://answers.unity3d.com/questions/701956/hsv-to-rgb-without-editorguiutilityhsvtorgb.html
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Color HSVtoRGB(float h, float s, float v, float a = 1f)
        {
            if (s == 0f)
            {
                return new Color(v, v, v, a);
            }

            if (v == 0f)
            {
                return new Color(0f, 0f, 0f, a);
            }

            Color col = Color.black;
            float Hval = h * 6f;
            int sel = Mathf.FloorToInt(Hval);
            float mod = Hval - sel;
            float v1 = v * (1f - s);
            float v2 = v * (1f - s * mod);
            float v3 = v * (1f - s * (1f - mod));
            switch (sel + 1)
            {
                case 0:
                    col.r = v;
                    col.g = v1;
                    col.b = v2;
                    break;

                case 1:
                    col.r = v;
                    col.g = v3;
                    col.b = v1;
                    break;

                case 2:
                    col.r = v2;
                    col.g = v;
                    col.b = v1;
                    break;

                case 3:
                    col.r = v1;
                    col.g = v;
                    col.b = v3;
                    break;

                case 4:
                    col.r = v1;
                    col.g = v2;
                    col.b = v;
                    break;

                case 5:
                    col.r = v3;
                    col.g = v1;
                    col.b = v;
                    break;

                case 6:
                    col.r = v;
                    col.g = v1;
                    col.b = v2;
                    break;

                case 7:
                    col.r = v;
                    col.g = v3;
                    col.b = v1;
                    break;
            }
            col.r = Mathf.Clamp(col.r, 0f, 1f);
            col.g = Mathf.Clamp(col.g, 0f, 1f);
            col.b = Mathf.Clamp(col.b, 0f, 1f);
            col.a = Mathf.Clamp(a, 0f, 1f);
            return col;
        }

        public static string RGBtoHex(Color col)
        {
            // this is RGBA, which seems to be common in some parts.
            // ARGB is also common, but oh well.
            int r = (int)Mathf.Clamp(col.r * 256f, 0, 255);
            int g = (int)Mathf.Clamp(col.g * 256f, 0, 255);
            int b = (int)Mathf.Clamp(col.b * 256f, 0, 255);
            int a = (int)Mathf.Clamp(col.a * 256f, 0, 255);

            return "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2") + a.ToString("X2");
        }

        /// <summary>
        ///     From http://answers.unity3d.com/comments/865281/view.html
        /// </summary>
        /// <param name="rgbColor"></param>
        /// <param name="H"></param>
        /// <param name="S"></param>
        /// <param name="V"></param>
        public static void RGBtoHSV(Color rgbColor, out float H, out float S, out float V)
        {
            if (rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r)
            {
                RGBtoHSV_Helper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
            }
            else
            {
                if (rgbColor.g > rgbColor.r)
                {
                    RGBtoHSV_Helper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
                }
                else
                {
                    RGBtoHSV_Helper(0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
                }
            }
        }

        /// <summary>
        ///     Attempt to get a numerical representation of an RGB(A) hexademical color string.
        /// </summary>
        /// <param name="hex">7 or 9 long string (including hashtag)</param>
        /// <param name="col">updated with the parsed color on succes</param>
        /// <returns>bool success</returns>
        public static bool TryHexToRGB(string hex, ref Color col)
        {
            Color clr = new Color(0, 0, 0);
            if (hex == null || hex.Length != 9 && hex.Length != 7)
            {
                return false;
            }

            try
            {
                string str = hex.Substring(1, hex.Length - 1);
                clr.r = int.Parse(str.Substring(0, 2), NumberStyles.AllowHexSpecifier) / 255.0f;
                clr.g = int.Parse(str.Substring(2, 2), NumberStyles.AllowHexSpecifier) / 255.0f;
                clr.b = int.Parse(str.Substring(4, 2), NumberStyles.AllowHexSpecifier) / 255.0f;
                if (str.Length == 8)
                {
                    clr.a = int.Parse(str.Substring(6, 2), NumberStyles.AllowHexSpecifier) / 255.0f;
                }
                else
                {
                    clr.a = 1.0f;
                }
            }
            catch
            {
                return false;
            }

            col = clr;
            return true;
        }

        // From http://answers.unity3d.com/comments/865281/view.html
        private static void RGBtoHSV_Helper(
            float offset,
            float dominantcolor,
            float colorone,
            float colortwo,
            out float H,
            out float S,
            out float V)
        {
            V = dominantcolor;
            if (V != 0f)
            {
                float num;
                if (colorone > colortwo)
                {
                    num = colortwo;
                }
                else
                {
                    num = colorone;
                }

                float num2 = V - num;
                if (num2 != 0f)
                {
                    S = num2 / V;
                    H = offset + (colorone - colortwo) / num2;
                }
                else
                {
                    S = 0f;
                    H = offset + (colorone - colortwo);
                }

                H /= 6f;
                if (H < 0f)
                {
                    H += 1f;
                }
            }
            else
            {
                S = 0f;
                H = 0f;
            }
        }
    }
}