using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawnPlus.Parts
{
	public class PartSignal
	{
		public string signalName;
		public PartSignalArg argument;
		
		public PartSignal(string signalName, PartSignalArg argument, string customSignalLabel = null)
		{
			this.signalName = signalName;
			this.argument = argument;
		}
	}
}
