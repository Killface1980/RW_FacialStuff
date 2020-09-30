using PawnPlus.Defs;
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
		public BodyPartLocator eyePartLocator;

		private Graphic _open;
		private Graphic _closed;
		private Graphic _dead;
		private Graphic _missing;
		private Graphic _inPain;
		private Graphic _aiming;
		private HumanEyeBehavior.BlinkPartSignalArg _blinkSignalArg;
		private Graphic _curGraphic;
		private Graphic _curPortraitGraphic;

		public void Initialize(
			Pawn pawn,
			BodyDef bodyDef,
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

			if(eyePartLocator != null)
			{
				bodyPartSignals.GetSignals(eyePartLocator.PartRecord, out List<PartSignal> partSignals);
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
			BodyPartStatus bodyPartStatus,
			ref bool updatePortrait)
		{
			additionalOffset = this.additionalOffset;
			// TODO check if portrait cache refresh is needed
			if(!pawnState.Alive)
			{
				_curGraphic = _dead;
				_curPortraitGraphic = _dead;
				return;
			}
			BodyPartStatus.Status partStatus;
			if(bodyPartStatus.GetPartStatus(eyePartLocator.PartRecord, out partStatus) && partStatus.missing)
			{
				_curGraphic = _missing;
				_curPortraitGraphic = _missing;
				return;
			}
			_curPortraitGraphic = _open;
			if(_blinkSignalArg.blinkClose || pawnState.Sleeping || !pawnState.Conscious)
			{
				_curGraphic = _closed;
				return;
			}
			if(pawnState.Aiming && closeWhenAiming)
			{
				_curGraphic = _aiming;
				return;
			}
			if(pawnState.InPainShock)
			{
				_curGraphic = _inPain;
				return;
			}
			_curGraphic = _open;
		}

		public void Render(
			Vector3 rootPos,
			Quaternion rootQuat,
			Rot4 rootRot4,
			Vector3 renderNodeOffset,
			Mesh renderNodeMesh,
			bool portrait)
		{
			Graphic graphic = portrait ?
				_curPortraitGraphic :
				_curGraphic;
			if(graphic == null)
			{
				return;
			}
			Material partMat = graphic.MatAt(rootRot4);
			if(partMat != null)
			{
				Vector3 offset = rootQuat * renderNodeOffset;
				GenDraw.DrawMeshNowOrLater(
						renderNodeMesh,
						rootPos + offset,
						rootQuat,
						partMat,
						portrait);
			}
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
