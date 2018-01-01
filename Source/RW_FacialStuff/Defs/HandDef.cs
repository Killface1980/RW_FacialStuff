// ReSharper disable StyleCop.SA1307
// ReSharper disable InconsistentNaming
// ReSharper disable StyleCop.SA1401
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnassignedField.Global
namespace FacialStuff.Defs
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using UnityEngine;

    using Verse;

    public class HandDef : ThingDef
    {
        #region Public Fields

        [NotNull]
        public List<CompTargets> WeaponCompLoader = new List<CompTargets>();

        #endregion Public Fields

        #region Public Classes

        public class CompTargets
        {
            #region Public Fields

            public Vector3 firstHandPosition = Vector3.zero;
            public Vector3 secondHandPosition = Vector3.zero;

            [NotNull]
            public List<string> thingTargets = new List<string>();

            public float? attackAngleOffset;

            public Vector3 weaponPositionOffset = Vector3.zero;

            #endregion Public Fields
        }

        #endregion Public Classes
    }
}
