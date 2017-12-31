namespace FacialStuff.AI
{
    using System.Collections.Generic;
    using System.Linq;

    using RimWorld;

    using UnityEngine;

    using Verse;
    using Verse.AI;

    public class MentalState_Possessed : MentalState
    {
        private const int CheckChooseNewTargetIntervalTicks = 250;

        private const int MaxSameTargetChaseTicks = 1250;

        private static readonly List<Pawn> candidates = new List<Pawn>();

        public bool insultedTargetAtLeastOnce;

        public int lastInsultTicks = -999999;

        public Pawn target;

        private int targetFoundTicks;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.target, "target");
            Scribe_Values.Look(ref this.insultedTargetAtLeastOnce, "insultedTargetAtLeastOnce");
            Scribe_Values.Look(ref this.lastInsultTicks, "lastInsultTicks");
            Scribe_Values.Look(ref this.targetFoundTicks, "targetFoundTicks");

            // Scribe_References.Look<Corpse>(ref this.corpse, "corpse", false);
        }

        public override void MentalStateTick()
        {
            if (this.target != null
                && !InsultingSpreeMentalStateUtility.CanChaseAndInsult(this.pawn, this.target))
            {
                this.ChooseNextTarget();
            }

            if (this.pawn.IsHashIntervalTick(CheckChooseNewTargetIntervalTicks) && (this.target == null || this.insultedTargetAtLeastOnce))
            {
                this.ChooseNextTarget();
            }

            base.MentalStateTick();
        }

        public override void PostStart(string reason)
        {
            base.PostStart(reason);
            this.ChooseNextTarget();

            // this.corpse = CorpseObsessionMentalStateUtility.GetClosestCorpseToDigUp(base.pawn);
        }

        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        // Verse.AI.MentalState_InsultingSpreeAll
        private void ChooseNextTarget()
        {
            InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(this.pawn, candidates);
            if (!candidates.Any())
            {
                this.target = null;
                this.insultedTargetAtLeastOnce = false;
                this.targetFoundTicks = -1;
            }
            else
            {
                Pawn pawn =
                    this.target == null || Find.TickManager.TicksGame - this.targetFoundTicks <= MaxSameTargetChaseTicks
                    || !candidates.Any(x => x != this.target)
                        ? candidates.RandomElementByWeight(x => this.GetCandidateWeight(x))
                        : (from x in candidates where x != this.target select x).RandomElementByWeight(
                            x => this.GetCandidateWeight(x));
                if (pawn != this.target)
                {
                    this.target = pawn;
                    this.insultedTargetAtLeastOnce = false;
                    this.targetFoundTicks = Find.TickManager.TicksGame;
                }
            }
        }

        private float GetCandidateWeight(Pawn candidate)
        {
            float num = this.pawn.Position.DistanceTo(candidate.Position);
            float num2 = Mathf.Min((float)(num / 40.0), 1f);
            return (float)(1.0 - num2 + 0.0099999997764825821);
        }
    }
}