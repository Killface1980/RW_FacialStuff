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
	class FacialHairRenderer : IPartRenderer
	{
		private TextureSet _textureSet;
		private MaterialPropertyBlock _matPropBlock = new MaterialPropertyBlock();
		private Color _hairColor;

		public Vector3 additionalOffset = new Vector3(0f, 0f, 0f);

		public void Initialize(
			Pawn pawn,
			BodyDef bodyDef,
			string defaultTexPath,
			Dictionary<string, string> namedTexPaths,
			BodyPartSignals bodyPartSignals,
			ref TickDelegate tickDelegate)
		{
			_hairColor = pawn.story.hairColor;
			_textureSet = TextureSet.Create(defaultTexPath);
			if(_textureSet == null)
			{
				return;
			}
			_matPropBlock.SetColor("_Color", _hairColor);
			_matPropBlock.SetTexture("_MainTex", _textureSet.GetTextureArray());
		}

		public void Render(
			Vector3 rootPos,
			Quaternion rootQuat,
			Rot4 rootRot4,
			Vector3 renderNodeOffset,
			Mesh renderNodeMesh,
			bool portrait)
		{
			if(_textureSet == null)
			{
				return;
			}
			_textureSet.GetIndexForRot(rootRot4, out float index);
			Vector3 offset = rootQuat * (renderNodeOffset + additionalOffset);
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
			} else
			{
				Shaders.FacePart.mainTexture = _textureSet.GetTextureArray();
				Shaders.FacePart.SetFloat(Shaders.TexIndexPropID, index);
				Shaders.FacePart.SetColor(Shaders.ColorOnePropID, _hairColor);
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
