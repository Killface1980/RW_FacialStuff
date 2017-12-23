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

        public float armLength;

        public float shoulderWidth;

        public float shoulderOffsetVerFromCenter;

        public float shoulderOffsetWhenFacingHorizontal = -0.05f;


        public float legLength;

        public float hipWidth;

        public float hipOffsetHorWhenFacingHorizontal;

        public float hipOffsetVerticalFromCenter;



    }
}
