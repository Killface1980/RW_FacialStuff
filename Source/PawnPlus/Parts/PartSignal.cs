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
