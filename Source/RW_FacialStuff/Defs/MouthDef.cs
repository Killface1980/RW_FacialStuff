namespace FacialStuff.Defs
{
    using System.Collections.Generic;

    using RimWorld;

    using Verse;

    public class MouthDef : Def
    {
        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

        public List<ThingDef> raceList = new List<ThingDef>();
    }
}