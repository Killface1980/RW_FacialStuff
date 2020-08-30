using System;
using System.Collections.Generic;
using Verse;

namespace FacialStuff
{
	public interface IEyeBehavior : ICloneable
	{
		public class Params
		{
			public bool render;
			public bool openEye;
			public bool mirror;

			public void Reset()
			{
				render = false;
				openEye = false;
				mirror = false;
			}
		}
		
		public int NumEyes { get; }

		public void Update(Pawn pawn, Rot4 headRot, PawnState pawnState, List<Params> eyeParams);

		public bool GetEyeMirrorFlagForPortrait(int eyeIndex);
	}
}
