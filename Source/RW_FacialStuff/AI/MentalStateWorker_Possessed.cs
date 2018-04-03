// Verse.AI.MentalStateWorker_InsultingSpreeAll

using JetBrains.Annotations;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace FacialStuff.AI
{
    public class MentalStateWorker_Possessed : MentalStateWorker
    {
        private static readonly List<Pawn> Candidates = new List<Pawn>();

        public override bool StateCanOccur([NotNull] Pawn pawn)
        {
            if (!base.StateCanOccur(pawn))
            {
                return false;
            }

            if (!Controller.settings.UseHeadRotator)
            {
                return false;
            }

            InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(pawn, Candidates);
            bool result = Candidates != null && Candidates.Count >= 2;
            Candidates?.Clear();
            return result;
        }
    }
}