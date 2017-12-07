// Verse.AI.MentalStateWorker_InsultingSpreeAll

using System.Collections.Generic;

using Verse;
using Verse.AI;

namespace FacialStuff.AI
{
    public class MentalStateWorker_Possessed : MentalStateWorker
    {
        private static List<Pawn> candidates = new List<Pawn>();

        public override bool StateCanOccur(Pawn pawn)
        {
            if (!base.StateCanOccur(pawn))
            {
                return false;
            }

            if (!Controller.settings.UseHeadRotator)
            {
                return false;
            }

            InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(pawn, candidates, true);
            bool result = candidates.Count >= 2;
            candidates.Clear();
            return result;
        }
    }
}
