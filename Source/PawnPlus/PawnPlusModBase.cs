namespace PawnPlus
{
    using System.Collections.Generic;

    using HugsLib;

    using PawnPlus.Defs;
    using PawnPlus.Parts;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
	class PawnPlusModBase : ModBase
	{
		static PawnPlusModBase()
		{
			// Check if any other mod added a parser for Quaternion before registering 
			// our own parser
			if(ParseHelper.Parsers<Quaternion>.parser == null)
			{
				ParseHelper.Parsers<Quaternion>.Register(QuaternionFromString);
			}
		}

		public static Quaternion QuaternionFromString(string str)
		{
			Vector4 vec4 = ParseHelper.FromStringVector4Adaptive(str);
			return new Quaternion(vec4.x, vec4.y, vec4.z, vec4.w);
		}

		public override string ModIdentifier
		{
			get
			{
				return "PawnPlus";
			}
		}

		protected override bool HarmonyAutoPatch
		{ 
			get
			{
				return false;
			}
		}

		public override void DefsLoaded()
		{
			base.DefsLoaded();
			
			List<PartDef> partDefs = DefDatabase<PartDef>.AllDefsListForReading;
			foreach(PartDef partDef in partDefs)
			{
				if(partDef.raceBodyDef == null)
				{
					Log.Warning("Pawn Plus: <raceBodyDef> property in PartDef " + partDef.defName + " is null. The PartDef will be ignored.");
					continue;
				}

				if(!PartDef._allParts.TryGetValue(partDef.raceBodyDef, out Dictionary<PartCategoryDef, List<PartDef>> partsInRace))
				{
					partsInRace = new Dictionary<PartCategoryDef, List<PartDef>>();
					PartDef._allParts.Add(partDef.raceBodyDef, partsInRace);
				}

				if(partDef.partClass.categoryDef == null)
				{
					Log.Warning("Pawn Plus: <categoryDef> property in PartDef " + partDef.defName + " is null. The PartDef will be ignored.");
					continue;
				}

				if(!partsInRace.TryGetValue(partDef.partClass.categoryDef, out List<PartDef> partsInCategory))
				{
					partsInCategory = new List<PartDef>();
					partsInRace.Add(partDef.partClass.categoryDef, partsInCategory);
				}

				partsInCategory.Add(partDef);
			}

			PartConstraintManager.ReadFromConstraintDefs();
		}
	}
}
