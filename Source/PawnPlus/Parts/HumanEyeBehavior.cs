using PawnPlus.Defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnPlus.Parts
{
	class HumanEyeBehavior : IPartBehavior
	{
		public class BlinkPartSignalArg : PartSignalArg
		{
			public bool blinkClose;
		}

		// Disable warnings for public variables whose values are defined in xml
		#pragma warning disable CS0649
		public int blinkCloseTicks;
		public int blinkOpenAverageTicks;
		public int blinkOpenMaxRandOffsetTicks;
		public BodyDef raceBodyDef;
		public BodyPartLocator leftEye;
		public BodyPartLocator rightEye;
		#pragma warning restore CS0649

		private bool _blinkClose;
		private int _nextStateChangeTick;
		// When the signal is consumed, the argument is discarded. Therefore it should be safe to cache the argument.
		private BlinkPartSignalArg _cachedBlinkSignalArg;
		
		public string UniqueID 
		{ 
			get 
			{
				return "PawnPlus_HumanEyeBehavior";
			}
		}

		public void Initialize(BodyDef bodyDef, BodyPartSignals bodyPartSignals)
		{
			List<BodyPartRecord> eyes = bodyDef.GetPartsWithDef(BodyPartDefOf.Eye).ToList();
			_cachedBlinkSignalArg = new BlinkPartSignalArg() { blinkClose = false };
			PartSignal blinkSignal = new PartSignal("PP_EyeBlink", _cachedBlinkSignalArg);
			leftEye?.LocateBodyPart(raceBodyDef);
			rightEye?.LocateBodyPart(raceBodyDef);
			bodyPartSignals.RegisterSignal(leftEye?._resolvedPartIndex ?? -1, blinkSignal);
			bodyPartSignals.RegisterSignal(rightEye?._resolvedPartIndex ?? -1, blinkSignal);
		}
		
		public void Update(Pawn pawn, PawnState pawnState)
		{
			if(!pawnState.Alive)
			{
				return;
			}
			// Eye blinking update
			if(Find.TickManager.TicksGame >= _nextStateChangeTick)
			{
				float consciousness = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
				_nextStateChangeTick =
					_blinkClose ?
						Find.TickManager.TicksGame + blinkCloseTicks :
						Find.TickManager.TicksGame + CalculateEyeOpenDuration(consciousness);
				_cachedBlinkSignalArg.blinkClose = _blinkClose;
				_blinkClose = !_blinkClose;
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
		
		public object Clone()
		{
			return MemberwiseClone();
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref _blinkClose, "blinkOpen");
			Scribe_Values.Look(ref _nextStateChangeTick, "nextStateChangeTick");
		}
	}
}
