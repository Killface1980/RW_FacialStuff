namespace FacialStuff.Defs
{
    using System.Collections.Generic;

    using Verse;

    public partial class WalkCycleDef : Def
    {
        #region Public Fields

        public List<PawnKeyframe> animation = new List<PawnKeyframe>();

        public SimpleCurve BodyAngle = new SimpleCurve();

        public SimpleCurve BodyAngleVertical = new SimpleCurve();

        public SimpleCurve BodyOffsetVertical = new SimpleCurve();

        public SimpleCurve FootAngle = new SimpleCurve();

        public SimpleCurve FootPositionX = new SimpleCurve();

        public SimpleCurve FootPositionY = new SimpleCurve();

        public SimpleCurve HandsSwingAngle = new SimpleCurve();

        public SimpleCurve HandsSwingPosVertical = new SimpleCurve();

#endregion Public Fields
    }
}