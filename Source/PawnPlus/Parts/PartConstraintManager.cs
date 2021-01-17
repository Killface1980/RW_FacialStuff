using PawnPlus.Defs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnPlus.Parts
{
	public static class PartConstraintManager
	{
		private static Dictionary<BodyDef, List<PartConstraintDef>> racePartConstraints = 
			new Dictionary<BodyDef, List<PartConstraintDef>>();

		public static void ReadFromConstraintDefs()
		{
			List<PartConstraintDef> constraintDefs = DefDatabase<PartConstraintDef>.AllDefsListForReading;
			foreach(var constraintDef in constraintDefs)
			{
				BodyDef raceBodyDef = constraintDef.raceBodyDef;
				if(!racePartConstraints.TryGetValue(raceBodyDef, out List<PartConstraintDef> constraints))
				{
					constraints = new List<PartConstraintDef>();
					racePartConstraints.Add(raceBodyDef, constraints);
				}
				foreach(var partClass in constraintDef.partClasses)
				{
					foreach(var otherPartClass in constraintDef.partClasses)
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
			Dictionary<string, PartDef> categoryParts, 
			out PartConstraintDef conflictingConstraintDef)
		{
			if(!racePartConstraints.TryGetValue(raceBodyDef, out List<PartConstraintDef> partConstraints))
			{
				conflictingConstraintDef = null;
				return true;
			}
			foreach(var constraintDef in partConstraints)
			{
				int count = 0;
				foreach(var partClass in constraintDef.partClasses)
				{
					if(categoryParts.TryGetValue(partClass.category, out PartDef partDef) &&
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
