using System;
using System.Collections.Generic;
using Verse;

namespace PawnPlus
{
	public enum EyeAction
	{
		None = 0,
		Closed = 1,
		Pain = 2
	}
	
	public interface IEyeBehavior : ICloneable, IExposable
	{
		public class Result
		{
			public EyeAction eyeAction;

			public void Reset()
			{
				eyeAction = EyeAction.None;
			}
		}
		
		public int NumEyes { get; }

		public void Update(Pawn pawn, Rot4 headRot, PawnState pawnState, List<Result> eyeResults);

		public bool GetEyeMirrorFlagForPortrait(int eyeIndex);
	}
}
