// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable MissingXmlDoc
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global

using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FacialStuff.Defs
{
    public class EarDef : Def
    {
        public StyleGender styleGender = StyleGender.Any;

        public List<string> styleTags = new List<string>();

        public List<ThingDef> forbiddenOnRace = new List<ThingDef>();

        public string texBasePath = string.Empty;

        public string texName = string.Empty;
    }
}