// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable MissingXmlDoc
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global

namespace FacialStuff.Defs
{
    using JetBrains.Annotations;
    using RimWorld;
    using System.Collections.Generic;
    using Verse;

    public class MoustacheDef : Def
    {
        public bool drawMouth = false;

        public HairGender hairGender = HairGender.Male;

        [NotNull]
        public List<string> hairTags = new List<string>();

        [NotNull]
        public List<ThingDef> raceList = new List<ThingDef>();

        [NotNull]
        public string texPath;
    }
}