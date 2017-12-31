using System.Collections.Generic;

namespace FacialStuff
{
    using Verse;

    public class CompProperties_Face : CompProperties
    {
        public bool needsBlankHumanHead;

        public bool hasMouth;

        public bool hasBeard;

        public bool hasWrinkles;

        public bool hasEyes;

        public bool canRotateHead;

        public bool hasOrganicHair;

        public List<PawnHeadDrawer> drawers = new List<PawnHeadDrawer>();

        public bool needsAlienHair;

        public CompProperties_Face()
        {
            this.compClass = typeof(CompFace);
        }
    }
}
