using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff
{
    using FacialStuff.Harmony;

    using Verse;

    public class CompProperties_Face : CompProperties
    {
        public bool needsBlankHumanHead;

        public bool hasMouth;

        public bool hasBeard;

        public bool hasWrinkles;

        public bool hasEyes;

        public bool canRotateHead;

        public bool hasHands;

        public string handType = "Human";

        public List<PawnDrawer> drawers = new List<PawnDrawer>();


        public CompProperties_Face()
        {
            this.compClass = typeof(CompFace);
        }
    }
}
