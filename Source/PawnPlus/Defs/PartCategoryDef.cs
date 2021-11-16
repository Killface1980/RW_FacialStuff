namespace PawnPlus.Defs
{
    using Verse;

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
