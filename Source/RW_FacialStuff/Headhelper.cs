using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    [StaticConstructorOnStartup]
    public static class Headhelper
    {
        public static Texture2D BlankTex = null;

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

        public static void MakeReadable(Texture2D texture, out Texture2D myTexture2D)
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
            myTexture2D = new Texture2D(texture.width, texture.width, TextureFormat.ARGB32, false);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();

            // Reset the active RenderTexture
            //    RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);
            //           return myTexture2D;
            // "myTexture2D" now has the same pixels from "texture" and it's readable.
        }

        public static void AddFacialHair(Pawn pawn, Texture2D beardTex, ref Texture2D finalTexture)
        {
            Texture2D tempBeardTex;
            MakeReadable(beardTex, out tempBeardTex);
            Color color = new Color(0.95f, 0.95f, 0.95f, 1f);

            // offset neede if beards are stretched => narrow
            int offset = (finalTexture.width - tempBeardTex.width) / 2;
            int startX = 0;
            int startY = finalTexture.height - tempBeardTex.height;

            for (int x = startX; x < finalTexture.width; x++)
            {

                for (int y = startY; y < finalTexture.height; y++)
                {
                    Color headColor = finalTexture.GetPixel(x, y);

                    Color beardColor = tempBeardTex.GetPixel(x - startX - offset, y - startY);

                    beardColor *= pawn.story.hairColor;

                    Color final_color = Color.Lerp(headColor, beardColor, beardColor.a / 1f);

                    final_color.a = headColor.a + beardColor.a;

                    finalTexture.SetPixel(x, y, final_color);
                }
            }
            Object.DestroyImmediate(tempBeardTex);
            finalTexture.Apply();
        }

        public static void MergeTwoGraphics(Texture2D topLayerTex, Color multiplyColor, ref Texture2D finalTexture)
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



        public static void MergeHeadWithHair(Color mutiplyHairColor, Texture2D top_layer, Texture2D maskTex, ref Texture2D finalTexture)
        {
            Texture2D tempMaskTex;
            MakeReadable(maskTex, out tempMaskTex);

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
                    hairColor *= mutiplyHairColor;

                    Color final_color = Color.Lerp(headColor, hairColor, hairColor.a);

                    //if (headColor.a > 0 || mutiplyHairColor.a > 0)
                    //    final_color.a = headColor.a + mutiplyHairColor.a;

                    finalTexture.SetPixel(x, y, final_color);
                }
            }
            Object.DestroyImmediate(tempMaskTex);
            finalTexture.Apply();
        }

        public static void MakeOld(Pawn pawn, Texture2D wrinkleTex, ref Texture2D finalhead)
        {
            Texture2D tempWrinkleTex;
            MakeReadable(wrinkleTex, out tempWrinkleTex);
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
            Object.DestroyImmediate(tempWrinkleTex);

            finalhead.Apply();
        }



        public static void ScaleTexture(Texture2D sourceTex, out Texture2D destTex, int targetWidth, int targetHeight)
        {
            float warpFactorX = 1f;
            float warpFactorY = 1f;
            Color[] destPix;

            Texture2D scaleTex;
            MakeReadable(sourceTex, out scaleTex);

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

        }

    }
}
