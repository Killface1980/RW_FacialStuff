// ReSharper disable StyleCop.SA1307
// ReSharper disable InconsistentNaming
// ReSharper disable StyleCop.SA1401
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnassignedField.Global

namespace FacialStuff.Defs
{
    using System.Collections.Generic;

    using Verse;

    public class MouthDef : Def
    {
        public List<string> hairTags = new List<string>();

        public List<ThingDef> forbiddenOnRace = new List<ThingDef>();

        public string texPath = string.Empty;
    }
}