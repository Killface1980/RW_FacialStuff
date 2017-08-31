using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
    public class IncidentWorker_FacialStuffUpdateNote : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            Find.LetterStack.ReceiveLetter(
                "LetterLabelFacialStuffUpdate".Translate(),
                "FacialStuffUpdate".Translate(new object[] { }),
                LetterDefOf.Good,
                null);
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            return true;
        }
    }
}