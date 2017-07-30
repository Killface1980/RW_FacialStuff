namespace FacialStuff.Wiggler

{
    using RimWorld;

    using UnityEngine;

    using Verse;

    public class PawnEyeWiggler
    {
        #region Constructors

        public PawnEyeWiggler(Pawn pawn)
        {
            this.pawn = pawn;
        }

        #endregion Constructors

        #region Fields

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

        private Vector3 eyeMoveL = new Vector3(0, 0, 0);

        private Vector3 eyeMoveR = new Vector3(0, 0, 0);

        private float factorX = 0.02f;

        private float factorY = 0.01f;

        private float flippedX;

        private float flippedY;

        private bool halfAnimX;

        private bool halfAnimY;

        private bool isAsleep;

        private int jitterLeft;

        private int jitterRight;

        private int lastBlinkended;

        private bool leftCanBlink;

        private bool moveX;

        private bool moveY;

        private int nextBlink = -5000;

        private int nextBlinkEnd = -5000;

        private Pawn pawn;

        private bool rightCanBlink;

        #endregion Fields

        #region Properties

        public Vector3 EyeMoveL
        {
            get => this.eyeMoveL;
            set => this.eyeMoveL = value;
        }

        public Vector3 EyeMoveR
        {
            get => this.eyeMoveR;
            set => this.eyeMoveR = value;
        }

        public bool IsAsleep
        {
            get => this.isAsleep;
            set => this.isAsleep = value;
        }

        public int JitterLeft
        {
            get => this.jitterLeft;
            set => this.jitterLeft = value;
        }

        public int JitterRight
        {
            get => this.jitterRight;
            set => this.jitterRight = value;
        }

        public bool LeftCanBlink
        {
            get => this.leftCanBlink;
            set => this.leftCanBlink = value;
        }

        public int NextBlink
        {
            get => this.nextBlink;
            set => this.nextBlink = value;
        }

        public int NextBlinkEnd
        {
            get => this.nextBlinkEnd;
            set => this.nextBlinkEnd = value;
        }

        public bool RightCanBlink
        {
            get => this.rightCanBlink;
            set => this.rightCanBlink = value;
        }

        #endregion Properties

        #region Methods

        public void WigglerTick()
        {
            int tickManagerTicksGame = Find.TickManager.TicksGame;

            float x = Mathf.InverseLerp(this.lastBlinkended, this.NextBlink, tickManagerTicksGame);
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

                if (this.RightCanBlink)
                {
                    this.EyeMoveR = new Vector3(movePixel * this.flippedX, 0, movePixelY * this.flippedY);
                }

                if (this.LeftCanBlink)
                {
                    this.EyeMoveL = new Vector3(movePixel * this.flippedX, 0, movePixelY * this.flippedY);
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

             float dynamic = this.pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
             float factor = Mathf.Lerp(0.125f, 1f, dynamic);
             ticksTillNextBlink *= factor;
             blinkDuration /= factor  * 2;
          // Log.Message(
          // "FS Blinker: " + this.pawn + " - Consc: " + dynamic.ToStringPercent() + " - factorC: " + factor.ToString("N2") + " - ticksTillNextBlink: " + ticksTillNextBlink.ToString("N0")
          // + " - blinkDuration: " + blinkDuration.ToString("N0"));
            this.NextBlink = (int)(tickManagerTicksGame + ticksTillNextBlink);
            this.NextBlinkEnd = (int)(this.NextBlink + blinkDuration);

            if (this.pawn.CurJob != null && this.pawn.jobs.curDriver.asleep)
            {
                this.IsAsleep = true;
                return;
            }

            this.IsAsleep = false;

            // this.JitterLeft = 1f;
            // this.JitterRight = 1f;

            // blinkRate = Mathf.Lerp(2f, 0.25f, this.pawn.needs.rest.CurLevel);

            // range *= (int)blinkRate;
            // blinkDuration /= (int)this.blinkRate;
            if (Rand.Value > 0.9f)
            {
                // early "nerous" blinking. I guss positive values have no effect ...
                this.JitterLeft = (int)Rand.Range(-10f, 90f);
                this.JitterRight = (int)Rand.Range(-10f, 90f);
            }
            else
            {
                this.JitterLeft = 0;
                this.JitterRight = 0;
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

        #endregion Methods
    }
}