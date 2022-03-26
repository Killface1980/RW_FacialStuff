// ReSharper disable StyleCop.SA1307
// ReSharper disable InconsistentNaming
// ReSharper disable StyleCop.SA1401
// ReSharper disable FieldCanBeMadeReadOnly.Global


using JetBrains.Annotations;

// ReSharper disable UnassignedField.Global

using System.Collections.Generic;
using UnityEngine;

namespace FacialStuff
{
    public class CompLoaderTargets
    {
        #region Public Fields

        public Vector3 firstHandPosition = Vector3.zero;
        public Vector3 secondHandPosition = Vector3.zero;

        [NotNull] public List<string> thingTargets = new();

        public float? attackAngleOffset;

        public Vector3 weaponPositionOffset = Vector3.zero;
        public Vector3 aimedWeaponPositionOffset;

        // Animals
        public string handType;

        public bool quadruped;
        public bool bipedWithHands;
        public List<PawnBodyDrawer> bodyDrawers;

        #endregion Public Fields
    }
}