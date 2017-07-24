namespace FacialStuff.Defs
{
    using System.Collections.Generic;

    using RimWorld;

    using Verse;

    public class WrinkleDef : Def
    {
        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<ThingDef> raceList = new List<ThingDef>();
    }
}