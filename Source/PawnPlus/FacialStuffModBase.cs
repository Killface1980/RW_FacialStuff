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
	class FacialStuffModBase : ModBase
	{
		public override string ModIdentifier
		{
			get
			{
				return "FacialStuff";
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

			var headRenderDefs = DefDatabase<HeadRenderDef>.AllDefsListForReading;
			foreach(var headRenderDef in headRenderDefs)
			{
				HeadRenderDef.headTextureMapping.Add(headRenderDef.headTexture, headRenderDef);
			}

			var partDefs = DefDatabase<PartDef>.AllDefsListForReading;
			foreach(var partDef in partDefs)
			{
				foreach(var pair in partDef.raceSettings)
				{
					BodyDef bodyDef = pair.Key;
					PartDef.PartInfo partInfo = pair.Value;

					if(!PartDef._allParts.TryGetValue(bodyDef, out Dictionary<string, List<PartDef.PartInfo>> partsInRace))
					{
						partsInRace = new Dictionary<string, List<PartDef.PartInfo>>();
						PartDef._allParts.Add(bodyDef, partsInRace);
					}
					if(!partsInRace.TryGetValue(partInfo.category, out List<PartDef.PartInfo> partsInCategory))
					{
						partsInCategory = new List<PartDef.PartInfo>();
						partsInRace.Add(partInfo.category, partsInCategory);
					}
					partsInCategory.Add(partInfo);

					foreach(var bodyPartParam in pair.Value.linkedBodyParts)
					{
						bodyPartParam.bodyPartLocator._parentPartDef = partDef;
						bodyPartParam.bodyPartLocator.LocateBodyPart(pair.Key);
					}
				}
			}
		}
	}
}
