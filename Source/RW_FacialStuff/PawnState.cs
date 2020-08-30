using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacialStuff
{
	public class PawnState
	{
		public bool alive = false;
		public bool sleeping = false;
		public bool aiming = false;
		public bool inPainShock = false;
		public bool fleeing = false;
		public bool burning = false;
		
		public const int kAliveBitNum = 0;
		public const int kSleepingBitNum = 1;
		public const int kAimingBitNum = 2;
		public const int kInPainShockBitNum = 3;
		public const int kFleeingBitNum = 4;
		public const int kBurningBitNum = 5;
		
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
