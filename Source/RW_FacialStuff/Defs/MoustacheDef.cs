// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable MissingXmlDoc
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global

using JetBrains.Annotations;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FacialStuff.Defs
{
    public class MoustacheDef : Def
    {
        public bool drawMouth;

        public StyleGender styleGender = StyleGender.Male;

        [NotNull]
        public List<string> styleTags = new();

        [NotNull]
        public List<ThingDef> forbiddenOnRace = new();

        [NotNull]
        public string texPath = string.Empty;
    }
}