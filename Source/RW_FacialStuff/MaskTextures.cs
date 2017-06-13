using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    [StaticConstructorOnStartup]
    public class MaskTextures
    {
        private static Texture2D maskTexNarrowFrontBack = null;
        private static Texture2D maskTexNarrowSide = null;
        private static Texture2D maskTexAverageFrontBack = null;
        private static Texture2D maskTexAverageSide = null;

        public static Texture2D MaskTex_Narrow_FrontBack
        {
            get
            {
                if (maskTexNarrowFrontBack == null)
                {
                    maskTexNarrowFrontBack = Headhelper.MakeReadable(ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Narrow_front+back"));
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
                    maskTexNarrowSide = Headhelper.MakeReadable(ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Narrow_side"));
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
                    maskTexAverageFrontBack = Headhelper.MakeReadable(ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Average_front+back"));
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
                    maskTexAverageSide = Headhelper.MakeReadable(ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Average_side"));
                }
                return maskTexAverageSide;
            }
        }
    }
}
