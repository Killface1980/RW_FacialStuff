using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    using RW_FacialStuff.Defs;

    using Object = UnityEngine.Object;

    [StaticConstructorOnStartup]
    public static class Headhelper
    {
        public static Texture2D BlankTex = null;

        public static readonly Color skinRottingMultiplyColor = new Color(0.35f, 0.38f, 0.3f);



        public static Texture2D MakeReadable(Texture2D texture)
        {
            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(
                                texture.width,
                                texture.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            Texture2D myTexture2D = new Texture2D(texture.width, texture.width, TextureFormat.ARGB32, false);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();

            // Reset the active RenderTexture
            //    RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);
            return myTexture2D;
            // "myTexture2D" now has the same pixels from "texture" and it's readable.

        }

        public static void ScaleTexture(Texture2D sourceTex, out Texture2D destTex, int targetWidth, int targetHeight)
        {
            // string xx = sourceTex.name;
            // try
            // {
            //     if (ScaledTexDict.ContainsKey(xx))
            //     {
            //         if (ScaledTexDict.TryGetValue(xx, out destTex))
            //         {
            //             return;
            //         }
            //     }
            // }
            //  catch (ArgumentNullException argumentNullException)
            //  {
            //  }

            float warpFactorX = 1f;
            float warpFactorY = 1f;
            Color[] destPix;

            Texture2D scaleTex = MakeReadable(sourceTex);

            destTex = new Texture2D(targetWidth, targetHeight, TextureFormat.ARGB32, false);
            destPix = new Color[destTex.width * destTex.height];
            int y = 0;
            while (y < destTex.height)
            {
                int x = 0;
                while (x < destTex.width)
                {
                    float xFrac = x * 1.0F / (destTex.width - 1);
                    float yFrac = y * 1.0F / (destTex.height - 1);
                    float warpXFrac = Mathf.Pow(xFrac, warpFactorX);
                    float warpYFrac = Mathf.Pow(yFrac, warpFactorY);
                    destPix[y * destTex.width + x] = scaleTex.GetPixelBilinear(warpXFrac, warpYFrac);
                    x++;
                }
                y++;
            }
            destTex.SetPixels(destPix);
            destTex.Apply();
            Object.DestroyImmediate(scaleTex);

            // try
            // {
            //     ScaledTexDict.Add(xx, destTex);
            // }
            // catch (ArgumentNullException argumentNullException)
            // {
            // }
        }
    }
}
