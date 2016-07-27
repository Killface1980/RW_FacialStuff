using RW_FacialStuff.Defs;
using Verse;

namespace RW_FacialStuff
{
    public class SaveablePawn : IExposable
    {
        public Pawn Pawn;

        public BeardDef BeardDef;
        public EyeDef EyeDef;
        public LipDef LipDef;
        public WrinkleDef WrinkleDef;
        public string headGraphicIndex;
        public string type;

        public string SkinColorHex;
        public string HairColorHex;

        public bool optimized;
        public bool sessionOptimized;

        public void ExposeData()
        {
            Scribe_References.LookReference(ref Pawn, "Pawn");
            Scribe_Defs.LookDef(ref EyeDef, "EyeDef");
            Scribe_Defs.LookDef(ref LipDef, "LipDef");
            Scribe_Defs.LookDef(ref WrinkleDef, "WrinkleDef");
            Scribe_Defs.LookDef(ref BeardDef, "BeardDef");
            Scribe_Values.LookValue(ref optimized, "optimized");

            Scribe_Values.LookValue(ref headGraphicIndex, "headGraphicIndex");
            Scribe_Values.LookValue(ref type, "type");
            Scribe_Values.LookValue(ref SkinColorHex, "SkinColorHex");
            Scribe_Values.LookValue(ref HairColorHex, "HairColorHex");
        }
    }
}
