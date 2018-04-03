// RimWorld.JobGiver_InsultingSpree

using Verse;
using Verse.AI;

namespace FacialStuff.AI
{
    public class JobGiver_Possess : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            MentalState_Possessed mentalStatePossessed = pawn.MentalState as MentalState_Possessed;
            if (mentalStatePossessed != null && mentalStatePossessed.Target != null && pawn.CanReach(mentalStatePossessed.Target, PathEndMode.Touch, Danger.Deadly))
            {
                return new Job(DefDatabase<JobDef>.GetNamed("Possess"), mentalStatePossessed.Target);
            }

            return null;
        }
    }
}