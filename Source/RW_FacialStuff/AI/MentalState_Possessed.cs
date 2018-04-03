using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FacialStuff.AI
{
    public class MentalState_Possessed : MentalState
    {
        private const int CheckChooseNewTargetIntervalTicks = 250;

        private const int MaxSameTargetChaseTicks = 1250;

        private static readonly List<Pawn> Candidates = new List<Pawn>();

        public bool InsultedTargetAtLeastOnce;

        public int LastInsultTicks = -999999;

        public Pawn Target;

        private int _targetFoundTicks;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.Target, "target");
            Scribe_Values.Look(ref this.InsultedTargetAtLeastOnce, "insultedTargetAtLeastOnce");
            Scribe_Values.Look(ref this.LastInsultTicks, "lastInsultTicks");
            Scribe_Values.Look(ref this._targetFoundTicks, "targetFoundTicks");

            // Scribe_References.Look<Corpse>(ref this.corpse, "corpse", false);
        }

        public override void MentalStateTick()
        {
            if (this.Target != null
             && !InsultingSpreeMentalStateUtility.CanChaseAndInsult(this.pawn, this.Target))
            {
                this.ChooseNextTarget();
            }

            if (this.pawn.IsHashIntervalTick(CheckChooseNewTargetIntervalTicks) &&
                (this.Target == null || this.InsultedTargetAtLeastOnce))
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
            InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(this.pawn, Candidates);
            if (!Candidates.Any())
            {
                this.Target = null;
                this.InsultedTargetAtLeastOnce = false;
                this._targetFoundTicks = -1;
            }
            else
            {
                bool finished = Find.TickManager.TicksGame - this._targetFoundTicks <= MaxSameTargetChaseTicks;
                Pawn p = this.Target == null || finished || !Candidates.Any(x => x != this.Target)
                                    ? Candidates.RandomElementByWeight(x => this.GetCandidateWeight(x))
                                    : (from x in Candidates where x != this.Target select x)
                                   .RandomElementByWeight(x => this.GetCandidateWeight(x));
                if (p != this.Target)
                {
                    this.Target = p;
                    this.InsultedTargetAtLeastOnce = false;
                    this._targetFoundTicks = Find.TickManager.TicksGame;
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