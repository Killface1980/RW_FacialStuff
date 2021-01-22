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
		
		private MaterialPropertyBlock _matPropBlock = new MaterialPropertyBlock();
		private TextureSet _open;
		private TextureSet _closed;
		private TextureSet _dead;
		private TextureSet _missing;
		private TextureSet _inPain;
		private TextureSet _aiming;
		private HumanEyeBehavior.BlinkPartSignalArg _blinkSignalArg;
		private TextureSet _curTexSet;
		private TextureSet _curPortraitMatProp;
		
		public void Initialize(
			Pawn pawn,
			BodyDef bodyDef,
			string defaultTexPath,
			Dictionary<string, string> namedTexPaths,
			BodyPartSignals bodyPartSignals,
			ref TickDelegate tickDelegate)
		{
			TextureSet defaultTexSet = TextureSet.Create(defaultTexPath);
			Dictionary<string, TextureSet> namedTexSets = 
				new Dictionary<string, TextureSet>()
				{
					{ "Open", null },
					{ "Closed", null },
					{ "Dead", null },
					{ "Missing", null },
					{ "InPain", null },
					{ "Aiming", null }
				};
			foreach(string key in new List<string>(namedTexSets.Keys))
			{
				if(namedTexPaths.ContainsKey(key))
				{
					namedTexSets[key] = TextureSet.Create(namedTexPaths[key]);
				} else
				{
					namedTexSets[key] = defaultTexSet;
				}
			}
			_open = namedTexSets["Open"];
			_closed = namedTexSets["Closed"];
			_dead = namedTexSets["Dead"];
			_missing = namedTexSets["Missing"];
			_inPain = namedTexSets["InPain"];
			_aiming = namedTexSets["Aiming"];

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
			// If shader property left uninitialized, then the result from other MateriaPropertyBlock 
			// using the same shader can interfere with it.
			_matPropBlock.SetColor("_Color", Color.white);
			tickDelegate.NormalUpdate = Update;
		}
		
		public void Update(
			PawnState pawnState,
			BodyPartStatus bodyPartStatus,
			ref bool updatePortrait)
		{
			// TODO check if portrait cache refresh is needed
			if(!pawnState.Alive)
			{
				_curTexSet = _dead;
				_curPortraitMatProp = _dead;
				return;
			}
			BodyPartStatus.Status partStatus;
			if(bodyPartStatus.GetPartStatus(eyePartLocator.PartRecord, out partStatus) && partStatus.missing)
			{
				_curTexSet = _missing;
				_curPortraitMatProp = _missing;
				return;
			}
			_curPortraitMatProp = _open;
			if(_blinkSignalArg.blinkClose || pawnState.Sleeping || !pawnState.Conscious)
			{
				_curTexSet = _closed;
				return;
			}
			if(pawnState.Aiming && closeWhenAiming)
			{
				_curTexSet = _aiming;
				return;
			}
			if(pawnState.InPainShock)
			{
				_curTexSet = _inPain;
				return;
			}
			_curTexSet = _open;
		}

		public void Render(
			Vector3 rootPos,
			Quaternion rootQuat,
			Rot4 rootRot4,
			Vector3 renderNodeOffset,
			Mesh renderNodeMesh,
			bool portrait)
		{
			TextureSet curTexSet = portrait ?
				_curPortraitMatProp :
				_curTexSet;
			if(curTexSet == null)
			{
				return;
			}
			curTexSet.GetIndexForRot(rootRot4, out float index);
			Texture2DArray curTextureArray = curTexSet.GetTextureArray();
			Vector3 offset = rootQuat * (renderNodeOffset + additionalOffset);
			if(!portrait)
			{
				_matPropBlock.SetTexture(Shaders.MainTexPropID, curTextureArray);
				_matPropBlock.SetFloat(Shaders.TexIndexPropID, index);
				Shaders.FacePart.SetColor(Shaders.ColorOnePropID, Color.black);
				UnityEngine.Graphics.DrawMesh(
					renderNodeMesh,
					Matrix4x4.TRS(rootPos + offset, rootQuat, Vector3.one),
					Shaders.FacePart,
					0,
					null,
					0,
					_matPropBlock);
			}
			else
			{
				Shaders.FacePart.mainTexture = curTextureArray;
				Shaders.FacePart.SetFloat(Shaders.TexIndexPropID, index);
				Shaders.FacePart.SetColor(Shaders.ColorOnePropID, Color.black);
				Shaders.FacePart.SetPass(0);
				UnityEngine.Graphics.DrawMeshNow(renderNodeMesh, rootPos + offset, rootQuat);
			}
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
