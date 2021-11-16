namespace PawnPlus
{
    using System.Collections.Generic;

    using Verse;

    public static class BodyPartRecordExtension
	{
		public static IEnumerable<BodyPartRecord> GetChildParts(this BodyPartRecord bodyPart)
		{
			yield return bodyPart;
			foreach(BodyPartRecord childPart in bodyPart.parts)
			{
				foreach(BodyPartRecord grandChildPart in childPart.GetChildParts())
				{
					yield return grandChildPart;
				}
			}
		}
	}
}
