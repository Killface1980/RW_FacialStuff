﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnPlus.Parts
{
	public class BodyPartSignals
	{
		private BodyDef _bodyDef;
		private Dictionary<int, List<PartSignal>> _bodyPartSignals = new Dictionary<int, List<PartSignal>>();

		public BodyPartSignals(BodyDef bodyDef)
		{
			_bodyDef = bodyDef;
		}

		public void GetSignals(BodyPartRecord bodyPartRecord, out List<PartSignal> partSignals)
		{
			if(!_bodyPartSignals.TryGetValue(bodyPartRecord.Index, out partSignals) || !IsValidBodyPartRecord(bodyPartRecord))
			{
				partSignals = new List<PartSignal>();
			}
		}

		public void RegisterSignal(BodyPartRecord bodyPartRecord, PartSignal partSignal)
		{
			if(!IsValidBodyPartRecord(bodyPartRecord))
			{
				return;
			}
			if(!_bodyPartSignals.TryGetValue(bodyPartRecord.Index, out List<PartSignal> partSignals))
			{
				partSignals = new List<PartSignal>();
				_bodyPartSignals.Add(bodyPartRecord.Index, partSignals);
			}
			partSignals.Add(partSignal);
		}

		private bool IsValidBodyPartRecord(BodyPartRecord bodyPartRecord)
		{
			return bodyPartRecord != null && bodyPartRecord.body == _bodyDef;
		}
	}
}
