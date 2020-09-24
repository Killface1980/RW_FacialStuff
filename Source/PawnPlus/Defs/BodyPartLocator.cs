using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnPlus.Defs
{
	public class BodyPartLocator
	{
		public BodyDef bodyDef;

		public BodyPartDef bodyPartDef;

		public string bodyPartLabel;

		[Unsaved(false)]
		public BodyPartRecord resolvedBodyPartRecord;

		[Unsaved(false)]
		public int resolvedPartIndex;

		public void LocateBodyPart()
		{
			resolvedBodyPartRecord =
				bodyDef.GetPartsWithDef(bodyPartDef).ToList().FindLast(i => i.untranslatedCustomLabel == bodyPartLabel);
			if(resolvedBodyPartRecord != null)
			{
				resolvedPartIndex =  bodyDef.GetIndexOfPart(resolvedBodyPartRecord);
			}
			else
			{
				resolvedPartIndex = -1;
			}
		}
	}
}
