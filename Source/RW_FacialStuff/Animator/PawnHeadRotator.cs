using JetBrains.Annotations;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FacialStuff.Animator
{
    public class PawnHeadRotator
    {
        #region Public Fields

        private readonly SimpleCurve _motionCurve =
            new SimpleCurve
            {
                new CurvePoint(0f,   0f),
                new CurvePoint(90f,  1.25f),
                new CurvePoint(270f, -1.25f),
                new CurvePoint(360f, 0f)
            };

        #endregion Public Fields

        #region Private Fields

        private readonly Pawn _pawn;

        private bool _clockwise;
        private Rot4 _currentRot = Rot4.Random;
        private int _headRotation;

        private int _nextRotationEnd = -5000;

        private bool _possessed;
        private RotationDirection _rotationMod;

        private int _rotCount;
        private Thing _target;

        #endregion Private Fields

        #region Public Constructors

        public PawnHeadRotator(Pawn p)
        {
            this._pawn = p;
        }

        #endregion Public Constructors

        #region Public Properties

        public float CurrentMovement => this._motionCurve.Evaluate(this._headRotation);

        #endregion Public Properties

        #region Public Methods

        public void LookAtPawn(Thing t)
        {
            this._target = t;
            this.FaceHead();
            this.SetNextRotation(Find.TickManager.TicksGame + 720);

            // Log.Message(this.pawn + " look at " + p);
        }

        public void RotateRandomly()
        {
            this._possessed = true;
            if (this._rotCount < 1)
            {
                this._rotCount = Rand.Range(12, 28);
                this._clockwise = !this._clockwise;
            }

            this._currentRot.Rotate(this._clockwise ? RotationDirection.Clockwise : RotationDirection.Counterclockwise);
            this._rotCount--;
        }

        public Rot4 Rotation(Rot4 headFacing, bool renderBody)
        {
            if (this._possessed)
            {
                return this._currentRot;
            }

            Rot4 rot = headFacing;
            bool flag = false;
            if (renderBody)
            {
                flag = true;
            }
            else if (!rot.IsHorizontal)
            {
                flag = true;
            }

            if (flag)
            {
                rot.Rotate(this._rotationMod);
            }

            this._currentRot = rot;
            return this._currentRot;
        }

        public void RotatorTick()
        {
            int tickManagerTicksGame = Find.TickManager.TicksGame;

            this._headRotation++;
            if (this._headRotation > 360)
            {
                this._headRotation = 0;
            }

            if (!Controller.settings.UseHeadRotator)
            {
                return;
            }

            if (tickManagerTicksGame > this._nextRotationEnd)
            {
                // Set upnext blinking cycle
                this.SetNextRotation(tickManagerTicksGame);

                // if (GenAI.InDangerousCombat(this.pawn))
                // {
                // this.rotationMod = RotationDirection.None;
                // return;
                // }
                this._target = null;
                this.FaceHead();
            }
            else
            {
                if (tickManagerTicksGame % 10 == 0)
                {
                    this.TrackHead();
                }
            }
        }

        public void SetUnPossessed()
        {
            this._possessed = false;
        }

        #endregion Public Methods

        #region Private Methods

        // Verse.AI.GenAI
        private bool EnemyIsNear([NotNull] Pawn p, float radius)
        {
            bool enemy = false;
            this._target = null;

            if (!p.Spawned)
            {
                return false;
            }

            List<IAttackTarget> potentialTargetsFor = p.Map?.attackTargetsCache?.GetPotentialTargetsFor(p);
            if (!potentialTargetsFor.NullOrEmpty())
            {
                // ReSharper disable once PossibleNullReferenceException
                foreach (IAttackTarget attackTarget in potentialTargetsFor)
                {
                    if (attackTarget == null || attackTarget.ThreatDisabled(p))
                    {
                        continue;
                    }

                    if (!p.Position.InHorDistOf(((Thing)attackTarget).Position, radius))
                    {
                        continue;
                    }

                    enemy = true;
                    break;
                }
            }

            if (!enemy)
            {
                return false;
            }

            Thing thing = (Thing)AttackTargetFinder.BestAttackTarget(
                                                                      p,
                                                                      TargetScanFlags.NeedReachable |
                                                                      TargetScanFlags.NeedThreat,
                                                                      x => x is Pawn,
                                                                      0f,
                                                                      radius,
                                                                      default,
                                                                      3.40282347E+38f,
                                                                      true);

            if (thing == null)
            {
                return false;
            }

            this._target = thing;
            return true;
        }

        private void FaceHead()
        {
            if (this._target == null)
            {
                this.FindClosestTarget();
            }

            if (this._target != null && this._pawn.Position.InHorDistOf(this._target.Position, 6f))
            {
                // if (random)
                // {
                // if (target is Pawn p && p.GetComp<CompFace>() != null)
                // {
                // // Log.Message(p + " look back at " + this.pawn);
                // p.GetComp<CompFace>().HeadRotator.LookAtPawn(this.pawn);
                // }
                // }
                float angle = (this._target.Position - this._pawn.Position).ToVector3().AngleFlat();
                Rot4 rot = Pawn_RotationTracker.RotFromAngleBiased(angle);
                if (rot != this._pawn.Rotation.Opposite)
                {
                    int rotty = this._pawn.Rotation.AsInt - rot.AsInt;
                    switch (rotty)
                    {
                        case 0:
                            this._rotationMod = RotationDirection.None;
                            break;

                        case -1:
                            this._rotationMod = RotationDirection.Clockwise;
                            break;

                        case 1:
                            this._rotationMod = RotationDirection.Counterclockwise;
                            break;
                    }

                    // Log.Message(this.pawn + " now watching " + target.GetThingList(this.pawn.Map));
                    return;
                }
            }
            else
            {
                this._target = null;
            }

            this._rotationMod = RotationDirection.None;
        }

        // RimWorld.JobDriver_StandAndBeSociallyActive
        private void FindClosestTarget()
        {
            if (!_pawn.Spawned)
            {
                return;
            }
            // Watch out for enemies
            Job job = this._pawn.CurJob;
            if (job == null)
            {
                    return;
            }
            else if (!job.targetA.IsValid)
            {
                if (this.EnemyIsNear(this._pawn, 40f))
                {
                    return;
                }
            }

            if (job.def == JobDefOf.SpectateCeremony)
            {
                _target = null;
                return;
            }

            float rand = Rand.Value;

            // Look at each other
            if (rand > 0.5f)
            {
                IntVec3 position = this._pawn.Position;

                // 8 = 1 field; 24 = 2 fields;
                for (int i = 0; i < 8; i++)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    IntVec3 intVec = position + GenRadial.RadialPattern[i];
                    if (intVec.InBounds(this._pawn.Map))
                    {
                        Thing thing = intVec.GetThingList(this._pawn.Map)?.Find(x => x is Pawn);

                        if (!(thing is Pawn otherPawn) || otherPawn == this._pawn || !otherPawn.Spawned) // || otherPawn.Dead || otherPawn.Downed)
                        {
                            continue;
                        }
                        
                        if (!_pawn.CanSee(otherPawn)) continue;

                        // Log.Message(this.pawn + " will look at random pawn " + thing);
                        this._target = otherPawn;
                        return;
                    }
                }
            }

            this._target = null;
        }

        private void SetNextRotation(int tickManagerTicksGame)
        {
            float blinkDuration = Rand.Range(120f, 240f);

            this._nextRotationEnd = (int)(tickManagerTicksGame + blinkDuration);
        }

        private void TrackHead()
        {
            if (this._target != null)
            {
                this.FaceHead();
            }
        }

        #endregion Private Methods
    }
}