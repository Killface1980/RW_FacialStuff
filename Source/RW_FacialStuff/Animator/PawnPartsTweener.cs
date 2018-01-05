using System.Collections.Generic;
using FacialStuff.AnimatorWindows;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FacialStuff
{
    public class PawnPartsTweener
    {
        #region Public Fields

        public readonly List<Vector3> PartPositions;

        #endregion Public Fields

        #region Private Fields

        private const float SoftSpringTightness = 0.05f;
        private const float MediumSpringTightness = 0.15f;
        private const float HardSpringTightness = 0.35f;
        private const float StiffSpringTightness = 0.6f;

        private readonly List<float> _springTightness;
        private readonly List<int> _lastDrawFrame;
        private readonly List<Vector3> _tweenedPartsPos;

        private readonly List<Vector3> _lastTickSpringPartPos;
        private readonly Pawn _pawn;
        private IntVec3 _startPos;

        #endregion Private Fields

        #region Public Constructors

        public PawnPartsTweener(Pawn p)
        {
            this._pawn = p;
            this.PartPositions = new List<Vector3>();
            this._lastDrawFrame = new List<int>();
            this._lastTickSpringPartPos = new List<Vector3>();
            this._tweenedPartsPos = new List<Vector3>();

            for (int i = 0; i < (int)TweenThing.Max; i++)
            {
                this.PartPositions.Add(Vector3.zero);
                this._lastDrawFrame.Add(-1);
                this._lastTickSpringPartPos.Add(Vector3.zero);
                this._tweenedPartsPos.Add(Vector3.zero);
            }

            this._springTightness =
            new List<float> { SoftSpringTightness, MediumSpringTightness, HardSpringTightness, StiffSpringTightness };
        }

        #endregion Public Constructors

        #region Public Properties

        public List<Vector3> LastTickTweenedHandsVelocity
        {
            get
            {
                List<Vector3> list = new List<Vector3>();
                for (int index = 0; index < this.TweenedPartsPos.Count; index++)
                {
                    Vector3 handsPos = this.TweenedPartsPos[index];
                    Vector3 lastTickSpringPos = this._lastTickSpringPartPos[index];
                    list.Add(handsPos - lastTickSpringPos);
                }

                return list;
            }
        }

        public List<Vector3> TweenedPartsPos
        {
            get { return this._tweenedPartsPos; }
        }

        #endregion Public Properties

        #region Public Methods

        public void PreHandPosCalculation(TweenThing tweenThing,
                                          SpringTightness spring = SpringTightness.Medium)
        {
            int side = (int)tweenThing;

            if (MainTabWindow_BaseAnimator.IsOpen)
            {
                this.ResetTweenedPartPosToRoot(side);
                return;
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

        public void ResetTweenedPartPosToRoot(int side)
        {
            this._tweenedPartsPos[side] = this.TweenedPartPosRoot(side);
            this._lastTickSpringPartPos[side] = this._tweenedPartsPos[side];
        }

        #endregion Public Methods

        #region Internal Methods

        internal void PreHandPosCalculation()
        {
            this.PreHandPosCalculation(TweenThing.HandLeft);
            this.PreHandPosCalculation(TweenThing.HandRight);
        }

        #endregion Internal Methods

        #region Private Methods

        private Vector3 TweenedPartPosRoot(int side)
        {
            return this.PartPositions[side];
        }

        #endregion Private Methods

        private bool _starting;
        private float _lockProgress;
        private bool _moving;
        private bool ended;

        public void Update(bool isMoving, float movedPercent)
        {
            if (Find.TickManager.Paused)
            {
                return;
            }

            Pawn_PathFollower pather = this._pawn.pather;
            if (pather == null)
            {
                this._startPos=IntVec3.Zero;
                return;
            }

            if (isMoving)
            {
                // Already walking?
                if (this._startPos == IntVec3.Zero)
                {
                    this._startPos = this._pawn.Position;
                    this._starting = true;
                    this._moving = false;
                    this.ended = false;
                }

                // close the lock at 20 %, end start sequence
                if (this._starting)
                {
                    this._lockProgress = Mathf.InverseLerp(0f, 0.15f, movedPercent);

                    if (this._lockProgress >= 1f)
                    {
                        this._starting = false;
                        this._moving = true;
                    }
                }

                if (this._moving)
                {
                    // open the lock when nearing destination
                    if ( pather.nextCell == pather.Destination.Cell)
                    {
                        this._lockProgress = Mathf.InverseLerp(1f, 0.85f, movedPercent);
                    }

                    if (this._lockProgress <= 0f)
                    {
                        this._moving = false;
                        this.ended = true;
                    }
                }

                if (this.ended)
                {
                    this._startPos = IntVec3.Zero;
                }
            }

        }
    }
}