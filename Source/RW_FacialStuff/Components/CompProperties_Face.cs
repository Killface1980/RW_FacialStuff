// ReSharper disable UnassignedField.Global
// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable InconsistentNaming

using System.Collections.Generic;

using Verse;

namespace FacialStuff
{
    public class CompProperties_Face : CompProperties
    {
        public bool canRotateHead;

        public List<PawnHeadDrawer> headDrawers = new List<PawnHeadDrawer>();

        public bool hasBeard;

        public bool hasEyes;

        public bool hasEars;

        public bool hasMouth;

        public bool hasOrganicHair;

        public bool hasWrinkles;

        public bool useAlienRacesHairTags;

        public bool needsBlankHumanHead;

        public CompProperties_Face()
        {
            this.compClass = typeof(CompFace);
        }
    }
}