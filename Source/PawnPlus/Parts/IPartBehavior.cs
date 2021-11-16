namespace PawnPlus.Parts
{
    using System;

    using Verse;

    public interface IPartBehavior : ICloneable, IExposable
	{
		public string UniqueID { get; }

		public void Initialize(BodyDef bodyDef, BodyPartSignals bodyPartSignals);
		
		public void Update(Pawn pawn, PawnState pawnState);
	}
}
