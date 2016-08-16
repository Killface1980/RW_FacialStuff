using RimWorld;
using Verse;
using Verse.AI;

namespace RW_FacialStuff
{
    public class JobGiver_BulimiaAttack : ThinkNode_JobGiver
    {
        protected bool IgnoreForbid(Pawn pawn)
        {
            return pawn.InMentalState;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
         //   bool desperate = pawn.needs.food.CurCategory == HungerCategory.Starving;
            Need_Food food = pawn.needs.food;
            if (food.CurLevelPercentage > food.MaxLevel*0.75)
            {
            return new Job(JobDefOf.Vomit);
            }

            bool desperate = true;
            Thing thing;
            ThingDef def;
            if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out thing, out def, true, true, false, true))
            {
                return null;
            }
          
            return new Job(JobDefOf.Ingest, thing)
            {
                maxNumToCarry = FoodUtility.WillEatStackCountOf(pawn, def)
            };
            return null;
        }



        private Job ConsumeJob(Pawn pawn)
        {
            Thing thing = BestConsumeTarget(pawn);
            if (thing == null)
            {
                return null;
            }
            ThingDef foodDef = FoodUtility.GetFoodDef(thing);
            return new Job(JobDefOf.Ingest, thing)
            {
                maxNumToCarry = foodDef.ingestible.maxNumToIngestAtOnce,
                ignoreForbidden = IgnoreForbid(pawn),
                overeat = true
            };
        }

        private const int BaseConsumeInterval = 600;

        private static readonly SimpleCurve IngestIntervalFactorCurve_Drunkness = new SimpleCurve
        {
            new CurvePoint(0f, 1f),
            new CurvePoint(1f, 4f)
        };

        protected int ConsumeInterval(Pawn pawn)
        {
            int num = BaseConsumeInterval;
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Alcohol);
            if (firstHediffOfDef != null)
            {
                num = (int)(num * IngestIntervalFactorCurve_Drunkness.Evaluate(firstHediffOfDef.Severity));
            }
            return num;
        }

        // RimWorld.JobGiver_GetFood
        public override float GetPriority(Pawn pawn)
        {
            Need_Food food = pawn.needs.food;
            if (food == null)
            {
                return 0f;
            }
            if (pawn.needs.food.CurCategory < HungerCategory.Starving && FoodUtility.ShouldBeFedBySomeone(pawn))
            {
                return 0f;
            }
            if (food.CurLevelPercentage < food.PercentageThreshHungry + 0.02f)
            {
                return 9.5f;
            }
            return 0f;
        }

        protected Thing BestConsumeTarget(Pawn pawn)
        {
            Thing result;
            ThingDef thingDef;
            if (FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, true, out result, out thingDef, false, true, true, true))
            {
                return result;
            }
            return null;
        }
    }
}
