using PawnPlus.Parts;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnPlus.Defs
{
    public class PartDef : Def
    {
        public class Part
		{
            public string renderNodeName;
            public List<string> occludedRenderNodes;
            public IPartRenderer partRenderer;
        }

        public HairGender hairGender;

        public List<string> hairTags = new List<string>();

        public string category;

        public string subcategory = "";

        public BodyDef raceBodyDef;

        public List<Part> parts;

        public Dictionary<string, string> namedTexPaths = new Dictionary<string, string>();

        public string defaultTexPath;

        public static Dictionary<string, List<PartDef>> GetCategoriesInRace(BodyDef raceBodyDef)
		{
            if(_allParts.TryGetValue(raceBodyDef, out Dictionary<string, List<PartDef>> partsInCategory))
			{
                return partsInCategory;
			}
            return new Dictionary<string, List<PartDef>>();
		}
        
        // The following variables are initialized in FacialStuffModBase.DefsLoaded()

        [Unsaved(false)]
        public static Dictionary<BodyDef, Dictionary<string, List<PartDef>>> _allParts =
            new Dictionary<BodyDef, Dictionary<string, List<PartDef>>>();
    }
}
