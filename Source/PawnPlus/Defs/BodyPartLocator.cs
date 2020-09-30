using RimWorld;
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
		public BodyPartDef bodyPartDef;

		public string bodyPartLabel;
				
		[Unsaved(false)]
		public BodyPartRecord _resolvedBodyPartRecord;
		
		[Unsaved(false)]
		public PartDef _parentPartDef;

		public BodyPartLocator()
		{

		}
		
		public void LocateBodyPart(BodyDef bodyDef)
		{
			_resolvedBodyPartRecord =
				bodyDef.GetPartsWithDef(bodyPartDef).ToList().FindLast(i => CompareBodyPartLabel(i.untranslatedCustomLabel, bodyPartLabel));			
		}

		private bool CompareBodyPartLabel(string candidatePartLabel, string searchLabel)
		{
			if(candidatePartLabel.NullOrEmpty())
			{
				return searchLabel.NullOrEmpty();
			}
			return candidatePartLabel == searchLabel;
		}
	}
}
