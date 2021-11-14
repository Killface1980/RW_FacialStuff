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
	public class HumanEyeRenderer : PartRendererBase
	{
		public Vector3 additionalOffset = new Vector3(0f, 0f, 0f);
		public BodyPartLocator leftEyePartLocator;
		public BodyPartLocator rightEyePartLocator;

		private TextureSet _open;
		private TextureSet _closed;
		private TextureSet _dead;
		private TextureSet _missing;
		private TextureSet _inPain;
		private TextureSet _aiming;
		private HumanEyeBehavior.BlinkPartSignalArg _blinkSignalArg;
		private EyeData[] _eyeData;
		
		private class EyeData
		{
			private HumanEyeRenderer _parent;
			private TextureSet _curTexture;
			private TextureSet _prevTexture;
			private TextureSet _portraitTexture;
			private MaterialPropertyBlock _matPropBlock;
			private BodyPartLocator _bodyPartLocator;
			private bool _closeOnAiming;

			public EyeData(HumanEyeRenderer parent, bool closeOnAiming, BodyPartLocator bodyPartLocator)
			{
				_parent = parent;
				_matPropBlock = new MaterialPropertyBlock();
				// If shader property left uninitialized, then the result from other MateriaPropertyBlock 
				// using the same shader can interfere with it.
				_matPropBlock.SetColor("_Color", Color.white);
				_bodyPartLocator = bodyPartLocator;
				_closeOnAiming = closeOnAiming;
				// Initialize portrait graphics because Render() could be called before first Update().
				_portraitTexture = parent._open;
			}

			public void Update(PawnState pawnState, BodyPartStatus bodyPartStatus, bool blink)
			{
				UpdateInternal(pawnState, bodyPartStatus, blink);
				if(_curTexture != _prevTexture)
				{
					_matPropBlock.SetTexture(Shaders.MainTexPropID, _curTexture.GetTextureArray());
				}
				_prevTexture = _curTexture;
			}

			public void UpdateInternal(PawnState pawnState, BodyPartStatus bodyPartStatus, bool blink)
			{
				if(!pawnState.Alive || pawnState.Sleeping || !pawnState.Conscious)
				{
					_curTexture = _parent._closed;
					_portraitTexture = _parent._closed;
					return;
				}
				if(pawnState.InPainShock)
				{
					_curTexture = _parent._inPain;
					_portraitTexture = _parent._inPain;
					return;
				}
				BodyPartStatus.Status partStatus;
				if(bodyPartStatus.GetPartStatus(_bodyPartLocator.PartRecord, out partStatus) && partStatus.missing)
				{
					_curTexture = _parent._missing;
					_portraitTexture = _parent._missing;
					return;
				}
				if(_closeOnAiming && pawnState.Aiming)
				{
					_curTexture = _parent._aiming;
					_portraitTexture = _parent._open;
					return;
				}
				_curTexture = blink ? _parent._closed : _parent._open;
				_portraitTexture = _parent._open;
			}
			
			public void Render(
				Vector3 rootPos,
				Quaternion rootQuat,
				Rot4 rootRot4,
				Vector3 renderNodeOffset,
				Mesh renderNodeMesh,
				bool portrait)
			{
				TextureSet curTexSet = portrait ? _portraitTexture : _curTexture;
				if(curTexSet == null)
				{
					return;
				}
				curTexSet.GetIndexForRot(rootRot4, out float index);
				Texture2DArray curTextureArray = curTexSet.GetTextureArray();
				Vector3 offset = rootQuat * (renderNodeOffset + _parent.additionalOffset);
				if(!portrait)
				{
					_matPropBlock.SetFloat(Shaders.TexIndexPropID, index);
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
		}
						
		public override void Initialize(
			Pawn pawn,
			BodyDef bodyDef,
			string defaultTexPath,
			Dictionary<string, string> namedTexPaths,
			BodyPartSignals bodyPartSignals)
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

			bodyPartSignals.GetSignals("PP_EyeBlink", out List<PartSignal> partSignals);
			PartSignal eyeBlinkSignal = partSignals.Find(i => i.argument is HumanEyeBehavior.BlinkPartSignalArg);
			_blinkSignalArg = eyeBlinkSignal?.argument as HumanEyeBehavior.BlinkPartSignalArg;
			// If blink signal couldn't be found, disable eye blinking.
			if(_blinkSignalArg == null)
			{
				_blinkSignalArg = new HumanEyeBehavior.BlinkPartSignalArg() { blinkClose = false };
			}
			_eyeData = new EyeData[2];
			_eyeData[0] = new EyeData(this, true, leftEyePartLocator);
			_eyeData[1] = new EyeData(this, false, rightEyePartLocator);
		}
		
		public override void Update(
			PawnState pawnState,
			BodyPartStatus bodyPartStatus,
			ref bool updatePortrait)
		{
			foreach(var eyePart in _eyeData)
			{
				eyePart.Update(pawnState, bodyPartStatus, _blinkSignalArg.blinkClose);
			}
		}
				
		public override void Render(
			Vector3 rootPos,
			Quaternion rootQuat,
			Rot4 rootRot4,
			Vector3 renderNodeOffset,
			Mesh renderNodeMesh,
			int partIdentifier,
			bool portrait)
		{
			_eyeData[partIdentifier].Render(rootPos, rootQuat, rootRot4, renderNodeOffset, renderNodeMesh, portrait);
		}

		}

		public override object Clone()
		{
			return MemberwiseClone();
		}
	}
}
