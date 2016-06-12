using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RW_FacialHair
{
    public class SaveablePawn : IExposable
    {
        public Pawn Pawn;

        public BeardDef BeardDef;
        public TacheDef TacheDef;

        public void ExposeData()
        {
            Scribe_References.LookReference(ref Pawn, "Pawn");
            Scribe_Defs.LookDef(ref BeardDef, "BeardDef");
            Scribe_Defs.LookDef(ref TacheDef, "TacheDef");
        }
    }
}
