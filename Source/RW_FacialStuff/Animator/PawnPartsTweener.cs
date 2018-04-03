using System.Collections.Generic;
using FacialStuff.AnimatorWindows;
using FacialStuff.Tweener;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FacialStuff.Animator
{
    public class PawnPartsTweener
    {
        #region Private Fields

        private const float SoftSpringTightness = 0.05f;
        private const float MediumSpringTightness = 0.15f;
        private const float HardSpringTightness = 0.35f;
        private const float StiffSpringTightness = 0.6f;

        private readonly List<int> _lastDrawFrame;
        private readonly List<Vector3> _lastTickSpringPartPos;
        private readonly Pawn _pawn;
        private readonly List<Vector3> _tweenedPartsPos;
        private bool _ended;
        private float _lockProgress;
        private bool _moving;
        private bool _starting;
        private IntVec3 _startPos;
        public float WeaponAngle;
        public float TweenedWeaponAngle;
        private float _lastTickAngle;
        private int _lastDrawFrameAngle;

        #endregion Private Fields

        #region Public Constructors

        public PawnPartsTweener(Pawn p)
        {
            this._pawn = p;
            this.PartPositions = new Vector3Tween[(int)TweenThing.Max];
        }

        #endregion Public Constructors



        #region Public Methods
        /*
        public void PreThingPosCalculation(TweenThing tweenThing, bool noTween = false,
                                          SpringTightness spring = SpringTightness.Medium)
        {
            int side = (int)tweenThing;

            if (MainTabWindow_BaseAnimator.IsOpen || ITab_Pawn_Weapons.IgnoreRenderer || noTween)
            {
                this.ResetTweenedPartPosToRoot(side);
                return;
            }

            if (this._pawn.carryTracker?.CarriedThing != null && tweenThing <= TweenThing.HandRight)
            {
                spring = SpringTightness.Hard;
            }
            // if (this._lockProgress >= 1f)
            // {
            //     this.ResetTweenedPartPosToRoot(side);
            //     return;
            // }

            if (this._lastDrawFrame[side] == RealTime.frameCount)
            {
                return;
            }

            if (this._lastDrawFrame[side] < RealTime.frameCount - 1)
            {
                this.ResetTweenedPartPosToRoot(side);
            }
            else
            {
                this._lastTickSpringPartPos[side] = this._tweenedPartsPos[side];
                float tickRateMultiplier = Find.TickManager.TickRateMultiplier;
                if (tickRateMultiplier < 5f)
                {
                    Vector3 a = this.TweenedPartPosRoot(side) - this._tweenedPartsPos[side];

                    float tightness = this._springTightness[(int)spring];
                    float progress = tightness * (RealTime.deltaTime * 60f * tickRateMultiplier);
                    // Add the lock
                    progress += this._lockProgress;
                    if (RealTime.deltaTime > 0.05f)
                    {
                        progress = Mathf.Min(progress, 1f);
                    }

                    Vector3 tweenedHandsPo = this._tweenedPartsPos[side] + a * progress;
                    tweenedHandsPo.y = this.PartPositions[side].y;
                    this._tweenedPartsPos[side] = tweenedHandsPo;
                }
                else
                {
                    this._tweenedPartsPos[side] = this.TweenedPartPosRoot(side);
                }
            }

            this._lastDrawFrame[side] = RealTime.frameCount;
        }
        */

        public void ResetTweenedAngle()
        {
            this.TweenedWeaponAngle = this.TweenedAngleRoot();
            this._lastTickAngle = this.TweenedWeaponAngle;
        }

        public void UpdateTweenerLock(bool isMoving, float movedPercent)
        {
            if (Find.TickManager.Paused)
            {
                return;
            }

            Pawn_PathFollower pather = this._pawn.pather;
            if (pather == null)
            {
                this._startPos = IntVec3.Zero;
                this._lockProgress = 0f;
                return;
            }

            if (!isMoving)
            {
                return;
            }

            // Already walking?
            if (this._startPos == IntVec3.Zero)
            {
                this._startPos = this._pawn.Position;
                this._starting = true;
                this._moving = false;
                this._ended = false;
            }

            // close the lock at 20 %, end start sequence
            if (this._starting)
            {
                this._lockProgress = Mathf.InverseLerp(0f, 0.1f, movedPercent);

                if (this._lockProgress >= 1f)
                {
                    this._starting = false;
                    this._moving = true;
                }
            }

            if (this._moving)
            {
                // open the lock when nearing destination
                if (pather.nextCell == pather.Destination.Cell)
                {
                    this._lockProgress = Mathf.InverseLerp(1f, 0.9f, movedPercent);
                }

                if (this._lockProgress <= 0.1f)
                {
                    this._moving = false;
                    this._ended = true;
                }
            }

            if (this._ended)
            {
                this._startPos = IntVec3.Zero;
                this._lockProgress = 0f;
            }
        }

        #endregion Public Methods

        #region Internal Methods

        //  internal void PreThingPosCalculation()
        //  {
        //      this.PreThingPosCalculation(TweenThing.HandLeft);
        //      this.PreThingPosCalculation(TweenThing.HandRight);
        //  }

        #endregion Internal Methods

        #region Private Methods

        //  private Vector3 TweenedPartPosRoot(int side)
        //  {
        //      return this.PartPositions[side];
        //  }

        private float TweenedAngleRoot()
        {
            return this.WeaponAngle;
        }

        #endregion Private Methods

        public void WeaponAngleCalculation()
        {
            if (MainTabWindow_BaseAnimator.IsOpen || ITab_Pawn_Weapons.IgnoreRenderer)
            {
                this.ResetTweenedAngle();
                return;
            }

            if (this._lastDrawFrameAngle == Find.TickManager.TicksGame)
            {
                return;
            }

            if (_lastDrawFrameAngle < Find.TickManager.TicksGame - 1)
            {
                ResetTweenedAngle();
            }
            else
            {
                this._lastTickAngle = this.TweenedWeaponAngle;
                float tickRateMultiplier = Find.TickManager.TickRateMultiplier;
                if (tickRateMultiplier < 5f)
                {
                    float a = this.TweenedAngleRoot() - TweenedWeaponAngle;

                    float tightness = 0.05f;
                    float progress = tightness * (RealTime.deltaTime * 60f * tickRateMultiplier);
                    if (RealTime.deltaTime > 0.05f)
                    {
                        progress = Mathf.Min(progress, 1f);
                    }

                    float tweenedHandsPo = this.TweenedWeaponAngle + a * progress;
                    this.TweenedWeaponAngle = tweenedHandsPo;
                }
                else
                {
                    this.TweenedWeaponAngle = this.TweenedAngleRoot();
                }
            }

            _lastDrawFrameAngle = Find.TickManager.TicksGame;
        }
    }
}