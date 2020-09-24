using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawnPlus
{
	public class PartSignal
	{
		public PartSignalType type;
		public PartSignalArg argument;
		public string customSignalLabel;
		
		public PartSignal(PartSignalType signalType, PartSignalArg argument, string customSignalLabel = null)
		{
			this.type = signalType;
			this.argument = argument;
			this.customSignalLabel = customSignalLabel;
		}
	}
}
