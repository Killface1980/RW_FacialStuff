namespace FacialStuff.Defs
{
    using System;
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using RimWorld;

    using Verse;

    public class MouthDef : Def
    {
        public HairGender hairGender = HairGender.Any;

        [NotNull]
        
        public List<string> hairTags = new List<string>();

        [NotNull]
        
        public List<ThingDef> raceList = new List<ThingDef>();

        [NotNull]
        public string texPath = string.Empty;
    }
}