﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Verse;

namespace PawnPlus
{	
	public interface IPartBehavior : ICloneable, IExposable
	{
		public void Initialize(BodyDef bodyDef, out List<int> usedBodyPartIndices);

		public void SetPartSignalSink(Dictionary<int, List<PartSignal>> bodyPartSignalSinks);

		public void Update(Pawn pawn, PawnState pawnState);
	}
}
