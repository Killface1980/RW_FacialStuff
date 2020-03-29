// ReSharper disable StyleCop.SA1307
// ReSharper disable InconsistentNaming
// ReSharper disable StyleCop.SA1401
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable NotNullMemberIsNotInitialized
// ReSharper disable CheckNamespace

using System.Collections.Generic;
using FacialStuff.Defs;
using JetBrains.Annotations;
using Verse;
using Verse.AI;

namespace RimWorld
{
    public class WalkCycleDef : Def
    {
        #region Public Fields

        public List<DefHyperlink> descriptionHyperlinks = new List<DefHyperlink>();
        [NotNull]
        public string WalkCycleType;

        public float shoulderAngle;

        [NotNull]
        public List<PawnKeyframe> keyframes = new List<PawnKeyframe>();

        [NotNull]
        public SimpleCurve BodyAngle = new SimpleCurve();

        [NotNull]
        public SimpleCurve BodyAngleVertical = new SimpleCurve();

        [NotNull]
        public SimpleCurve HeadAngleX = new SimpleCurve();

        [NotNull]
        public SimpleCurve HeadOffsetZ = new SimpleCurve();

        [NotNull]
        public SimpleCurve BodyOffsetZ = new SimpleCurve();

        [NotNull]
        public SimpleCurve FootAngle = new SimpleCurve();

        [NotNull]
        public SimpleCurve FootPositionX = new SimpleCurve();

        [NotNull]
        public SimpleCurve FootPositionZ = new SimpleCurve();

        [NotNull]
        public SimpleCurve HandsSwingAngle = new SimpleCurve();

        [NotNull]
        public SimpleCurve HandsSwingPosVertical = new SimpleCurve();

        // public SimpleCurve FootPositionVerticalZ = new SimpleCurve();

        // public SimpleCurve BodyOffsetVerticalZ = new SimpleCurve();
        [NotNull]
        public SimpleCurve FrontPawAngle = new SimpleCurve();

        [NotNull]
        public SimpleCurve FrontPawPositionX = new SimpleCurve();

        [NotNull]
        public SimpleCurve FrontPawPositionZ = new SimpleCurve();

        // public SimpleCurve FrontPawPositionVerticalZ = new SimpleCurve();
        [NotNull]
        public SimpleCurve ShoulderOffsetHorizontalX = new SimpleCurve();

        [NotNull]
        public SimpleCurve HipOffsetHorizontalX = new SimpleCurve();

        public LocomotionUrgency locomotionUrgency = LocomotionUrgency.None;

        #endregion Public Fields
    }
}