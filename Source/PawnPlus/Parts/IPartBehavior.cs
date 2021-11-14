using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Verse;

namespace PawnPlus.Parts
{	
	public interface IPartBehavior : ICloneable, IExposable
	{
		public string UniqueID { get; }

		public void Initialize(BodyDef bodyDef, BodyPartSignals bodyPartSignals);
		
		public void Update(Pawn pawn, PawnState pawnState);
	}
}
