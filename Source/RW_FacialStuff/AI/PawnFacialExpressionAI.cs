using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FacialStuff.AI
{
	public class PawnFacialExpressionAI
	{
		#region Inner Types

        // State variables for all of eyes combined
		private struct EyeVars
		{
            public const int kCloseDuration = 10;
            public const int kOpenAverageDuration = 120;
            public const int kOpenRandomMaxOffset = 30;
            
            public bool blinkOpen;
            public int ticksSinceLastState;
            public int ticksUntilNextState;
        }

        // State variables for individual eyes
		private class PerEyeVars
		{ 
            public bool open;
            public bool canBlink;
            public bool closeWhileAiming;
            public bool closeWhileSleeping;
        }
        
		private struct MouthVars
		{
            public float mood;
            public MouthState state;
            public int ticksSinceLastUpdate;
        }
        
		#endregion

		#region Private Member Variables

		private Pawn _pawn;
        private MouthVars _mouth;
        private EyeVars _eye;
        private List<PerEyeVars> _perEye;
        private CompProperties_Face _faceProp;

		#endregion

		#region Public Properties

		public int MouthGraphicIndex { get; private set; }
        
		#endregion
        
		public PawnFacialExpressionAI(Pawn pawn, CompProperties_Face faceProp)
		{
			_pawn = pawn;
            _mouth.mood = 0.5f;
            _mouth.state = MouthState.Mood;
            _mouth.ticksSinceLastUpdate = 0;
            _eye.blinkOpen = true;
            _eye.ticksSinceLastState = 0;
            _eye.ticksUntilNextState = CalculateEyeOpenDuration(1f);
            _perEye = new List<PerEyeVars>(faceProp.perEyeDefs.Count);
            for(int i = 0; i < faceProp.perEyeDefs.Count; ++i)
			{
                PerEyeVars perEyeVar = new PerEyeVars();
                perEyeVar.canBlink = faceProp.perEyeDefs[i].canBlink;
                perEyeVar.closeWhileAiming = faceProp.perEyeDefs[i].closeWhileAiming;
                perEyeVar.closeWhileSleeping = faceProp.perEyeDefs[i].closeWhileSleeping;
                _perEye.Add(perEyeVar);
            }
            _faceProp = faceProp;
        }

		public void Tick(bool canUpdatePawn, CompFace compFace, PawnState pawnState)
		{
			if(!canUpdatePawn)
			{
				return;
			}
            EyeTick(pawnState);
            MouthTick(compFace, pawnState);
            ++_mouth.ticksSinceLastUpdate;
            ++_eye.ticksSinceLastState;
        }

        private void EyeTick(PawnState pawnState)
		{
            // Check for any cases where eye should be closed forcefully.
            bool inComa = false;
            float consciousness = _pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
            if(consciousness < PawnCapacityDefOf.Consciousness.minForCapable)
			{
                inComa = true;
			}
            bool closeOverride = inComa;

            // Eye blinking update
            if(_eye.ticksSinceLastState >= _eye.ticksUntilNextState)
			{
                _eye.ticksSinceLastState = 0;
                _eye.ticksUntilNextState = 
                    _eye.blinkOpen ? 
                        EyeVars.kCloseDuration :
                        CalculateEyeOpenDuration(consciousness);
                _eye.blinkOpen = !_eye.blinkOpen;
            }

            for(int i = 0; i < _perEye.Count; ++i)
            {
                bool eyeAimingOpen = !(_perEye[i].closeWhileAiming && pawnState.aiming);
                bool eyeBlinkOpen = 
                    _eye.blinkOpen
                    // If the eye can't blink, then override eyeBlinkOpen to true by OR'ing.
                    || !(_perEye[i].canBlink && Controller.settings.MakeThemBlink);
                _perEye[i].open = 
                    eyeBlinkOpen &&
                    eyeAimingOpen &&
                    !(_perEye[i].closeWhileSleeping && pawnState.sleeping) && 
                    !closeOverride;
            }
		}

        public bool IsEyeOpen(int eyeIdx)
        {
            return _perEye[eyeIdx].open;
        }

        private void MouthTick(CompFace compFace, PawnState pawnState)
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
            offset = (int)(1f - consciousness) * EyeVars.kOpenAverageDuration;
            return
                EyeVars.kOpenAverageDuration +
                UnityEngine.Random.Range(0, EyeVars.kOpenRandomMaxOffset * 2) -
                EyeVars.kOpenRandomMaxOffset +
                offset;
        }
    }
}
