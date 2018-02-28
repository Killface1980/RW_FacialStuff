// ReSharper disable MissingXmlDoc

using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.Animator
{
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
        private readonly CompFace _compFace;

        private readonly SimpleCurve _consciousnessCurve =
            new SimpleCurve { new CurvePoint(0f, 5f), new CurvePoint(0.5f, 2f), new CurvePoint(1f, 1f) };

        private readonly float _factorX = 0.02f;

        private readonly float _factorY = 0.01f;

        private readonly SimpleCurve _painCurve =
            new SimpleCurve { new CurvePoint(0f, 1f), new CurvePoint(0.65f, 1f), new CurvePoint(1f, 2f) };

        private readonly Pawn _pawn;

        private float _flippedX;

        private float _flippedY;

        private bool _halfAnimX;

        private bool _halfAnimY;

        private int _jitterLeft;

        private int _jitterRight;

        private int _lastBlinkended;

        private bool _moveX;

        private bool _moveY;

        private int _nextBlink = -5000;

        #endregion Private Fields

        #region Public Constructors

        public PawnEyeWiggler(CompFace face)
        {
            this._compFace = face;
            this._pawn = face.Pawn;
        }

        #endregion Public Constructors

        #region Public Properties

        public bool EyeLeftBlinkNow
        {
            get
            {
                bool blinkNow = Find.TickManager.TicksGame >= this._nextBlink + this._jitterLeft;
                return blinkNow;
            }
        }

        public Vector3 EyeMoveL { get; private set; } = new Vector3(0, 0, 0);

        public Vector3 EyeMoveR { get; private set; } = new Vector3(0, 0, 0);

        public bool EyeRightBlinkNow
        {
            get
            {
                bool blinkNow = Find.TickManager.TicksGame >= this._nextBlink + this._jitterRight;
                return blinkNow;
            }
        }

        private int NextBlinkEnd { get; set; } = -5000;

        #endregion Public Properties

        #region Public Methods

        public void WigglerTick()
        {
            if (!Controller.settings.MakeThemBlink)
            {
                return;
            }

            int tickManagerTicksGame = Find.TickManager.TicksGame;

            float x = Mathf.InverseLerp(this._lastBlinkended, this._nextBlink, tickManagerTicksGame);
            float movePixel = 0f;
            float movePixelY = 0f;

            if (this._moveX || this._moveY)
            {
                if (this._moveX)
                {
                    if (this._halfAnimX)
                    {
                        movePixel = EyeMotionHalfCurve.Evaluate(x) * this._factorX;
                    }
                    else
                    {
                        movePixel = EyeMotionFullCurve.Evaluate(x) * this._factorX;
                    }
                }

                if (this._moveY)
                {
                    if (this._halfAnimY)
                    {
                        movePixelY = EyeMotionHalfCurve.Evaluate(x) * this._factorY;
                    }
                    else
                    {
                        movePixelY = EyeMotionFullCurve.Evaluate(x) * this._factorY;
                    }
                }

                if (this._compFace.BodyStat.EyeRight == PartStatus.Natural)
                {
                    this.EyeMoveR = new Vector3(movePixel * this._flippedX, 0, movePixelY * this._flippedY);
                }

                if (this._compFace.BodyStat.EyeLeft == PartStatus.Natural)
                {
                    this.EyeMoveL = new Vector3(movePixel * this._flippedX, 0, movePixelY * this._flippedY);
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
            Pawn_HealthTracker health = this._pawn.health;
            float pain = 0f;
            if (health != null)
            {
                if (health.capacities != null)
                {
                    float consciousness = this._consciousnessCurve.Evaluate(health.capacities.GetLevel(PawnCapacityDefOf.Consciousness));

                    ticksTillNextBlink /= consciousness;
                    blinkDuration *= consciousness;
                }

                if (health.hediffSet != null)
                {
                    pain = this._painCurve.Evaluate(health.hediffSet.PainTotal);
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
            this._nextBlink = (int)(tickManagerTicksGame + ticksTillNextBlink);
            this.NextBlinkEnd = (int)(this._nextBlink + blinkDuration);

            // this.JitterLeft = 1f;
            // this.JitterRight = 1f;

            // blinkRate = Mathf.Lerp(2f, 0.25f, this.pawn.needs.rest.CurLevel);

            // range *= (int)blinkRate;
            // blinkDuration /= (int)this.blinkRate;
            if (Rand.Value > 0.9f)
            {
                // early "nerous" blinking. I guss positive values have no effect ...
                this._jitterLeft = (int)Rand.Range(-10f, 90f);
                this._jitterRight = (int)Rand.Range(-10f, 90f);
            }
            else
            {
                this._jitterLeft = 0;
                this._jitterRight = 0;
            }

            // only animate eye movement if animation lasts at least 2.5 seconds
            if (ticksTillNextBlink > 80f)
            {
                this._moveX = Rand.Value > 0.7f;
                this._moveY = Rand.Value > 0.85f;
                this._halfAnimX = Rand.Value > 0.3f;
                this._halfAnimY = Rand.Value > 0.3f;
                this._flippedX = Rand.Range(-1f, 1f);
                this._flippedY = Rand.Range(-1f, 1f);
            }
            else
            {
                this._moveX = this._moveY = false;
            }

            this._lastBlinkended = tickManagerTicksGame;
        }

        #endregion Private Methods
    }
}