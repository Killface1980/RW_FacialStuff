using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawnPlus.Parts
{
	public class PartSignal
	{
		public PartSignalArg argument;
		
		public PartSignal(PartSignalArg argument, string customSignalLabel = null)
		{
			this.argument = argument;
		}
	}
}
