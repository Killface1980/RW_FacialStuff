namespace FacialStuff.Wiggler

{
    using RimWorld;

    using UnityEngine;

    using Verse;

    public class PawnEyeWiggler
    {
        private bool moveX;

        private bool moveY;

        private Pawn pawn;

        private bool halfAnimX;

        private bool halfAnimY;

        public int jitterLeft;

        public int jitterRight;
        private float flippedX;

        private float flippedY;


        public Vector3 EyemoveL = new Vector3(0, 0, 0);

        public Vector3 EyemoveR = new Vector3(0, 0, 0);

        private int lastBlinkended;

        public bool leftCanBlink;

        public int nextBlink = -5000;

        public bool asleep;

        public int nextBlinkEnd = -5000;

        public bool rightCanBlink;
        private float factorX = 0.02f;

        private float factorY = 0.01f;
        private static readonly SimpleCurve EyeMotionFullCurve =
            new SimpleCurve
                {
                    new CurvePoint(0f, 0f),
                    new CurvePoint(0.05f, -1f),
                    new CurvePoint(0.65f, 1f),
                    new CurvePoint(0.85f, 0f)
                };

        private static readonly SimpleCurve EyeMotionHalfCurve =
            new SimpleCurve
                {
                    new CurvePoint(0f, 0f),
                    new CurvePoint(0.1f, 1f),
                    new CurvePoint(0.65f, 1f),
                    new CurvePoint(0.75f, 0f)
                };
        public PawnEyeWiggler(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void WigglerTick()
        {
            int tickManagerTicksGame = Find.TickManager.TicksGame;

            float x = Mathf.InverseLerp(this.lastBlinkended, this.nextBlink, tickManagerTicksGame);
            float movePixel = 0f;
            float movePixelY = 0f;

            if (this.moveX || this.moveY)
            {
                if (this.moveX)
                {
                    if (this.halfAnimX)
                    {
                        movePixel = EyeMotionHalfCurve.Evaluate(x) * this.factorX;
                    }
                    else
                    {
                        movePixel = EyeMotionFullCurve.Evaluate(x) * this.factorX;
                    }
                }

                if (this.moveY)
                {
                    if (this.halfAnimY)
                    {
                        movePixelY = EyeMotionHalfCurve.Evaluate(x) * this.factorY;
                    }
                    else
                    {
                        movePixelY = EyeMotionFullCurve.Evaluate(x) * this.factorY;
                    }
                }

                if (this.rightCanBlink)
                {
                    this.EyemoveR = new Vector3(movePixel * this.flippedX, 0, movePixelY * this.flippedY);
                }

                if (this.leftCanBlink)
                {
                    this.EyemoveL = new Vector3(movePixel * this.flippedX, 0, movePixelY * this.flippedY);
                }
            }

            if (tickManagerTicksGame > this.nextBlinkEnd)
            {
                // Set upnext blinking cycle
                this.SetNextBlink(tickManagerTicksGame);

                // Make them smile.
            }
        }

        private void SetNextBlink(int tickManagerTicksGame)
        {
            // Eye blinking controller
            float ticksTillNextBlink = Rand.Range(60f, 240f);
            float blinkDuration = Rand.Range(5f, 20f);

            // Log.Message(
            // "FS Blinker: " + this.pawn + " - ticksTillNextBlinkORG: " + ticksTillNextBlink.ToString("N0")
            // + " - blinkDurationORG: " + blinkDuration.ToString("N0"));
            float dynamic = this.pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
            float factor = Mathf.Lerp(0.125f, 1f, dynamic);

            float dynamic2 = this.pawn.needs.rest.CurLevel;
            float factor2 = Mathf.Lerp(0.125f, 1f, dynamic2);

            ticksTillNextBlink *= factor * factor2;
            blinkDuration /= factor * factor * factor2 * 2;

            // Log.Message(
            // "FS Blinker: " + this.pawn + " - Consc: " + dynamic.ToStringPercent() + " - factorC: " + factor.ToString("N2") + " - Rest: "
            // + dynamic2.ToStringPercent() + " - factorR: " + factor2.ToString("N2") + " - ticksTillNextBlink: " + ticksTillNextBlink.ToString("N0")
            // + " - blinkDuration: " + blinkDuration.ToString("N0"));
            this.nextBlink = (int)(tickManagerTicksGame + ticksTillNextBlink);
            this.nextBlinkEnd = (int)(this.nextBlink + blinkDuration);

            if (this.pawn.CurJob != null && this.pawn.jobs.curDriver.asleep)
            {
                this.asleep = true;
                return;
            }

            this.asleep = false;

            // this.jitterLeft = 1f;
            // this.jitterRight = 1f;

            // blinkRate = Mathf.Lerp(2f, 0.25f, this.pawn.needs.rest.CurLevel);

            // range *= (int)blinkRate;
            // blinkDuration /= (int)this.blinkRate;
            if (Rand.Value > 0.9f)
            {
                // early "nerous" blinking. I guss positive values have no effect ...
                this.jitterLeft = (int)Rand.Range(-10f, 90f);
                this.jitterRight = (int)Rand.Range(-10f, 90f);
            }
            else
            {
                this.jitterLeft = 0;
                this.jitterRight = 0;
            }

            // only animate eye movement if animation lasts at least 2.5 seconds
            if (ticksTillNextBlink > 80f)
            {
                this.moveX = Rand.Value > 0.7f;
                this.moveY = Rand.Value > 0.85f;
                this.halfAnimX = Rand.Value > 0.3f;
                this.halfAnimY = Rand.Value > 0.3f;
                this.flippedX = Rand.Range(-1f, 1f);
                this.flippedY = Rand.Range(-1f, 1f);
            }
            else
            {
                this.moveX = this.moveY = false;
            }

            this.lastBlinkended = tickManagerTicksGame;
        }

    }
}
