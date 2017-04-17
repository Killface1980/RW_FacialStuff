using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RW_FacialStuff.Defs
{

    public class BeardDef : Def
    {
        public string texPathAverageNormal;
        public string texPathAveragePointy;
        public string texPathAverageWide;

        public bool drawMouth;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

    }

    public class EyeDef : Def
    {

        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

    }

    public class BrowDef : Def
    {

        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

    }

    public class WrinkleDef : Def
    {
        public string texPathAverageNormal;
        public string texPathAveragePointy;
        public string texPathAverageWide;

        public HairGender hairGender = HairGender.Any;

    }

    public class MouthDef : Def
    {
        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

    }

}

