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
	public class HumanWrinkleRenderer : IPartRenderer
	{
		public SimpleCurve ageIntensityCurve;
		public Vector3 additionalOffset = new Vector3(0f, 0f, 0f);

		private MaterialPropertyBlock _matPropBlock = new MaterialPropertyBlock();
		private TextureSet _textureSet;
		private Color _skinColor;

		public void Initialize(
			Pawn pawn, 
			BodyDef bodyDef, 
			string defaultTexPath, 
			Dictionary<string, string> namedTexPaths, 
			BodyPartSignals bodyPartSignals, 
			ref TickDelegate tickDelegate)
		{
			_skinColor = pawn.story.SkinColor;
			float intensity = ageIntensityCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
			_skinColor.a = intensity;
			_textureSet = TextureSet.Create(defaultTexPath);
			_matPropBlock.SetTexture("_MainTex", _textureSet.GetTextureArray());
			_matPropBlock.SetColor("_Color", _skinColor);
		}

		public void Render(
			Vector3 rootPos, 
			Quaternion rootQuat, 
			Rot4 rootRot4, 
			Vector3 renderNodeOffset, 
			Mesh renderNodeMesh, 
			bool portrait)
		{
			Vector3 offset = rootQuat * (renderNodeOffset + additionalOffset);
			_textureSet.GetIndexForRot(rootRot4, out float index);
			if(!portrait)
			{
				_matPropBlock.SetFloat(Shaders.TexIndexPropID, index);
				_matPropBlock.SetColor(Shaders.ColorOnePropID, _skinColor);
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
				Shaders.FacePart.SetColor(Shaders.ColorOnePropID, _skinColor);
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
