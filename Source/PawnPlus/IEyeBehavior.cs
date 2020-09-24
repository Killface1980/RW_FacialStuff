using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Verse;

namespace PawnPlus
{	
	public interface IEyeBehavior : ICloneable, IExposable
	{
		public void Initialize(BodyDef bodyDef, out List<int> usedBodyPartIndices);

		public void Update(Pawn pawn, PawnState pawnState, Dictionary<int, Queue<PartSignal>> bodyPartSignals);
	}
}
