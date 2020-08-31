using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FacialStuff.AI
{
	class HumanEyeBehavior : IEyeBehavior
	{
		// Disable warnings for public variables whose values are defined in xml
		#pragma warning disable CS0649
		public int blinkCloseTicks;
		public int blinkOpenAverageTicks;
		public int blinkOpenMaxRandOffsetTicks;
		#pragma warning restore CS0649

		private bool _blinkOpen;
		private int _nextStateChangeTick;

		public int NumEyes { get { return 2; } }
		
		public void Update(Pawn pawn, Rot4 headRot, PawnState pawnState, List<IEyeBehavior.Params> eyeParams)
		{
			if(!pawnState.alive)
			{
				return;
			}
			// Check for any cases where eye should be closed forcefully.
			bool inComa = false;
			float consciousness = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
			if(consciousness < PawnCapacityDefOf.Consciousness.minForCapable)
			{
				inComa = true;
			}
			bool closeOverride = inComa;

			// Eye blinking update
			if(Find.TickManager.TicksGame >= _nextStateChangeTick)
			{
				_nextStateChangeTick =
					_blinkOpen ?
						Find.TickManager.TicksGame + blinkCloseTicks :
						Find.TickManager.TicksGame + CalculateEyeOpenDuration(consciousness);
				_blinkOpen = !_blinkOpen;
			}
			// 0 is left eye, 1 is right eye.
			for(int i = 0; i < NumEyes; ++i)
			{
				// Bit 0 is north, bit 1 is east, bit 2 is south, bit 3 is west. If the bit is set, then render the eye.
				int rotationFlag = i == 0 ? 0b1100 : 0b0110;
				eyeParams[i].render = (rotationFlag & (1 << headRot.AsInt)) != 0;
				// Mirror the right eye texture to get left eye texture.
				eyeParams[i].mirror = i == 0 ? true : false;
				// If pawnState.aiming is false, then it will be always be true for both eyes. Therefore, it won't have
				// any effect when AND'ing with other booleans.
				// Close left eye and open right eye while aiming, regardless of blinking status.
				bool eyeAimingOpen = !((i == 0 ? true : false) && pawnState.aiming);
				bool eyeBlinkOpen = _blinkOpen || !Controller.settings.MakeThemBlink;
				eyeParams[i].openEye =
					eyeBlinkOpen &&
					eyeAimingOpen &&
					!pawnState.sleeping &&
					!closeOverride;
			}
		}
		
		private int CalculateEyeOpenDuration(float consciousness)
		{
			consciousness = Mathf.Clamp(consciousness, 0f, 1f);
			int offset = (int)(1f - consciousness) * blinkOpenAverageTicks;
			return
				blinkOpenAverageTicks +
				UnityEngine.Random.Range(0, blinkOpenMaxRandOffsetTicks * 2) -
				blinkOpenMaxRandOffsetTicks +
				offset;
		}

		public bool GetEyeMirrorFlagForPortrait(int eyeIndex)
		{
			return eyeIndex == 0 ? true : false;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref _blinkOpen, "blinkOpen");
			Scribe_Values.Look(ref _nextStateChangeTick, "nextStateChangeTick");
		}
	}
}
