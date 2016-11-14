using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    [StaticConstructorOnStartup]
    public class MaskTextures
    {
        public static readonly Texture2D MaskTex_Narrow_FrontBack = ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Narrow_front+back");
        public static readonly Texture2D MaskTex_Narrow_Side = ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Narrow_side");
        public static readonly Texture2D MaskTex_Average_FrontBack = ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Average_front+back");
        public static readonly Texture2D MaskTex_Average_Side = ContentFinder<Texture2D>.Get("MaskTex/MaskTex_Average_side");
    }
}
