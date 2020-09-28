using PawnPlus.AI;
using PawnPlus.Graphics;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus
{
	class HumanEyeGraphicProvider : IGraphicProvider
	{
		public bool closeWhenAiming = false;
		public Vector3 additionalOffset = new Vector3(0f, 0f, 0f);

		private Graphic _open;
		private Graphic _closed;
		private Graphic _dead;
		private Graphic _missing;
		private Graphic _inPain;
		private Graphic _aiming;
		private int _eyeBlinkEndTick = 0;

		public void Initialize(
			Pawn pawn,
			BodyDef bodyDef, 
			BodyPartRecord bodyPartRecord, 
			string defaultTexPath, 
			Dictionary<string, string> namedTexPaths)
		{
			Graphic defaultGraphic = GraphicDatabase.Get<Graphic_Multi>(
				defaultTexPath,
				Shaders.FacePart);
			Dictionary<string, Graphic> namedGraphics = new Dictionary<string, Graphic>()
			{
				{ "Open", null },
				{ "Closed", null },
				{ "Dead", null },
				{ "Missing", null },
				{ "InPain", null },
				{ "Aiming", null }
			};
			foreach(string key in new List<string>(namedGraphics.Keys))
			{
				if(namedTexPaths.ContainsKey(key))
				{
					Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(
						namedTexPaths[key],
						Shaders.FacePart);
					namedGraphics[key] = graphic;
				}
				else
				{
					namedGraphics[key] = defaultGraphic;
				}
			}
			_open = namedGraphics["Open"];
			_closed = namedGraphics["Closed"];
			_dead = namedGraphics["Dead"];
			_missing = namedGraphics["Missing"];
			_inPain = namedGraphics["InPain"];
			_aiming = namedGraphics["Aiming"];
		}
		
		public void Update(
			PawnState pawnState,
			in BodyPartStatus partStatus,
			out Graphic graphic, 
			out Graphic portraitGraphic,
			ref Vector3 additionalOffset, 
			ref bool updatePortrait, 
			IReadOnlyList<PartSignal> partSignals)
		{
			for(int i = 0; i < partSignals.Count; ++i)
			{
				PartSignal signal = partSignals[i];
				if(signal.type == PartSignalType.EyeBlink)
				{
					if(signal.argument is HumanEyeBehavior.BlinkPartSignalArg signalArg)
					{
						_eyeBlinkEndTick = Find.TickManager.TicksGame + signalArg.blinkDuration;
					}
				}
			}
			additionalOffset = this.additionalOffset;
			// TODO check if portrait cache refresh is needed
			if(!pawnState.Alive)
			{
				graphic = _dead;
				portraitGraphic = _dead;
				return;
			}
			if(partStatus.missing)
			{
				graphic = _missing;
				portraitGraphic = _missing;
				return;
			}
			portraitGraphic = _open;
			if(Find.TickManager.TicksGame < _eyeBlinkEndTick || pawnState.Sleeping || !pawnState.Conscious)
			{
				graphic = _closed;
				return;
			}
			if(pawnState.Aiming && closeWhenAiming)
			{
				graphic = _aiming;
				return;
			}
			if(pawnState.InPainShock)
			{
				graphic = _inPain;
				return;
			}
			graphic = _open;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
