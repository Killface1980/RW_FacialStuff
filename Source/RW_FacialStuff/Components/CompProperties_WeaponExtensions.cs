namespace FacialStuff.Components
{
    using UnityEngine;

    using Verse;

    // ReSharper disable UnassignedField.Global
    // ReSharper disable StyleCop.SA1307
    // ReSharper disable StyleCop.SA1401
    // ReSharper disable InconsistentNaming
    public class CompProperties_WeaponExtensions : CompProperties
    {
        #region Public Fields

        public float? AttackAngleOffset;

        public Vector3 LeftHandPosition;

        public Vector3 RightHandPosition;

        public Vector3 WeaponPositionOffset;

        #endregion Public Fields

        #region Public Constructors

        public CompProperties_WeaponExtensions()
        {
            this.compClass = typeof(CompWeaponExtensions);
        }

        #endregion Public Constructors
    }
}