using Verse;

namespace PawnPlus
{
    public struct BodyPartStatus
    {
        // It is better to indicate missing instead of existing because the variable defaults to false.
        public bool missing;
        public Hediff_AddedPart hediffAddedPart;
    }
}
