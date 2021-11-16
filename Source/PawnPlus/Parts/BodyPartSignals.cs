namespace PawnPlus.Parts
{
    using System.Collections.Generic;

    using Verse;

    public class BodyPartSignals
	{
		private BodyDef _bodyDef;
		private Dictionary<string, List<PartSignal>> _bodyPartSignals = new Dictionary<string, List<PartSignal>>();

		public BodyPartSignals(BodyDef bodyDef)
		{
			_bodyDef = bodyDef;
		}

		public bool GetSignals(string signalName, out List<PartSignal> partSignals)
		{
			if(!_bodyPartSignals.TryGetValue(signalName, out partSignals))
			{
				partSignals = new List<PartSignal>();
				return false;
			}

			return true;
		}

		public bool RegisterSignal(string signalName, PartSignal partSignal)
		{
			if(!_bodyPartSignals.TryGetValue(signalName, out List<PartSignal> partSignals))
			{
				partSignals = new List<PartSignal>();
				_bodyPartSignals.Add(signalName, partSignals);
			}

			partSignals.Add(partSignal);
			return true;
		}
	}
}
