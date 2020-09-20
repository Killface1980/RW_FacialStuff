using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnPlus.Defs
{
    public class EyeDef : Def
    {
        public HairGender hairGender;

        public string texBasePath;
        
        public List<ThingDef> allowedRaceThingDefs = new List<ThingDef>();

        public string texName;

        public EyeDef closedEyeDef;

        public EyeDef inPainEyeDef;
        
        public EyeDef missingEyeDef;

        public List<string> hairTags = new List<string>();
    }
}