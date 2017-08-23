namespace FacialStuff
{
    using FacialStuff.Defs;
    using FacialStuff.Graphics_FS;

    using JetBrains.Annotations;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public static class FacialGraphics
    {

        #region Public Fields

        [JetBrains.Annotations.NotNull]
        public static readonly Texture2D BlankTexture;
        public static readonly Color SkinRottingMultiplyColor = new Color(0.35f, 0.38f, 0.3f);

        [CanBeNull]
        public static Texture2D MaskTex_Average_FrontBack;

        [NotNull]
        public static Texture2D MaskTex_Narrow_Side;

        #endregion Public Fields

        #region Private Fields

        [CanBeNull]
        private static Texture2D maskTexAverageFrontBack;

        [CanBeNull]
        private static Texture2D maskTexAverageSide;

        [CanBeNull]
        private static Texture2D maskTexNarrowFrontBack;

        [CanBeNull]
        private static Texture2D maskTexNarrowSide;

        #endregion Private Fields

        #region Public Constructors

        static FacialGraphics()
        {
            BlankTexture = new Texture2D(128, 128, TextureFormat.ARGB32, false);

            for (int x = 0; x < BlankTexture.width; x++)
            {
                for (int y = 0; y < BlankTexture.height; y++)
                {
                    BlankTexture.SetPixel(x, y, Color.clear);
                }
            }

            BlankTexture.name = "Blank";

            BlankTexture.Compress(false);
            BlankTexture.Apply(false, true);


            MaskTex_Average_FrontBack = MakeReadable(
                ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Average_front+back"));

            MaskTex_Narrow_Side = MakeReadable(ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Narrow_side"));


        }

        #endregion Public Constructors

        #region Public Properties

        [NotNull]
        public static Texture2D MaskTex_Average_Side
        {
            get
            {
                if (maskTexAverageSide == null)
                {
                    maskTexAverageSide = MakeReadable(ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Average_side"));
                }

                return maskTexAverageSide;
            }
        }

        [NotNull]
        public static Texture2D MaskTex_Narrow_FrontBack
        {
            get
            {
                if (maskTexNarrowFrontBack == null)
                {
                    maskTexNarrowFrontBack = MakeReadable(
                        ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Narrow_front+back"));
                }

                return maskTexNarrowFrontBack;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public static Color DarkerBeardColor(Color value)
        {
            Color darken = new Color(0.9f, 0.9f, 0.9f);

            return value * darken;
        }

        [JetBrains.Annotations.NotNull]
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

        #endregion Public Methods

    }
}