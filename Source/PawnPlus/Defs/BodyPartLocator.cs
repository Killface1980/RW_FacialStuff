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
		private BodyDef bodyDef;

		private BodyPartDef bodyPartDef;

		private string bodyPartLabel;
		
		[Unsaved(false)]
		private BodyPartRecord _resolvedBodyPartRecord;

		[Unsaved(false)]
		private bool _resolved = false;

		public BodyPartRecord PartRecord
		{
			get
			{
				if(!_resolved)
				{
					LocateBodyPart(bodyDef);
					_resolved = true;
				}
				return _resolvedBodyPartRecord;
			}
		}

		private void LocateBodyPart(BodyDef bodyDef)
		{
			_resolvedBodyPartRecord =
				bodyDef?.GetPartsWithDef(bodyPartDef).ToList().FindLast(i => CompareBodyPartLabel(i.untranslatedCustomLabel, bodyPartLabel));			
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
