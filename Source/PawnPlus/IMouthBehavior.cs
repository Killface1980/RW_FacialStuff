using System;
using System.Collections.ObjectModel;
using Verse;

namespace PawnPlus
{
	public interface IMouthBehavior : ICloneable, IExposable
	{
		public class Params
		{
			public int mouthTextureIdx;

			public void Reset()
			{
				mouthTextureIdx = -1;
			}
		}

		public void InitializeTextureIndex(ReadOnlyCollection<string> textureNames);

		public void Update(Pawn pawn, Rot4 headRot, PawnState pawnState, Params mouthParams);

		public int GetTextureIndexForPortrait();
	}
}
