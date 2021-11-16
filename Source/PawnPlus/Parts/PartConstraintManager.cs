namespace PawnPlus.Parts
{
    using System.Collections.Generic;

    using PawnPlus.Defs;

    using Verse;

    public static class PartConstraintManager
	{
		private static Dictionary<BodyDef, List<PartConstraintDef>> racePartConstraints = 
			new Dictionary<BodyDef, List<PartConstraintDef>>();

		public static void ReadFromConstraintDefs()
		{
			List<PartConstraintDef> constraintDefs = DefDatabase<PartConstraintDef>.AllDefsListForReading;
			foreach(PartConstraintDef constraintDef in constraintDefs)
			{
				BodyDef raceBodyDef = constraintDef.raceBodyDef;
				if(!racePartConstraints.TryGetValue(raceBodyDef, out List<PartConstraintDef> constraints))
				{
					constraints = new List<PartConstraintDef>();
					racePartConstraints.Add(raceBodyDef, constraints);
				}

				foreach(PartClass partClass in constraintDef.partClasses)
				{
					foreach(PartClass otherPartClass in constraintDef.partClasses)
					{
						if(partClass != otherPartClass)
						{
							constraints.Add(constraintDef);
						}
					}
				}
			}
		}

		public static bool CheckConstraint(
			BodyDef raceBodyDef, 
			Dictionary<PartCategoryDef, PartDef> categoryParts, 
			out PartConstraintDef conflictingConstraintDef)
		{
			if(!racePartConstraints.TryGetValue(raceBodyDef, out List<PartConstraintDef> partConstraints))
			{
				conflictingConstraintDef = null;
				return true;
			}

			foreach(PartConstraintDef constraintDef in partConstraints)
			{
				int count = 0;
				foreach(PartClass partClass in constraintDef.partClasses)
				{
					if(partClass.categoryDef == null)
					{
						Log.Warning(
							"Pawn Plus: one of the categoryDefs in the constraint def " + constraintDef.defName + 
							" is null. The constraint def will be ignored.");
						conflictingConstraintDef = null;
						return true;
					}

					if(categoryParts.TryGetValue(partClass.categoryDef, out PartDef partDef) &&
						partDef.partClass.subcategory == partClass.subcategory)
					{
						count++;
					}
				}

				if(count >= constraintDef.partClasses.Count)
				{
					conflictingConstraintDef = constraintDef;
					return false;
				}
			}

			conflictingConstraintDef = null;
			return true;
		}
	}
}
