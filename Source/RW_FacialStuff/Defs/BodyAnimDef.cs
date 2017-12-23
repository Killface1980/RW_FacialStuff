using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Defs
{
    using RimWorld;

    using Verse;

   public class BodyAnimDef : Def
    {
        public BodyType bodyType = BodyType.Undefined;

        public float hipOffsetHorWhenFacingHorizontal;

        public float hipWidth;

        public float hipOffsetVerticalFromCenter;

        public float legLength;

        public float shoulderWidth;

        public float shoulderOffsetVerFromCenter;

        public float armLength;

        public float shoulderOffsetWhenFacingHorizontal = -0.05f;
    }
}
