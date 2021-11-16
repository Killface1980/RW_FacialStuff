namespace PawnPlus.Defs
{
    using System.Linq;

    using Verse;

    public class BodyPartLocator
	{
		private BodyDef bodyDef;

		private BodyPartDef bodyPartDef;

		private string bodyPartLabel;
		
		[Unsaved()]
		private BodyPartRecord _resolvedBodyPartRecord;

		[Unsaved()]
		private bool _resolved;

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
