using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.Parts
{	
	public abstract class PartRendererBase : ICloneable
	{
		public abstract void Initialize(
			Pawn pawn, 
			BodyDef bodyDef, 
			string defaultTexPath, 
			Dictionary<string, string> namedTexPaths,
			BodyPartSignals bodyPartSignals);
		
		public abstract void Render(
			Vector3 rootPos,
			Quaternion rootQuat,
			Rot4 rootRot4,
			Vector3 renderNodeOffset,
			Mesh renderNodeMesh,
			int partIdentifier,
			bool portrait);

		public virtual void Update(
			PawnState pawnState,
			BodyPartStatus bodyPartStatus,
			ref bool updatePortrait)
		{

		}

		public virtual void UpdateRare(
			PawnState pawnState,
			BodyPartStatus bodyPartStatus,
			ref bool updatePortrait)
		{

		}

		public virtual void UpdateLong(
			PawnState pawnState,
			BodyPartStatus bodyPartStatus,
			ref bool updatePortrait)
		{

		}
		
		public abstract void DoCustomizationGUI(Rect contentRect);

		public abstract object Clone();
	}
}
