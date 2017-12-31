// ReSharper disable MissingXmlDoc

namespace FacialStuff.Animator
{
    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class PawnEyeWiggler
    {
        #region Private Fields

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

        [NotNull]
        private readonly CompFace compFace;

        private readonly SimpleCurve consciousnessCurve =
            new SimpleCurve { new CurvePoint(0f, 5f), new CurvePoint(0.5f, 2f), new CurvePoint(1f, 1f) };

        private readonly float factorX = 0.02f;

        private readonly float factorY = 0.01f;

        private readonly SimpleCurve painCurve =
            new SimpleCurve { new CurvePoint(0f, 1f), new CurvePoint(0.65f, 1f), new CurvePoint(1f, 2f) };

        private readonly Pawn pawn;

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

        #endregion Private Fields

        #region Public Constructors

        public PawnEyeWiggler(CompFace face)
        {
            this.compFace = face;
            this.pawn = face.Pawn;
        }

        #endregion Public Constructors

        #region Public Properties

        public bool EyeLeftBlinkNow
        {
            get
            {
                bool blinkNow = Find.TickManager.TicksGame >= this.nextBlink + this.jitterLeft;
                return blinkNow;
            }
        }

        public Vector3 EyeMoveL { get; private set; } = new Vector3(0, 0, 0);

        public Vector3 EyeMoveR { get; private set; } = new Vector3(0, 0, 0);

        public bool EyeRightBlinkNow
        {
            get
            {
                bool blinkNow = Find.TickManager.TicksGame >= this.nextBlink + this.jitterRight;
                return blinkNow;
            }
        }

        public int NextBlinkEnd { get; private set; } = -5000;

        #endregion Public Properties

        #region Public Methods

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

                if (this.compFace.bodyStat.EyeRight == PartStatus.Natural)
                {
                    this.EyeMoveR = new Vector3(movePixel * this.flippedX, 0, movePixelY * this.flippedY);
                }

                if (this.compFace.bodyStat.EyeLeft == PartStatus.Natural)
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

        #endregion Public Methods

        #region Private Methods

        private void SetNextBlink(int tickManagerTicksGame)
        {
            // Eye blinking controller
            float ticksTillNextBlink = Rand.Range(60f, 240f);
            float blinkDuration = Rand.Range(10f, 40f);

            // Log.Message(
            // "FS Blinker: " + this.pawn + " - ticksTillNextBlinkORG: " + ticksTillNextBlink.ToString("N0")
            // + " - blinkDurationORG: " + blinkDuration.ToString("N0"));

            // TODO: use a curve for evaluation => more control, precise setting of blinking
            Pawn_HealthTracker health = this.pawn.health;
            float pain = 0f;
            if (health != null)
            {
                if (health.capacities != null)
                {
                    float consciousness =
                        this.consciousnessCurve.Evaluate(health.capacities.GetLevel(PawnCapacityDefOf.Consciousness));

                    ticksTillNextBlink /= consciousness;
                    blinkDuration *= consciousness;
                }

                if (health.hediffSet != null)
                {
                    pain = this.painCurve.Evaluate(health.hediffSet.PainTotal);
                }
            }

            ticksTillNextBlink /= pain;
            blinkDuration *= pain;

            // float factor = Mathf.Lerp(0.1f, 1f, dynamic);
            // ticksTillNextBlink *= factor;
            // blinkDuration /= Mathf.Pow(factor, 3f);

            // Log.Message(
            // "FS Blinker: " + this.pawn + " - Consc: " + dynamic.ToStringPercent() + " - factorC: " + factor.ToString("N2") + " - ticksTillNextBlink: " + ticksTillNextBlink.ToString("N0")
            // + " - blinkDuration: " + blinkDuration.ToString("N0"));
            this.nextBlink = (int)(tickManagerTicksGame + ticksTillNextBlink);
            this.NextBlinkEnd = (int)(this.nextBlink + blinkDuration);

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

        #endregion Private Methods
    }
}