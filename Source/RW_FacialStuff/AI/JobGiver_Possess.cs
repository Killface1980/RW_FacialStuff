// RimWorld.JobGiver_InsultingSpree

using RimWorld;

using Verse;
using Verse.AI;

namespace FacialStuff.AI
{
    public class JobGiver_Possess : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            MentalState_Possessed mentalStatePossessed = pawn.MentalState as MentalState_Possessed;
            if (mentalStatePossessed != null && mentalStatePossessed.target != null && pawn.CanReach(mentalStatePossessed.target, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                return new Job(DefDatabase<JobDef>.GetNamed("Possess"), mentalStatePossessed.target);
            }

            return null;
        }
    }
}
