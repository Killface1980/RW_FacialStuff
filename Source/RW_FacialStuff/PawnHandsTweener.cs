using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class PawnHandsTweener
    {
        #region Public Fields

        public readonly List<Vector3> HandPositions;

        #endregion Public Fields

        #region Private Fields

        private const float SoftSpringTightness   = 0.15f;
        private const float MediumSpringTightness = 0.2f;
        private const float HardSpringTightness   = 0.3f;
        private const float StiffSpringTightness   = 0.6f;

        private readonly List<float>   _springTightness;
        private readonly List<int>     _lastDrawFrame;
        private readonly List<Vector3> _tweenedHandsPos;

        private List<Vector3> _lastTickSpringHandPos;

        #endregion Private Fields

        #region Public Constructors

        public PawnHandsTweener()
        {
            this.HandPositions          = new List<Vector3>();
            this._lastDrawFrame         = new List<int>();
            this._lastTickSpringHandPos = new List<Vector3>();
            this._tweenedHandsPos       = new List<Vector3>();

            for (int i = 0; i < (int) TweenThing.Max; i++)
            {
                this.HandPositions.Add(Vector3.zero);
                this._lastDrawFrame.Add(-1);
                this._lastTickSpringHandPos.Add(Vector3.zero);
                this._tweenedHandsPos.Add(Vector3.zero);
            }

            this._springTightness =
            new List<float> {SoftSpringTightness, MediumSpringTightness, HardSpringTightness, StiffSpringTightness };
        }

        #endregion Public Constructors

        #region Public Properties

        public List<Vector3> LastTickTweenedHandsVelocity
        {
            get
            {
                List<Vector3> list = new List<Vector3>();
                for (int index = 0; index < this.TweenedHandsPos.Count; index++)
                {
                    Vector3 handsPos          = this.TweenedHandsPos[index];
                    Vector3 lastTickSpringPos = this._lastTickSpringHandPos[index];
                    list.Add(handsPos - lastTickSpringPos);
                }

                return list;
            }
        }

        public List<Vector3> TweenedHandsPos
        {
            get { return this._tweenedHandsPos; }
        }

        #endregion Public Properties

        #region Public Methods

        public void PreHandPosCalculation(TweenThing      tweenThing, bool isMoving,
                                          SpringTightness spring = SpringTightness.Medium)
        {
            int side = (int)tweenThing;

            if (isMoving || MainTabWindow_Animator.isOpen)
            {
                this.ResetTweenedHandPosToRoot(side);
                return;
            }

            if (this._lastDrawFrame[side] == RealTime.frameCount)
            {
                return;
            }

            if (this._lastDrawFrame[side] < RealTime.frameCount - 1)
            {
                this.ResetTweenedHandPosToRoot(side);
            }
            else
            {
                this._lastTickSpringHandPos[side] = this._tweenedHandsPos[side];
                float tickRateMultiplier          = Find.TickManager.TickRateMultiplier;
                if (tickRateMultiplier < 5f)
                {
                    Vector3 a        = this.TweenedHandPosRoot(side) - this._tweenedHandsPos[side];

                    float tightness = this._springTightness[(int)spring];
                    float progress = tightness * (RealTime.deltaTime * 60f * tickRateMultiplier);
                    if (RealTime.deltaTime > 0.05f)
                    {
                        progress = Mathf.Min(progress, 1f);
                    }

                    Vector3 tweenedHandsPo      = this._tweenedHandsPos[side] + a * progress;
                    tweenedHandsPo.y            = this.HandPositions[side].y;
                    this._tweenedHandsPos[side] = tweenedHandsPo;
                }
                else
                {
                    this._tweenedHandsPos[side] = this.TweenedHandPosRoot(side);
                }
            }

            this._lastDrawFrame[side] = RealTime.frameCount;
        }

        public void ResetTweenedHandPosToRoot(int side)
        {
            this._tweenedHandsPos[side]       = this.TweenedHandPosRoot(side);
            this._lastTickSpringHandPos[side] = this._tweenedHandsPos[side];
        }

        #endregion Public Methods

        #region Internal Methods

        internal void PreHandPosCalculation(bool isMoving)
        {
            this.PreHandPosCalculation(TweenThing.HandLeft, isMoving);
            this.PreHandPosCalculation(TweenThing.HandRight, isMoving);
        }

        #endregion Internal Methods

        #region Private Methods

        private Vector3 TweenedHandPosRoot(int side)
        {
            return this.HandPositions[side];
        }

        #endregion Private Methods
    }
}