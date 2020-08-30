using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace FacialStuff.AI
{
	class HumanMouth : IMouthBehavior
	{
		private int _deadTexIdx;
		private int _extremeTexIdx;
		private int _majorTexIdx;
		private int _minorTexIdx;
		private int _normalTexIdx;
		private int _happyTexIdx;
		private int _cryingTexIdx;
		private int _ticksSinceLastUpdate;
		private int _curMouthTextureIdx;

		public void InitializeTextureIndex(ReadOnlyCollection<string> textureNames)
		{
			_extremeTexIdx = textureNames.IndexOf("Extreme");
			_majorTexIdx = textureNames.IndexOf("Major");
			_minorTexIdx = textureNames.IndexOf("Minor");
			_normalTexIdx = textureNames.IndexOf("Normal");
			_happyTexIdx = textureNames.IndexOf("Happy");
			_cryingTexIdx = textureNames.IndexOf("Crying");
			_deadTexIdx = textureNames.IndexOf("Dead");
			if(_deadTexIdx < 0)
			{
				_deadTexIdx = _minorTexIdx;
			}
		}

		public void Update(Pawn pawn, Rot4 headRot, PawnState pawnState, out bool render, ref int mouthTextureIdx, ref bool mirror)
		{
			++_ticksSinceLastUpdate;
			if(headRot == Rot4.North)
			{
				render = false;
				return;
			}
			render = true;
			mouthTextureIdx = _curMouthTextureIdx;
			if(_ticksSinceLastUpdate >= 90)
			{
				if(!pawnState.alive)
				{
					_curMouthTextureIdx = _deadTexIdx;
					return;
				}
				if(pawnState.fleeing || pawnState.inPainShock)
				{
					_curMouthTextureIdx = _cryingTexIdx;
					return;
				}
				float moodLevel = pawn.needs.mood.CurInstantLevel;
				if(moodLevel <= pawn.mindState.mentalBreaker.BreakThresholdExtreme)
				{
					_curMouthTextureIdx = _extremeTexIdx;
					return;
				}
				if(moodLevel <= pawn.mindState.mentalBreaker.BreakThresholdMajor)
				{
					_curMouthTextureIdx = _majorTexIdx;
					return;
				}
				if(moodLevel <= pawn.mindState.mentalBreaker.BreakThresholdMinor)
				{
					_curMouthTextureIdx = _minorTexIdx;
					return;
				}
				float happyThreshold =
					pawn.mindState.mentalBreaker.BreakThresholdMinor +
					((1f - pawn.mindState.mentalBreaker.BreakThresholdMinor) / 2f);
				if(moodLevel < happyThreshold)
				{
					_curMouthTextureIdx = _normalTexIdx;
					return;
				}
				_curMouthTextureIdx = _happyTexIdx;
			}
		}

		public int GetTextureIndexForPortrait()
		{
			return _normalTexIdx;
		}
	}
}
