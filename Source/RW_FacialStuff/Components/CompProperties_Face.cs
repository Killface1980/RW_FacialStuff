// ReSharper disable UnassignedField.Global
// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable InconsistentNaming

using FacialStuff.Defs;
using System.Collections.Generic;

using Verse;

namespace FacialStuff
{
    public class CompProperties_Face : CompProperties
    {
        public bool canRotateHead;

        public List<PawnHeadDrawer> headDrawers = new List<PawnHeadDrawer>();

        public bool hasBeard;
        
        public bool hasMouth;

        public bool hasOrganicHair;

        public bool hasWrinkles;

        public bool useAlienRacesHairTags;

        public bool needsBlankHumanHead;
        
        public List<string> useTexCollection;

        public List<PerEyeBehavior> perEyeBehaviors = new List<PerEyeBehavior>();
        
        public CompProperties_Face()
        {
            this.compClass = typeof(CompFace);
        }
    }
}