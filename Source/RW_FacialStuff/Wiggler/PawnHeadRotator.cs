namespace FacialStuff.Wiggler
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
        private int lastBlinkended;
        public int NextBlink => this.nextBlink;
        public int NextBlinkEnd => this.nextBlinkEnd;
        private int nextBlink = -5000;
        private int nextBlinkEnd = -5000;

        private RotationDirection rotationMod;

        public void RotatorTick()
        {
            int tickManagerTicksGame = Find.TickManager.TicksGame;

            if (tickManagerTicksGame > this.NextBlinkEnd)
            {
                // Set upnext blinking cycle
                this.SetNextBlink(tickManagerTicksGame);

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

        private void SetNextBlink(int tickManagerTicksGame)
        {
            // Eye blinking controller
            float ticksTillNextBlink = Rand.Range(120f, 240f);
            float blinkDuration = Rand.Range(10f, 40f);

            // Log.Message(
            // "FS Blinker: " + this.pawn + " - ticksTillNextBlinkORG: " + ticksTillNextBlink.ToString("N0")
            // + " - blinkDurationORG: " + blinkDuration.ToString("N0"));



            // float factor = Mathf.Lerp(0.1f, 1f, dynamic);
            // ticksTillNextBlink *= factor;
            // blinkDuration /= Mathf.Pow(factor, 3f);

            // Log.Message(
            // "FS Blinker: " + this.pawn + " - Consc: " + dynamic.ToStringPercent() + " - factorC: " + factor.ToString("N2") + " - ticksTillNextBlink: " + ticksTillNextBlink.ToString("N0")
            // + " - blinkDuration: " + blinkDuration.ToString("N0"));
            this.nextBlink = (int)(tickManagerTicksGame + ticksTillNextBlink);
            this.nextBlinkEnd = (int)(this.NextBlink + blinkDuration);


            // this.JitterLeft = 1f;
            // this.JitterRight = 1f;

            // blinkRate = Mathf.Lerp(2f, 0.25f, this.pawn.needs.rest.CurLevel);


            this.lastBlinkended = tickManagerTicksGame;
        }

        public Rot4 Rotation(Rot4 headFacing)
        {
            Rot4 rot = headFacing;
            rot.Rotate(this.rotationMod);
            return rot;
        }
    }
}