// ReSharper disable InconsistentNaming

namespace FacialStuff.Defs
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Verse;

    public class MouthDef : Def
    {
        public List<string> hairTags = new List<string>();

        public List<ThingDef> raceList = new List<ThingDef>();

        public string texPath = string.Empty;
    }
}