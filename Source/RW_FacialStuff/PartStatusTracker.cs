using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FacialStuff
{
	public enum BodyPartLevel : byte
	{
		Natural = 0,
		Removed = 1,
		Medieval = 2,
		Prosthetic = 3,
		Bionic = 4,
		Archotech = 5
	}
	
	// This class doesn't need to be serialized because OnHediffAdded will be called when the game loads the pawns.
	public class PartStatusTracker
	{
		private BodyPartLevel[] _partStatus;
		private List<int> _eyeIndices = new List<int>();

		public int EyeCount =>_eyeIndices.Count;

		public PartStatusTracker(BodyDef bodyDef)
		{
			_partStatus = new BodyPartLevel[bodyDef.AllParts.Count];
			int curIndex = 0;
			foreach(var bodyPartRecord in bodyDef.AllParts)
			{
				if(bodyPartRecord.IsInGroup(BodyPartGroupDefOf.Eyes))
				{
					_eyeIndices.Add(curIndex);
				}
				// Could probably use GetIndexOfPart(). But this does the same job and may be more efficient.
				++curIndex;
			}
		}

		public BodyPartLevel GetEyePartLevel(int eyeIdx)
		{
			return _partStatus[_eyeIndices[eyeIdx]];
		}

		public void NotifyHediffAdded(Hediff hediff)
		{
			int partIndex = hediff.Part.Index;
			if(hediff is Hediff_MissingPart)
			{
				_partStatus[partIndex] = BodyPartLevel.Removed;
			}
			else if(hediff is Hediff_AddedPart)
			{
				// No reliable way to test whether the part is prosthetic, bionic, or archotech. 
				// This code uses the part efficiency to determine the level.
				// TODO: consider adding ModExtension to the hediff defs instead.
				float partEfficiency = hediff.def.addedPartProps.partEfficiency;
				// Archotech
				if(partEfficiency >= 1.5f - 0.01f)
				{
					_partStatus[partIndex] = BodyPartLevel.Archotech;
				}
				// Bionic
				else if(partEfficiency >= 1.25f - 0.01f)
				{
					_partStatus[partIndex] = BodyPartLevel.Bionic;
				}
				// There is no hediff corresponding to natural parts, but in case there is a mod
				// that adds equivalent parts..
				else if(partEfficiency >= 1f - 0.01f)
				{
					_partStatus[partIndex] = BodyPartLevel.Natural;
				}
				// Prosthesis
				else if(partEfficiency >= 0.85 - 0.01f)
				{
					_partStatus[partIndex] = BodyPartLevel.Prosthetic;
				}
				// Peg leg, hand, etc.
				else
				{
					_partStatus[partIndex] = BodyPartLevel.Medieval;
				}
			}
		}

		public void NotifyHediffRemoved(Hediff hediff)
		{
			int partIndex = hediff.Part.Index;
			if(hediff is Hediff_MissingPart)
			{
				// This may be possible when using any mod that enables installing natural body parts.
				_partStatus[partIndex] = BodyPartLevel.Natural;
			}
		}

		public void NotifyBodyPartRestored(BodyPartRecord part)
		{
			int partIndex = part.Index;
			_partStatus[partIndex] = BodyPartLevel.Natural;
		}
	}
}
