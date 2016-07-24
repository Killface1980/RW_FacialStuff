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

        public bool optimized;

        public void ExposeData()
        {
            Scribe_References.LookReference(ref Pawn, "Pawn");
            Scribe_Defs.LookDef(ref EyeDef, "EyeDef");
            Scribe_Defs.LookDef(ref LipDef, "LipDef");
            Scribe_Defs.LookDef(ref WrinkleDef, "WrinkleDef");
            Scribe_Defs.LookDef(ref BeardDef, "BeardDef");
            Scribe_Values.LookValue(ref optimized, "optimized");
            Scribe_Values.LookValue(ref headGraphicIndex, "headGraphicIndex");
        }
    }
}
