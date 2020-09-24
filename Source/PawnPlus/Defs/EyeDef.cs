using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnPlus.Defs
{
    public class EyeDef : Def
    {
        public HairGender hairGender;

        public IGraphicProvider graphicProvider;

        public List<BodyPartLocator> representBodyParts = new List<BodyPartLocator>();

        public Dictionary<string, string> namedTexPaths = new Dictionary<string, string>();

        public string defaultTexPath;

        public List<string> hairTags = new List<string>();

        // The following variable is initialized in FacialStuffModBase.DefsLoaded()
        [Unsaved(false)]
        public List<BodyDef> _allowedRaceBodyDefs = new List<BodyDef>();
	}
}