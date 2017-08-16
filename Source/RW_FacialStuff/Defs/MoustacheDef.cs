namespace FacialStuff.Defs
{
    using RimWorld;
    using System.Collections.Generic;
    using Verse;

    public class MoustacheDef : Def
    {
        public bool drawMouth = false;

        public HairGender hairGender = HairGender.Male;

        public List<string> hairTags = new List<string>();

        public List<ThingDef> raceList = new List<ThingDef>();

        public string texPath;
    }
}