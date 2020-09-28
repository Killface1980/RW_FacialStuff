using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus
{	
	public interface IGraphicProvider : ICloneable
	{
		public void Initialize(
			Pawn pawn, 
			BodyDef bodyDef, 
			BodyPartRecord bodyPartRecord, 
			string defaultTexPath, 
			Dictionary<string, string> namedTexPaths);
		
		public void Update(
			PawnState pawnState,
			in BodyPartStatus partStatus, 
			out Graphic graphic, 
			out Graphic portraitGraphic, 
			ref Vector3 additionalOffset,
			ref bool updatePortrait,
			IReadOnlyList<PartSignal> partSignals);
	}
}
