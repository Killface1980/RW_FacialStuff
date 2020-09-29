using PawnPlus.Graphics;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.Parts
{
	class HumanEyeRenderer : IPartRenderer
	{
		public bool closeWhenAiming = false;
		public Vector3 additionalOffset = new Vector3(0f, 0f, 0f);

		private Graphic _open;
		private Graphic _closed;
		private Graphic _dead;
		private Graphic _missing;
		private Graphic _inPain;
		private Graphic _aiming;
		private HumanEyeBehavior.BlinkPartSignalArg _blinkSignalArg;

		public void Initialize(
			Pawn pawn,
			BodyDef bodyDef,
			BodyPartRecord bodyPartRecord,
			string defaultTexPath,
			Dictionary<string, string> namedTexPaths,
			BodyPartSignals bodyPartSignals)
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
				} else
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

			if(bodyPartRecord != null)
			{
				bodyPartSignals.GetSignals(bodyPartRecord.Index, out List<PartSignal> partSignals);
				List<PartSignal> eyeBlinkSignals = partSignals.FindAll(i => i.signalName == "PP_EyeBlink");
				for(int i = eyeBlinkSignals.Count - 1; i >= 0; --i)
				{
					if(eyeBlinkSignals[i].argument is HumanEyeBehavior.BlinkPartSignalArg blinkSignalArg)
					{
						_blinkSignalArg = blinkSignalArg;
						break;
					}
				}
			}
			// If blink signal couldn't be found, disable eye blinking.
			if(_blinkSignalArg == null)
			{
				_blinkSignalArg = new HumanEyeBehavior.BlinkPartSignalArg() { blinkClose = false };
			}
		}
		
		public void Update(
			PawnState pawnState,
			in BodyPartStatus partStatus,
			out Graphic graphic, 
			out Graphic portraitGraphic,
			ref Vector3 additionalOffset, 
			ref bool updatePortrait)
		{
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
			if(_blinkSignalArg.blinkClose || pawnState.Sleeping || !pawnState.Conscious)
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
