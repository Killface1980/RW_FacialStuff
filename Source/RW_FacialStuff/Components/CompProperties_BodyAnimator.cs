using System.Collections.Generic;
using FacialStuff.DefOfs;
using JetBrains.Annotations;
using RimWorld;
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
        #region Public Fields

        public           bool                 bipedWithHands;
        [NotNull] public List<PawnBodyDrawer> drawers          = new List<PawnBodyDrawer>();

        public string  handType = "Human";
        public Vector3 hipOffset;

        public bool    quadruped;
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