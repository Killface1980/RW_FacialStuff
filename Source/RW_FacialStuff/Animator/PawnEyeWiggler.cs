// ReSharper disable MissingXmlDoc

namespace FacialStuff.Animator
{
    using RimWorld;

    using UnityEngine;

    using Verse;

    public class PawnEyeWiggler
    {
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

        private readonly SimpleCurve consciousnessCurve =
            new SimpleCurve { new CurvePoint(0f, 5f), new CurvePoint(0.5f, 2f), new CurvePoint(1f, 1f) };

        private readonly float factorX = 0.02f;

        private readonly float factorY = 0.01f;

        private readonly SimpleCurve painCurve =
            new SimpleCurve { new CurvePoint(0f, 1f), new CurvePoint(0.65f, 1f), new CurvePoint(1f, 2f) };

        private readonly Pawn pawn;

        private Vector3 eyeMoveL = new Vector3(0, 0, 0);

        private Vector3 eyeMoveR = new Vector3(0, 0, 0);

        private float flippedX;

        private float flippedY;

        private bool halfAnimX;

        private bool halfAnimY;

        private int jitterLeft;

        private int jitterRight;

        private int lastBlinkended;

        private bool moveX;

        private bool moveY;

        private int nextBlink = -5000;

        private int nextBlinkEnd = -5000;

        public PawnEyeWiggler(Pawn p)
        {
            this.pawn = p;
            this.EyeLeftCanBlink = true;
            this.EyeRightCanBlink = true;
        }

        public bool EyeLeftBlinkNow
        {
            get
            {
                bool blinkNow = Find.TickManager.TicksGame >= this.nextBlink + this.jitterLeft;
                return blinkNow;
            }
        }

        public bool EyeLeftCanBlink { get; set; }

        public Vector3 EyeMoveL => this.eyeMoveL;

        public Vector3 EyeMoveR => this.eyeMoveR;

        public bool EyeRightBlinkNow
        {
            get
            {
                bool blinkNow = Find.TickManager.TicksGame >= this.nextBlink + this.jitterRight;
                return blinkNow;
            }
        }

        public bool EyeRightCanBlink { get; set; }

        public int NextBlinkEnd => this.nextBlinkEnd;

        public void WigglerTick()
        {
            if (!Controller.settings.MakeThemBlink)
            {
                return;
            }

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

                if (this.EyeRightCanBlink)
                {
                    this.eyeMoveR = new Vector3(movePixel * this.flippedX, 0, movePixelY * this.flippedY);
                }

                if (this.EyeLeftCanBlink)
                {
                    this.eyeMoveL = new Vector3(movePixel * this.flippedX, 0, movePixelY * this.flippedY);
                }
            }

            if (tickManagerTicksGame > this.NextBlinkEnd)
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
            float blinkDuration = Rand.Range(10f, 40f);

            // Log.Message(
            // "FS Blinker: " + this.pawn + " - ticksTillNextBlinkORG: " + ticksTillNextBlink.ToString("N0")
            // + " - blinkDurationORG: " + blinkDuration.ToString("N0"));

            // TODO: use a curve for evaluation => more control, precise setting of blinking
            float consciousness =
                this.consciousnessCurve.Evaluate(this.pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness));

            ticksTillNextBlink /= consciousness;
            blinkDuration *= consciousness;

            float pain = this.painCurve.Evaluate(this.pawn.health.hediffSet.PainTotal);

            ticksTillNextBlink /= pain;
            blinkDuration *= pain;

            // float factor = Mathf.Lerp(0.1f, 1f, dynamic);
            // ticksTillNextBlink *= factor;
            // blinkDuration /= Mathf.Pow(factor, 3f);

            // Log.Message(
            // "FS Blinker: " + this.pawn + " - Consc: " + dynamic.ToStringPercent() + " - factorC: " + factor.ToString("N2") + " - ticksTillNextBlink: " + ticksTillNextBlink.ToString("N0")
            // + " - blinkDuration: " + blinkDuration.ToString("N0"));
            this.nextBlink = (int)(tickManagerTicksGame + ticksTillNextBlink);
            this.nextBlinkEnd = (int)(this.nextBlink + blinkDuration);

            // this.JitterLeft = 1f;
            // this.JitterRight = 1f;

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