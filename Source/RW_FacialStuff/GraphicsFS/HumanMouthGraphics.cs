using System.Collections.Generic;
using System.IO;
using FacialStuff.DefOfs;
using FacialStuff.Defs;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
    [StaticConstructorOnStartup]
    public class HumanMouthGraphics
    {
        public List<Graphic_Multi_NaturalHeadParts> HumanMouthGraphic;

        public Graphic_Multi_NaturalHeadParts MouthGraphicCrying;

        public HumanMouthGraphics(MouthSetDef mouthSetDef)
        {
            Color color = Color.white;
            HumanMouthGraphic = new List<Graphic_Multi_NaturalHeadParts>(mouthSetDef.texNames.Count);
            for(int i = 0; i < mouthSetDef.texNames.Count; ++i)
			{
                HumanMouthGraphic.Add(
                    GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                        mouthSetDef.texBasePath + 
                            mouthSetDef.texCollection + "_" +
                            mouthSetDef.texSetName + "_" +
                            mouthSetDef.texNames[i],
                        ShaderDatabase.CutoutSkin,
                        Vector2.one,
                        color) as Graphic_Multi_NaturalHeadParts);
            }
        }
    }
}