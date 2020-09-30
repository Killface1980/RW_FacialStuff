using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawnPlus.Parts
{
	public struct TickDelegate
	{
		public delegate void Update(
			PawnState pawnState,
			BodyPartStatus bodyPartStatus,
			ref bool updatePortrait);
		
		public Update NormalUpdate { internal get; set; }

		public Update RareUpdate { internal get; set; }

		public Update LongUpdate { internal get; set; }
	}
}
