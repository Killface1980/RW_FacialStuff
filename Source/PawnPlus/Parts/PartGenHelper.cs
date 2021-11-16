namespace PawnPlus.Parts
{
    using System.Collections.Generic;
    using System.Linq;

    using PawnPlus.Defs;

    using RimWorld;

    using Verse;

    public class PartGenHelper
	{
		public class PartGenParam
		{
			public PartCategoryDef categoryDef;
			public Dictionary<Gender, SimpleCurve> genChanceAgeCurvePerGender = new Dictionary<Gender, SimpleCurve>
                                                                                    {
                                                                                        [Gender.Female] = new SimpleCurve(new List<CurvePoint> { new CurvePoint(0, 1f) }),
                                                                                        [Gender.Male] = new SimpleCurve(new List<CurvePoint> { new CurvePoint(0, 1f) }),
                                                                                        [Gender.None] = new SimpleCurve(new List<CurvePoint> { new CurvePoint(0, 1f) })
                                                                                    };
		}

		public List<PartGenParam> partGenParams;

		public virtual void PartsPreGeneration(Pawn pawn)
		{

		}

		public virtual Dictionary<PartCategoryDef, PartDef> GeneratePartInCategory(
			Pawn pawn, 
			FactionDef pawnFactionDef,
			Dictionary<PartCategoryDef, List<PartDef>> partsInCategory)
		{
			Dictionary<PartCategoryDef, PartDef> genParts = new Dictionary<PartCategoryDef, PartDef>();
			foreach(PartGenParam partGenParam in partGenParams)
			{
				float rand = Rand.Value;
				if(!partGenParam.genChanceAgeCurvePerGender.TryGetValue(pawn.gender, out SimpleCurve genChanceCurve))
				{
					continue;
				}

				if(rand > genChanceCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat))
				{
					continue;
				}

				PartCategoryDef categoryDef = partGenParam.categoryDef;
				PartDef genPart = null;
				if(categoryDef.defName == "Hair")
				{
					ModExtensionHair modExtHair = pawn.story.hairDef.GetModExtension<ModExtensionHair>();
					if(modExtHair == null || modExtHair.partDef == null)
					{
						continue;
					}

					genPart = modExtHair.partDef;
				}
				else
				{
					if(!partsInCategory.TryGetValue(categoryDef, out List<PartDef> partDefList) || partDefList.NullOrEmpty())
					{
						Log.Warning(
							"Pawn Plus: could not generate part for " +
							pawn +
							" in the part category " +
							categoryDef.defName +
							". No parts are available.");
						continue;
					}

					IEnumerable<PartDef> candidates = GetCandidates(pawn, pawnFactionDef, partDefList);
					genPart = candidates.RandomElementByWeight(p => PartChoiceLikelyhoodFor(p.hairGender, pawn.gender));
				}
				
				genParts.Add(categoryDef, genPart);
				if(!PartConstraintManager.CheckConstraint(pawn.RaceProps.body, genParts, out PartConstraintDef conflictingConstraintDef))
				{
					genParts.Remove(categoryDef);
				}
			}

			return genParts;
		}

		public virtual void PartsPostGeneration(Pawn pawn)
		{

		}

		public static float PartChoiceLikelyhoodFor(StyleGender preferGender, Gender pawnGender)
		{
			if(pawnGender == Gender.None)
			{
				return 100f;
			}

			if(pawnGender == Gender.Male)
			{
				switch(preferGender)
				{
					case StyleGender.Female:
						return 1f;
					case StyleGender.FemaleUsually:
						return 5f;
					case StyleGender.MaleUsually:
						return 30f;
					case StyleGender.Male:
						return 70f;
					case StyleGender.Any:
						return 60f;
				}
			}

			if(pawnGender == Gender.Female)
			{
				switch(preferGender)
				{
					case StyleGender.Female:
						return 70f;
					case StyleGender.FemaleUsually:
						return 30f;
					case StyleGender.MaleUsually:
						return 5f;
					case StyleGender.Male:
						return 1f;
					case StyleGender.Any:
						return 60f;
				}
			}

			return 0f;
		}

		protected IEnumerable<PartDef> GetCandidates(Pawn pawn, FactionDef factionDef, List<PartDef> partDefList)
		{
			IEnumerable<PartDef> partDefCandidates =
					from partDef in partDefList
					where partDef.hairTags.Contains(factionDef.allowedCultures.RandomElement().styleItemTags.FirstOrDefault().Tag)
					select partDef;
			if(!partDefCandidates.Any())
			{
				Log.Warning(
					"Pawn Plus: no parts are available for the pawn " +
					pawn +
					" in the part category " +
					partDefList.First()?.partClass.categoryDef.defName +
					". Pawn generation constraints will be ignored.");
				partDefCandidates = partDefList;
			}

			return partDefCandidates;
		}
	}
}
