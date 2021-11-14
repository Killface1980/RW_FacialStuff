using PawnPlus.Defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.UI
{
	public partial class DialogEditAppearance
	{
		private abstract class CategoryWorker
		{
			public Vector2 scrollPos;

			public abstract string CategoryTabLabel
			{
				get;
			}

			public abstract List<TabRecord> GetGenderTabs();

			public abstract List<TabRecord> GetTagTabs();
		}

		private class CategoryWorkerHair : CategoryWorker
		{
			public override string CategoryTabLabel => "Hair";

			private int _genderTabIndex;
			private int _tagTabIndex;

			public override List<TabRecord> GetGenderTabs()
			{
				return new List<TabRecord>()
				{
					new TabRecord("All", delegate { _genderTabIndex = 0; }, _genderTabIndex == 0),
					new TabRecord("Male", delegate { _genderTabIndex = 1; }, _genderTabIndex == 1),
					new TabRecord("Female", delegate { _genderTabIndex = 2; }, _genderTabIndex == 2)
				};
			}

			public override List<TabRecord> GetTagTabs()
			{
				return new List<TabRecord>()
				{
					new TabRecord("All", delegate { }, true),
					new TabRecord("Tag1", delegate { }, false),
					new TabRecord("Tag2", delegate { }, false),
					new TabRecord("Tag3", delegate { }, false),
					new TabRecord("Tag4", delegate { }, false),
					new TabRecord("Tag5", delegate { }, false)
				};
			}
		}

		private class CategoryWorkerPartDef : CategoryWorker
		{
			private int _genderTabIndex;
			private int _tagTabIndex;
			private PartCategoryDef _categoryDef;
			private List<PartDef> _allPartsInCategory;
			private List<string> _tags;
			private Dictionary<string, List<PartDef>> _partsPerTag;
			private List<HairGender> _genders;
			private Dictionary<HairGender, List<PartDef>> _partsPerGender;

			public override string CategoryTabLabel => _categoryDef.label;

			public CategoryWorkerPartDef(PartCategoryDef categoryDef, List<PartDef> partsInCategory)
			{
				_categoryDef = categoryDef;
				_allPartsInCategory = partsInCategory;
				_tags =
					(from partDef in partsInCategory
					 from tag in partDef.hairTags
					 select tag)
					.Distinct().OrderBy(tagName => tagName).ToList();
				_partsPerTag = new Dictionary<string, List<PartDef>>();
				foreach(string tag in _tags)
				{
					List<PartDef> parts =
						(from partDef in partsInCategory
						 where partDef.hairTags.Contains(tag)
						 select partDef).ToList();
					_partsPerTag.Add(tag, parts);
				}
				_genders =
					(from partDef in partsInCategory
					 select partDef.hairGender).Distinct().OrderBy(gender => gender).ToList();
				_partsPerGender = new Dictionary<HairGender, List<PartDef>>();
				foreach(HairGender gender in _genders)
				{
					List<PartDef> parts =
						(from partDef in partsInCategory
						 where partDef.hairGender == gender
						 select partDef).ToList();
					_partsPerGender.Add(gender, parts);
				}
			}

			public override List<TabRecord> GetGenderTabs()
			{
				List<TabRecord> genderTabRecords = new List<TabRecord>();
				int index = 1;
				if(_genders.Count > 1)
				{
					genderTabRecords.Add(new TabRecord(
					"All",
					delegate { _genderTabIndex = 0; },
					_genderTabIndex == 0));
					++index;
				}
				foreach(HairGender gender in _genders)
				{
					int tempIndex = index;
					++index;
					genderTabRecords.Add(new TabRecord(
						gender.ToString(),
						delegate { _genderTabIndex = tempIndex; },
						_genderTabIndex == tempIndex));
				}
				return genderTabRecords;
			}

			public override List<TabRecord> GetTagTabs()
			{
				List<TabRecord> tagTabRecords = new List<TabRecord>();
				tagTabRecords.Add(new TabRecord(
					"All",
					delegate { _tagTabIndex = 0; },
					_tagTabIndex == 0));
				int index = 1;
				foreach(string tag in _tags)
				{
					int tempIndex = index;
					++index;
					tagTabRecords.Add(new TabRecord(
						tag,
						delegate { _tagTabIndex = tempIndex; },
						_tagTabIndex == tempIndex));
				}
				return tagTabRecords;
			}
		}
	}
}
