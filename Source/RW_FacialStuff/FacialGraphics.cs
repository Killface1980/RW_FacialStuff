using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff
{
    using FacialStuff.Defs;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public static class FacialGraphics
    {

        public static readonly Graphic MouthGraphic1 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Smile.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black);

        public static readonly Graphic MouthGraphic2 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Default.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black);

        public static readonly Graphic MouthGraphic3 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Large.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black);
        public static readonly Graphic MouthGraphic4 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Medium.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black);

        public static readonly Graphic MouthGraphic5 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Big.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black);


        public static readonly Graphic MouthGraphic6 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Psycho.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black);
        public static readonly Graphic MouthGraphic7 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Sad.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black);
    }
}
