using FacialStuff.Defs;
using HugsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FacialStuff
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
		}
	}
}
