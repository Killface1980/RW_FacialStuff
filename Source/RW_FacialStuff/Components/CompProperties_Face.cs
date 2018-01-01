// ReSharper disable UnassignedField.Global
// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable InconsistentNaming
namespace FacialStuff
{
    using System.Collections.Generic;

    using Verse;

    public class CompProperties_Face : CompProperties
    {
        public bool canRotateHead;

        public List<PawnHeadDrawer> drawers = new List<PawnHeadDrawer>();

        public bool hasBeard;

        public bool hasEyes;

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