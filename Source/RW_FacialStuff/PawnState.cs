using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FacialStuff
{
	public class PawnState
	{
		public const int kAliveBitNum = 0;
		public const int kSleepingBitNum = 1;
		public const int kAimingBitNum = 2;
		public const int kInPainShockBitNum = 3;
		public const int kFleeingBitNum = 4;
		public const int kBurningBitNum = 5;
		
		public bool alive = false;
		public bool sleeping = false;
		public bool aiming = false;
		public bool inPainShock = false;
		public bool fleeing = false;
		public bool burning = false;
		
		public Thing AimedThing { get; private set; }

		private Pawn _pawn;

		public PawnState(Pawn pawn)
		{
			_pawn = pawn;
		}

		public void UpdateState()
		{
			alive = !_pawn.Dead;
			Stance_Busy stance = _pawn.stances?.curStance as Stance_Busy;
			aiming =
				stance != null &&
				!stance.neverAimWeapon &&
				stance.focusTarg.IsValid;
			AimedThing = stance.focusTarg.Thing;
			inPainShock = _pawn.health.InPainShock;
			fleeing = _pawn.Fleeing();
			burning = _pawn.IsBurning();
			sleeping = !_pawn.Awake();
		}

		public long ToBitFlags()
		{
			return 
				Convert.ToInt64(alive) << kAliveBitNum |
				Convert.ToInt64(sleeping) << kSleepingBitNum |
				Convert.ToInt64(aiming) << kAimingBitNum |
				Convert.ToInt64(inPainShock) << kInPainShockBitNum |
				Convert.ToInt64(fleeing) << kFleeingBitNum |
				Convert.ToInt64(burning) << kBurningBitNum;
		}
	}
}
