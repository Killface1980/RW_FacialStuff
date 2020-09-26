using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnPlus.Defs
{
    public class PartDef : Def
    {
        public HairGender hairGender;

        public IGraphicProvider graphicProvider;

        public List<BodyPartLocator> representBodyParts = new List<BodyPartLocator>();

        public Dictionary<string, string> namedTexPaths = new Dictionary<string, string>();

        public string defaultTexPath;

        public List<string> hairTags = new List<string>();

        // The following variables are initialized in FacialStuffModBase.DefsLoaded()

        [Unsaved(false)]
        public List<BodyDef> _allowedRaceBodyDefs = new List<BodyDef>();

        [Unsaved(false)]
        public static HashSet<PartDef> _eyePartDefs =  new HashSet<PartDef>();
	}
}
