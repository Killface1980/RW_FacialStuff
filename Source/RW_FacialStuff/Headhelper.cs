using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
   public static class Headhelper
    {
        public static Texture2D texture_= null;

        public static readonly Color skinRottingMultiplyColor = new Color(0.35f, 0.38f, 0.3f);

        public static void PaintHeadWithColor(Texture2D finalHeadFront, Color color)
        {
            for (int x = 0; x < 128; x++)
            {

                for (int y = 0; y < 128; y++)
                {
                    Color headColor = finalHeadFront.GetPixel(x, y);
                    headColor *= color;

                    //      Color final_color = Color.Lerp(headColor, eyeColor, eyeColor.a / 1f);


                    finalHeadFront.SetPixel(x, y, headColor);
                }
            }

            finalHeadFront.Apply();
        }

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

        public static void AddFacialHair(Pawn pawn, ref Texture2D finalTexture, Texture2D beardTex)
        {
            Texture2D tempBeardTex = MakeReadable(beardTex);
            // offset neede if beards are stretched => narrow
            int offset = (finalTexture.width - tempBeardTex.width) / 2;
            int startX = 0;
            int startY = finalTexture.height - tempBeardTex.height;

            for (int x = startX; x < finalTexture.width; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {
                    Color headColor = finalTexture.GetPixel(x, y);

                    Color beardColor;


                    beardColor = tempBeardTex.GetPixel(x - startX - offset, y - startY);


                    beardColor *= pawn.story.hairColor;

                    Color final_color = Color.Lerp(headColor, beardColor, beardColor.a / 1f);

                    final_color.a = headColor.a + beardColor.a;

                    finalTexture.SetPixel(x, y, final_color);
                }
            }

            finalTexture.Apply();
        }

        public static void MergeTwoGraphics(ref Texture2D finalTexture, Texture2D topLayerTex, Color multiplyColor)
        {
            // offset neede if beards are stretched => narrow

            int offset = (finalTexture.width - topLayerTex.width) / 2;

            for (int x = 0; x < 128; x++)
            {

                for (int y = 0; y < 128; y++)
                {
                    Color topColor;

                    topColor = topLayerTex.GetPixel(x - offset, y);
                    Color headColor = finalTexture.GetPixel(x, y);
                    //          eyeColor = topLayerTex.GetPixel(x, y);
                    topColor *= multiplyColor;
                    //      eyeColor *= eyeColorRandom;

                    Color finalColor = Color.Lerp(headColor, topColor, topColor.a / 1f);

                    finalColor.a = headColor.a + topColor.a;

                    finalTexture.SetPixel(x, y, finalColor);
                }
            }


            finalTexture.Apply();
        }

        public static void MergeTwoLayers(ref Texture2D finalTexture, Texture2D top_layer, Color topColor)
        {

            int offset = (finalTexture.width - top_layer.width) / 2;

            int startX = 0;
            int startY = finalTexture.height - top_layer.height;


            for (int x = startX; x < top_layer.width + offset; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {

                    Color headColor = finalTexture.GetPixel(x, y);
                    Color hairColor;

                    hairColor = top_layer.GetPixel(x - startX - offset, y - startY);

                    if (y > 82)
                        hairColor.a = 0;
                    if (y > 79 && y < 82 && hairColor.a > 0)
                        hairColor = Color.black;

                    hairColor *= topColor;

                    Color final_color = Color.Lerp(headColor, hairColor, hairColor.a);

                    if (headColor.a > 0 || hairColor.a > 0)
                        final_color.a = headColor.a + hairColor.a;

                    finalTexture.SetPixel(x, y, final_color);
                }
            }

            finalTexture.Apply();
        }

        public static void MergeTwoLayers(ref Texture2D finalTexture, Texture2D top_layer, Texture2D maskTex, Color topColor)
        {
            Texture2D tempMaskTex = MakeReadable(maskTex);

            int offset = (finalTexture.width - top_layer.width) / 2;

            int startX = 0;
            int startY = finalTexture.height - top_layer.height;


            for (int x = startX; x < top_layer.width + offset; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {

                    Color headColor = finalTexture.GetPixel(x, y);
                    Color maskColor = tempMaskTex.GetPixel(x, y);

                    Color hairColor = top_layer.GetPixel(x - startX - offset, y - startY);

                    hairColor *= maskColor;

                    hairColor *= topColor;

                    Color final_color = Color.Lerp(headColor, hairColor, hairColor.a);

                    //if (headColor.a > 0 || hairColor.a > 0)
                    //    final_color.a = headColor.a + hairColor.a;

                    finalTexture.SetPixel(x, y, final_color);
                }
            }

            finalTexture.Apply();
        }

        public static void MakeOld(Pawn pawn, ref Texture2D finalhead, Texture2D wrinkleTex)
        {
            Texture2D tempWrinkleTex = MakeReadable(wrinkleTex);
            int startX = 0;
            int startY = finalhead.height - tempWrinkleTex.height;

            for (int x = startX; x < finalhead.width; x++)
            {
                for (int y = startY; y < finalhead.height; y++)
                {
                    Color headColor = finalhead.GetPixel(x, y);
                    Color wrinkleColor = tempWrinkleTex.GetPixel(x - startX, y - startY);

                    Color final_color = headColor;

                    final_color = Color.Lerp(headColor, wrinkleColor, (wrinkleColor.a / 0.6f) * Mathf.InverseLerp(50, 200, pawn.ageTracker.AgeBiologicalYearsFloat));

                    final_color.a = headColor.a + wrinkleColor.a;

                    finalhead.SetPixel(x, y, final_color);
                }
            }

            finalhead.Apply();
        }

        private static Color[] destPix;

        public static void ScaleTexture(Texture2D sourceTex, ref Texture2D destTex, int targetWidth, int targetHeight)
        {
            float warpFactorX = 1f;
            float warpFactorY = 1f;

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

        }

    }
}
