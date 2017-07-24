using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FacialStuff.Defs
{

    public class BeardDef : Def
    {
        public string texPath;

        public bool drawMouth;

        public HairGender hairGender = HairGender.Male;

        public List<string> hairTags = new List<string>();

        public List<ThingDef> raceList = new List<ThingDef>();

        public BeardType beardType = BeardType.FullBeard;
    }
}

