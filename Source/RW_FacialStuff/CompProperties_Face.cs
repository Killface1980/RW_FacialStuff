using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RW_FacialStuff.Defs;
using Verse;

namespace RW_FacialStuff
{
    public class CompProperties_Face : CompProperties
    {
        public BeardDef BeardDef;

        public CompProperties_Face()
        {
            this.compClass = typeof(CompFace);
        }
    }

}
