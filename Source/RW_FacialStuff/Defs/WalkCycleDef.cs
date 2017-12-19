namespace FacialStuff.Defs
{
    using Verse;

    public class WalkCycleDef : Def
    {
        #region Public Fields

        public static float passingBegin = 0f;
        public static float up1 = 0.125f;
        public static float contact1 = 0.25f;
        public static float down1 = 0.375f;
        public static float passingMid = 0.5f;
        public static float up2 = 0.625f;
        public static float contact2 = 0.75f;
        public static float down2 = 0.875f;
        public static float passingEnd = 1f;

        public static float armSwingContact = 40f;
        public static float armSwingMax = 50f;

        public SimpleCurve CurveBodyAngle;

        public SimpleCurve CurveBodyAngleVertical;

        public SimpleCurve BodyWobble;

        public SimpleCurve CurveFootAngle;

        public SimpleCurve CurveHandsSwingingVertical;

        public SimpleCurve CurveHorizontalFoot;

        public SimpleCurve CurveVerticalFoot;

        public SimpleCurve SwingCurveHands;

        #endregion Public Fields
    }
}