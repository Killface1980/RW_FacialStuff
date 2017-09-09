namespace FacialStuff
{
    using RimWorld;

    using UnityEngine;

    using Verse;

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

                // Make them smile.
                if (this.pawn.pather.Moving)
                {
                    this.rotationMod = RotationDirection.None;
                    return;
                }

                float rand = Rand.Value;
                if (rand < 0.15f)
                {
                    this.rotationMod = RotationDirection.Clockwise;
                }
                else if (rand < 0.3f)
                {
                    this.rotationMod = RotationDirection.Counterclockwise;
                }
                else
                {
                    this.rotationMod = RotationDirection.None;
                }
            }

        }

        private void SetNextRotation(int tickManagerTicksGame)
        {
            float blinkDuration = Rand.Range(120f, 180f);

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