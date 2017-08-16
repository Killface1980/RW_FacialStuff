namespace FacialStuff.Defs
{
    using RimWorld;
    using System.Collections.Generic;
    using Verse;

    public class BrowDef : Def
    {
        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

        public List<ThingDef> raceList = new List<ThingDef>();

        public string texPath;
    }
}