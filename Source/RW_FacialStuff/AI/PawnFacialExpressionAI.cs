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
        }
                
		#endregion

		#region Private Member Variables

		private Pawn _pawn;
        private EyeVars _eye;
        private List<PerEyeVars> _perEye;
        private CompProperties_Face _faceProp;

		#endregion
                
		public PawnFacialExpressionAI(Pawn pawn, CompProperties_Face faceProp)
		{
			_pawn = pawn;
            _eye.blinkOpen = true;
            _eye.ticksSinceLastState = 0;
            _eye.ticksUntilNextState = CalculateEyeOpenDuration(1f);
            _perEye = new List<PerEyeVars>(faceProp.perEyeBehaviors.Count);
            for(int i = 0; i < faceProp.perEyeBehaviors.Count; ++i)
			{
                PerEyeVars perEyeVar = new PerEyeVars();
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
            EyeTick(compFace.Props, pawnState);
            ++_eye.ticksSinceLastState;
        }

        private void EyeTick(CompProperties_Face faceProp, PawnState pawnState)
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
                bool eyeAimingOpen = !(faceProp.perEyeBehaviors[i].closeWhileAiming && pawnState.aiming);
                bool eyeBlinkOpen = 
                    _eye.blinkOpen
                    // If the eye can't blink, then override eyeBlinkOpen to true by OR'ing.
                    || !(faceProp.perEyeBehaviors[i].canBlink && Controller.settings.MakeThemBlink);
                _perEye[i].open = 
                    eyeBlinkOpen &&
                    eyeAimingOpen &&
                    !(faceProp.perEyeBehaviors[i].closeWhileSleeping && pawnState.sleeping) && 
                    !closeOverride;
            }
		}

        public bool IsEyeOpen(int eyeIdx)
        {
            return _perEye[eyeIdx].open;
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
