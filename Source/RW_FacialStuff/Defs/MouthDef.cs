// ReSharper disable InconsistentNaming
namespace FacialStuff.Defs
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using RimWorld;

    using Verse;

    [SuppressMessage("ReSharper", "StyleCop.SA1307")]
    public class MouthDef : Def
    {
      
        public List<string> hairTags = new List<string>();

      
        public List<ThingDef> raceList = new List<ThingDef>();

      
        public string texPath = string.Empty;
    }
}