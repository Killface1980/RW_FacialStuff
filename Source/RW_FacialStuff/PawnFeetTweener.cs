using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class PawnFeetTweener
    {
        private List<Vector3> _tweenedFootPos = new List<Vector3> { Vector3.zero, Vector3.zero };

        private int _lastDrawFrame = -1;

        private List<Vector3> _lastTickSpringFootPos = new List<Vector3> { Vector3.zero, Vector3.zero };

        private const float SpringTightness = 0.35f;

        public List<Vector3> TweenedFootPos
        {
            get
            {
                return this._tweenedFootPos;
            }
        }

        public List<Vector3> LastTickTweenedFeetVelocity
        {
            get
            {
                var list = new List<Vector3>();
                for (var index = 0; index < this.TweenedFootPos.Count; index++)
                {
                    Vector3 feetPos = this.TweenedFootPos[index];
                    Vector3 lastTickSpringPos = this._lastTickSpringFootPos[index];
                    list.Add(feetPos - lastTickSpringPos);
                }

                return list;
            }
        }

        public PawnFeetTweener()
        {
        }

        public void PreFootPosCalculation()
        {
            if (this._lastDrawFrame == RealTime.frameCount)
            {
                return;
            }
            if (this._lastDrawFrame < RealTime.frameCount - 1)
            {
                this.ResetTweenedFootPosToRoot();
            }
            else
            {
                this._lastTickSpringFootPos = this._tweenedFootPos;
                float tickRateMultiplier = Find.TickManager.TickRateMultiplier;
                if (tickRateMultiplier < 5f)
                {
                    for (int i = 0; i < this._tweenedFootPos.Count; i++)
                    {
                        Vector3 a = this.TweenedFootPosRoot()[i] - this._tweenedFootPos[i];
                        float num = SpringTightness * (RealTime.deltaTime * 60f * tickRateMultiplier);
                        if (RealTime.deltaTime > 0.05f)
                        {
                            num = Mathf.Min(num, 1f);
                        }

                        this._tweenedFootPos[i] += a * num;
                        // this.tweenedFeetPos[i].y = this.Feet[i].y;
                    }
                }
                else
                {
                    this._tweenedFootPos = this.TweenedFootPosRoot();
                }
            }
            this._lastDrawFrame = RealTime.frameCount;
        }

        public void ResetTweenedFootPosToRoot()
        {
            this._tweenedFootPos = this.TweenedFootPosRoot();
            this._lastTickSpringFootPos = this._tweenedFootPos;
        }

        public List<Vector3> FootPositions = new List<Vector3> { Vector3.zero, Vector3.zero };

        private List<Vector3> TweenedFootPosRoot()
        {
            return this.FootPositions;
        }

    }
}
