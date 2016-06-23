using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RW_FacialStuff
{
    public class SaveablePawn : IExposable
    {
        public Pawn Pawn;

        public BeardDef BeardDef;
        public SideburnDef SideburnDef;
        public TacheDef TacheDef;
        public EyeDef EyeDef;
        public LipDef LipDef;
        public WrinkleDef WrinkleDef;

        public bool optimized = false;

        public void ExposeData()
        {
            Scribe_References.LookReference(ref Pawn, "Pawn");
            Scribe_Defs.LookDef(ref EyeDef, "EyeDef");
            Scribe_Defs.LookDef(ref LipDef, "LipDef");
            Scribe_Defs.LookDef(ref WrinkleDef, "WrinkleDef");
            Scribe_Defs.LookDef(ref TacheDef, "TacheDef");
            Scribe_Defs.LookDef(ref SideburnDef, "SideburnDef");
            Scribe_Defs.LookDef(ref BeardDef, "BeardDef");
            Scribe_Values.LookValue(ref optimized, "optimized");
        }
    }
}
