using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    // ReSharper disable UnassignedField.Global
    // ReSharper disable StyleCop.SA1307
    // ReSharper disable StyleCop.SA1401
    // ReSharper disable InconsistentNaming
    public class CompProperties_BodyAnimator : CompProperties
    {
        #region Public Constructors

        public CompProperties_BodyAnimator()
        {
            this.compClass = typeof(CompBodyAnimator);
        }

        #endregion Public Constructors

        #region Public Fields

        [NotNull] public List<PawnBodyDrawer> bodyDrawers = new();

        public string handType = "Human";
        public Vector3 hipOffset;
        public Vector3 shoulderOffset;

        public bool bipedWithHands;
        public bool quadruped;

        #endregion Public Fields
    }
}