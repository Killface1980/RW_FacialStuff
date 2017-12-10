namespace FacialStuff.Components
{
    using UnityEngine;

    using Verse;

    public class CompProperties_WeaponExtensions : CompProperties
    {
        #region Public Fields

        public Vector3 FirstHandPosition = Vector3.zero;

        public Vector3 SecondHandPosition = Vector3.zero;

        #endregion Public Fields

        #region Public Constructors

        public CompProperties_WeaponExtensions()
        {
            this.compClass = typeof(CompWeaponExtensions);
        }

        #endregion Public Constructors
    }
}

