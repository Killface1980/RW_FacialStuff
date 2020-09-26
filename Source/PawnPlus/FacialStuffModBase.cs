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
				bool isEye = false;
				foreach(BodyPartLocator partLocator in partDef.representBodyParts)
				{
					partLocator.LocateBodyPart();
					if(!partDef._allowedRaceBodyDefs.Contains(partLocator.bodyDef))
					{
						partDef._allowedRaceBodyDefs.Add(partLocator.bodyDef);
						isEye |= partLocator.resolvedBodyPartRecord.groups.Contains(BodyPartGroupDefOf.Eyes);
					}
				}
				if(isEye)
				{
					PartDef._eyePartDefs.Add(partDef);
				}
			}
		}
	}
}
