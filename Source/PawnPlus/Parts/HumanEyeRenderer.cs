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
	[StaticConstructorOnStartup]
	class HumanEyeRenderer : IPartRenderer
	{
		public bool closeWhenAiming = false;
		public Vector3 additionalOffset = new Vector3(0f, 0f, 0f);
		public BodyPartLocator eyePartLocator;
		
		private MatProps_Multi _open;
		private MatProps_Multi _closed;
		private MatProps_Multi _dead;
		private MatProps_Multi _missing;
		private MatProps_Multi _inPain;
		private MatProps_Multi _aiming;
		private HumanEyeBehavior.BlinkPartSignalArg _blinkSignalArg;
		private MatProps_Multi _curMatProp;
		private MatProps_Multi _curPortraitMatProp;
		
		public void Initialize(
			Pawn pawn,
			BodyDef bodyDef,
			string defaultTexPath,
			Dictionary<string, string> namedTexPaths,
			BodyPartSignals bodyPartSignals,
			ref TickDelegate tickDelegate)
		{
			MatProps_Multi defautMapProps = MatProps_Multi.Create(defaultTexPath);
			Dictionary<string, MatProps_Multi> namedMatProps = 
				new Dictionary<string, MatProps_Multi>()
				{
					{ "Open", null },
					{ "Closed", null },
					{ "Dead", null },
					{ "Missing", null },
					{ "InPain", null },
					{ "Aiming", null }
				};
			foreach(string key in new List<string>(namedMatProps.Keys))
			{
				if(namedTexPaths.ContainsKey(key))
				{
					namedMatProps[key] = MatProps_Multi.Create(namedTexPaths[key]);
				} else
				{
					namedMatProps[key] = defautMapProps;
				}
			}
			_open = namedMatProps["Open"];
			_closed = namedMatProps["Closed"];
			_dead = namedMatProps["Dead"];
			_missing = namedMatProps["Missing"];
			_inPain = namedMatProps["InPain"];
			_aiming = namedMatProps["Aiming"];

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
			// Initialize portrait graphics because Render() could be called before first Update().
			_curPortraitMatProp = _open;
			tickDelegate.NormalUpdate = Update;
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
				_curMatProp = _dead;
				_curPortraitMatProp = _dead;
				return;
			}
			BodyPartStatus.Status partStatus;
			if(bodyPartStatus.GetPartStatus(eyePartLocator.PartRecord, out partStatus) && partStatus.missing)
			{
				_curMatProp = _missing;
				_curPortraitMatProp = _missing;
				return;
			}
			_curPortraitMatProp = _open;
			if(_blinkSignalArg.blinkClose || pawnState.Sleeping || !pawnState.Conscious)
			{
				_curMatProp = _closed;
				return;
			}
			if(pawnState.Aiming && closeWhenAiming)
			{
				_curMatProp = _aiming;
				return;
			}
			if(pawnState.InPainShock)
			{
				_curMatProp = _inPain;
				return;
			}
			_curMatProp = _open;
		}

		public void Render(
			Vector3 rootPos,
			Quaternion rootQuat,
			Rot4 rootRot4,
			Vector3 renderNodeOffset,
			Mesh renderNodeMesh,
			bool portrait)
		{
			MatProps_Multi matProps = portrait ?
				_curPortraitMatProp :
				_curMatProp;
			if(matProps == null)
			{
				return;
			}
			MaterialPropertyBlock matPropBlock = matProps.GetMaterialProperty(rootRot4);
			if(matPropBlock != null)
			{
				Vector3 offset = rootQuat * renderNodeOffset;
				if(!portrait)
				{
					UnityEngine.Graphics.DrawMesh(
						renderNodeMesh,
						Matrix4x4.TRS(rootPos + offset, rootQuat, Vector3.one),
						Shaders.FacePart,
						0,
						null,
						0,
						matPropBlock);
				}
				else
				{
					Shaders.FacePart.mainTexture = matPropBlock.GetTexture(Shaders.MainTexPropID);
					Shaders.FacePart.SetPass(0);
					UnityEngine.Graphics.DrawMeshNow(renderNodeMesh, rootPos + offset, rootQuat);
				}
			}
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
