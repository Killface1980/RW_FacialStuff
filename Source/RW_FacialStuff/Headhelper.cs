namespace FacialStuff
{
    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public static class HeadHelper
    {
        private static Texture2D blankTexture;

        private static bool blankExists;

        public static Color DarkerBeardColor(Color value)
        {
            Color darken = new Color(0.9f, 0.9f, 0.9f);

            return value * darken;
        }

        public static Texture2D BlankTexture()
        {
            if (blankExists)
            {
                return blankTexture;
            }

            blankTexture = new Texture2D(128, 128, TextureFormat.ARGB32, false);

            for (int x = 0; x < blankTexture.width; x++)
            {
                for (int y = 0; y < blankTexture.height; y++)
                {
                    blankTexture.SetPixel(x, y, Color.clear);
                }
            }

            blankTexture.name = "Blank";

            blankTexture.Compress(false);
            blankTexture.Apply(false, true);
            blankExists = true;
            return blankTexture;
        }

        private static Texture2D maskTexNarrowFrontBack;

        private static Texture2D maskTexNarrowSide;

        private static Texture2D maskTexAverageFrontBack;

        private static Texture2D maskTexAverageSide;

        public static Texture2D MaskTex_Narrow_FrontBack
        {
            get
            {
                if (maskTexNarrowFrontBack == null)
                {
                    maskTexNarrowFrontBack = HeadHelper.MakeReadable(ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Narrow_front+back"));
                }

                return maskTexNarrowFrontBack;
            }
        }

        public static Texture2D MaskTex_Narrow_Side
        {
            get
            {
                if (maskTexNarrowSide == null)
                {
                    maskTexNarrowSide = HeadHelper.MakeReadable(ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Narrow_side"));
                }

                return maskTexNarrowSide;
            }
        }

        public static Texture2D MaskTex_Average_FrontBack
        {
            get
            {
                if (maskTexAverageFrontBack == null)
                {
                    maskTexAverageFrontBack = HeadHelper.MakeReadable(ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Average_front+back"));
                }

                return maskTexAverageFrontBack;
            }
        }

        public static Texture2D MaskTex_Average_Side
        {
            get
            {
                if (maskTexAverageSide == null)
                {
                    maskTexAverageSide = HeadHelper.MakeReadable(ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Average_side"));
                }

                return maskTexAverageSide;
            }
        }

        public static readonly Color skinRottingMultiplyColor = new Color(0.35f, 0.38f, 0.3f);

        public static Texture2D MakeReadable(Texture2D texture)
        {
            RenderTexture previous = RenderTexture.active;

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
             RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);
            return myTexture2D;

            // "myTexture2D" now has the same pixels from "texture" and it's readable.
        }
    }
}