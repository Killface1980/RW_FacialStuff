using JetBrains.Annotations;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FacialStuff.Defs
{
    public class ExtremityDef : Def
    {
        [NotNull]
        public string texPath;

        public Color overlayColor = Color.white;

        public PartStatus BodyPartStatus = PartStatus.Natural;

        public ExtremityStatus ExtremityType;

        public enum ExtremityStatus
        {
            Hand = 0, Foot = 1, Paw = 2
        }
    }
}