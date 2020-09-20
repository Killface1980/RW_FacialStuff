using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PawnPlus.AI
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
		
		public void Update(Pawn pawn, Rot4 headRot, PawnState pawnState, List<IEyeBehavior.Result> eyeResults)
		{
			if(!pawnState.Alive)
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
				if(pawnState.InPainShock)
				{
					eyeResults[i].eyeAction = EyeAction.Pain;
				}
				else
				{
					// If pawnState.aiming is false, then it will be always be true for both eyes. Therefore, it won't have
					// any effect when AND'ing with other booleans.
					// Close left eye and open right eye while aiming, regardless of blinking status.
					bool eyeAimingOpen = !((i == 0 ? true : false) && pawnState.Aiming);
					bool eyeBlinkOpen = _blinkOpen || !Controller.settings.MakeThemBlink;
					bool eyeOpen =
						eyeBlinkOpen &&
						eyeAimingOpen &&
						!pawnState.Sleeping &&
						!closeOverride;
					eyeResults[i].eyeAction = eyeOpen ? EyeAction.None : EyeAction.Closed;
				}
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
