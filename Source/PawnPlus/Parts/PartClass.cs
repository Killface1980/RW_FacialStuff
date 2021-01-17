using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawnPlus.Parts
{
	public class PartClass
	{
		public Defs.PartCategoryDef categoryDef;
		public string subcategory = "";
		
		public static bool operator ==(PartClass a, PartClass b)
		{
			return a.categoryDef == b.categoryDef && a.subcategory == b.subcategory;
		}

		public static bool operator !=(PartClass a, PartClass b)
		{
			return a.categoryDef != b.categoryDef || a.subcategory != b.subcategory;
		}
	}
}
