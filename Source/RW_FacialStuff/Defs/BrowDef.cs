// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable MissingXmlDoc
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
namespace FacialStuff.Defs
{
    using System.Collections.Generic;



    using RimWorld;

    using Verse;

    public class BrowDef : Def
    {
        public HairGender hairGender = HairGender.Any;

      
        
        public List<string> hairTags = new List<string>();

      
        
        public List<ThingDef> raceList = new List<ThingDef>();

      
        public string texPath;
    }
}