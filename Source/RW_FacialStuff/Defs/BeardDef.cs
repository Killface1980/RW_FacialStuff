// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable MissingXmlDoc
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnassignedField.Global

// ReSharper disable StyleCop.SA1310

using JetBrains.Annotations;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FacialStuff.Defs
{
    public class BeardDef : Def
    {
        public BeardType beardType;

        public bool drawMouth;

        public HairGender hairGender;

        public List<string> hairTags = new List<string>();

        public List<ThingDef> forbiddenOnRace = new List<ThingDef>();

        [NotNull]
        public string texPath = string.Empty;
    }
}