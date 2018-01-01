// ReSharper disable StyleCop.SA1307
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

    using UnityEngine;

    using Verse;
    using Verse.AI;

    public class BodyAnimDef : Def
    {
        #region Public Fields

        public float armLength;

        public float extraLegLength;

        public List<Vector3> hipOffsets = new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        public float offCenterX;

        public List<Vector3> shoulderOffsets =
            new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        public Dictionary<LocomotionUrgency, WalkCycleDef> walkCycles =
            new Dictionary<LocomotionUrgency, WalkCycleDef>();

        public string WalkCycleType;

        #endregion Public Fields

        // public float hipOffsetVerticalFromCenter;
    }
}