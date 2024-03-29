﻿// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable MissingXmlDoc
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnassignedField.Global

// ReSharper disable StyleCop.SA1310
// ReSharper disable CheckNamespace

namespace RimWorld
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using PawnPlus;

    using UnityEngine;

    using Verse;
    using Verse.AI;

    public class BodyAnimDef : Def
    {
        #region Public Fields
        public string thingTarget = null;

        public List<PawnBodyDrawer> bodyDrawers;
        public string handType;
        public bool quadruped;
        public bool bipedWithHands;

        public float armLength;

        public float extraLegLength;

        public List<Vector3> hipOffsets = new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        public Vector2 headOffset = Vector2.zero;

        public float offCenterX;

        public List<Vector3> shoulderOffsets =
            new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        [NotNull]
        public Dictionary<LocomotionUrgency, WalkCycleDef> walkCycles =
            new Dictionary<LocomotionUrgency, WalkCycleDef>();

        public string WalkCycleType = "Undefined";

        [NotNull]
        public List<PoseCycleDef> poseCycles = new List<PoseCycleDef>();

        public string PoseCycleType = "Undefined";

        #endregion Public Fields

        // public float hipOffsetVerticalFromCenter;
    }
}