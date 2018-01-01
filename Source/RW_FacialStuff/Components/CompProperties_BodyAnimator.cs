namespace FacialStuff
{
    // ReSharper disable UnassignedField.Global
    // ReSharper disable StyleCop.SA1307
    // ReSharper disable StyleCop.SA1401
    // ReSharper disable InconsistentNaming

    using FacialStuff.Defs;
    using JetBrains.Annotations;
    using RimWorld;
    using System.Collections.Generic;

    using FacialStuff.DefOfs;

    using UnityEngine;
    using Verse;

    public class CompProperties_BodyAnimator : CompProperties
    {

        #region Public Fields

        public bool bipedWithHands = false;
        public WalkCycleDef defaultCycleWalk = WalkCycleDefOf.Biped_Walk;
        [NotNull]
        public List<PawnBodyDrawer> drawers = new List<PawnBodyDrawer>();

        public string handType = "Human";
        public Vector3 hipOffset;

        public bool quadruped = false;
        public Vector3 shoulderOffset;

        #endregion Public Fields

        #region Public Constructors

        public CompProperties_BodyAnimator()
        {
            this.compClass = typeof(CompBodyAnimator);
        }

        #endregion Public Constructors

    }
}