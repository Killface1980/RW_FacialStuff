using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnPlus.Defs
{
    public class PartDef : Def
    {
        public class LinkedBodyPart
		{
            public BodyPartLocator bodyPartLocator;
            public PartRender.Attachment attachment;
            public IGraphicProvider graphicProvider;
        }

        public HairGender hairGender;

        public List<string> hairTags = new List<string>();

        public string category;

        public BodyDef raceBodyDef;

        public List<LinkedBodyPart> linkedBodyParts;

        public Dictionary<string, string> namedTexPaths = new Dictionary<string, string>();

        public string defaultTexPath;

        public static IEnumerable<string> GetCategoriesInRace(BodyDef raceBodyDef)
		{
            if(_allParts.TryGetValue(raceBodyDef, out Dictionary<string, List<PartDef>> partsInCategory))
			{
                foreach(var pair in partsInCategory)
				{
                    yield return pair.Key;
				}
			}
		}

        public static List<PartDef> GetAllPartsFromCategory(BodyDef raceBodyDef, string category)
		{
            if(_allParts.TryGetValue(raceBodyDef, out Dictionary<string, List<PartDef>> partGenGroups))
			{
                if(partGenGroups.TryGetValue(category, out List<PartDef> parts))
				{
                    return parts;
				}
			}
            return null;
		}

        // The following variables are initialized in FacialStuffModBase.DefsLoaded()

        [Unsaved(false)]
        public static Dictionary<BodyDef, Dictionary<string, List<PartDef>>> _allParts =
            new Dictionary<BodyDef, Dictionary<string, List<PartDef>>>();
    }
}
