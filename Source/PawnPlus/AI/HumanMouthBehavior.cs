using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace PawnPlus.AI
{
	class HumanMouthBehavior : IMouthBehavior
	{
		private int _deadTexIdx;
		private int _extremeTexIdx;
		private int _majorTexIdx;
		private int _minorTexIdx;
		private int _normalTexIdx;
		private int _happyTexIdx;
		private int _cryingTexIdx;
		private int _ticksSinceLastUpdate;
		private int _curMouthTextureIdx = -1;
		private List<string> _textureNames;
		private string _savedCurTexName;
		
		public void InitializeTextureIndex(ReadOnlyCollection<string> textureNames)
		{
			_extremeTexIdx = textureNames.IndexOf("HumanM1Extreme");
			_majorTexIdx = textureNames.IndexOf("HumanM1Major");
			_minorTexIdx = textureNames.IndexOf("HumanM1Minor");
			_normalTexIdx = textureNames.IndexOf("HumanM1Normal");
			_happyTexIdx = textureNames.IndexOf("HumanM1Happy");
			_cryingTexIdx = textureNames.IndexOf("HumanM1Crying");
			_deadTexIdx = textureNames.IndexOf("HumanM1Dead");
			if(_deadTexIdx < 0)
			{
				_deadTexIdx = _minorTexIdx;
			}

			// Attempt to load current texture from the saved texture name.
			if(!_savedCurTexName.NullOrEmpty())
			{
				_curMouthTextureIdx = textureNames.IndexOf(_savedCurTexName);
			}
			if(_curMouthTextureIdx < 0)
			{
				_curMouthTextureIdx = _normalTexIdx;
			}

			// Copy the textureNames list for saving the texture name as string instead of int.
			_textureNames = new List<string>(textureNames.Count);
			foreach(var texName in textureNames)
			{
				_textureNames.Add(texName);
			}
		}

		public void Update(Pawn pawn, Rot4 headRot, PawnState pawnState, IMouthBehavior.Params mouthParams)
		{
			mouthParams.mouthTextureIdx = _curMouthTextureIdx;
			if(Find.TickManager.TicksGame >= _ticksSinceLastUpdate + 90)
			{
				_ticksSinceLastUpdate = Find.TickManager.TicksGame;
				if(!pawnState.Alive)
				{
					_curMouthTextureIdx = _deadTexIdx;
					return;
				}
				if(pawnState.Fleeing || pawnState.InPainShock)
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

		public object Clone()
		{
			return MemberwiseClone();
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref _ticksSinceLastUpdate, "ticksSinceLastUpdate");
			if(Scribe.mode == LoadSaveMode.Saving)
			{
				_savedCurTexName = _textureNames[_curMouthTextureIdx];
			}
			Scribe_Values.Look(ref _savedCurTexName, "curTexName");
		}
	}
}
