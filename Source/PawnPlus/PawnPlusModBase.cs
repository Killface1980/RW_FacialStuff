﻿using PawnPlus.Defs;
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
				if(!PartDef._allParts.TryGetValue(partDef.raceBodyDef, out Dictionary<string, List<PartDef>> partsInRace))
				{
					partsInRace = new Dictionary<string, List<PartDef>>();
					PartDef._allParts.Add(partDef.raceBodyDef, partsInRace);
				}
				if(!partsInRace.TryGetValue(partDef.category, out List<PartDef> partsInCategory))
				{
					partsInCategory = new List<PartDef>();
					partsInRace.Add(partDef.category, partsInCategory);
				}
				partsInCategory.Add(partDef);
			}

			Parts.PartConstraintManager.ReadFromConstraintDefs();
		}
	}
}
