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

    using RimWorld;

    using Verse;

    public class BeardDef : Def
    {
        public BeardType beardType = BeardType.FullBeard;

        public bool drawMouth;

        public HairGender hairGender = HairGender.Male;


      
       
        public List<string> hairTags = new List<string>();


      
       
        public List<ThingDef> raceList = new List<ThingDef>();

      
        public string texPath;
    }
}