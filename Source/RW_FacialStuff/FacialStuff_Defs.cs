using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FacialStuff.Defs
{

    public class BeardDef : Def
    {
        public string texPath;

        public bool drawMouth;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

        public List<ThingDef> raceList = new List<ThingDef>();
    }

    public class EyeDef : Def
    {

        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

        public List<ThingDef> raceList = new List<ThingDef>();
    }

    public class BrowDef : Def
    {

        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

        public List<ThingDef> raceList = new List<ThingDef>();
    }

    public class WrinkleDef : Def
    {
        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<ThingDef> raceList = new List<ThingDef>();
    }

    public class MouthDef : Def
    {
        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

        public List<ThingDef> raceList = new List<ThingDef>();
    }

}

