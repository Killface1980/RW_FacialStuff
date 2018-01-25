using UnityEngine;
using Verse;

namespace FacialStuff
{
    // ReSharper disable UnassignedField.Global
    // ReSharper disable StyleCop.SA1307
    // ReSharper disable StyleCop.SA1401
    // ReSharper disable InconsistentNaming
    public class CompProperties_WeaponExtensions : CompProperties
    {
        #region Public Fields

        public float? AttackAngleOffset;

        public Vector3 LeftHandPosition = Vector3.zero;

        public Vector3 RightHandPosition = Vector3.zero;

        public Vector3 WeaponPositionOffset = Vector3.zero;
        public Vector3 AimedWeaponPositionOffset = Vector3.zero;

        #endregion Public Fields

        #region Public Constructors

        public CompProperties_WeaponExtensions()
        {
            this.compClass = typeof(CompWeaponExtensions);
        }

        #endregion Public Constructors
    }
}