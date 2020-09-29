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
		public int _resolvedPartIndex;

		[Unsaved(false)]
		public PartDef _parentPartDef;

		public BodyPartLocator()
		{

		}

		public BodyPartLocator(BodyPartLocator other)
		{
			bodyPartDef = other.bodyPartDef;
			bodyPartLabel = other.bodyPartLabel;
			_resolvedBodyPartRecord = other._resolvedBodyPartRecord;
			_resolvedPartIndex = other._resolvedPartIndex;
			_parentPartDef = other._parentPartDef;
		}

		public void LocateBodyPart(BodyDef bodyDef)
		{
			_resolvedBodyPartRecord =
				bodyDef.GetPartsWithDef(bodyPartDef).ToList().FindLast(i => CompareBodyPartLabel(i.untranslatedCustomLabel, bodyPartLabel));			
			if(_resolvedBodyPartRecord != null)
			{
				_resolvedPartIndex =  bodyDef.GetIndexOfPart(_resolvedBodyPartRecord);
			}
			else
			{
				_resolvedPartIndex = -1;
			}
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
