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
		
		public bool Alive { get; private set; } = false;
		public bool Sleeping { get; private set; } = false;
		public bool Aiming { get; private set; } = false;
		public bool InPainShock { get; private set; } = false;
		public bool Fleeing { get; private set; } = false;
		public bool Burning { get; private set; } = false;
		
		public Thing Aiming_Target { get; private set; }

		private Pawn _pawn;

		public PawnState(Pawn pawn)
		{
			_pawn = pawn;
		}

		public void UpdateState()
		{
			Alive = !_pawn.Dead;
			Stance_Busy stance = _pawn.stances?.curStance as Stance_Busy;
			Aiming =
				stance != null &&
				!stance.neverAimWeapon &&
				stance.focusTarg.IsValid;
			Aiming_Target = stance.focusTarg.Thing;
			InPainShock = _pawn.health.InPainShock;
			Fleeing = _pawn.Fleeing();
			Burning = _pawn.IsBurning();
			Sleeping = !_pawn.Awake();
		}

		public long ToBitFlags()
		{
			return 
				Convert.ToInt64(Alive) << kAliveBitNum |
				Convert.ToInt64(Sleeping) << kSleepingBitNum |
				Convert.ToInt64(Aiming) << kAimingBitNum |
				Convert.ToInt64(InPainShock) << kInPainShockBitNum |
				Convert.ToInt64(Fleeing) << kFleeingBitNum |
				Convert.ToInt64(Burning) << kBurningBitNum;
		}
	}
}
