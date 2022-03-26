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
    public class BrowDef : Def
    {
        public StyleGender styleGender = StyleGender.Any;

        public List<string> styleTags = new();

        public List<ThingDef> forbiddenOnRace = new();

        public string texBasePath;

        public string texName;
    }
}