using RW_FacialStuff.Defs;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    public class SaveablePawn : IExposable
    {
        public Pawn Pawn;

        public BeardDef BeardDef;
        public EyeDef EyeDef;
        public BrowDef BrowDef;
        public MouthDef MouthDef;
        public WrinkleDef WrinkleDef;
        public string headGraphicIndex;
        public string type;

        public string SkinColorHex;
        public Color HairColorOrg;

        public bool optimized;
        public bool sessionOptimized;
        public bool drawMouth;

        public void ExposeData()
        {
            Scribe_References.Look(ref Pawn, "Pawn");
            Scribe_Defs.Look(ref EyeDef, "EyeDef");
            Scribe_Defs.Look(ref BrowDef, "BrowDef");
            Scribe_Defs.Look(ref MouthDef, "MouthDef");
            Scribe_Defs.Look(ref WrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref BeardDef, "BeardDef");
            Scribe_Values.Look(ref optimized, "optimized");
            Scribe_Values.Look(ref drawMouth, "drawMouth");

            Scribe_Values.Look(ref headGraphicIndex, "headGraphicIndex");
            Scribe_Values.Look(ref type, "type");
            Scribe_Values.Look(ref SkinColorHex, "SkinColorHex");
            Scribe_Values.Look(ref HairColorOrg, "HairColorOrg");
        }
    }
}
