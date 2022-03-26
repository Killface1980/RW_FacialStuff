// ReSharper disable StyleCop.SA1307
// ReSharper disable InconsistentNaming
// ReSharper disable StyleCop.SA1401
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnassignedField.Global

using System.Collections.Generic;
using Verse;

namespace FacialStuff.Defs
{
    public class MouthDef : Def
    {
        public List<string> styleTags = new();

        public List<ThingDef> forbiddenOnRace = new();

        public string texPath = string.Empty;
    }
}