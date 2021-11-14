using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnPlus
{
	public static class BodyPartRecordExtension
	{
		public static IEnumerable<BodyPartRecord> GetChildParts(this BodyPartRecord bodyPart)
		{
			yield return bodyPart;
			foreach(var childPart in bodyPart.parts)
			{
				foreach(var grandChildPart in childPart.GetChildParts())
				{
					yield return grandChildPart;
				}
			}
		}
	}
}
