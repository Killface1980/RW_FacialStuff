using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityCoreLibrary;
using RimWorld;
using RW_FacialStuff.Sexuality;

namespace RW_FacialStuff
{
    class DetourInjector : SpecialInjector
    {
        public override bool Inject()
        {
            // Detour FloatMenuMaker
            if (!Detours.TryDetourFromTo(typeof(Pawn_RelationsTracker).GetMethod("AttractionTo"),
                typeof(Pawn_RelationsTrackerModded).GetMethod("AttractionToModded")))
                return false;
            return true;
        }

    }
}
