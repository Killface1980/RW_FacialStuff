using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Defs
{
    using RimWorld;

    using UnityEngine;

    using Verse;

   public class BodyAnimDef : Def
    {
        public BodyType bodyType = BodyType.Undefined;

        public float armLength;

        public float shoulderWidth;


       // public float legLength;

        public float hipWidth;


        public float extraLegLength;


        public List<Vector3> shoulderOffsets =
            new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        public List<Vector3> hipOffsets =
            new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        // public float hipOffsetVerticalFromCenter;



}
}
