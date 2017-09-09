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

        private Pawn pawn;

        private int nextRotation = -5000;
        private int nextRotationEnd = -5000;

        private RotationDirection rotationMod;

        public void RotatorTick()
        {
            int tickManagerTicksGame = Find.TickManager.TicksGame;

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

                var rand = Rand.Value;
                if (rand < 0.25f)
                {
                    this.rotationMod = RotationDirection.Clockwise;
                }
                else if (rand < 0.5f)
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