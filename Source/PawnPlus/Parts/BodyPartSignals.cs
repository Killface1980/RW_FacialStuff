using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnPlus.Parts
{
	public class BodyPartSignals
	{
		private int _numBodyParts;
		private Dictionary<int, List<PartSignal>> _bodyPartSignals = new Dictionary<int, List<PartSignal>>();

		public BodyPartSignals(BodyDef bodyDef)
		{
			_numBodyParts = bodyDef.AllParts.Count;
		}

		public void GetSignals(int bodyPartIndex, out List<PartSignal> partSignals)
		{
			if(!_bodyPartSignals.TryGetValue(bodyPartIndex, out partSignals) || !IndexWithinRange(bodyPartIndex))
			{
				partSignals = new List<PartSignal>();
			}
		}

		public void RegisterSignal(int bodyPartIndex, PartSignal partSignal)
		{
			if(!IndexWithinRange(bodyPartIndex))
			{
				return;
			}
			if(!_bodyPartSignals.TryGetValue(bodyPartIndex, out List<PartSignal> partSignals))
			{
				partSignals = new List<PartSignal>();
				_bodyPartSignals.Add(bodyPartIndex, partSignals);
			}
			partSignals.Add(partSignal);
		}

		private bool IndexWithinRange(int bodyPartIndex)
		{
			return bodyPartIndex >= 0 && bodyPartIndex < _numBodyParts;
		}
	}
}
