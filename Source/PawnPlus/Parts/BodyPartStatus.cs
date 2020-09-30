using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnPlus.Parts
{
	public class BodyPartStatus
	{
		public struct Status
		{
			// It is better to indicate missing instead of existing because the variable defaults to false.
			public bool missing;
			public Hediff_AddedPart hediffAddedPart;
		}

		private Pawn _pawn;
		private BodyDef _bodyDef;
		private Status[] _partStatus;
		private Status _invalidStatus = new Status
		{
			missing = false,
			hediffAddedPart = null
		};

		public BodyPartStatus(Pawn pawn)
		{
			_pawn = pawn;
			_bodyDef = _pawn.RaceProps.body;
			_partStatus = new Status[_bodyDef.AllParts.Count];
		}

		public bool GetPartStatus(BodyPartRecord bodyPartRecord, out Status partStatus)
		{
			if(IsValidBodyPartRecord(bodyPartRecord))
			{
				partStatus = _partStatus[bodyPartRecord.Index];
				return true;
			}
			partStatus = _invalidStatus;
			return false;
		}

		public void NotifyBodyPartHediffGained(BodyPartRecord bodyPart, Hediff hediff)
		{
			if(hediff is Hediff_AddedPart hediffAddedPart)
			{
				foreach(var childPart in bodyPart.GetChildParts())
				{
					_partStatus[childPart.Index] =
						new Status()
						{
							missing = false,
							hediffAddedPart = hediffAddedPart
						};
				}
			} else if(hediff is Hediff_MissingPart)
			{
				foreach(var childPart in bodyPart.GetChildParts())
				{
					_partStatus[childPart.Index] =
						new Status()
						{
							missing = true,
							hediffAddedPart = null
						};
				}
			}
		}

		public void NotifyBodyPartHediffLost(BodyPartRecord bodyPart, Hediff hediff)
		{
			if(hediff is Hediff_AddedPart)
			{
				foreach(var childPart in bodyPart.GetChildParts())
				{
					_partStatus[childPart.Index] =
						new Status()
						{
							missing = _partStatus[childPart.Index].missing,
							hediffAddedPart = null
						};
				}
			}
		}

		public void NotifyBodyPartRestored(BodyPartRecord bodyPart)
		{
			HashSet<int> affectedBodyParts = new HashSet<int>();
			foreach(var childPart in bodyPart.GetChildParts())
			{
				affectedBodyParts.Add(childPart.Index);
				_partStatus[childPart.Index] =
					new Status()
					{
						missing = false,
						hediffAddedPart = null
					};
			}
			// It is possible that the hediff still exists after restoration due to HediffDef.keepOnBodyPartRestoration.
			foreach(var hediff in _pawn.health.hediffSet.hediffs)
			{
				if(hediff.Part == null)
				{
					continue;
				}
				if(affectedBodyParts.Contains(hediff.Part.Index) &&
					hediff is Hediff_AddedPart hediffAddedPart)
				{
					_partStatus[hediff.Part.Index] =
						new Status()
						{
							missing = false,
							hediffAddedPart = hediffAddedPart
						};
				}
			}
		}

		private bool IsValidBodyPartRecord(BodyPartRecord bodyPartRecord)
		{
			return bodyPartRecord != null && bodyPartRecord.body == _bodyDef;
		}
	}
}
