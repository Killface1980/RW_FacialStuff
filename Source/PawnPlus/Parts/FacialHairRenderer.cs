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
		private MatProps_Multi _materialProps;
		private Color _hairColor;

		public void Initialize(
			Pawn pawn,
			BodyDef bodyDef,
			string defaultTexPath,
			Dictionary<string, string> namedTexPaths,
			BodyPartSignals bodyPartSignals,
			ref TickDelegate tickDelegate)
		{
			_hairColor = pawn.story.hairColor;
			_materialProps = MatProps_Multi.Create(defaultTexPath);
			_materialProps.SetColor("_Color", _hairColor);
		}
		
		public void Render(
			Vector3 rootPos,
			Quaternion rootQuat,
			Rot4 rootRot4,
			Vector3 renderNodeOffset,
			Mesh renderNodeMesh,
			bool portrait)
		{
			MaterialPropertyBlock matPropBlock = _materialProps.GetMaterialProperty(rootRot4);
			if(!portrait)
			{
				UnityEngine.Graphics.DrawMesh(
					renderNodeMesh,
					Matrix4x4.TRS(rootPos + renderNodeOffset, rootQuat, Vector3.one),
					Shaders.FacePart,
					0,
					null,
					0,
					matPropBlock);
			} else
			{
				Shaders.FacePart.mainTexture = matPropBlock.GetTexture(Shaders.MainTexPropID);
				Shaders.FacePart.SetColor(Shaders.ColorOnePropID, _hairColor);
				Shaders.FacePart.SetPass(0);
				UnityEngine.Graphics.DrawMeshNow(renderNodeMesh, rootPos + renderNodeOffset, rootQuat);
			}
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
