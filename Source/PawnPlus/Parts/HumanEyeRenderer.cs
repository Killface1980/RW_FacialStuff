﻿using PawnPlus.Defs;
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
		public BodyPartLocator leftEyePartLocator;
		public BodyPartLocator rightEyePartLocator;

		private struct EyeData
		{
			public TextureSet _curTexture;
			public TextureSet _portraitTexture;
		}

		private MaterialPropertyBlock _matPropBlock = new MaterialPropertyBlock();
		private TextureSet _open;
		private TextureSet _closed;
		private TextureSet _dead;
		private TextureSet _missing;
		private TextureSet _inPain;
		private TextureSet _aiming;
		private HumanEyeBehavior.BlinkPartSignalArg _blinkSignalArg;
		private EyeData[] _eyeData = new EyeData[2];
				
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

			if(leftEyePartLocator != null)
			{
				bodyPartSignals.GetSignals(leftEyePartLocator.PartRecord, out List<PartSignal> partSignals);
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
			SetAllEyesPortrait(_open);
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
			if(!UpdateEyesCommon(pawnState))
			{
				if(!UpdateEye(pawnState, bodyPartStatus, leftEyePartLocator, 0))
				{
					_eyeData[0]._portraitTexture = _open;
					if(pawnState.Aiming)
					{
						_eyeData[0]._curTexture = _aiming;
					}
					else
					{
						_eyeData[0]._curTexture = _blinkSignalArg.blinkClose ? _closed : _open;
					}
				}
				if(!UpdateEye(pawnState, bodyPartStatus, rightEyePartLocator, 1))
				{
					_eyeData[1]._portraitTexture = _open;
					_eyeData[1]._curTexture = _blinkSignalArg.blinkClose ? _closed : _open;
				}
			}
		}

		private bool UpdateEyesCommon(PawnState pawnState)
		{
			if(!pawnState.Alive || pawnState.Sleeping || !pawnState.Conscious)
			{
				SetAllEyes(_closed);
				SetAllEyesPortrait(_closed);
				return true;
			}
			if(pawnState.InPainShock)
			{
				SetAllEyes(_inPain);
				SetAllEyesPortrait(_inPain);
				return true;
			}
			return false;
		}

		private bool UpdateEye(
			PawnState pawnState,
			BodyPartStatus bodyPartStatus,
			BodyPartLocator bodyPartLocator,
			int partIdentifier)
		{
			BodyPartStatus.Status partStatus;
			if(bodyPartStatus.GetPartStatus(leftEyePartLocator.PartRecord, out partStatus) && partStatus.missing)
			{
				_eyeData[partIdentifier]._curTexture = _missing;
				_eyeData[partIdentifier]._portraitTexture = _missing;
				return true;
			}
			return false;
		}
		
		private void SetAllEyes(TextureSet texture)
		{
			_eyeData[0]._curTexture = texture;
			_eyeData[1]._curTexture = texture;
		}

		private void SetAllEyesPortrait(TextureSet texture)
		{
			_eyeData[0]._portraitTexture = texture;
			_eyeData[1]._portraitTexture = texture;
		}

		public void Render(
			Vector3 rootPos,
			Quaternion rootQuat,
			Rot4 rootRot4,
			Vector3 renderNodeOffset,
			Mesh renderNodeMesh,
			int partIdentifier,
			bool portrait)
		{
			TextureSet curTexSet = portrait ?
				_eyeData[partIdentifier]._portraitTexture:
				_eyeData[partIdentifier]._curTexture;
			if(curTexSet == null)
			{
				return;
			}
			curTexSet.GetIndexForRot(rootRot4, out float index);
			Texture2DArray curTextureArray = curTexSet.GetTextureArray();
			Vector3 offset = rootQuat * (renderNodeOffset + additionalOffset);
			if(!portrait)
			{
				_matPropBlock.SetTexture(Shaders.MainTexPropID, _eyeData[partIdentifier]._curTexture.GetTextureArray());
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
