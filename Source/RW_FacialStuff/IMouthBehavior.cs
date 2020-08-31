using System;
using System.Collections.ObjectModel;
using Verse;

namespace FacialStuff
{
	public interface IMouthBehavior : ICloneable, IExposable
	{
		public class Params
		{
			public bool render;
			public int mouthTextureIdx;
			public bool mirror;

			public void Reset()
			{
				render = false;
				mouthTextureIdx = -1;
				mirror = false;
			}
		}

		public void InitializeTextureIndex(ReadOnlyCollection<string> textureNames);

		public void Update(Pawn pawn, Rot4 headRot, PawnState pawnState, Params mouthParams);

		public int GetTextureIndexForPortrait(out bool mirror);
	}
}
