using System;
using System.Collections.Generic;

namespace FacialStuff
{
    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public static class CutHairDB
    {
        #region Fields

        private static List<HairCutPawn> _pawnHairCache = new List<HairCutPawn>();
        private static Dictionary<GraphicRequest, Graphic> allGraphics = new Dictionary<GraphicRequest, Graphic>();
        private static Texture2D maskTexFrontBack;

        private static Texture2D maskTexSide;

        #endregion Fields

        #region Methods

        public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color) where T : Graphic, new()
        {
            // Added second 'color' to get a separate graphic
            GraphicRequest req = new GraphicRequest(typeof(T), path, shader, drawSize, color, color, null, 0);
            return GetInner<T>(req);

        }

        public static HairCutPawn GetHairCache(Pawn pawn)
        {
            foreach (HairCutPawn c in _pawnHairCache)
            {
                if (c.Pawn == pawn)
                {
                    return c;
                }
            }
            HairCutPawn n = new HairCutPawn { Pawn = pawn };
            _pawnHairCache.Add(n);
            return n;

            // if (!PawnApparelStatCaches.ContainsKey(pawn))
            // {
            // PawnApparelStatCaches.Add(pawn, new StatCache(pawn));
            // }
            // return PawnApparelStatCaches[pawn];
        }

        private static void CutOutHair(ref Texture2D hairTex, Texture2D maskTex)
        {

            for (int x = 0; x < hairTex.width; x++)
            {
                for (int y = 0; y < hairTex.height; y++)
                {

                    Color maskColor = maskTex.GetPixel(x, y);
                    Color hairColor = hairTex.GetPixel(x, y);

                    Color final_color1 = hairColor * maskColor;

                    hairTex.SetPixel(x, y, final_color1);
                }
            }

            hairTex.Apply();
        }

        private static T GetInner<T>(GraphicRequest req) where T : Graphic, new()
        {
            Graphic graphic;
            if (!allGraphics.TryGetValue(req, out graphic))
            {
                graphic = Activator.CreateInstance<T>();
                graphic.Init(req);
                allGraphics.Add(req, graphic);
                
                Texture2D temptexturefront = new Texture2D(128, 128, TextureFormat.ARGB32, true);
                Texture2D temptextureside = new Texture2D(128, 128, TextureFormat.ARGB32, true);
                Texture2D temptextureback = new Texture2D(128, 128, TextureFormat.ARGB32, true);


                temptexturefront = FacialGraphics.MakeReadable(graphic.MatFront.mainTexture as Texture2D);
                temptextureside = FacialGraphics.MakeReadable(graphic.MatSide.mainTexture as Texture2D);
                temptextureback = FacialGraphics.MakeReadable(graphic.MatBack.mainTexture as Texture2D);

                // intentional, only 1 mask tex. todo: rename, cleanup
                maskTexFrontBack = FacialGraphics.MaskTex_Average_FrontBack;
                maskTexSide = FacialGraphics.MaskTex_Narrow_Side;

                CutOutHair(ref temptexturefront, maskTexFrontBack);

                CutOutHair(ref temptextureside, maskTexSide);

                CutOutHair(ref temptextureback, maskTexFrontBack);

                // if (false)
                // {
                // byte[] bytes = canvasHeadFront.EncodeToPNG();
                // File.WriteAllBytes("Mods/FacialStuff/MergedHeads/" + this.pawn.Name + "_01front.png", bytes);
                // byte[] bytes2 = canvasHeadSide.EncodeToPNG();
                // File.WriteAllBytes("Mods/FacialStuff/MergedHeads/" + this.pawn.Name + "_02side.png", bytes2);
                // byte[] bytes3 = canvasHeadBack.EncodeToPNG();
                // File.WriteAllBytes("Mods/FacialStuff/MergedHeads/" + this.pawn.Name + "_03back.png", bytes3);
                // }
                temptexturefront.Compress(true);
                temptextureside.Compress(true);
                temptextureback.Compress(true);

                temptexturefront.mipMapBias = 0.5f;
                temptextureside.mipMapBias = 0.5f;
                temptextureback.mipMapBias = 0.5f;

                temptexturefront.Apply(false, true);
                temptextureside.Apply(false, true);
                temptextureback.Apply(false, true);

                graphic.MatFront.mainTexture = temptexturefront;
                graphic.MatSide.mainTexture = temptextureside;
                graphic.MatBack.mainTexture = temptextureback;


                // Object.Destroy(temptexturefront);
                // Object.Destroy(temptextureside);
                // Object.Destroy(temptextureback);
            }

            return (T)((object)graphic);
        }

        #endregion Methods
    }
}
