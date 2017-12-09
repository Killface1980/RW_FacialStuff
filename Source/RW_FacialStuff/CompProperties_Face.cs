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
        public bool hasMouth = true;

        public bool hasBeard = true;

        public bool hasWrinkles = true;

        public bool hasEyes = true;

        public bool canRotateHead = true;

        public List<FaceDrawer> comps = new List<FaceDrawer>();

        public CompProperties_Face()
        {
            this.compClass = typeof(CompFace);
        }
    }
}
