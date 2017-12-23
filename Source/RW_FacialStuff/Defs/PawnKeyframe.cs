namespace FacialStuff.Defs
{

    public class PawnKeyframe
    {
        #region Public Fields

        public int keyIndex;

        public float? BodyAngle;

        public float? BodyAngleVertical;

        public float? BodyOffsetVerticalZ;

        public float? BodyOffsetZ;

        public float? FootAngle;

        public float? FootPositionVerticalZ;

        public float? FootPositionX;

        public float? FootPositionZ;

        public float? HandsSwingAngle;

        public float? HandsSwingPosVertical;


        // Quadrupeds
        public float? FrontPawAngle;

        public float? FrontPawPositionX;

        public float? FrontPawPositionZ;

        public float? FrontPawPositionVerticalZ;

        #endregion Public Fields

        #region Public Constructors

        public PawnKeyframe() { }
        public PawnKeyframe(int index)
        {
            this.keyIndex = index;
        }

        #endregion Public Constructors
    }

}