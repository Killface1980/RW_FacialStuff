namespace FacialStuff
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

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

        public int wheelRotation = 0;

        public float CurrentMovement
        {
            get
            {
                return this.MotionCurve.Evaluate(this.wheelRotation);
            }

        }
        // Verse.AI.GenAI
        public static bool EnemyIsNear([NotNull] Pawn p, float radius, out IntVec3 attacker)
        {
            attacker = IntVec3.Zero;
            bool enemy = false;
            if (!p.Spawned)
            {
                return false;
            }

            List<IAttackTarget> potentialTargetsFor = p.Map.attackTargetsCache.GetPotentialTargetsFor(p);
            for (int i = 0; i < potentialTargetsFor.Count; i++)
            {
                IAttackTarget attackTarget = potentialTargetsFor[i];
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
                }
                else
                {
                    enemy = false;
                }
            }
            return enemy;
        }

        // RimWorld.JobDriver_StandAndBeSociallyActive
        private IntVec3 FindClosestTarget()
        {
            IntVec3 position = this.pawn.Position;

            // Watch out for enemies
            if (EnemyIsNear(this.pawn, 40f, out IntVec3 vec))
            {
                return vec;
            }
            float rand = Rand.Value;

            // Look at each other
            if (rand > 0.5f)
            {
                for (int i = 0; i < 24; i++)
                {
                    IntVec3 intVec = position + GenRadial.RadialPattern[i];
                    if (intVec.InBounds(this.pawn.Map))
                    {
                        Thing thing = intVec.GetThingList(this.pawn.Map).Find((Thing x) => x is Pawn);

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
            if (rand > 0.25f)
            {
                if (this.pawn.CurJob.targetA != null)
                {
                    return this.pawn.CurJob.targetA.Cell;
                }
            }

            return IntVec3.Zero;
        }


        public void RotatorTick()
        {

            int tickManagerTicksGame = Find.TickManager.TicksGame;

            this.wheelRotation++;
            if (this.wheelRotation > 360)
            {
                this.wheelRotation = 0;
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
            float blinkDuration = Rand.Range(60f, 120f);

            this.nextRotationEnd = (int)(tickManagerTicksGame + blinkDuration);
        }

        public Rot4 Rotation(Rot4 headFacing)
        {
            Rot4 rot = headFacing;
            rot.Rotate(this.rotationMod);
            return rot;
        }
    }
}