using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace RW_FacialHair
{
    public class BeardDef : Def
    {
        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> BeardTags = new List<string>();

    }

    public class TacheDef : Def
    {
        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> BeardTags = new List<string>();

    }
}

