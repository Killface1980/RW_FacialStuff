using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class PawnHandsTweener
    {
        private readonly List<Vector3> _tweenedHandsPos = new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero };

        private readonly List<int> _lastDrawFrame = new List<int> { -1, -1, -1 };

        private List<Vector3> _lastTickSpringHandPos = new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero };

        private const float SpringTightness = 0.25f;

        public List<Vector3> TweenedHandsPos
        {
            get
            {
                return this._tweenedHandsPos;
            }
        }

        public List<Vector3> LastTickTweenedHandsVelocity
        {
            get
            {
                var list = new List<Vector3>();
                for (var index = 0; index < this.TweenedHandsPos.Count; index++)
                {
                    Vector3 handsPos = this.TweenedHandsPos[index];
                    Vector3 lastTickSpringPos = this._lastTickSpringHandPos[index];
                    list.Add(handsPos - lastTickSpringPos);
                }

                return list;
            }
        }

        public PawnHandsTweener()
        {
        }

        public void PreHandPosCalculation(int side, float? tight = null)
        {
            float tightness = tight ?? SpringTightness;
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
                float tickRateMultiplier = Find.TickManager.TickRateMultiplier;
                if (tickRateMultiplier < 5f)
                {

                    Vector3 a = this.TweenedHandPosRoot(side) - this._tweenedHandsPos[side];
                    float progress = tightness * (RealTime.deltaTime * 60f * tickRateMultiplier);
                    if (RealTime.deltaTime > 0.05f)
                    {
                        progress = Mathf.Min(progress, 1f);
                    }

                    Vector3 tweenedHandsPo = this._tweenedHandsPos[side] + a * progress;
                    tweenedHandsPo.y = this.HandPositions[side].y;
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
            this._tweenedHandsPos[side] = this.TweenedHandPosRoot(side);
            this._lastTickSpringHandPos = this._tweenedHandsPos;
        }

        public readonly List<Vector3> HandPositions = new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero };

        private Vector3 TweenedHandPosRoot(int side)
        {
            return this.HandPositions[side];
        }

        internal void PreHandPosCalculation()
        {
            this.PreHandPosCalculation(0);
            this.PreHandPosCalculation(1);
        }
    }
}
