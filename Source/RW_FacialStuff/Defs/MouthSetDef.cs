﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FacialStuff.Defs
{
	public class MouthSetDef : Def
	{
		public HairGender hairGender;
		
		public List<ThingDef> allowedRaceThingDefs;

		public string texBasePath;
		
		public List<string> texNames = new List<string>();

		public string defaultMouth;

		public string deadMouth;

		public List<string> hairTags = new List<string>();
	}
}
