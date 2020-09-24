using PawnPlus.Defs;
using HugsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

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

			var partDefs = DefDatabase<EyeDef>.AllDefsListForReading;
			foreach(var partDef in partDefs)
			{
				foreach(BodyPartLocator partLocator in partDef.representBodyParts)
				{
					partLocator.LocateBodyPart();
					if(!partDef._allowedRaceBodyDefs.Contains(partLocator.bodyDef))
					{
						partDef._allowedRaceBodyDefs.Add(partLocator.bodyDef);
					}
				}
			}
		}
	}
}
