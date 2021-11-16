namespace PawnPlus.Parts
{
    using PawnPlus.Defs;

    public class PartClass
	{
		public PartCategoryDef categoryDef;
		public string subcategory = string.Empty;
		
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
