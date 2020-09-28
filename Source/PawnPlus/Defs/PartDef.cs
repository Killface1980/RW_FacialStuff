using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnPlus.Defs
{
    public class PartDef : Def
    {
        public class PartInfo
		{
            public HairGender hairGender;
            public List<string> hairTags = new List<string>();
            public string category;
            public List<LinkedBodyPart> linkedBodyParts;
        }

        public class LinkedBodyPart
		{
            public BodyPartLocator bodyPartLocator;
            public PartRender.Attachment attachment;
            public IGraphicProvider graphicProvider;
        }
        
        public Dictionary<BodyDef, PartInfo> raceSettings = new Dictionary<BodyDef, PartInfo>();

        public Dictionary<string, string> namedTexPaths = new Dictionary<string, string>();

        public string defaultTexPath;

        public static IEnumerable<string> GetCategoriesInRace(BodyDef raceBodyDef)
		{
            if(_allParts.TryGetValue(raceBodyDef, out Dictionary<string, List<PartDef.PartInfo>> partsInCategory))
			{
                foreach(var pair in partsInCategory)
				{
                    yield return pair.Key;
				}
			}
		}

        public static List<PartInfo> GetAllPartsFromCategory(BodyDef raceBodyDef, string category)
		{
            if(_allParts.TryGetValue(raceBodyDef, out Dictionary<string, List<PartDef.PartInfo>> partGenGroups))
			{
                if(partGenGroups.TryGetValue(category, out List<PartInfo> parts))
				{
                    return parts;
				}
			}
            return null;
		}

        // The following variables are initialized in FacialStuffModBase.DefsLoaded()

        [Unsaved(false)]
        public static Dictionary<BodyDef, Dictionary<string, List<PartDef.PartInfo>>> _allParts =
            new Dictionary<BodyDef, Dictionary<string, List<PartDef.PartInfo>>>();
    }
}
