namespace FacialStuff.Animator
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using Verse;
    using Verse.AI;

    public class PawnHeadRotator
    {
        public PawnHeadRotator(Pawn p)
        {
            this.pawn = p;
        }

        public readonly SimpleCurve MotionCurve =
            new SimpleCurve
                {
                    new CurvePoint(0f, 0f),
                    new CurvePoint(90f, 1.25f),
                    new CurvePoint(270f, -1.25f),
                    new CurvePoint(360f, 0f)
                };

        private Pawn pawn;

        private int nextRotation = -5000;
        private int nextRotationEnd = -5000;

        private RotationDirection rotationMod;

        private int headRotation = 0;

        public float CurrentMovement
        {
            get
            {
                return this.MotionCurve.Evaluate(this.headRotation);
            }

        }
        // Verse.AI.GenAI
        private static bool EnemyIsNear([NotNull] Pawn p, float radius, out IntVec3 attacker)
        {
            bool enemy = false;
            attacker = IntVec3.Zero;

            if (!p.Spawned)
            {
                return false;
            }

            List<IAttackTarget> potentialTargetsFor = p.Map.attackTargetsCache.GetPotentialTargetsFor(p);
            foreach (IAttackTarget attackTarget in potentialTargetsFor)
            {
                if (!attackTarget.ThreatDisabled())
                {
                    if (p.Position.InHorDistOf(((Thing)attackTarget).Position, radius))
                    {
                        enemy = true;
                        break;
                    }
                }
            }
            if (enemy)
            {
                Thing thing = (Thing)AttackTargetFinder.BestAttackTarget(
                    p,
                    TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat,
                    (Thing x) => x is Pawn,
                    0f,
                    radius,
                    default(IntVec3),
                    3.40282347E+38f,
                    true);

                if (thing != null)
                {
                    attacker = thing.Position;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        // RimWorld.JobDriver_StandAndBeSociallyActive
        private IntVec3 FindClosestTarget()
        {
            // Watch out for enemies
            if (EnemyIsNear(this.pawn, 40f, out IntVec3 vec))
            {
                return vec;
            }

            float rand = Rand.Value;

            // Look at each other
            if (rand > 0.5f)
            {
                IntVec3 position = this.pawn.Position;

                // 8=1field; 24 =2 fields;
                for (int i = 0; i < 8; i++)
                {
                    IntVec3 intVec = position + GenRadial.RadialPattern[i];
                    if (intVec.InBounds(this.pawn.Map))
                    {
                        Thing thing = intVec.GetThingList(this.pawn.Map).Find(x => x is Pawn);

                        if (thing != null && thing != this.pawn)
                        {
                            if (GenSight.LineOfSight(position, intVec, this.pawn.Map, false, null, 0, 0))
                            {
                                return thing.Position;
                            }
                        }
                    }
                }
            }

            // Look at current target ...
            Job job = this.pawn.CurJob;
            if (job != null && job.targetA.IsValid)
            {
                IntVec3 findClosestTarget = this.pawn.CurJob.targetA.Cell;
                if (this.pawn.Position.InHorDistOf(findClosestTarget, 5f))
                {
                    return findClosestTarget;
                }
            }

            return IntVec3.Zero;
        }

        public void RotatorTick()
        {

            int tickManagerTicksGame = Find.TickManager.TicksGame;

            this.headRotation++;
            if (this.headRotation > 360)
            {
                this.headRotation = 0;
            }

            if (tickManagerTicksGame > this.nextRotationEnd)
            {
                // Set upnext blinking cycle
                this.SetNextRotation(tickManagerTicksGame);

                // if (GenAI.InDangerousCombat(this.pawn))
                // {
                //     this.rotationMod = RotationDirection.None;
                //     return;
                // }

                IntVec3 target = this.FindClosestTarget();

                // Make them smile.
                // if (this.pawn.pather.Moving)
                // {
                //     this.rotationMod = RotationDirection.None;
                //     return;
                // }
                if (target != IntVec3.Zero)
                {
                    float angle = (target - this.pawn.Position).ToVector3().AngleFlat();
                    Rot4 rot = PawnRotator.RotFromAngleBiased(angle);
                    if (rot != this.pawn.Rotation.Opposite)
                    {
                        int rotty = this.pawn.Rotation.AsInt - rot.AsInt;
                        switch (rotty)
                        {
                            case 0:
                                this.rotationMod = RotationDirection.None;
                                break;
                            case -1:
                                this.rotationMod = RotationDirection.Clockwise;
                                break;
                            case 1:
                                this.rotationMod = RotationDirection.Counterclockwise;
                                break;
                        }
                        //  Log.Message(this.pawn + " now watching " + target.GetThingList(this.pawn.Map));
                        return;
                    }
                }
                this.rotationMod = RotationDirection.None;
            }

        }

        private void SetNextRotation(int tickManagerTicksGame)
        {
            float blinkDuration = Rand.Range(30f, 90f);

            this.nextRotationEnd = (int)(tickManagerTicksGame + blinkDuration);
        }

        public Rot4 Rotation(Rot4 headFacing, bool renderBody)
        {
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
                rot.Rotate(this.rotationMod);
            }
            return rot;
        }
    }
}