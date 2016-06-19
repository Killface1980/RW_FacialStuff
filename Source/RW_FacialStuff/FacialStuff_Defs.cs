using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace RW_FacialStuff
{
    public class BeardDef : Def
    {
        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

        public CrownType crownType;


    }

    public class TacheDef : Def
    {
        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

        public CrownType crownType;

    }

    public class SideburnDef : Def
    {
        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

        public CrownType crownType;

    }

    public class EyeDef: Def
    {

        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

        public CrownType crownType;

    }
}

