using PawnPlus.Defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnPlus.Parts
{
	public class PartGenHelper
	{
		public virtual void PartsPreGeneration(Pawn pawn)
		{

		}

		public virtual Dictionary<string, PartDef> GeneratePartInCategory(
			Pawn pawn, 
			FactionDef pawnFactionDef,
			Dictionary<string, List<PartDef>> partsInCategory)
		{
			Dictionary<string, PartDef> genParts = new Dictionary<string, PartDef>();
			foreach(var pair in partsInCategory)
			{
				string category = pair.Key;
				List<PartDef> partDefList = pair.Value;
				if(partDefList.NullOrEmpty())
				{
					Log.Warning(
						"Pawn Plus: could not generate part for " +
						pawn +
						" in the part category " +
						category +
						". No parts are availble.");
					continue;
				}
				if(category == "Beard" || category == "Moustache")
				{
					continue;
				}
				IEnumerable<PartDef> partDefCandidates = 
					from partDef in partDefList
					where partDef.hairTags.SharesElementWith(pawnFactionDef.hairTags)
					select partDef;
				if(!partDefCandidates.Any())
				{
					Log.Warning(
						"Pawn Plus: no parts are available for the pawn " + 
						pawn + 
						" in the part category " + 
						category + 
						". Pawn generation constraints will be ignored.");
					partDefCandidates = partDefList;
				}
				PartDef genPart = partDefCandidates.RandomElementByWeight(p => PartGenHelper.PartChoiceLikelyhoodFor(p.hairGender, pawn.gender));
				genParts.Add(category, genPart);
			}
			return genParts;
		}

		public virtual void PartsPostGeneration(Pawn pawn)
		{

		}

		public static float PartChoiceLikelyhoodFor(HairGender preferGender, Gender pawnGender)
		{
			if(pawnGender == Gender.None)
			{
				return 100f;
			}
			if(pawnGender == Gender.Male)
			{
				switch(preferGender)
				{
					case HairGender.Female:
						return 1f;
					case HairGender.FemaleUsually:
						return 5f;
					case HairGender.MaleUsually:
						return 30f;
					case HairGender.Male:
						return 70f;
					case HairGender.Any:
						return 60f;
				}
			}
			if(pawnGender == Gender.Female)
			{
				switch(preferGender)
				{
					case HairGender.Female:
						return 70f;
					case HairGender.FemaleUsually:
						return 30f;
					case HairGender.MaleUsually:
						return 5f;
					case HairGender.Male:
						return 1f;
					case HairGender.Any:
						return 60f;
				}
			}
			return 0f;
		}
	}
}
