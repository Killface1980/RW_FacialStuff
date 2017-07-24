using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FacialStuff.Defs
{

    public class MoustacheDef : Def
    {
        public string texPath;

        public bool drawMouth = false;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

        public List<ThingDef> raceList = new List<ThingDef>();

    }
}

