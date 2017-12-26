using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RimWorld
{
    using FacialStuff;

    using RimWorld;

    using UnityEngine;

    using Verse;
    using Verse.AI;

    public class BodyAnimDef : Def
    {
        public float armLength;

        public Dictionary<LocomotionUrgency, WalkCycleDef> walkCycles =
            new Dictionary<LocomotionUrgency, WalkCycleDef>();

        // public float legLength;


        public float extraLegLength;

        public string WalkCycleType;

        public List<Vector3> shoulderOffsets =
            new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        public List<Vector3> hipOffsets =
            new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };


        // public float hipOffsetVerticalFromCenter;



    }
}
