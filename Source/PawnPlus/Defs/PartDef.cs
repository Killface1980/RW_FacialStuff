using PawnPlus.Parts;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnPlus.Defs
{
    public class PartDef : Def
    {
        public class SinglePart
		{
            public string renderNodeName;
            public int partIdentifier;
            public List<string> occludedRenderNodes;
        }

        public HairGender hairGender;

        public List<string> hairTags = new List<string>();

        public Parts.PartClass partClass = new PartClass();

        public IPartRenderer partRenderer;

        public BodyDef raceBodyDef;

        public List<PartDef.SinglePart> parts;

        public Dictionary<string, string> namedTexPaths = new Dictionary<string, string>();

        public string defaultTexPath;
                
        public static Dictionary<PartCategoryDef, List<PartDef>> GetCategoriesInRace(BodyDef raceBodyDef)
		{
            if(_allParts.TryGetValue(raceBodyDef, out Dictionary<PartCategoryDef, List<PartDef>> partsInCategory))
			{
                return partsInCategory;
			}
            return new Dictionary<PartCategoryDef, List<PartDef>>();
		}
        
        // The following variables are initialized in FacialStuffModBase.DefsLoaded()

        [Unsaved(false)]
        public static Dictionary<BodyDef, Dictionary<PartCategoryDef, List<PartDef>>> _allParts =
            new Dictionary<BodyDef, Dictionary<PartCategoryDef, List<PartDef>>>();
    }
}
