using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnPlus.AI
{
	class HumanEyeBehavior : IEyeBehavior
	{
		public class BlinkPartSignalArg : PartSignalArg
		{
			public int blinkDuration;
		}

		// Disable warnings for public variables whose values are defined in xml
		#pragma warning disable CS0649
		public int blinkCloseTicks;
		public int blinkOpenAverageTicks;
		public int blinkOpenMaxRandOffsetTicks;
		#pragma warning restore CS0649

		private bool _blinkOpen;
		private int _nextStateChangeTick;

		private int _leftEyeIdx;
		private int _rightEyeIdx;
		// When the signal is consumed, the argument is discarded. Therefore it should be safe to cache the argument.
		private PartSignal _cachedBlinkSignal;

		public void Initialize(BodyDef bodyDef, out List<int> usedBodyPartIndices)
		{
			List<BodyPartRecord> eyes = bodyDef.GetPartsWithDef(BodyPartDefOf.Eye).ToList();
			usedBodyPartIndices = new List<int>(2);
			_leftEyeIdx = bodyDef.GetIndexOfPart(eyes.FindLast(i => i.untranslatedCustomLabel == "left eye"));
			usedBodyPartIndices.Add(_leftEyeIdx);
			_rightEyeIdx = bodyDef.GetIndexOfPart(eyes.FindLast(i => i.untranslatedCustomLabel == "right eye"));
			usedBodyPartIndices.Add(_rightEyeIdx);
			_cachedBlinkSignal = new PartSignal(PartSignalType.EyeBlink, new BlinkPartSignalArg() { blinkDuration = blinkCloseTicks });
		}

		public void Update(Pawn pawn, PawnState pawnState, Dictionary<int, Queue<PartSignal>> bodyPartSignals)
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
					_blinkOpen ?
						Find.TickManager.TicksGame + blinkCloseTicks :
						Find.TickManager.TicksGame + CalculateEyeOpenDuration(consciousness);
				if(!_blinkOpen)
				{
					foreach(var partSignalQueue in bodyPartSignals)
					{
						partSignalQueue.Value.Enqueue(_cachedBlinkSignal);
					}
				}
				_blinkOpen = !_blinkOpen;
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
			Scribe_Values.Look(ref _blinkOpen, "blinkOpen");
			Scribe_Values.Look(ref _nextStateChangeTick, "nextStateChangeTick");
		}
	}
}
