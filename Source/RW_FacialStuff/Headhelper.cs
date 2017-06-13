namespace RW_FacialStuff
{
    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public static class Headhelper
    {
        public static readonly Color skinRottingMultiplyColor = new Color(0.35f, 0.38f, 0.3f);

        public static Texture2D BlankTex = null;

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