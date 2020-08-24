using FacialStuff.Animator;
using FacialStuff.GraphicsFS;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.AI
{
	public class PawnFacialExpressionAI
	{
		private Pawn _pawn;
        
        private struct EyeState
		{
            public const int kCloseDuration = 10;
            public const int kOpenAverageDuration = 120;
            public const int kOpenRandomMaxOffset = 30;
            
            public bool isOpen;
            // If true, eye will be forced to close regardless of blinking interval.
            public bool closeOverride;
            public int ticksSinceLastState;
            public int ticksUntilNextState;
        }
        private EyeState _eyeState;

		private struct MouthState
		{
            public float mood;
            public int ticksSinceLastUpdate;
        }
        private MouthState _mouthState;

        public int MouthGraphicIndex { get; private set; }
        public bool EyeOpen
        {
            get
            {
                return 
                    !_eyeState.closeOverride && 
                    _eyeState.isOpen;
            }
        }

		public PawnFacialExpressionAI(Pawn pawn)
		{
			_pawn = pawn;
            _mouthState.mood = 0.5f;
            _mouthState.ticksSinceLastUpdate = 0;
            _eyeState.isOpen = true;
            _eyeState.closeOverride = false;
            _eyeState.ticksSinceLastState = 0;
            _eyeState.ticksUntilNextState = CalculateEyeOpenDuration(1f);
        }

		public void Tick(bool canUpdatePawn, CompFace compFace, bool isAsleep)
		{
			if(!canUpdatePawn)
			{
				return;
			}
            EyeTick(isAsleep);
            MouthTick(compFace);
            ++_mouthState.ticksSinceLastUpdate;
            ++_eyeState.ticksSinceLastState;
        }

        private void EyeTick(bool isAsleep)
		{
            bool inComa = false;
            float consciousness = _pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
            if(consciousness < PawnCapacityDefOf.Consciousness.minForCapable)
			{
                inComa = true;
			}
            _eyeState.closeOverride = isAsleep || inComa;
            if(_eyeState.ticksSinceLastState >= _eyeState.ticksUntilNextState)
			{
                _eyeState.ticksSinceLastState = 0;
                _eyeState.ticksUntilNextState = 
                    _eyeState.isOpen ? 
                        EyeState.kCloseDuration :
                        CalculateEyeOpenDuration(consciousness);
                if(Controller.settings.MakeThemBlink)
			    {
                    _eyeState.isOpen = !_eyeState.isOpen;
                }
                else
				{
                    _eyeState.isOpen = true;
				}
                
            }
		}

        private void MouthTick(CompFace compFace)
		{
            if(_mouthState.ticksSinceLastUpdate >= 90)
            {
                if(!Controller.settings.UseMouth || compFace.BodyStat.Jaw != PartStatus.Natural)
                {
                    return;
                }
                if(_pawn.Fleeing() || _pawn.IsBurning())
                {
                    compFace.PawnFaceGraphic.MouthGraphic = compFace.PawnFaceGraphic.Mouthgraphic.MouthGraphicCrying;
                    return;
                }
                if(_pawn.health.InPainShock && !compFace.IsAsleep)
                {
                    if(!EyeOpen)
                    {
                        compFace.PawnFaceGraphic.MouthGraphic = compFace.PawnFaceGraphic.Mouthgraphic.MouthGraphicCrying;
                        return;
                    }
                }
                if(_pawn.needs?.mood?.thoughts != null)
                {
                    _mouthState.mood = _pawn.needs.mood.CurInstantLevel;
                }
                MouthGraphicIndex = compFace.PawnFaceGraphic.Mouthgraphic.GetMouthTextureIndexOfMood(_mouthState.mood);
                _mouthState.ticksSinceLastUpdate = 0;
            }
        }

        private static int CalculateEyeOpenDuration(float consciousness)
        {
            int offset = 0;
            consciousness = Mathf.Clamp(consciousness, 0f, 1f);
            offset = (int)(1f - consciousness) * EyeState.kOpenAverageDuration;
            return
                EyeState.kOpenAverageDuration +
                UnityEngine.Random.Range(0, EyeState.kOpenRandomMaxOffset * 2) -
                EyeState.kOpenRandomMaxOffset +
                offset;
        }
    }
}
