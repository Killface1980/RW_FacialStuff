namespace FacialStuff.Defs
{
    using System.Collections.Generic;

    using Verse;

    public class WalkCycleDef : Def
    {
        #region Public Fields

        public float shoulderAngle;

        public List<PawnKeyframe> animation = new List<PawnKeyframe>();

        public SimpleCurve BodyAngle = new SimpleCurve();

        public SimpleCurve BodyAngleVertical = new SimpleCurve();

        public SimpleCurve BodyOffsetZ = new SimpleCurve();

        public SimpleCurve FootAngle = new SimpleCurve();

        public SimpleCurve FootPositionX = new SimpleCurve();

        public SimpleCurve FootPositionZ = new SimpleCurve();

        public SimpleCurve HandsSwingAngle = new SimpleCurve();

        public SimpleCurve HandsSwingPosVertical = new SimpleCurve();

        public SimpleCurve FootPositionVerticalZ = new SimpleCurve();

        public SimpleCurve BodyOffsetVerticalZ = new SimpleCurve();

        public SimpleCurve FrontPawAngle = new SimpleCurve();

        public SimpleCurve FrontPawPositionX = new SimpleCurve();

        public SimpleCurve FrontPawPositionZ = new SimpleCurve();

        public SimpleCurve FrontPawPositionVerticalZ = new SimpleCurve();

        #endregion Public Fields
    }
}