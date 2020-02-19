using Verse;

namespace FacialStuff
{
    using RimWorld;

    public class IncidentWorker_FacialStuffUpdateNote : IncidentWorker
    {
        // RimWorld.IncidentWorker
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            Find.LetterStack.ReceiveLetter(
                "LetterLabelFacialStuffUpdate".Translate(),
                "FacialStuffUpdate".Translate(new object[] { }),
                LetterDefOf.PositiveEvent,
                null);
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            return true;
        }
    }
}