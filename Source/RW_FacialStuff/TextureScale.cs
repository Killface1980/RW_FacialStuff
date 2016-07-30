// Only works on ARGB32, RGB24 and Alpha8 textures that are marked readable

using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace RW_FacialStuff
{
    public class TextureScale
    {
        public class ThreadData
        {
            public int start;
            public int end;
            public ThreadData(int s, int e)
            {
                start = s;
                end = e;
            }
        }

        private static Color[] texColors;
        private static Color[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static Mutex mutex;

        public static void Point(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, false);
        }

        public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, true);
        }

        private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
        {
            texColors = tex.GetPixels();
            newColors = new Color[newWidth * newHeight];
            if (useBilinear)
            {
                ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
                ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            }
            else
            {
                ratioX = ((float)tex.width) / newWidth;
                ratioY = ((float)tex.height) / newHeight;
            }
            w = tex.width;
            w2 = newWidth;

            finishCount = 0;
            if (mutex == null)
            {
                mutex = new Mutex(false);
            }
            {
                ThreadData threadData = new ThreadData(0, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
            }

            tex.Resize(newWidth, newHeight);
            tex.SetPixels(newColors);
            tex.Apply();
        }

        public static void BilinearScale(ThreadData obj)
        {
            ThreadData threadData = obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                var y1 = yFloor * w;
                var y2 = (yFloor + 1) * w;
                var yw = y * w2;

                for (var x = 0; x < w2; x++)
                {
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    var xLerp = x * ratioX - xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                                                           ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                                                           y * ratioY - yFloor);
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        public static void PointScale(ThreadData obj)
        {
            ThreadData threadData = obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                var thisY = (int)(ratioY * y) * w;
                var yw = y * w2;
                for (var x = 0; x < w2; x++)
                {
                    newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
        {
            return new Color(c1.r + (c2.r - c1.r) * value,
                              c1.g + (c2.g - c1.g) * value,
                              c1.b + (c2.b - c1.b) * value,
                              c1.a + (c2.a - c1.a) * value);
        }



        public static Color32[] ResizeCanvas(Texture2D texture, int width, int height)
        {
            var newPixels = ResizeCanvas(texture.GetPixels32(), texture.width, texture.height, width, height);
            texture.Resize(width, height);
            texture.SetPixels32(newPixels);
            texture.Apply();
            return newPixels;
        }

        private static Color32[] ResizeCanvas(IList<Color32> pixels, int oldWidth, int oldHeight, int width, int height)
        {
            var newPixels = new Color32[(width * height)];
            var wBorder = (width - oldWidth) / 2;
            var hBorder = (height - oldHeight) / 2;

            for (int r = 0; r < height; r++)
            {
                var oldR = r - hBorder;
                if (oldR < 0) { continue; }
                if (oldR >= oldHeight) { break; }

                for (int c = 0; c < width; c++)
                {
                    var oldC = c - wBorder;
                    if (oldC < 0) { continue; }
                    if (oldC >= oldWidth) { break; }

                    var oldI = oldR * oldWidth + oldC;
                    var i = r * width + c;
                    newPixels[i] = pixels[oldI];
                }
            }

            return newPixels;
        }





    }
}