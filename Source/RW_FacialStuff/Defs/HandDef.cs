namespace FacialStuff.Defs
{
    using System.Collections.Generic;

    using UnityEngine;

    using Verse;

    public class HandDef : ThingDef
    {
        #region Public Fields

        public List<CompTargets> WeaponCompLoader = new List<CompTargets>();

        #endregion Public Fields

        #region Public Classes

        public class CompTargets
        {
            #region Public Fields

            public Vector3 firstHandPosition = Vector3.zero;
            public Vector3 secondHandPosition = Vector3.zero;
            public List<string> thingTargets = new List<string>();

            #endregion Public Fields
        }

        #endregion Public Classes
    }

}
