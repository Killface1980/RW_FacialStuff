// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable MissingXmlDoc
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable All
namespace FacialStuff.Defs
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using RimWorld;

    using Verse;

    public class WrinkleDef : Def
    {
        public HairGender hairGender = HairGender.Any;

        [NotNull]
        public List<ThingDef> raceList = new List<ThingDef>();

        [NotNull]
        public string texPath;
    }
}