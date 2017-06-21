namespace FacialStuff
{
    using FacialStuff.Defs;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public static class FacialGraphics
    {

        public static readonly Graphic_Multi_NaturalHeadParts MouthGraphic01 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Mood01.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black) as Graphic_Multi_NaturalHeadParts;

        public static readonly Graphic_Multi_NaturalHeadParts MouthGraphic02 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Mood02.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black) as Graphic_Multi_NaturalHeadParts;

        public static readonly Graphic_Multi_NaturalHeadParts MouthGraphic03 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Mood03.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black) as Graphic_Multi_NaturalHeadParts;

        public static readonly Graphic_Multi_NaturalHeadParts MouthGraphic04 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Mood04.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black) as Graphic_Multi_NaturalHeadParts;

        public static readonly Graphic_Multi_NaturalHeadParts MouthGraphic05 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Mood05.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black) as Graphic_Multi_NaturalHeadParts;


        public static readonly Graphic_Multi_NaturalHeadParts MouthGraphic06 = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_Mood06.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.black) as Graphic_Multi_NaturalHeadParts;

        public static Graphic_Multi_NaturalHeadParts MouthGraphicBionic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
            MouthDefOf.Mouth_BionicJaw.texPath,
            ShaderDatabase.Transparent,
            Vector2.one,
            Color.white) as Graphic_Multi_NaturalHeadParts;
    }
}
