using PawnPlus.Defs;
using PawnPlus.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.Parts
{
	class HumanMouthRenderer : PartRendererBase
	{
		public BodyPartLocator mouthPartLocator;
		public Vector3 additionalOffset = new Vector3(0f, 0f, 0f);

		private Pawn _pawn;
		private TextureSet _normal;
		private TextureSet _happy;
		private TextureSet _minor;
		private TextureSet _major;
		private TextureSet _extreme;
		private TextureSet _crying;
		private TextureSet _dead;
		private TextureSet _default;
		private TextureSet _curTexSet;
		private MaterialPropertyBlock _matPropBlock = new MaterialPropertyBlock();
		private int _ticksSinceLastUpdate;

		public override void Initialize(
			Pawn pawn,
			BodyDef bodyDef,
			string defaultTexPath,
			Dictionary<string, string> namedTexPaths,
			BodyPartSignals bodyPartSignals)
		{
			_pawn = pawn;
			_default = TextureSet.Create(defaultTexPath);
			Dictionary<string, TextureSet> namedGraphics = new Dictionary<string, TextureSet>()
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
					namedGraphics[key] = TextureSet.Create(namedTexPaths[key]);
				} else
				{
					namedGraphics[key] = _default;
				}
			}
			_normal = namedGraphics["Normal"];
			_happy = namedGraphics["Happy"];
			_minor = namedGraphics["Minor"];
			_major = namedGraphics["Major"];
			_extreme = namedGraphics["Extreme"];
			_crying = namedGraphics["Crying"];
			_dead = namedGraphics["Dead"];
			_curTexSet = _normal;
			// If shader property left uninitialized, then the result from other MaterialPropertyBlock  
			// can interfere with it.
			_matPropBlock.SetColor("_Color", Color.white);
		}

		public override void Update(
			PawnState pawnState,
			BodyPartStatus bodyPartStatus,
			ref bool updatePortrait)
		{
			if(!pawnState.Alive)
			{
				_curTexSet = _dead;
				return;
			}
			if(pawnState.Fleeing || pawnState.InPainShock)
			{
				_curTexSet = _crying;
				return;
			}
			if(_pawn.needs == null)
			{
				_curTexSet = default;
				return;
			}
			float moodLevel = _pawn.needs.mood.CurInstantLevel;
			if(moodLevel <= _pawn.mindState.mentalBreaker.BreakThresholdExtreme)
			{
				_curTexSet = _extreme;
				return;
			}
			if(moodLevel <= _pawn.mindState.mentalBreaker.BreakThresholdMajor)
			{
				_curTexSet = _major;
				return;
			}
			if(moodLevel <= _pawn.mindState.mentalBreaker.BreakThresholdMinor)
			{
				_curTexSet = _minor;
				return;
			}
			float happyThreshold =
				_pawn.mindState.mentalBreaker.BreakThresholdMinor +
				((1f - _pawn.mindState.mentalBreaker.BreakThresholdMinor) / 2f);
			if(moodLevel < happyThreshold)
			{
				_curTexSet = _normal;
				return;
			}
			_curTexSet = _happy;
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
			TextureSet curTexSet = portrait ?
				_normal :
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
				UnityEngine.Graphics.DrawMesh(
					renderNodeMesh,
					Matrix4x4.TRS(rootPos + offset, rootQuat, Vector3.one),
					Shaders.FacePart,
					0,
					null,
					0,
					_matPropBlock);
			} else
			{
				Shaders.FacePart.mainTexture = curTextureArray;
				Shaders.FacePart.SetFloat(Shaders.TexIndexPropID, index);
				Shaders.FacePart.SetPass(0);
				UnityEngine.Graphics.DrawMeshNow(renderNodeMesh, rootPos + offset, rootQuat);
			}
		}


		public override object Clone()
		{
			return MemberwiseClone();
		}
	}
}
