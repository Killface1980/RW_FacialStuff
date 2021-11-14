using System;
using Verse;

namespace PawnPlus
{
	public interface IHeadBehavior : ICloneable, IExposable
	{
		public enum TargetType
		{
			// Value determines the actions priority

			// No particular target to look at 
			None = 0,
			// Ocassionally turn head when sleeping. No target
			Sleep = 1,
			// Look at someone nearby (can be disabled in settings)
			RandomStare = 2,
			// Target is a social interaction initiator. This means this instance is a recipient, which
			// has delay of kSocialInteractionRecipientDelayTick before looking at the initiator.
			SocialInitiator = 3,
			// Target is a social interaction recipient
			SocialRecipient = 4,
			// Look at the aimed Thing
			Aim = 5
		}
		
		public void Initialize(Pawn pawn);

		public void SetTarget(Thing target, IHeadBehavior.TargetType targetType);

		public void Update(Pawn pawn, PawnState pawnState, out Rot4 headFacing);

		public Rot4 GetRotationForPortrait();
	}
}
