﻿using PawnPlus.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.Parts
{
	class HumanMouthGraphicProvider : IGraphicProvider
	{
		private Pawn _pawn;
		private Graphic _normal;
		private Graphic _happy;
		private Graphic _minor;
		private Graphic _major;
		private Graphic _extreme;
		private Graphic _crying;
		private Graphic _dead;
		private Graphic _curGraphic;
		private int _ticksSinceLastUpdate;

		public void Initialize(
			Pawn pawn,
			BodyDef bodyDef,
			BodyPartRecord bodyPartRecord,
			string defaultTexPath,
			Dictionary<string, string> namedTexPaths,
			BodyPartSignals bodyPartSignals)
		{
			_pawn = pawn;
			Graphic defaultGraphic = GraphicDatabase.Get<Graphic_Multi>(
				defaultTexPath,
				Shaders.FacePart);
			Dictionary<string, Graphic> namedGraphics = new Dictionary<string, Graphic>()
			{
				{ "Normal", null },
				{ "Happy", null },
				{ "Minor", null },
				{ "Major", null },
				{ "Extreme", null },
				{ "Crying", null },
				{ "Dead", null }
			};
			foreach(string key in new List<string>(namedGraphics.Keys))
			{
				if(namedTexPaths.ContainsKey(key))
				{
					Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(
						namedTexPaths[key],
						Shaders.FacePart);
					namedGraphics[key] = graphic;
				} else
				{
					namedGraphics[key] = defaultGraphic;
				}
			}
			_normal = namedGraphics["Normal"];
			_happy = namedGraphics["Happy"];
			_minor = namedGraphics["Minor"];
			_major = namedGraphics["Major"];
			_extreme = namedGraphics["Extreme"];
			_crying = namedGraphics["Crying"];
			_dead = namedGraphics["Dead"];

			_ticksSinceLastUpdate = Find.TickManager.TicksGame;
		}

		public void Update(
			PawnState pawnState,
			in BodyPartStatus partStatus,
			out Graphic graphic,
			out Graphic portraitGraphic,
			ref Vector3 additionalOffset,
			ref bool updatePortrait)
		{
			portraitGraphic = _normal;
			if(Find.TickManager.TicksGame >= _ticksSinceLastUpdate + 90)
			{
				_ticksSinceLastUpdate = Find.TickManager.TicksGame;
				UpdateCurrentGraphic(pawnState);
			}
			graphic = _curGraphic;
		}

		private void UpdateCurrentGraphic(PawnState pawnState)
		{
			if(!pawnState.Alive)
			{
				_curGraphic = _dead;
				return;
			}
			if(pawnState.Fleeing || pawnState.InPainShock)
			{
				_curGraphic = _crying;
				return;
			}
			float moodLevel = _pawn.needs.mood.CurInstantLevel;
			if(moodLevel <= _pawn.mindState.mentalBreaker.BreakThresholdExtreme)
			{
				_curGraphic = _extreme;
				return;
			}
			if(moodLevel <= _pawn.mindState.mentalBreaker.BreakThresholdMajor)
			{
				_curGraphic = _major;
				return;
			}
			if(moodLevel <= _pawn.mindState.mentalBreaker.BreakThresholdMinor)
			{
				_curGraphic = _minor;
				return;
			}
			float happyThreshold =
				_pawn.mindState.mentalBreaker.BreakThresholdMinor +
				((1f - _pawn.mindState.mentalBreaker.BreakThresholdMinor) / 2f);
			if(moodLevel < happyThreshold)
			{
				_curGraphic = _normal;
				return;
			}
			_curGraphic = _happy;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
