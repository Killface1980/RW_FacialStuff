using System.Collections.ObjectModel;
using Verse;

namespace FacialStuff
{
	public interface IMouthBehavior
	{
		public void InitializeTextureIndex(ReadOnlyCollection<string> textureNames);

		public void Update(Pawn pawn, Rot4 headRot, PawnState pawnState, out bool render, ref int mouthTextureIdx);

		public int GetTextureIndexForPortrait();
	}
}
