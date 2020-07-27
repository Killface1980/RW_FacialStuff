using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

// ReSharper disable InconsistentNaming
namespace FacialStuff.AI
{
    public class JobDriver_Possess : JobDriver
    {
        private readonly TargetIndex TargetInd = TargetIndex.A;

        private int ticksLeft;
        /*
                private TargetIndex VomitInd = TargetIndex.B;
        */

        private Pawn Target => (Pawn)(Thing)this.pawn.CurJob.GetTarget(this.TargetInd);

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil newToils = this.HaveFunWithDemons().WithEffect(EffecterDefOf.Vomit, this.TargetInd)
               .PlaySustainerOrSound(() => SoundDef.Named("Vomit"));

            yield return Toils_Interpersonal.GotoInteractablePosition(this.TargetInd).FailOnDespawnedOrNull(this.TargetInd);

            yield return this.InsultingSpreeDelayToil();
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);

            Toil finalgoto = Toils_Interpersonal.GotoInteractablePosition(this.TargetInd);
            finalgoto.socialMode = RandomSocialMode.Off;
            yield return finalgoto;

            yield return newToils;

            yield return this.InteractToil();
        }

        private Toil HaveFunWithDemons()
        {
            Toil toil = new Toil();
            this.pawn.GetCompFace(out CompFace compFace);

            toil.initAction = delegate
                              {
                                  // this.ticksLeft = Rand.Range(300, 900);
                                  this.ticksLeft = Rand.Range(450, 1200);
                                  int num2 = 0;
                                  IntVec3 c;
                                  while (true)
                                  {
                                      c = this.pawn.Position + GenAdj.AdjacentCellsAndInside[Rand.Range(0, 9)];
                                      num2++;
                                      if (num2 > 12)
                                      {
                                          c = this.pawn.Position;
                                          break;
                                      }

                                      if (c.InBounds(this.pawn.Map) && c.Standable(this.pawn.Map))
                                      {
                                          break;
                                      }
                                  }

                                  this.job.targetB = c;
                                  this.pawn.pather.StopDead();

                                  DefDatabase<SoundDef>.GetNamed("Pawn_Cat_Angry")
                                     .PlayOneShot(new TargetInfo(this.pawn.Position, this.pawn.Map)); // GenExplosion.DoExplosion(

                                  // this.pawn.Position,
                                  // this.pawn.Map,
                                  // 2,
                                  // DamageDefOf.Smoke,
                                  // null,
                                  // 0,
                                  // DefDatabase<SoundDef>.GetNamed("Explosion_Smoke"));
                                  // for (int i = 0; i < 2; i++)
                                  // {
                                  // var loc = this.pawn.Position.ToVector3Shifted();
                                  // var map = this.pawn.Map;
                                  // MoteMaker.ThrowSmoke(loc, map, 1f);
                                  // MoteMaker.ThrowMicroSparks(loc, map);
                                  // MoteMaker.ThrowLightningGlow(loc, map, 1f);
                                  // }
                              };

            int accellerator = 25;
            toil.tickAction = delegate
                              {
                                  if (this.ticksLeft % 60 == 0)
                                  {
                                       // MoteMaker.ThrowFireGlow(this.pawn.Position, this.pawn.Map, 0.1f);
                                   }

                                   // if (this.ticksLeft % 45 == 0)
                                   // {
                                   // MoteMaker.ThrowHeatGlow(this.pawn.Position, this.pawn.Map, 0.3f);
                                   // }
                                   if (this.ticksLeft % accellerator == 0)
                                  {
                                      compFace.HeadRotator.RotateRandomly();

                                       // MoteMaker.ThrowSmoke(this.pawn.Position.ToVector3(), this.pawn.Map, 0.2f);
                                       if (accellerator > 20)
                                      {
                                          accellerator--;
                                      }
                                      else if (accellerator < 37)
                                      {
                                          accellerator++;
                                      }
                                  }

                                  if (this.ticksLeft % 150 == 149)
                                  {
                                      DefDatabase<SoundDef>.GetNamed("Pawn_Cat_Angry")
                                         .PlayOneShot(new TargetInfo(this.pawn.Position, this.pawn.Map));
                                      FilthMaker.TryMakeFilth(this.job.targetA.Cell, this.Map,
                                                           ThingDefOf.Filth_Vomit, this.pawn.LabelIndefinite());
                                      if (this.pawn.needs.food.CurLevelPercentage > 0.10000000149011612)
                                      {
                                          this.pawn.needs.food.CurLevel -= (float)(this.pawn.needs.food.MaxLevel * 0.02);
                                      }
                                  }

                                  if (this.ticksLeft % 50 == 0)
                                  {
                                      FilthMaker.TryMakeFilth(this.pawn.Position.RandomAdjacentCell8Way(), this.Map,
                                                           ThingDefOf.Filth_Vomit, this.pawn.LabelIndefinite());
                                  }

                                  this.ticksLeft--;
                                  if (this.ticksLeft <= 0)
                                  {
                                      this.ReadyForNextToil();
                                      TaleRecorder.RecordTale(TaleDefOf.Vomited, this.pawn);
                                  }
                              };

            toil.AddFinishAction(compFace.HeadRotator.SetUnPossessed);

            toil.defaultCompleteMode = ToilCompleteMode.Never;
            return toil;
        }

        private Toil InsultingSpreeDelayToil()
        {
            void Action()
            {
                if (this.pawn.MentalState is MentalState_Possessed mentalState_InsultingSpree &&
                    Find.TickManager.TicksGame - mentalState_InsultingSpree.LastInsultTicks < 300)
                {
                    return;
                }

                this.pawn.jobs.curDriver.ReadyForNextToil();
            }

            Toil toil = new Toil
            {
                initAction = Action,
                tickAction = Action,
                socialMode = RandomSocialMode.Off,
                defaultCompleteMode = ToilCompleteMode.Never
            };
            return toil;
        }

        private Toil InteractToil()
        {
            return Toils_General.Do(delegate
                                    {
                                        if (!this.pawn.interactions.TryInteractWith(this.Target, InteractionDefOf.Insult))
                                        {
                                            return;
                                        }

                                        if (!(this.pawn.MentalState is MentalState_Possessed mentalState_InsultingSpree))
                                        {
                                            return;
                                        }

                                        mentalState_InsultingSpree.LastInsultTicks = Find.TickManager.TicksGame;
                                        if (mentalState_InsultingSpree.Target == this.Target)
                                        {
                                            mentalState_InsultingSpree.InsultedTargetAtLeastOnce = true;
                                        }
                                    });
        }
    }
}