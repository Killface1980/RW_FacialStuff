namespace FacialStuff.Defs
{
    using RimWorld;
    using System.Collections.Generic;
    using Verse;

    public class BeardDef : Def
    {
        public BeardType beardType = BeardType.FullBeard;

        public bool drawMouth;

        public HairGender hairGender = HairGender.Male;

        public List<string> hairTags = new List<string>();

        public List<ThingDef> raceList = new List<ThingDef>();

        public string texPath;
    }
}