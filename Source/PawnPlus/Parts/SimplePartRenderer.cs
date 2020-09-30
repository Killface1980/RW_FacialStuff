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
	public class SimplePartRenderer : IPartRenderer
	{
		protected Graphic _graphic;

		public virtual void Initialize(
			Pawn pawn, 
			BodyDef bodyDef, 
			string defaultTexPath, 
			Dictionary<string, string> namedTexPaths, 
			BodyPartSignals bodyPartSignals)
		{
			_graphic = GraphicDatabase.Get<Graphic_Multi>(
				defaultTexPath,
				Shaders.FacePart);
		}
		
		public void Update(PawnState pawnState, BodyPartStatus partStatus, ref bool updatePortrait)
		{
			
		}
		
		public void Render(Vector3 rootPos, Quaternion rootQuat, Rot4 rootRot4, Vector3 renderNodeOffset, Mesh renderNodeMesh, bool portrait)
		{
			if(_graphic == null)
			{
				return;
			}
			Material partMat = _graphic.MatAt(rootRot4);
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
