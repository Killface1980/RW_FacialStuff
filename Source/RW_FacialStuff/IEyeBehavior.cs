using System;
using System.Collections.Generic;
using Verse;

namespace FacialStuff
{
	public interface IEyeBehavior : ICloneable, IExposable
	{
		public class Result
		{
			public bool openEye;

			public void Reset()
			{
				openEye = false;
			}
		}
		
		public int NumEyes { get; }

		public void Update(Pawn pawn, Rot4 headRot, PawnState pawnState, List<Result> eyeResults);

		public bool GetEyeMirrorFlagForPortrait(int eyeIndex);
	}
}
