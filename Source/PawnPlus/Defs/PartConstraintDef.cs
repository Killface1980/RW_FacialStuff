namespace PawnPlus.Defs
{
    using System.Collections.Generic;

    using PawnPlus.Parts;

    using Verse;

    public class PartConstraintDef : Def
	{
		public BodyDef raceBodyDef;
		public List<PartClass> partClasses = new List<PartClass>();
	}
}
