// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable MissingXmlDoc
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global

namespace FacialStuff.Defs
{
    using System.Collections.Generic;

    using FacialStuff.Enums;

    using JetBrains.Annotations;

    using RimWorld;

    using Verse;

    public class BeardDef : Def
    {
        public BeardType beardType;

        public bool drawMouth;

        public HairGender hairGender;

        public List<string> hairTags = new List<string>();

        public List<ThingDef> forbiddenOnRace = new List<ThingDef>();

        [NotNull]
        public string texPath;
    }
}