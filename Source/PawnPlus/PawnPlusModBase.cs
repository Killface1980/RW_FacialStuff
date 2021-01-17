using PawnPlus.Defs;
using HugsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace PawnPlus
{
	class PawnPlusModBase : ModBase
	{
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
			
			var partDefs = DefDatabase<PartDef>.AllDefsListForReading;
			foreach(var partDef in partDefs)
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

			Parts.PartConstraintManager.ReadFromConstraintDefs();
		}
	}
}
