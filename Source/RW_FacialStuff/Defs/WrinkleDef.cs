namespace FacialStuff.Defs
{
    using RimWorld;
    using System.Collections.Generic;
    using Verse;

    public class WrinkleDef : Def
    {
        public HairGender hairGender = HairGender.Any;

        public List<ThingDef> raceList = new List<ThingDef>();

        public string texPath;
    }
}