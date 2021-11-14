﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnPlus.Defs
{
	public class PartCategoryDef : Def
	{
		public class CustomizationOption
		{
			public bool optional = false;
			public bool requireBodyShaping = false;
		}

		public CustomizationOption customization = null;

	}
}
