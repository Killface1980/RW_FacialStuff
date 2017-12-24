using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.ExportCycleDefs
{
    using FacialStuff.Defs;

    using RimWorld;

    using Verse;

    public class Defs
    {
        public WalkCycleDef WalkCycleDef;

        public Defs(WalkCycleDef walkCycleDef)
        {
            this.WalkCycleDef = walkCycleDef;
        }
    }
}
